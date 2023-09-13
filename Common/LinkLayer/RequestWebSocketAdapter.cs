using Common.TopicMessage;
using Common.Utility;
using Kaazing.JMS;
//using Spring.Context;
//using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Common.LinkLayer
{
    /// <summary>
    /// 處理完所有回應相同RequestID資料的事件參數類別
    /// </summary>
    public class WebSocketResponseFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _ResponseResultTable;
        public WebSocketResponseFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public WebSocketResponseFinishedEventArgs(string errorMessage, DataTable ResponseResultTable)
        {
            _errorMessage = errorMessage;
            _ResponseResultTable = ResponseResultTable;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
        public DataTable ResponseResultTable
        {
            get { return _ResponseResultTable; }
            set { _ResponseResultTable = value; }
        }
    }
    /// <summary>
    /// MessageHeader's Count與MessageBody's DataRow Count不符合事件參數類別
    /// </summary>
    public class WebSocketResponseMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public WebSocketResponseMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public WebSocketResponseMismatchedEventArgs(string MismatchedMessage)
        {
            _MismatchedMessage = MismatchedMessage;
        }
        public string MismatchedMessage
        {
            get { return _MismatchedMessage; }
            set { _MismatchedMessage = value; }
        }
    }

    public class RequestWebSocketAdapter : BaseWebSocketAdapter
    {
        // Delegate
        public delegate void WebSocketResponseFinishedEventHandler(object sender, WebSocketResponseFinishedEventArgs e);
        List<WebSocketResponseFinishedEventHandler> WebSocketResponseFinishedEventDelegates = new List<WebSocketResponseFinishedEventHandler>();
        private event WebSocketResponseFinishedEventHandler _WebSocketResponseFinished;
        public event WebSocketResponseFinishedEventHandler WebSocketResponseFinished
        {
            add
            {
                _WebSocketResponseFinished += value;
                WebSocketResponseFinishedEventDelegates.Add(value);
            }
            remove
            {
                _WebSocketResponseFinished -= value;
                WebSocketResponseFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void WebSocketResponseMismatchedEventHandler(object sender, WebSocketResponseMismatchedEventArgs e);
        List<WebSocketResponseMismatchedEventHandler> WebSocketResponseMismatchedEventDelegates = new List<WebSocketResponseMismatchedEventHandler>();
        private event WebSocketResponseMismatchedEventHandler _WebSocketResponseMismatched;
        protected event WebSocketResponseMismatchedEventHandler WebSocketResponseMismatched
        {
            add
            {
                _WebSocketResponseMismatched += value;
                WebSocketResponseMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _WebSocketResponseMismatched -= value;
                WebSocketResponseMismatchedEventDelegates.Remove(value);
            }
        }

        //IApplicationContext applicationContext = ContextRegistry.GetContext();
        //Config config;

        /// <summary>
        /// 完成所有相同RequestID的資料處理時事件
        /// </summary>
        protected virtual void OnWebSocketResponseFinished(object state)
        {
            WebSocketResponseFinishedEventArgs e = state as WebSocketResponseFinishedEventArgs;
            if (_WebSocketResponseFinished != null)
            {
                _WebSocketResponseFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnWebSocketResponseMismatched(object state)
        {
            WebSocketResponseMismatchedEventArgs e = state as WebSocketResponseMismatchedEventArgs;
            if (_WebSocketResponseMismatched != null)
            {
                _WebSocketResponseMismatched(this, e);
            }
        }

        protected bool _IsResponseFinished = false;
        protected Dictionary<string, string> _DicDataType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string, MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static RequestWebSocketAdapter singleton;

        public RequestWebSocketAdapter() : base() 
        { 
            //config = (Config)applicationContext.GetObject("Config");
            this.WebSocketResponseMismatched += new WebSocketResponseMismatchedEventHandler(RequestWebSocketAdapter_WebSocketResponseMismatched);
        }

        public RequestWebSocketAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName) 
        { 
            //config = (Config)applicationContext.GetObject("Config");
            this.WebSocketResponseMismatched += new WebSocketResponseMismatchedEventHandler(RequestWebSocketAdapter_WebSocketResponseMismatched);
        }

        public RequestWebSocketAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd) 
        { 
            //config = (Config)applicationContext.GetObject("Config");
            this.WebSocketResponseMismatched += new WebSocketResponseMismatchedEventHandler(RequestWebSocketAdapter_WebSocketResponseMismatched);
        }

        public static RequestWebSocketAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new RequestWebSocketAdapter();
            }
            return singleton;
        }

        public static RequestWebSocketAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new RequestWebSocketAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }

        public static RequestWebSocketAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new RequestWebSocketAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
            }
            return singleton;
        }

        public bool IsResponseFinished
        {
            get { return _IsResponseFinished; }
        }

        public override void RemoveAllEvents()
        {
            base.RemoveAllEvents();
            foreach (WebSocketResponseFinishedEventHandler eh in WebSocketResponseFinishedEventDelegates)
            {
                _WebSocketResponseFinished -= eh;
            }
            WebSocketResponseFinishedEventDelegates.Clear();
            foreach (WebSocketResponseMismatchedEventHandler eh in WebSocketResponseMismatchedEventDelegates)
            {
                _WebSocketResponseMismatched -= eh;
            }
            WebSocketResponseMismatchedEventDelegates.Clear();
        }

        public override void processMessage(IMessage message)
        {
            try
            {
                ClearTimeOutReceivedMessage();
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
                if (_MessageID != null && MessageDictionary.Values.Contains(_MessageID))
                {
                    string Message = "";
                    string _ErrMsg = "";
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
                        Common.LogHelper.Logger.LogInfo<RequestWebSocketAdapter>(_ErrMsg);
                        RunOnMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    _DicDataType = Util.ConvertTagClassConstants(_DataType);
                    //2.驗證WebSocket傳過來的TagData的tag正確性(與指定的TagType)
                    foreach (string key in MessageDictionary.Keys)
                    {
                        if (!_DicDataType.ContainsKey(key))
                        {
                            _ErrMsg = string.Format("Tag Data's Tag[{0}] Not in the assigned type[{1}]", key, _DataType.Name);
                            Common.LogHelper.Logger.LogInfo<RequestWebSocketAdapter>(_ErrMsg);
                            RunOnMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    string MessageID = _DataType.GetField("MessageID") == null ? "710" : _DataType.GetField("MessageID").GetValue(_DataType).ToString();
                    //3.驗證資料內容的Message總筆數
                    string TotalRecords = _DataType.GetField("TotalRecords") == null ? "10038" : _DataType.GetField("TotalRecords").GetValue(_DataType).ToString();
                    if (MessageDictionary.ContainsKey(TotalRecords))
                    {
                        int iTotalRecords;
                        //驗證筆數資料正確性
                        //如果筆數不是數值
                        if (!int.TryParse(MessageDictionary[TotalRecords].ToString(), out iTotalRecords))
                        {
                            _ErrMsg = "TotalRecords value must be digit";
                            Common.LogHelper.Logger.LogInfo<RequestWebSocketAdapter>(_ErrMsg);
                            RunOnMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    //驗證MessageID是否存在
                    if (!MessageDictionary.ContainsKey(MessageID))
                    {
                        _ErrMsg = "MessageID Of Message in MessageBody is not exist";
                        Common.LogHelper.Logger.LogInfo<RequestWebSocketAdapter>(_ErrMsg);
                        RunOnMessageHandleFinished(_ErrMsg, null);
                        return;
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
                        Common.LogHelper.Logger.LogInfo<RequestWebSocketAdapter>(_ErrMsg);
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
                            _IsResponseFinished = true;
                            RunOnResponseFinished(_ErrMsg, ResultTable);
                            ClearGuidInDictionary(MessageDictionary[MessageID].ToString());
                            _IsResponseFinished = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.Logger.LogError<RequestWebSocketAdapter>(ex);
            }
        }

        /// <summary>
        /// 清除逾時的已接收的WebSocketMessage
        /// </summary>
        public void ClearTimeOutReceivedMessage()
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
                        Common.LogHelper.Logger.LogInfo<RequestWebSocketAdapter>(_ErrMsg);
                        OnWebSocketResponseMismatched(new WebSocketResponseMismatchedEventArgs(_ErrMsg));
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

        void RequestWebSocketAdapter_WebSocketResponseMismatched(object sender, WebSocketResponseMismatchedEventArgs e)
        {
            Common.LogHelper.Logger.LogInfo<RequestWebSocketAdapter>(e.MismatchedMessage);
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

        private void RunOnResponseFinished(string ErrorMessage, DataTable ResponseResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnWebSocketResponseFinished, new WebSocketResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
            else
            {
                OnWebSocketResponseFinished(new WebSocketResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
        }
    }
}
