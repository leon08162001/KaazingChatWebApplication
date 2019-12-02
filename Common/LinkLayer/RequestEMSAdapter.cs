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
        public delegate void EMSResponseFinishedEventHandler(object sender, EMSResponseFinishedEventArgs e);
        List<EMSResponseFinishedEventHandler> EMSResponseFinishedEventDelegates = new List<EMSResponseFinishedEventHandler>();
        private event EMSResponseFinishedEventHandler _EMSResponseFinished;
        public event EMSResponseFinishedEventHandler EMSResponseFinished
        {
            add
            {
                _EMSResponseFinished += value;
                EMSResponseFinishedEventDelegates.Add(value);
            }
            remove
            {
                _EMSResponseFinished -= value;
                EMSResponseFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void EMSResponseMismatchedEventHandler(object sender, EMSResponseMismatchedEventArgs e);
        List<EMSResponseMismatchedEventHandler> EMSResponseMismatchedEventDelegates = new List<EMSResponseMismatchedEventHandler>();
        private event EMSResponseMismatchedEventHandler _EMSResponseMismatched;
        protected event EMSResponseMismatchedEventHandler EMSResponseMismatched
        {
            add
            {
                _EMSResponseMismatched += value;
                EMSResponseMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _EMSResponseMismatched -= value;
                EMSResponseMismatchedEventDelegates.Remove(value);
            }
        }

        IApplicationContext applicationContext = ContextRegistry.GetContext();
        Config config;

        /// <summary>
        /// Tibco EMS完成所有相同RequestID的資料處理時事件
        /// </summary>
        protected virtual void OnEMSResponseFinished(object state)
        {
            EMSResponseFinishedEventArgs e = state as EMSResponseFinishedEventArgs;
            if (_EMSResponseFinished != null)
            {
                _EMSResponseFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutEMSReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnEMSResponseMismatched(object state)
        {
            EMSResponseMismatchedEventArgs e = state as EMSResponseMismatchedEventArgs;
            if (_EMSResponseMismatched != null)
            {
                _EMSResponseMismatched(this, e);
            }
        }

        protected bool _IsResponseFinished = false;
        protected Type _DataType;
        protected Dictionary<string, string> _DicTagType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string, MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static RequestEMSAdapter singleton;

        public RequestEMSAdapter() : base() { config = (Config)applicationContext.GetObject("Config"); this.EMSResponseMismatched += new EMSResponseMismatchedEventHandler(RequestEMSAdapter_EMSResponseMismatched); }

        public RequestEMSAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName) { config = (Config)applicationContext.GetObject("Config"); this.EMSResponseMismatched += new EMSResponseMismatchedEventHandler(RequestEMSAdapter_EMSResponseMismatched); }

        public RequestEMSAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd) { config = (Config)applicationContext.GetObject("Config"); this.EMSResponseMismatched += new EMSResponseMismatchedEventHandler(RequestEMSAdapter_EMSResponseMismatched); }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static RequestEMSAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new RequestEMSAdapter();
            }
            return singleton;
        }

        public static RequestEMSAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new RequestEMSAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }

        public static RequestEMSAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new RequestEMSAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
            }
            return singleton;
        }

        public Type DataType
        {
            set { _DataType = value; }
            get { return _DataType; }
        }

        public bool IsResponseFinished
        {
            get { return _IsResponseFinished; }
        }

        public override void RemoveAllEvents()
        {
            base.RemoveAllEvents();
            foreach (EMSResponseFinishedEventHandler eh in EMSResponseFinishedEventDelegates)
            {
                _EMSResponseFinished -= eh;
            }
            EMSResponseFinishedEventDelegates.Clear();
            foreach (EMSResponseMismatchedEventHandler eh in EMSResponseMismatchedEventDelegates)
            {
                _EMSResponseMismatched -= eh;
            }
            EMSResponseMismatchedEventDelegates.Clear();
        }

        public override void processEMSMessage(Message message)
        {
            try
            {
                ClearTimeOutEMSReceivedMessage();
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
                if (_MessageID != null && EMSMessageDictionary.Values.Contains(_MessageID))
                {
                    string Message = "";
                    string _ErrMsg = "";
                    if (EMSMessageDictionary.Keys.Count == 0)
                    {
                        return;
                    }
                    foreach (string key in EMSMessageDictionary.Keys)
                    {
                        Message += key + "=" + EMSMessageDictionary[key] + ";";
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
                        int iTotalRecords;
                        //驗證筆數資料正確性
                        //如果筆數不是數值
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
                            _IsResponseFinished = true;
                            RunOnEMSResponseFinished(_ErrMsg, ResultTable);
                            ClearGuidInDictionary(EMSMessageDictionary[MessageID].ToString());
                            _IsResponseFinished = false;
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
                        OnEMSResponseMismatched(new EMSResponseMismatchedEventArgs(_ErrMsg));
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

        void RequestEMSAdapter_EMSResponseMismatched(object sender, EMSResponseMismatchedEventArgs e)
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

        private void RunOnEMSResponseFinished(string ErrorMessage, DataTable ResponseResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnEMSResponseFinished, new EMSResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
            else
            {
                OnEMSResponseFinished(new EMSResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
        }
    }
}
