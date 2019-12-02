using Common.TopicMessage;
using Common.Utility;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using TIBCO.EMS;

namespace Common.LinkLayer
{
    /// <summary>
    /// Tibco EMS處理完批次資料的事件參數類別
    /// </summary>
    public class EMSBatchFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _BatchResultTable;
        public EMSBatchFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public EMSBatchFinishedEventArgs(string errorMessage, DataTable BatchResultTable)
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
    public class EMSBatchMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public EMSBatchMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public EMSBatchMismatchedEventArgs(string MismatchedMessage)
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

    public class BatchEMSAdapter : BaseEMSAdapter
    {
        // Delegate
        public delegate void EMSBatchFinishedEventHandler(object sender, EMSBatchFinishedEventArgs e);
        List<EMSBatchFinishedEventHandler> EMSBatchFinishedEventDelegates = new List<EMSBatchFinishedEventHandler>();
        private event EMSBatchFinishedEventHandler _EMSBatchFinished;
        public event EMSBatchFinishedEventHandler EMSBatchFinished
        {
            add
            {
                _EMSBatchFinished += value;
                EMSBatchFinishedEventDelegates.Add(value);
            }
            remove
            {
                _EMSBatchFinished -= value;
                EMSBatchFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void EMSBatchMismatchedEventHandler(object sender, EMSBatchMismatchedEventArgs e);
        List<EMSBatchMismatchedEventHandler> EMSBatchMismatchedEventDelegates = new List<EMSBatchMismatchedEventHandler>();
        private event EMSBatchMismatchedEventHandler _EMSBatchMismatched;
        protected event EMSBatchMismatchedEventHandler EMSBatchMismatched
        {
            add
            {
                _EMSBatchMismatched += value;
                EMSBatchMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _EMSBatchMismatched -= value;
                EMSBatchMismatchedEventDelegates.Remove(value);
            }
        }

        IApplicationContext applicationContext = ContextRegistry.GetContext();
        Config config;

        /// <summary>
        /// Tibco EMS完成所有批次資料處理時事件
        /// </summary>
        protected virtual void OnEMSBatchFinished(object state)
        {
            EMSBatchFinishedEventArgs e = state as EMSBatchFinishedEventArgs;
            if (_EMSBatchFinished != null)
            {
                _EMSBatchFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutEMSReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnEMSBatchMismatched(object state)
        {
            EMSBatchMismatchedEventArgs e = state as EMSBatchMismatchedEventArgs;
            if (_EMSBatchMismatched != null)
            {
                _EMSBatchMismatched(this, e);
            }
        }

        protected bool _IsBatchFinished = false;
        protected Type _DataType;
        protected Dictionary<string, string> _DicTagType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string,MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static BatchEMSAdapter singleton;

        public BatchEMSAdapter() : base() { config = (Config)applicationContext.GetObject("Config"); this.EMSBatchMismatched += new EMSBatchMismatchedEventHandler(BatchEMSAdapter_EMSBatchMismatched); }

        public BatchEMSAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName) { config = (Config)applicationContext.GetObject("Config"); this.EMSBatchMismatched += new EMSBatchMismatchedEventHandler(BatchEMSAdapter_EMSBatchMismatched); }

        public BatchEMSAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd) { config = (Config)applicationContext.GetObject("Config"); this.EMSBatchMismatched += new EMSBatchMismatchedEventHandler(BatchEMSAdapter_EMSBatchMismatched); }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static BatchEMSAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new BatchEMSAdapter();
            }
            return singleton;
        }

        public static BatchEMSAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new BatchEMSAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }

        public static BatchEMSAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new BatchEMSAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
            }
            return singleton;
        }

        public Type DataType
        {
            set { _DataType = value; }
            get { return _DataType; }
        }

        public bool IsBatchFinished
        {
            get { return _IsBatchFinished; }
        }

        public override void RemoveAllEvents()
        {
            base.RemoveAllEvents();
            foreach (EMSBatchFinishedEventHandler eh in EMSBatchFinishedEventDelegates)
            {
                _EMSBatchFinished -= eh;
            }
            EMSBatchFinishedEventDelegates.Clear();
            foreach (EMSBatchMismatchedEventHandler eh in EMSBatchMismatchedEventDelegates)
            {
                _EMSBatchMismatched -= eh;
            }
            EMSBatchMismatchedEventDelegates.Clear();
        }

        public override void processEMSMessage(Message message)
        {
            try
            {
                ClearTimeOutEMSReceivedMessage();
                string _ErrMsg = "";
                //接收檔案
                if (message.PropertyExists("filename"))
                {
                    BytesMessage msg = message as BytesMessage;
                    DataTable MessageDT = new DataTable();
                    MessageDT.TableName = "file";
                    try
                    {
                        System.Collections.IEnumerator PropertyNames = msg.PropertyNames;
                        PropertyNames.Reset();
                        while (PropertyNames.MoveNext())
                        {
                            string key = PropertyNames.Current.ToString();
                            MessageDT.Columns.Add(key, typeof(System.String));
                        }
                        MessageDT.Columns.Add("content", typeof(byte[]));
                        //匯入檔案內容到Datatable
                        DataRow MessageRow;
                        MessageRow = MessageDT.NewRow();
                        PropertyNames.Reset();
                        while (PropertyNames.MoveNext())
                        {
                            string key = PropertyNames.Current.ToString();
                            MessageRow[key.ToString()] = msg.GetStringProperty(key);
                        }
                        byte[] byteArr = new byte[msg.BodyLength];
                        msg.ReadBytes(byteArr);
                        MessageRow["content"] = byteArr;
                        MessageDT.Rows.Add(MessageRow);
                        RunOnEMSMessageHandleFinished(_ErrMsg, MessageRow);
                        if (this.Handler != null)
                        {
                            this.Handler.WorkItemQueue.Enqueue(MessageDT);
                        }
                        _IsBatchFinished = true;
                        RunOnEMSBatchFinished(_ErrMsg, MessageDT);
                        _IsBatchFinished = false;
                    }
                    catch (Exception ex1)
                    {
                        _ErrMsg = ex1.Message;
                        RunOnEMSMessageHandleFinished(_ErrMsg, null);
                        _IsBatchFinished = true;
                        RunOnEMSBatchFinished(_ErrMsg, MessageDT);
                        _IsBatchFinished = false;
                        if (log.IsErrorEnabled) log.Error(ex1.Message, ex1);
                    }
                }
                //接收文字訊息
                else
                {
                    Dictionary<string, string> EMSMessageDictionary = new Dictionary<string, string>();
                    System.Collections.IEnumerator PropertyNames = message.PropertyNames;
                    PropertyNames.Reset();
                    while (PropertyNames.MoveNext())
                    {
                        string key = PropertyNames.Current.ToString();
                        if (key.Equals("JMSXDeliveryCount"))
                        {
                            continue;
                        }
                        EMSMessageDictionary.Add(key, message.GetStringProperty(key));
                    }
                    if (EMSMessageDictionary.Keys.Count == 0)
                    {
                        return;
                    }
                    //0.檢查是否為HeartBeat訊息,若是則忽略不處理
                    if (EMSMessageDictionary.ContainsKey("0"))
                    {
                        return;
                    }
                    //1.檢查是否有指定TagType,以便與傳過來的TagData作驗證用
                    if (_DataType == null)
                    {
                        _ErrMsg = "not yet assigned Tag Type of Tag Data";
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        RunOnEMSMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    _DicTagType = Util.ConvertTagClassConstants(_DataType);
                    //2.驗證EMS傳過來的TagData的tag正確性(與指定的TagType)
                    foreach (string key in EMSMessageDictionary.Keys)
                    {
                        if (!_DicTagType.ContainsKey(key))
                        {
                            _ErrMsg = string.Format("Tag Data's Tag[{0}] Not in the assigned type[{1}]", key, _DataType.Name);
                            if (log.IsInfoEnabled) log.Info(_ErrMsg);
                            RunOnEMSMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    string MessageID = _DataType.GetField("MessageID") == null ? "710" : _DataType.GetField("MessageID").GetValue(_DataType).ToString();
                    //3.驗證資料內容的Message總筆數
                    string TotalRecords = _DataType.GetField("TotalRecords") == null ? "10038" : _DataType.GetField("TotalRecords").GetValue(_DataType).ToString();
                    if (EMSMessageDictionary.ContainsKey(TotalRecords))
                    {
                        //驗證筆數資料正確性
                        //如果筆數不是數值
                        int iTotalRecords;
                        if (!int.TryParse(EMSMessageDictionary[TotalRecords].ToString(), out iTotalRecords))
                        {
                            _ErrMsg = "TotalRecords value must be digit";
                            if (log.IsInfoEnabled) log.Info(_ErrMsg);
                            RunOnEMSMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    //驗證MessageID是否存在
                    if (!EMSMessageDictionary.ContainsKey(MessageID))
                    {
                        _ErrMsg = "MessageID Of Message in MessageBody is not exist";
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        RunOnEMSMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    //MessageID存在則檢查DicMessageBody內是否存在此MessageID,沒有則建立DataTable Schema並加入一筆MessageBody至DicMessageBody
                    if (!DicMessageBody.ContainsKey(EMSMessageDictionary[MessageID].ToString()))
                    {
                        DataTable DT = new DataTable();
                        DT = Util.CreateTableSchema(_DicTagType, _DataType);
                        DicMessageBody.Add(EMSMessageDictionary[MessageID].ToString(), new MessageBody(DT, System.DateTime.Now));
                    }
                    //匯入每筆message到屬於此MessageID的MessageBody
                    MessageBody MB = DicMessageBody[EMSMessageDictionary[MessageID].ToString()];
                    DataRow MessageRow;
                    MessageRow = Util.AddMessageToRow(EMSMessageDictionary, _DicTagType, _DataType, MB.Messages);
                    if (MessageRow != null)
                    {
                        _ErrMsg = "";
                        MB.Messages.Rows.Add(MessageRow);
                        RunOnEMSMessageHandleFinished(_ErrMsg, MessageRow);
                    }
                    else
                    {
                        _ErrMsg = "Error happened when generate DataRow";
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        RunOnEMSMessageHandleFinished(_ErrMsg, null);
                    }
                    if (DicMessageBody.ContainsKey(EMSMessageDictionary[MessageID].ToString()) && MB.Messages.Rows.Count > 0)
                    {
                        int iTotalRecords = Convert.ToInt32(MB.Messages.Rows[0]["TotalRecords"].ToString());
                        //若此MessageID TotalRecords的筆數與在DicMessageBody的Messages筆數相同
                        if (iTotalRecords == DicMessageBody[EMSMessageDictionary[MessageID].ToString()].Messages.Rows.Count)
                        {
                            _ErrMsg = "";
                            //DataTable ResultTable = DicMessageBody[EMSMessageDictionary[MessageID].ToString()].Messages.Copy();
                            DataTable ResultTable = DicMessageBody[EMSMessageDictionary[MessageID].ToString()].Messages;
                            if (ResultTable.Rows.Count > 0 && ResultTable.Columns.Contains("MacAddress") && !ResultTable.Rows[0].IsNull("MacAddress") && this.SendName.IndexOf("#") != -1)
                            {
                                this.ReStartSender(this.SendName.Replace("#", ResultTable.Rows[0]["MacAddress"].ToString()));
                            }
                            if (this.Handler != null)
                            {
                                this.Handler.WorkItemQueue.Enqueue(ResultTable);
                            }
                            _IsBatchFinished = true;
                            RunOnEMSBatchFinished(_ErrMsg, ResultTable);
                            ClearGuidInDictionary(EMSMessageDictionary[MessageID].ToString());
                            _IsBatchFinished = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// 清除逾時的已接收的EMSMessage
        /// </summary>
        public void ClearTimeOutEMSReceivedMessage()
        {
            int TimeOut = Convert.ToInt32(10);
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
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        OnEMSBatchMismatched(new EMSBatchMismatchedEventArgs(_ErrMsg));
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

        void BatchEMSAdapter_EMSBatchMismatched(object sender, EMSBatchMismatchedEventArgs e)
        {
            if (log.IsInfoEnabled) log.Info(e.MismatchedMessage);
        }

        private void RunOnEMSMessageHandleFinished(string ErrorMessage, DataRow MessageRow)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnEMSMessageHandleFinished, new EMSMessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
            else
            {
                OnEMSMessageHandleFinished(new EMSMessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
        }

        private void RunOnEMSBatchFinished(string ErrorMessage, DataTable BatchResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnEMSBatchFinished, new EMSBatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
            else
            {
                OnEMSBatchFinished(new EMSBatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
        }
    }
}
