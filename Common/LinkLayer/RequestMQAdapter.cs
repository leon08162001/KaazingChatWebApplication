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
    /// MQ處理完所有回應相同RequestID資料的事件參數類別
    /// </summary>
    public class MQResponseFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _ResponseResultTable;
        public MQResponseFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public MQResponseFinishedEventArgs(string errorMessage, DataTable ResponseResultTable)
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
    public class MQResponseMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public MQResponseMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public MQResponseMismatchedEventArgs(string MismatchedMessage)
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
    public class RequestMQAdapter : BaseMQAdapter
    {
        // Delegate
        public delegate void MQResponseFinishedEventHandler(object sender, MQResponseFinishedEventArgs e);
        List<MQResponseFinishedEventHandler> MQResponseFinishedEventDelegates = new List<MQResponseFinishedEventHandler>();
        private event MQResponseFinishedEventHandler _MQResponseFinished;
        public event MQResponseFinishedEventHandler MQResponseFinished
        {
            add
            {
                _MQResponseFinished += value;
                MQResponseFinishedEventDelegates.Add(value);
            }
            remove
            {
                _MQResponseFinished -= value;
                MQResponseFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void MQResponseMismatchedEventHandler(object sender, MQResponseMismatchedEventArgs e);
        List<MQResponseMismatchedEventHandler> MQResponseMismatchedEventDelegates = new List<MQResponseMismatchedEventHandler>();
        private event MQResponseMismatchedEventHandler _MQResponseMismatched;
        protected event MQResponseMismatchedEventHandler MQResponseMismatched
        {
            add
            {
                _MQResponseMismatched += value;
                MQResponseMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _MQResponseMismatched -= value;
                MQResponseMismatchedEventDelegates.Remove(value);
            }
        }

        //IApplicationContext applicationContext = ContextRegistry.GetContext();
        //Config config;

        /// <summary>
        /// MQ完成所有相同RequestID的資料處理時事件
        /// </summary>
        protected virtual void OnResponseFinished(object state)
        {
            MQResponseFinishedEventArgs e = state as MQResponseFinishedEventArgs;
            if (_MQResponseFinished != null)
            {
                _MQResponseFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutMQReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnResponseMismatched(object state)
        {
            MQResponseMismatchedEventArgs e = state as MQResponseMismatchedEventArgs;
            if (_MQResponseMismatched != null)
            {
                _MQResponseMismatched(this, e);
            }
        }

        protected bool _IsResponseFinished = false;
        protected Dictionary<string, string> _DicTagType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string, MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static RequestMQAdapter singleton;

        public RequestMQAdapter() : base()
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.MQResponseMismatched += new MQResponseMismatchedEventHandler(RequestMQAdapter_MQResponseMismatched);
        }

        public RequestMQAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName)
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.MQResponseMismatched += new MQResponseMismatchedEventHandler(RequestMQAdapter_MQResponseMismatched);
        }

        public RequestMQAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd)
        {
            //config = (Config)applicationContext.GetObject("Config");
            this.MQResponseMismatched += new MQResponseMismatchedEventHandler(RequestMQAdapter_MQResponseMismatched);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static RequestMQAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new RequestMQAdapter();
            }
            return singleton;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static RequestMQAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new RequestMQAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static RequestMQAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new RequestMQAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
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
            foreach (MQResponseFinishedEventHandler eh in MQResponseFinishedEventDelegates)
            {
                _MQResponseFinished -= eh;
            }
            MQResponseFinishedEventDelegates.Clear();
            foreach (MQResponseMismatchedEventHandler eh in MQResponseMismatchedEventDelegates)
            {
                _MQResponseMismatched -= eh;
            }
            MQResponseMismatchedEventDelegates.Clear();
        }

        public override void processMessage(IMessage message)
        {
            try
            {
                ClearTimeOutMQReceivedMessage();
                string[] tempValues = new string[message.Properties.Values.Count];
                message.Properties.Values.CopyTo(tempValues, 0);
                if (_MessageID != null && tempValues.Contains(_MessageID))
                {
                    string Message = "";
                    string _ErrMsg = "";
                    Dictionary<string, string> MQMessageDictionary = new Dictionary<string, string>();
                    foreach (object key in message.Properties.Keys)
                    {
                        MQMessageDictionary.Add(key.ToString(), message.Properties[key.ToString()].ToString());
                    }
                    if (MQMessageDictionary.Keys.Count == 0)
                    {
                        return;
                    }
                    foreach (string key in MQMessageDictionary.Keys)
                    {
                        Message += key + "=" + MQMessageDictionary[key] + ";";
                    }
                    //0.檢查是否為HeartBeat訊息,若是則忽略不處理
                    if (MQMessageDictionary.ContainsKey("0"))
                    {
                        return;
                    }
                    //1.檢查是否有指定TagType,以便與傳過來的TagData作驗證用
                    if (_DataType == null)
                    {
                        _ErrMsg = "not yet assigned Tag Type of Tag Data";
                        Common.LogHelper.Logger.LogInfo<RequestMQAdapter>(_ErrMsg);
                        RunOnMQMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    _DicTagType = Util.ConvertTagClassConstants(_DataType);
                    //2.驗證MQ傳過來的TagData的tag正確性(與指定的TagType)
                    if (MQMessageDictionary.ContainsKey("__AMQ_CID"))
                    {
                        MQMessageDictionary.Remove("__AMQ_CID");
                    }
                    foreach (string key in MQMessageDictionary.Keys)
                    {
                        if (!_DicTagType.ContainsKey(key))
                        {
                            _ErrMsg = string.Format("Tag Data's Tag[{0}] Not in the assigned type[{1}]", key, _DataType.Name);
                            Common.LogHelper.Logger.LogInfo<RequestMQAdapter>(_ErrMsg);
                            RunOnMQMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    string MessageID = _DataType.GetField("MessageID") == null ? "710" : _DataType.GetField("MessageID").GetValue(_DataType).ToString();
                    //3.驗證資料內容的Message總筆數
                    string TotalRecords = _DataType.GetField("TotalRecords") == null ? "10038" : _DataType.GetField("TotalRecords").GetValue(_DataType).ToString();
                    if (MQMessageDictionary.ContainsKey(TotalRecords))
                    {
                        int iTotalRecords;
                        //驗證筆數資料正確性
                        //如果筆數不是數值
                        if (!int.TryParse(MQMessageDictionary[TotalRecords].ToString(), out iTotalRecords))
                        {
                            _ErrMsg = "TotalRecords value must be digit";
                            Common.LogHelper.Logger.LogInfo<RequestMQAdapter>(_ErrMsg);
                            RunOnMQMessageHandleFinished(_ErrMsg, null);
                            return;
                        }
                    }
                    //驗證MessageID是否存在
                    if (!MQMessageDictionary.ContainsKey(MessageID))
                    {
                        _ErrMsg = "MessageID Of Message in MessageBody is not exist";
                        Common.LogHelper.Logger.LogInfo<RequestMQAdapter>(_ErrMsg);
                        RunOnMQMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    //MessageID存在則檢查DicMessageBody內是否存在此MessageID,沒有則建立DataTable Schema並加入一筆MessageBody至DicMessageBody
                    if (!DicMessageBody.ContainsKey(MQMessageDictionary[MessageID].ToString()))
                    {
                        DataTable DT = new DataTable();
                        DT = Util.CreateTableSchema(_DicTagType, _DataType);
                        DicMessageBody.Add(MQMessageDictionary[MessageID].ToString(), new MessageBody(DT, System.DateTime.Now));
                    }
                    //匯入每筆message到屬於此MessageID的MessageBody
                    MessageBody MB = DicMessageBody[MQMessageDictionary[MessageID].ToString()];
                    DataRow MessageRow;
                    MessageRow = Util.AddMessageToRow(MQMessageDictionary, _DicTagType, _DataType, MB.Messages);
                    if (MessageRow != null)
                    {
                        _ErrMsg = "";
                        MB.Messages.Rows.Add(MessageRow);
                        RunOnMQMessageHandleFinished(_ErrMsg, MessageRow);
                    }
                    else
                    {
                        _ErrMsg = "Error happened when generate DataRow";
                        Common.LogHelper.Logger.LogInfo<RequestMQAdapter>(_ErrMsg);
                        RunOnMQMessageHandleFinished(_ErrMsg, null);
                    }
                    if (DicMessageBody.ContainsKey(MQMessageDictionary[MessageID].ToString()) && MB.Messages.Rows.Count > 0)
                    {
                        int iTotalRecords = Convert.ToInt32(MB.Messages.Rows[0]["TotalRecords"].ToString());
                        //若此MessageID TotalRecords的筆數與在DicMessageBody的Messages筆數相同
                        if (iTotalRecords == DicMessageBody[MQMessageDictionary[MessageID].ToString()].Messages.Rows.Count)
                        {
                            _ErrMsg = "";
                            //DataTable ResultTable = DicMessageBody[MQMessageDictionary[MessageID].ToString()].Messages.Copy();
                            DataTable ResultTable = DicMessageBody[MQMessageDictionary[MessageID].ToString()].Messages;
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
                            ClearGuidInDictionary(MQMessageDictionary[MessageID].ToString());
                            _IsResponseFinished = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.Logger.LogError<RequestMQAdapter>(ex);
            }
        }

        /// <summary>
        /// 清除逾時的已接收的MQMessage
        /// </summary>
        public void ClearTimeOutMQReceivedMessage()
        {
            int TimeOut = Convert.ToInt32(Config.MQReceivedMessageReservedSeconds);
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
                        Common.LogHelper.Logger.LogInfo<RequestMQAdapter>(_ErrMsg);
                        OnResponseMismatched(new MQResponseMismatchedEventArgs(_ErrMsg));
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

        void RequestMQAdapter_MQResponseMismatched(object sender, MQResponseMismatchedEventArgs e)
        {
            Common.LogHelper.Logger.LogInfo<RequestMQAdapter>(e.MismatchedMessage);
        }

        private void RunOnMQMessageHandleFinished(string ErrorMessage, DataRow MessageRow)
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

        private void RunOnResponseFinished(string ErrorMessage, DataTable ResponseResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnResponseFinished, new MQResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
            else
            {
                OnResponseFinished(new MQResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
        }
    }
}
