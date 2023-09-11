using Common.TopicMessage;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using TIBCO.EMS;

namespace Common.LinkLayer
{
    /// <summary>
    /// Tibco EMS處理完所有回應相同RequestID資料的事件參數類別
    /// </summary>
    public class EMSResponseFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _ResponseResultTable;
        public EMSResponseFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public EMSResponseFinishedEventArgs(string errorMessage, DataTable ResponseResultTable)
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
    public class EMSResponseMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public EMSResponseMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public EMSResponseMismatchedEventArgs(string MismatchedMessage)
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

    public class RequestEMSAdapter : BaseEMSAdapter
    {
        // Delegate
        public delegate void ResponseFinishedEventHandler(object sender, EMSResponseFinishedEventArgs e);
        List<ResponseFinishedEventHandler> ResponseFinishedEventDelegates = new List<ResponseFinishedEventHandler>();
        private event ResponseFinishedEventHandler _ResponseFinished;
        public event ResponseFinishedEventHandler ResponseFinished
        {
            add
            {
                _ResponseFinished += value;
                ResponseFinishedEventDelegates.Add(value);
            }
            remove
            {
                _ResponseFinished -= value;
                ResponseFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void ResponseMismatchedEventHandler(object sender, EMSResponseMismatchedEventArgs e);
        List<ResponseMismatchedEventHandler> ResponseMismatchedEventDelegates = new List<ResponseMismatchedEventHandler>();
        private event ResponseMismatchedEventHandler _ResponseMismatched;
        protected event ResponseMismatchedEventHandler ResponseMismatched
        {
            add
            {
                _ResponseMismatched += value;
                ResponseMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _ResponseMismatched -= value;
                ResponseMismatchedEventDelegates.Remove(value);
            }
        }

        //IApplicationContext applicationContext = ContextRegistry.GetContext();
        //Config config;

        /// <summary>
        /// Tibco EMS完成所有相同RequestID的資料處理時事件
        /// </summary>
        protected virtual void OnResponseFinished(object state)
        {
            EMSResponseFinishedEventArgs e = state as EMSResponseFinishedEventArgs;
            if (_ResponseFinished != null)
            {
                _ResponseFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutEMSReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnResponseMismatched(object state)
        {
            EMSResponseMismatchedEventArgs e = state as EMSResponseMismatchedEventArgs;
            if (_ResponseMismatched != null)
            {
                _ResponseMismatched(this, e);
            }
        }

        protected bool _IsResponseFinished = false;
        protected Dictionary<string, string> _DicTagType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string, MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static RequestEMSAdapter singleton;

        public RequestEMSAdapter() : base()
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.ResponseMismatched += new ResponseMismatchedEventHandler(RequestEMSAdapter_ResponseMismatched);
        }

        public RequestEMSAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName)
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.ResponseMismatched += new ResponseMismatchedEventHandler(RequestEMSAdapter_ResponseMismatched);
        }

        public RequestEMSAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd)
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.ResponseMismatched += new ResponseMismatchedEventHandler(RequestEMSAdapter_ResponseMismatched);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static RequestEMSAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new RequestEMSAdapter();
            }
            return singleton;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static RequestEMSAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new RequestEMSAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static RequestEMSAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new RequestEMSAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
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
            foreach (ResponseFinishedEventHandler eh in ResponseFinishedEventDelegates)
            {
                _ResponseFinished -= eh;
            }
            ResponseFinishedEventDelegates.Clear();
            foreach (ResponseMismatchedEventHandler eh in ResponseMismatchedEventDelegates)
            {
                _ResponseMismatched -= eh;
            }
            ResponseMismatchedEventDelegates.Clear();
        }

        public override void processMessage(Message message)
        {
            try
            {
                ClearTimeOutEMSReceivedMessage();
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
                    MessageDictionary.Add(key, message.GetStringProperty(key));
                }
                if (_MessageID != null && MessageDictionary.Values.Contains(_MessageID))
                {
                    string Message = "";
                    string _ErrMsg = "";
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
                        Common.LogHelper.Logger.LogInfo<RequestEMSAdapter>(_ErrMsg);
                        RunOnMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    _DicTagType = Util.ConvertTagClassConstants(_DataType);
                    //2.驗證EMS傳過來的TagData的tag正確性(與指定的TagType)
                    foreach (string key in MessageDictionary.Keys)
                    {
                        if (!_DicTagType.ContainsKey(key))
                        {
                            _ErrMsg = string.Format("Tag Data's Tag[{0}] Not in the assigned type[{1}]", key, _DataType.Name);
                            Common.LogHelper.Logger.LogInfo<RequestEMSAdapter>(_ErrMsg);
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
                            Common.LogHelper.Logger.LogInfo<RequestEMSAdapter>(_ErrMsg);
                            RunOnMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    //驗證MessageID是否存在
                    if (!MessageDictionary.ContainsKey(MessageID))
                    {
                        _ErrMsg = "MessageID Of Message in MessageBody is not exist";
                        Common.LogHelper.Logger.LogInfo<RequestEMSAdapter>(_ErrMsg);
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
                        Common.LogHelper.Logger.LogInfo<RequestEMSAdapter>(_ErrMsg);
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
                Common.LogHelper.Logger.LogError<RequestEMSAdapter>(ex);
            }
        }

        /// <summary>
        /// 清除逾時的已接收的Message
        /// </summary>
        public void ClearTimeOutEMSReceivedMessage()
        {
            int TimeOut = Convert.ToInt32(Config.EMSReceivedMessageReservedSeconds);
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
                        Common.LogHelper.Logger.LogInfo<RequestEMSAdapter>(_ErrMsg);
                        OnResponseMismatched(new EMSResponseMismatchedEventArgs(_ErrMsg));
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

        void RequestEMSAdapter_ResponseMismatched(object sender, EMSResponseMismatchedEventArgs e)
        {
            Common.LogHelper.Logger.LogInfo<RequestEMSAdapter>(e.MismatchedMessage);
        }

        private void RunOnMessageHandleFinished(string ErrorMessage, DataRow MessageRow)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnMessageHandleFinished, new EMSMessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
            else
            {
                OnMessageHandleFinished(new EMSMessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
        }

        private void RunOnResponseFinished(string ErrorMessage, DataTable ResponseResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnResponseFinished, new EMSResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
            else
            {
                OnResponseFinished(new EMSResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
        }
    }
}
