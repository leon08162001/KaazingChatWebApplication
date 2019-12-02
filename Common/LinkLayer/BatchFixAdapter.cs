using Common.TopicMessage;
using Common.Utility;
using Kaazing.JMS;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Common.LinkLayer
{
    /// <summary>
    /// 處理完批次資料的事件參數類別
    /// </summary>
    public class BatchFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _BatchResultTable;
        public BatchFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public BatchFinishedEventArgs(string errorMessage, DataTable BatchResultTable)
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
    public class BatchMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public BatchMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public BatchMismatchedEventArgs(string MismatchedMessage)
        {
            _MismatchedMessage = MismatchedMessage;
        }
        public string MismatchedMessage
        {
            get { return _MismatchedMessage; }
            set { _MismatchedMessage = value; }
        }
    }

    public class BatchFixAdapter : BaseWebSocketAdapter
    {
        // Delegate
        public delegate void BatchFinishedEventHandler(object sender, BatchFinishedEventArgs e);
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

        protected delegate void BatchMismatchedEventHandler(object sender, BatchMismatchedEventArgs e);
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

        IApplicationContext applicationContext = ContextRegistry.GetContext();
        Config config;

        /// <summary>
        /// 完成所有批次資料處理時事件
        /// </summary>
        protected virtual void OnBatchFinished(object state)
        {
            BatchFinishedEventArgs e = state as BatchFinishedEventArgs;
            if (_BatchFinished != null)
            {
                _BatchFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnBatchMismatched(object state)
        {
            BatchMismatchedEventArgs e = state as BatchMismatchedEventArgs;
            if (_BatchMismatched != null)
            {
                _BatchMismatched(this, e);
            }
        }

        protected bool _IsBatchFinished = false;
        protected Type _FixTagType;
        protected Dictionary<string, string> _DicTagType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string,MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static BatchFixAdapter singleton;

        public BatchFixAdapter() : base() { config = (Config)applicationContext.GetObject("Config"); this.BatchMismatched += new BatchMismatchedEventHandler(BatchFixAdapter_BatchMismatched); }

        public BatchFixAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName) { config = (Config)applicationContext.GetObject("Config"); this.BatchMismatched += new BatchMismatchedEventHandler(BatchFixAdapter_BatchMismatched); }

        public BatchFixAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd) { config = (Config)applicationContext.GetObject("Config"); this.BatchMismatched += new BatchMismatchedEventHandler(BatchFixAdapter_BatchMismatched); }

        public static BatchFixAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new BatchFixAdapter();
            }
            return singleton;
        }

        public static BatchFixAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new BatchFixAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }

        public static BatchFixAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new BatchFixAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
            }
            return singleton;
        }

        public Type FixTagType
        {
            set { _FixTagType = value; }
            get { return _FixTagType; }
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
                ClearTimeOutReceivedMessage();
                 string Message = "";
                string _ErrMsg = "";
                Dictionary<string, string> MessageDictionary = new Dictionary<string, string>();
                System.Collections.IEnumerator PropertyNames = message.PropertyNames;
                PropertyNames.Reset();
                while (PropertyNames.MoveNext())
                {
                    string key = PropertyNames.Current.ToString();
                    if (key.Equals("JMSXDeliveryCount"))
                    {
                        continue;
                    }
                    if (key.IndexOf("N")==0)
                    {
                        MessageDictionary.Add(key.Substring(1), message.GetStringProperty(key));
                    }
                    else
                    {
                        MessageDictionary.Add(key, message.GetStringProperty(key));
                    }
                }
                if (MessageDictionary.Keys.Count == 0)
                {
                    return;
                }
                foreach (string key in MessageDictionary.Keys)
                {
                    Message += key + "=" + MessageDictionary[key] + ";";
                }
                //0.檢查是否為HeartBeat訊息,若是則忽略不處理
                if (MessageDictionary.ContainsKey("0"))
                {
                    return;
                }
                //1.檢查是否有指定TagType,以便與傳過來的TagData作驗證用
                if (_FixTagType == null)
                {
                    _ErrMsg = "not yet assigned Tag Type of Tag Data";
                    if (log.IsInfoEnabled) log.Info(_ErrMsg);
                    if (UISyncContext != null && IsEventInUIThread)
                    {
                        UISyncContext.Post(OnMessageHandleFinished, new MessageHandleFinishedEventArgs(_ErrMsg, null));
                    }
                    else
                    {
                        OnMessageHandleFinished(new MessageHandleFinishedEventArgs(_ErrMsg, null));
                    }
                    return;
                }
                _DicTagType = Util.ConvertTagClassConstants(_FixTagType);
                //2.驗證WebSocket傳過來的TagData的tag正確性(與指定的TagType)
                foreach (string key in MessageDictionary.Keys)
                {
                    if (!_DicTagType.ContainsKey(key))
                    {
                        _ErrMsg = string.Format("Tag Data's Tag[{0}] Not in the assigned type[{1}]", key, _FixTagType.Name);
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        if (UISyncContext != null && IsEventInUIThread)
                        {
                            UISyncContext.Post(OnMessageHandleFinished, new MessageHandleFinishedEventArgs(_ErrMsg, null));
                        }
                        else
                        {
                            OnMessageHandleFinished(new MessageHandleFinishedEventArgs(_ErrMsg, null));
                        }
                        return;
                    }
                }
                string MessageID = _FixTagType.GetField("MessageID").GetValue(_FixTagType).ToString();
                //3.驗證資料內容的Message總筆數
                string TotalRecords = _FixTagType.GetField("TotalRecords").GetValue(_FixTagType).ToString();
                if (MessageDictionary.ContainsKey(TotalRecords))
                {
                    //驗證筆數資料正確性
                    //如果筆數不是數值
                    int iTotalRecords;
                    if (!int.TryParse(MessageDictionary[TotalRecords].ToString(), out iTotalRecords))
                    {
                        _ErrMsg = "TotalRecords value must be digit";
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        RunOnMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                }
                //驗證MessageID是否存在
                if (!MessageDictionary.ContainsKey(MessageID))
                {
                    _ErrMsg = "MessageID Of Message in MessageBody is not exist";
                    if (log.IsInfoEnabled) log.Info(_ErrMsg);
                    RunOnMessageHandleFinished(_ErrMsg, null);                    
                }
                    //MessageID存在則檢查DicMessageBody內是否存在此MessageID,沒有則建立DataTable Schema並加入一筆MessageBody至DicMessageBody
                if (!DicMessageBody.ContainsKey(MessageDictionary[MessageID].ToString()))
                {
                    DataTable DT = new DataTable();
                    DT = Util.CreateTableSchema(_DicTagType, _FixTagType);
                    DicMessageBody.Add(MessageDictionary[MessageID].ToString(), new MessageBody(DT, System.DateTime.Now));
                }
                //匯入每筆message到屬於此MessageID的MessageBody
                MessageBody MB = DicMessageBody[MessageDictionary[MessageID].ToString()];
                DataRow MessageRow;
                MessageRow = Util.AddMessageToRow(MessageDictionary, _DicTagType, _FixTagType, MB.Messages);
                if (MessageRow != null)
                {
                    _ErrMsg = "";
                    MB.Messages.Rows.Add(MessageRow);
                    RunOnMessageHandleFinished(_ErrMsg, MessageRow);
                }
                else
                {
                    _ErrMsg = "Error happened when generate DataRow";
                    if (log.IsInfoEnabled) log.Info(_ErrMsg);
                    RunOnMessageHandleFinished(_ErrMsg, null);
                }
                if (DicMessageBody.ContainsKey(MessageDictionary[MessageID].ToString()) && MB.Messages.Rows.Count > 0)
                {
                    int iTotalRecords = Convert.ToInt32(MB.Messages.Rows[0]["TotalRecords"].ToString());
                    //若此MessageID TotalRecords的筆數與在DicMessageBody的Messages筆數相同
                    if (iTotalRecords == DicMessageBody[MessageDictionary[MessageID].ToString()].Messages.Rows.Count)
                    {
                        _ErrMsg = "";
                        DataTable ResultTable = DicMessageBody[MessageDictionary[MessageID].ToString()].Messages.Copy();
                        if (ResultTable.Rows.Count > 0 && ResultTable.Columns.Contains("MacAddress") && !ResultTable.Rows[0].IsNull("MacAddress") && this.SendName.IndexOf("#") != -1)
                        {
                            this.ReStartSender(this.SendName.Replace("#", ResultTable.Rows[0]["MacAddress"].ToString()));
                        }
                        _IsBatchFinished = true;
                        RunOnBatchFinished(_ErrMsg, ResultTable);
                        ClearGuidInDictionary(MessageDictionary[MessageID].ToString());
                        _IsBatchFinished = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// 清除逾時的已接收的WebSocketMessage
        /// </summary>
        public void ClearTimeOutReceivedMessage()
        {
            int TimeOut = Convert.ToInt32(config.WebSocketReceivedMessageReservedSeconds);
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
                        OnBatchMismatched(new BatchMismatchedEventArgs(_ErrMsg));
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

        void BatchFixAdapter_BatchMismatched(object sender, BatchMismatchedEventArgs e)
        {
            if (log.IsInfoEnabled) log.Info(e.MismatchedMessage);
        }

        private void RunOnMessageHandleFinished(string ErrorMessage, DataRow MessageRow)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnMessageHandleFinished, new MessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
            else
            {
                OnMessageHandleFinished(new MessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
        }

        private void RunOnBatchFinished(string ErrorMessage, DataTable BatchResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnBatchFinished, new BatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
            else
            {
                OnBatchFinished(new BatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
        }
    }
}
