using Apache.NMS;
using Common.TopicMessage;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Common.LinkLayer
{
    /// <summary>
    /// MQ處理完批次資料的事件參數類別
    /// </summary>
    public class MQBatchFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _BatchResultTable;
        public MQBatchFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public MQBatchFinishedEventArgs(string errorMessage, DataTable BatchResultTable)
        {
            _errorMessage = errorMessage;
            _BatchResultTable = BatchResultTable;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
        public DataTable BatchResultTable
        {
            get { return _BatchResultTable; }
            set { _BatchResultTable = value; }
        }
    }
    /// <summary>
    /// MessageHeader's Count與MessageBody's DataRow Count不符合事件參數類別
    /// </summary>
    public class MQBatchMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public MQBatchMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public MQBatchMismatchedEventArgs(string MismatchedMessage)
        {
            _MismatchedMessage = MismatchedMessage;
        }
        public string MismatchedMessage
        {
            get { return _MismatchedMessage; }
            set { _MismatchedMessage = value; }
        }
    }
    [Serializable]
    public class BatchMQAdapter : BaseMQAdapter
    {
        // Delegate
        public delegate void BatchFinishedEventHandler(object sender, MQBatchFinishedEventArgs e);
        List<BatchFinishedEventHandler> BatchFinishedEventDelegates = new List<BatchFinishedEventHandler>();
        private event BatchFinishedEventHandler _BatchFinished;
        public event BatchFinishedEventHandler BatchFinished
        {
            add
            {
                _BatchFinished += value;
                BatchFinishedEventDelegates.Add(value);
            }
            remove
            {
                _BatchFinished -= value;
                BatchFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void BatchMismatchedEventHandler(object sender, MQBatchMismatchedEventArgs e);
        List<BatchMismatchedEventHandler> BatchMismatchedEventDelegates = new List<BatchMismatchedEventHandler>();
        private event BatchMismatchedEventHandler _BatchMismatched;
        protected event BatchMismatchedEventHandler BatchMismatched
        {
            add
            {
                _BatchMismatched += value;
                BatchMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _BatchMismatched -= value;
                BatchMismatchedEventDelegates.Remove(value);
            }
        }

        //IApplicationContext applicationContext = ContextRegistry.GetContext();
        //Config config;

        /// <summary>
        /// MQ完成所有批次資料處理時事件
        /// </summary>
        protected virtual void OnBatchFinished(object state)
        {
            MQBatchFinishedEventArgs e = state as MQBatchFinishedEventArgs;
            if (_BatchFinished != null)
            {
                _BatchFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutMQReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnBatchMismatched(object state)
        {
            MQBatchMismatchedEventArgs e = state as MQBatchMismatchedEventArgs;
            if (_BatchMismatched != null)
            {
                _BatchMismatched(this, e);
            }
        }

        protected bool _IsBatchFinished = false;
        protected Dictionary<string, string> _DicTagType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string,MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static BatchMQAdapter singleton;

        public BatchMQAdapter() : base()
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.BatchMismatched += new BatchMismatchedEventHandler(BatchMQAdapter_BatchMismatched);
        }

        public BatchMQAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName)
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.BatchMismatched += new BatchMismatchedEventHandler(BatchMQAdapter_BatchMismatched);
        }

        public BatchMQAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd)
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.BatchMismatched += new BatchMismatchedEventHandler(BatchMQAdapter_BatchMismatched);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static BatchMQAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new BatchMQAdapter();
            }
            return singleton;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static BatchMQAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new BatchMQAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static BatchMQAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new BatchMQAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
            }
            return singleton;
        }

        public bool IsBatchFinished
        {
            get { return _IsBatchFinished; }
        }

        public override void RemoveAllEvents()
        {
            base.RemoveAllEvents();
            foreach (BatchFinishedEventHandler eh in BatchFinishedEventDelegates)
            {
                _BatchFinished -= eh;
            }
            BatchFinishedEventDelegates.Clear();
            foreach (BatchMismatchedEventHandler eh in BatchMismatchedEventDelegates)
            {
                _BatchMismatched -= eh;
            }
            BatchMismatchedEventDelegates.Clear();
        }

        public override void processMessage(IMessage message)
        {
            try
            {
                ClearTimeOutMQReceivedMessage();
                string _ErrMsg = "";
                //接收檔案
                if (message.Properties.Contains("filename"))
                {
                    IBytesMessage msg = message as IBytesMessage;
                    DataTable MessageDT = new DataTable();
                    MessageDT.TableName = "file";
                    try
                    {
                        foreach (object key in msg.Properties.Keys)
                        {
                            MessageDT.Columns.Add(key.ToString(), typeof(System.String));
                        }
                        //MessageDT.Columns.Add("content", typeof(byte[]));
                        MessageDT.Columns.Add("content", typeof(IBytesMessage));
                        //匯入檔案內容到Datatable
                        DataRow MessageRow;
                        MessageRow = MessageDT.NewRow();
                        foreach (object key in msg.Properties.Keys)
                        {
                            MessageRow[key.ToString()] = msg.Properties[key.ToString()];
                        }
                        //byte[] byteArr = new byte[msg.BodyLength];
                        //msg.ReadBytes(byteArr);
                        //MessageRow["content"] = byteArr;
                        //MessageDT.Rows.Add(MessageRow);

                        MessageRow["content"] = msg;
                        MessageDT.Rows.Add(MessageRow);
                        RunOnMessageHandleFinished(_ErrMsg, MessageRow);
                        if (this.Handler != null)
                        {
                            this.Handler.WorkItemQueue.Enqueue(MessageDT);
                        }
                        _IsBatchFinished = true;
                        RunOnBatchFinished(_ErrMsg, MessageDT);
                        _IsBatchFinished = false;
                    }
                    catch (Exception ex1)
                    {
                        _ErrMsg = ex1.Message;
                        RunOnMessageHandleFinished(_ErrMsg, null);
                        _IsBatchFinished = true;
                        RunOnBatchFinished(_ErrMsg, MessageDT);
                        _IsBatchFinished = false;
                        Common.LogHelper.Logger.LogError<BatchMQAdapter>(ex1);
                    }
                }
                //接收文字訊息
                else
                {
                    if (_DataType.Equals(typeof(String)))
                    {
                        ITextMessage msg = message as ITextMessage;
                        DataTable ResultTable = new DataTable();
                        ResultTable.Columns.Add("message");
                        DataRow dr = ResultTable.NewRow();
                        dr[0] = msg.Text;
                        ResultTable.Rows.Add(dr);
                        RunOnMessageHandleFinished(_ErrMsg, dr);
                        if (this.Handler != null)
                        {
                            this.Handler.WorkItemQueue.Enqueue(ResultTable);
                        }
                        _IsBatchFinished = true;
                        RunOnBatchFinished(_ErrMsg, ResultTable);
                        _IsBatchFinished = false;
                        return;
                    }
                    Dictionary<string, string> MessageDictionary = new Dictionary<string, string>();
                    foreach (object key in message.Properties.Keys)
                    {
                        MessageDictionary.Add(key.ToString(), message.Properties[key.ToString()] == null ? null : message.Properties[key.ToString()].ToString());
                    }
                    if (MessageDictionary.Keys.Count == 0)
                    {
                        return;
                    }
                    //0.檢查是否為HeartBeat訊息,若是則忽略不處理
                    if (MessageDictionary.ContainsKey("0"))
                    {
                        return;
                    }
                    //1.檢查是否有指定TagType,以便與傳過來的TagData作驗證用
                    if (_DataType == null)
                    {
                        _ErrMsg = "not yet assigned Tag Type of Tag Data";
                        Common.LogHelper.Logger.LogInfo<BatchMQAdapter>(_ErrMsg);
                        RunOnMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    _DicTagType = Util.ConvertTagClassConstants(_DataType);
                    //2.驗證MQ傳過來的TagData的tag正確性(與指定的TagType)
                    if (MessageDictionary.ContainsKey("__AMQ_CID"))
                    {
                        MessageDictionary.Remove("__AMQ_CID");
                    }
                    foreach (string key in MessageDictionary.Keys)
                    {
                        if (!_DicTagType.ContainsKey(key))
                        {
                            _ErrMsg = string.Format("Tag Data's Tag[{0}] Not in the assigned type[{1}]", key, _DataType.Name);
                            Common.LogHelper.Logger.LogInfo<BatchMQAdapter>(_ErrMsg);
                            RunOnMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    string MessageID = _DataType.GetField("MessageID") == null ? "710" : _DataType.GetField("MessageID").GetValue(_DataType).ToString();
                    //3.驗證資料內容的Message總筆數
                    string TotalRecords = _DataType.GetField("TotalRecords") == null ? "10038" : _DataType.GetField("TotalRecords").GetValue(_DataType).ToString();
                    if (MessageDictionary.ContainsKey(TotalRecords))
                    {
                        //驗證筆數資料正確性
                        //如果筆數不是數值
                        int iTotalRecords;
                        if (!int.TryParse(MessageDictionary[TotalRecords].ToString(), out iTotalRecords))
                        {
                            _ErrMsg = "TotalRecords value must be digit";
                            Common.LogHelper.Logger.LogInfo<BatchMQAdapter>(_ErrMsg);
                            RunOnMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    //驗證MessageID是否存在
                    if (!MessageDictionary.ContainsKey(MessageID))
                    {
                        _ErrMsg = "MessageID Of Message in MessageBody is not exist";
                        Common.LogHelper.Logger.LogInfo<BatchMQAdapter>(_ErrMsg);
                        RunOnMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    //MessageID存在則檢查DicMessageBody內是否存在此MessageID,沒有則建立DataTable Schema並加入一筆MessageBody至DicMessageBody
                    if (!DicMessageBody.ContainsKey(MessageDictionary[MessageID].ToString()))
                    {
                        DataTable DT = new DataTable();
                        DT = Util.CreateTableSchema(_DicTagType, _DataType);
                        DicMessageBody.Add(MessageDictionary[MessageID].ToString(), new MessageBody(DT, System.DateTime.Now));
                    }
                    //匯入每筆message到屬於此MessageID的MessageBody
                    MessageBody MB = DicMessageBody[MessageDictionary[MessageID].ToString()];
                    DataRow MessageRow;
                    MessageRow = Util.AddMessageToRow(MessageDictionary, _DicTagType, _DataType, MB.Messages);
                    if (MessageRow != null)
                    {
                        _ErrMsg = "";
                        MB.Messages.Rows.Add(MessageRow);
                        RunOnMessageHandleFinished(_ErrMsg, MessageRow);
                    }
                    else
                    {
                        _ErrMsg = "Error happened when generate DataRow";
                        Common.LogHelper.Logger.LogInfo<BatchMQAdapter>(_ErrMsg);
                        RunOnMessageHandleFinished(_ErrMsg, null);
                    }
                    if (DicMessageBody.ContainsKey(MessageDictionary[MessageID].ToString()) && MB.Messages.Rows.Count > 0)
                    {
                        int iTotalRecords = Convert.ToInt32(MB.Messages.Rows[0]["TotalRecords"].ToString());
                        //若此MessageID TotalRecords的筆數與在DicMessageBody的Messages筆數相同
                        if (iTotalRecords == DicMessageBody[MessageDictionary[MessageID].ToString()].Messages.Rows.Count)
                        {
                            _ErrMsg = "";
                            //DataTable ResultTable = DicMessageBody[MessageDictionary[MessageID].ToString()].Messages.Copy();
                            DataTable ResultTable = DicMessageBody[MessageDictionary[MessageID].ToString()].Messages;
                            if (ResultTable.Rows.Count > 0 && ResultTable.Columns.Contains("MacAddress") && !ResultTable.Rows[0].IsNull("MacAddress") && this.SendName.IndexOf("#") != -1)
                            {
                                this.ReStartSender(this.SendName.Replace("#", ResultTable.Rows[0]["MacAddress"].ToString()));
                            }
                            if (this.Handler != null)
                            {
                                this.Handler.WorkItemQueue.Enqueue(ResultTable);
                            }
                            _IsBatchFinished = true;
                            RunOnBatchFinished(_ErrMsg, ResultTable);
                            ClearGuidInDictionary(MessageDictionary[MessageID].ToString());
                            _IsBatchFinished = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.Logger.LogError<BatchMQAdapter>(ex);
            }
        }

        /// <summary>
        /// 清除逾時的已接收的Message
        /// </summary>
        public void ClearTimeOutMQReceivedMessage()
        {
            int TimeOut = this.ReceivedMessageTimeOut;
            DateTime SysTime = System.DateTime.Now;
            foreach (string Guid in DicMessageBody.Keys.ToArray())
            {
                if ((SysTime - DicMessageBody[Guid].CreatedTime).Seconds >= TimeOut)
                {
                    MessageBody MB = DicMessageBody[Guid];
                    int iTotalRecords = Convert.ToInt32(MB.Messages.Rows[0]["TotalRecords"].ToString());
                    int BodyCount = MB.Messages.Rows.Count;
                    if (iTotalRecords != BodyCount)
                    {
                        string _ErrMsg = string.Format("Message Body Rows({0}) of Message ID:{1} is not match TotalRecords({2})", BodyCount, Guid, iTotalRecords);
                        Common.LogHelper.Logger.LogInfo<BatchMQAdapter>(_ErrMsg);
                        OnBatchMismatched(new MQBatchMismatchedEventArgs(_ErrMsg));
                    }
                    DicMessageBody.Remove(Guid);
                }
            }
        }
        /// <summary>
        /// 清除Dictionary裏指定的Guid
        /// </summary>
        /// <param name="Guid"></param>
        public void ClearGuidInDictionary(string Guid)
        {
            DicMessageBody.Remove(Guid);
        }

        void BatchMQAdapter_BatchMismatched(object sender, MQBatchMismatchedEventArgs e)
        {
            Common.LogHelper.Logger.LogInfo<BatchMQAdapter>(e.MismatchedMessage);
        }

        private void RunOnMessageHandleFinished(string ErrorMessage, DataRow MessageRow)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnMessageHandleFinished, new MQMessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
            else
            {
                OnMessageHandleFinished(new MQMessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
        }

        private void RunOnBatchFinished(string ErrorMessage, DataTable BatchResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnBatchFinished, new MQBatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
            else
            {
                OnBatchFinished(new MQBatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
        }
    }
}
