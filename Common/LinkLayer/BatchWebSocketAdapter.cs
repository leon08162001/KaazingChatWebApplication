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
    public class WebSocketBatchFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _BatchResultTable;
        public WebSocketBatchFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public WebSocketBatchFinishedEventArgs(string errorMessage, DataTable BatchResultTable)
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
    public class WebSocketBatchMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public WebSocketBatchMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public WebSocketBatchMismatchedEventArgs(string MismatchedMessage)
        {
            _MismatchedMessage = MismatchedMessage;
        }
        public string MismatchedMessage
        {
            get { return _MismatchedMessage; }
            set { _MismatchedMessage = value; }
        }
    }

    public class BatchWebSocketAdapter : BaseWebSocketAdapter
    {
        // Delegate
        public delegate void WebSocketBatchFinishedEventHandler(object sender, WebSocketBatchFinishedEventArgs e);
        List<WebSocketBatchFinishedEventHandler> WebSocketBatchFinishedEventDelegates = new List<WebSocketBatchFinishedEventHandler>();
        private event WebSocketBatchFinishedEventHandler _WebSocketBatchFinished;
        public event WebSocketBatchFinishedEventHandler WebSocketBatchFinished
        {
            add
            {
                _WebSocketBatchFinished += value;
                WebSocketBatchFinishedEventDelegates.Add(value);
            }
            remove
            {
                _WebSocketBatchFinished -= value;
                WebSocketBatchFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void WebSocketBatchMismatchedEventHandler(object sender, WebSocketBatchMismatchedEventArgs e);
        List<WebSocketBatchMismatchedEventHandler> WebSocketBatchMismatchedEventDelegates = new List<WebSocketBatchMismatchedEventHandler>();
        private event WebSocketBatchMismatchedEventHandler _WebSocketBatchMismatched;
        protected event WebSocketBatchMismatchedEventHandler WebSocketBatchMismatched
        {
            add
            {
                _WebSocketBatchMismatched += value;
                WebSocketBatchMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _WebSocketBatchMismatched -= value;
                WebSocketBatchMismatchedEventDelegates.Remove(value);
            }
        }

        IApplicationContext applicationContext = ContextRegistry.GetContext();
        Config config;

        /// <summary>
        /// 完成所有批次資料處理時事件
        /// </summary>
        protected virtual void OnWebSocketBatchFinished(object state)
        {
            WebSocketBatchFinishedEventArgs e = state as WebSocketBatchFinishedEventArgs;
            if (_WebSocketBatchFinished != null)
            {
                _WebSocketBatchFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnWebSocketBatchMismatched(object state)
        {
            WebSocketBatchMismatchedEventArgs e = state as WebSocketBatchMismatchedEventArgs;
            if (_WebSocketBatchMismatched != null)
            {
                _WebSocketBatchMismatched(this, e);
            }
        }

        protected bool _IsBatchFinished = false;
        protected Type _DataType;
        protected Dictionary<string, string> _DicDataType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string,MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static BatchWebSocketAdapter singleton;

        public BatchWebSocketAdapter() : base() { config = (Config)applicationContext.GetObject("Config"); this.WebSocketBatchMismatched += new WebSocketBatchMismatchedEventHandler(BatchWebSocketAdapter_WebSocketBatchMismatched); }

        public BatchWebSocketAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName) { config = (Config)applicationContext.GetObject("Config"); this.WebSocketBatchMismatched += new WebSocketBatchMismatchedEventHandler(BatchWebSocketAdapter_WebSocketBatchMismatched); }

        public BatchWebSocketAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd) { config = (Config)applicationContext.GetObject("Config"); this.WebSocketBatchMismatched += new WebSocketBatchMismatchedEventHandler(BatchWebSocketAdapter_WebSocketBatchMismatched); }

        public static BatchWebSocketAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new BatchWebSocketAdapter();
            }
            return singleton;
        }

        public static BatchWebSocketAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new BatchWebSocketAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }

        public static BatchWebSocketAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new BatchWebSocketAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
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
            foreach (WebSocketBatchFinishedEventHandler eh in WebSocketBatchFinishedEventDelegates)
            {
                _WebSocketBatchFinished -= eh;
            }
            WebSocketBatchFinishedEventDelegates.Clear();
            foreach (WebSocketBatchMismatchedEventHandler eh in WebSocketBatchMismatchedEventDelegates)
            {
                _WebSocketBatchMismatched -= eh;
            }
            WebSocketBatchMismatchedEventDelegates.Clear();
        }

        public override void processMessage(IMessage message)
        {
            try
            {
                ClearTimeOutReceivedMessage();
                string Message = "";
                string _ErrMsg = "";
                //接收檔案
                if (message.PropertyExists("filename"))
                {
                    IBytesMessage msg = message as IBytesMessage;
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
                        RunOnMessageHandleFinished(_ErrMsg, MessageRow);
                        if (this.Handler != null)
                        {
                            this.Handler.WorkItemQueue.Enqueue(MessageDT);
                        }
                        _IsBatchFinished = true;
                        _Session.Commit();
                        RunOnBatchFinished(_ErrMsg, MessageDT);
                        _IsBatchFinished = false;
                    }
                    catch (Exception ex1)
                    {
                        _ErrMsg = ex1.Message;
                        RunOnMessageHandleFinished(_ErrMsg, null);
                        _IsBatchFinished = true;
                        _Session.Commit();
                        RunOnBatchFinished(_ErrMsg, MessageDT);
                        _IsBatchFinished = false;
                        if (log.IsErrorEnabled) log.Error(ex1.Message, ex1);
                    }
                }
                //接收文字訊息
                else
                {
                    if (_DataType.Equals(typeof (String)))
                    {
                        ITextMessage msg = message as ITextMessage;
                        DataTable dt = new DataTable();
                        dt.Columns.Add("message");
                        DataRow dr = dt.NewRow();
                        dr[0] = msg.Text;
                        RunOnMessageHandleFinished(_ErrMsg, dr);
                        return;
                    }
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
                        if (key.IndexOf("N") == 0)
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
                    if (_DataType == null)
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
                    _DicDataType = Util.ConvertTagClassConstants(_DataType);
                    //2.驗證WebSocket傳過來的TagData的tag正確性(與指定的TagType)
                    foreach (string key in MessageDictionary.Keys)
                    {
                        if (!_DicDataType.ContainsKey(key))
                        {
                            _ErrMsg = string.Format("Tag Data's Tag[{0}] Not in the assigned type[{1}]", key, _DataType.Name);
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
                    string MessageID = _DataType.GetField("MessageID").GetValue(_DataType).ToString();
                    //3.驗證資料內容的Message總筆數
                    string TotalRecords = _DataType.GetField("TotalRecords").GetValue(_DataType).ToString();
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
                        DT = Util.CreateTableSchema(_DicDataType, _DataType);
                        DicMessageBody.Add(MessageDictionary[MessageID].ToString(), new MessageBody(DT, System.DateTime.Now));
                    }
                    //匯入每筆message到屬於此MessageID的MessageBody
                    MessageBody MB = DicMessageBody[MessageDictionary[MessageID].ToString()];
                    DataRow MessageRow;
                    MessageRow = Util.AddMessageToRow(MessageDictionary, _DicDataType, _DataType, MB.Messages);
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
                            _Session.Commit();
                            RunOnBatchFinished(_ErrMsg, ResultTable);
                            ClearGuidInDictionary(MessageDictionary[MessageID].ToString());
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
                        OnWebSocketBatchMismatched(new WebSocketBatchMismatchedEventArgs(_ErrMsg));
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

        void BatchWebSocketAdapter_WebSocketBatchMismatched(object sender, WebSocketBatchMismatchedEventArgs e)
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
                UISyncContext.Post(OnWebSocketBatchFinished, new WebSocketBatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
            else
            {
                OnWebSocketBatchFinished(new WebSocketBatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
        }
    }
}
