using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.ComponentModel;
using Spring.Context;
using Spring.Context.Support;
using Common.Utility;
using Common.TopicMessage;

namespace Common.LinkLayer
{
    /// <summary>
    /// Tibco處理完所有回應相同RequestID資料的事件參數類別
    /// </summary>
    public class TibcoResponseFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _ResponseResultTable;
        public TibcoResponseFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public TibcoResponseFinishedEventArgs(string errorMessage, DataTable ResponseResultTable)
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
    public class TibcoResponseMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public TibcoResponseMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public TibcoResponseMismatchedEventArgs(string MismatchedMessage)
        {
            _MismatchedMessage = MismatchedMessage;
        }
        public string MismatchedMessage
        {
            get { return _MismatchedMessage; }
            set { _MismatchedMessage = value; }
        }
    }

    public class RequestTibcoFixAdapter : BaseTibcoAdapter
    {
        // Delegate
        public delegate void TibcoResponseFinishedEventHandler(object sender, TibcoResponseFinishedEventArgs e);
        List<TibcoResponseFinishedEventHandler> TibcoResponseFinishedEventDelegates = new List<TibcoResponseFinishedEventHandler>();
        private event TibcoResponseFinishedEventHandler _TibcoResponseFinished;
        public event TibcoResponseFinishedEventHandler TibcoResponseFinished
        {
            add
            {
                _TibcoResponseFinished += value;
                TibcoResponseFinishedEventDelegates.Add(value);
            }
            remove
            {
                _TibcoResponseFinished -= value;
                TibcoResponseFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void TibcoResponseMismatchedEventHandler(object sender, TibcoResponseMismatchedEventArgs e);
        List<TibcoResponseMismatchedEventHandler> TibcoResponseMismatchedEventDelegates = new List<TibcoResponseMismatchedEventHandler>();
        private event TibcoResponseMismatchedEventHandler _TibcoResponseMismatched;
        protected event TibcoResponseMismatchedEventHandler TibcoResponseMismatched
        {
            add
            {
                _TibcoResponseMismatched += value;
                TibcoResponseMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _TibcoResponseMismatched -= value;
                TibcoResponseMismatchedEventDelegates.Remove(value);
            }
        }

        IApplicationContext applicationContext = ContextRegistry.GetContext();
        Config config;

        /// <summary>
        /// Tibco完成所有相同RequestID的資料處理時事件
        /// </summary>
        protected virtual void OnTibcoResponseFinished(object state)
        {
            TibcoResponseFinishedEventArgs e = state as TibcoResponseFinishedEventArgs;
            if (_TibcoResponseFinished != null)
            {
                _TibcoResponseFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutTibcoReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnTibcoResponseMismatched(object state)
        {
            TibcoResponseMismatchedEventArgs e = state as TibcoResponseMismatchedEventArgs;
            if (_TibcoResponseMismatched != null)
            {
                _TibcoResponseMismatched(this, e);
            }
        }

        protected bool _IsResponseFinished = false;
        protected Type _FixTagType;
        protected Dictionary<string, string> _DicTagType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string, MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static RequestTibcoFixAdapter singleton;

        public RequestTibcoFixAdapter() : base() 
        {
            try
            {
                config = (Config)applicationContext.GetObject("Config");
                this.TibcoResponseMismatched += new TibcoResponseMismatchedEventHandler(RequestTibcoFixAdapter_TibcoResponseMismatched);
                TIBCO.Rendezvous.Environment.Open();
                TIBCO.Rendezvous.Environment.StringEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            }
            catch (TIBCO.Rendezvous.RendezvousException exception)
            {
                if (log.IsErrorEnabled) log.Error("BatchTibcoFixAdapter BatchTibcoFixAdapter(): " + exception.ToString());
                throw exception;
            }
        }

        public RequestTibcoFixAdapter(string tibcoService, string tibcoNetwork, string tibcoDaemon)
            : base(tibcoService, tibcoNetwork, tibcoDaemon) 
        {
            try
            {
                config = (Config)applicationContext.GetObject("Config");
                this.TibcoResponseMismatched += new TibcoResponseMismatchedEventHandler(RequestTibcoFixAdapter_TibcoResponseMismatched);
                TIBCO.Rendezvous.Environment.Open();
                TIBCO.Rendezvous.Environment.StringEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            }
            catch (TIBCO.Rendezvous.RendezvousException exception)
            {
                if (log.IsErrorEnabled) log.Error("BatchTibcoFixAdapter BatchTibcoFixAdapter(): " + exception.ToString());
                throw exception;
            }
        }

        public RequestTibcoFixAdapter(string tibcoService, string tibcoNetwork, string tibcoDaemon, string ListenName, string SendName)
            : base(tibcoService, tibcoNetwork, tibcoDaemon, ListenName, SendName) 
        {
            try
            {
                config = (Config)applicationContext.GetObject("Config");
                this.TibcoResponseMismatched += new TibcoResponseMismatchedEventHandler(RequestTibcoFixAdapter_TibcoResponseMismatched);
                TIBCO.Rendezvous.Environment.Open();
                TIBCO.Rendezvous.Environment.StringEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            }
            catch (TIBCO.Rendezvous.RendezvousException exception)
            {
                if (log.IsErrorEnabled) log.Error("BatchTibcoFixAdapter BatchTibcoFixAdapter(): " + exception.ToString());
                throw exception;
            }
        }

        public static RequestTibcoFixAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new RequestTibcoFixAdapter();
            }
            return singleton;
        }

        public static RequestTibcoFixAdapter getSingleton(string tibcoService, string tibcoNetwork, string tibcoDaemon)
        {
            if (singleton == null)
            {
                singleton = new RequestTibcoFixAdapter(tibcoService, tibcoNetwork, tibcoDaemon);
            }
            return singleton;
        }

        public static RequestTibcoFixAdapter getSingleton(string tibcoService, string tibcoNetwork, string tibcoDaemon, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new RequestTibcoFixAdapter(tibcoService, tibcoNetwork, tibcoDaemon, ListenName, SendName);
            }
            return singleton;
        }

        public Type FixTagType
        {
            set { _FixTagType = value; }
            get { return _FixTagType; }
        }

        public bool IsResponseFinished
        {
            get { return _IsResponseFinished; }
        }

        public override void RemoveAllEvents()
        {
            base.RemoveAllEvents();
            foreach (TibcoResponseFinishedEventHandler eh in TibcoResponseFinishedEventDelegates)
            {
                _TibcoResponseFinished -= eh;
            }
            TibcoResponseFinishedEventDelegates.Clear();
            foreach (TibcoResponseMismatchedEventHandler eh in TibcoResponseMismatchedEventDelegates)
            {
                _TibcoResponseMismatched -= eh;
            }
            TibcoResponseMismatchedEventDelegates.Clear();
        }

        public override void processTibcoMessage(TIBCO.Rendezvous.Message message)
        {
            try
            {
                ClearTimeOutTibcoReceivedMessage();
                if (_MessageID != null && message.GetFieldByIndex(0).Value.ToString().Trim().Contains(_MessageID))
                {
                    string Message = "";
                    string _ErrMsg = "";
                    Dictionary<string, string> TibcoMessageDictionary = new Dictionary<string, string>();
                    for (uint i = 0; i < message.FieldCount; i++)
                    {
                        TIBCO.Rendezvous.MessageField field = message.GetFieldByIndex(i);
                        TibcoMessageDictionary.Add(field.Name, field.Value.ToString());
                    }
                    if (TibcoMessageDictionary.Keys.Count == 0)
                    {
                    return;
                    }
                    foreach (string key in TibcoMessageDictionary.Keys)
                    {
                        Message += key + "=" + TibcoMessageDictionary[key] + ";";
                    }
                    //1.檢查是否有指定TagType,以便與傳過來的TagData作驗證用
                    if (_FixTagType == null)
                    {
                        _ErrMsg = "not yet assigned Tag Type of Tag Data";
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        if (UISyncContext != null && IsEventInUIThread)
                        {
                            UISyncContext.Post(OnTibcoMessageHandleFinished, new TibcoMessageHandleFinishedEventArgs(_ErrMsg, null));
                        }
                        else
                        {
                            OnTibcoMessageHandleFinished(new TibcoMessageHandleFinishedEventArgs(_ErrMsg, null));
                        }
                        return;
                    }
                    _DicTagType = Util.ConvertTagClassConstants(_FixTagType);
                    //2.驗證Tibco傳過來的TagData的tag正確性(與指定的TagType)
                    foreach (string key in TibcoMessageDictionary.Keys)
                    {
                        if (!_DicTagType.ContainsKey(key))
                        {
                            _ErrMsg = string.Format("Tag Data's Tag[{0}] Not in the assigned type[{1}]", key, _FixTagType.Name);
                            if (log.IsInfoEnabled) log.Info(_ErrMsg);
                            if (UISyncContext != null && IsEventInUIThread)
                            {
                                UISyncContext.Post(OnTibcoMessageHandleFinished, new TibcoMessageHandleFinishedEventArgs(_ErrMsg, null));
                            }
                            else
                            {
                                OnTibcoMessageHandleFinished(new TibcoMessageHandleFinishedEventArgs(_ErrMsg, null));
                            }
                            return;
                        }
                    }
                    string MessageID = _FixTagType.GetField("MessageID").GetValue(_FixTagType).ToString();
                    //3.驗證資料內容的Message總筆數
                    string TotalRecords = _FixTagType.GetField("TotalRecords").GetValue(_FixTagType).ToString();
                    if (TibcoMessageDictionary.ContainsKey(TotalRecords))
                    {
                        //驗證筆數資料正確性
                        //如果筆數不是數值
                        int iTotalRecords;
                        if (!int.TryParse(TibcoMessageDictionary[TotalRecords].ToString(), out iTotalRecords))
                        {
                            _ErrMsg = "TotalRecords value must be digit";
                            if (log.IsInfoEnabled) log.Info(_ErrMsg);
                            RunOnTibcoMessageHandleFinished(_ErrMsg, null); 
                            return;
                        }
                    }
                    //驗證MessageID是否存在
                    if (!TibcoMessageDictionary.ContainsKey(MessageID))
                    {
                        _ErrMsg = "MessageID Of Message in MessageBody is not exist";
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        RunOnTibcoMessageHandleFinished(_ErrMsg, null);
                        return;
                    }
                    //MessageID存在則檢查DicMessageBody內是否存在此MessageID,沒有則建立DataTable Schema並加入一筆MessageBody至DicMessageBody
                    if (!DicMessageBody.ContainsKey(TibcoMessageDictionary[MessageID].ToString()))
                    {
                        DataTable DT = new DataTable();
                        DT = Util.CreateTableSchema(_DicTagType, _FixTagType);
                        DicMessageBody.Add(TibcoMessageDictionary[MessageID].ToString(), new MessageBody(DT, System.DateTime.Now));
                    }
                    //匯入每筆message到屬於此MessageID的MessageBody
                    MessageBody MB = DicMessageBody[TibcoMessageDictionary[MessageID].ToString()];
                    DataRow MessageRow;
                    MessageRow = Util.AddMessageToRow(TibcoMessageDictionary, _DicTagType, _FixTagType, MB.Messages);
                    if (MessageRow != null)
                    {
                        _ErrMsg = "";
                        MB.Messages.Rows.Add(MessageRow);
                        RunOnTibcoMessageHandleFinished(_ErrMsg, MessageRow);
                    }
                    else
                    {
                        _ErrMsg = "Error happened when generate DataRow";
                        if (log.IsInfoEnabled) log.Info(_ErrMsg);
                        RunOnTibcoMessageHandleFinished(_ErrMsg, null);
                    }
                    if (DicMessageBody.ContainsKey(TibcoMessageDictionary[MessageID].ToString()) && MB.Messages.Rows.Count > 0)
                    {
                        int iTotalRecords = Convert.ToInt32(MB.Messages.Rows[0]["TotalRecords"].ToString());
                        //若此MessageID TotalRecords的筆數與在DicMessageBody的Messages筆數相同
                        if (iTotalRecords == DicMessageBody[TibcoMessageDictionary[MessageID].ToString()].Messages.Rows.Count)
                        {
                            _ErrMsg = "";
                            DataTable ResultTable = DicMessageBody[TibcoMessageDictionary[MessageID].ToString()].Messages.Copy();
                            _IsResponseFinished = true;
                            RunOnTibcoResponseFinished(_ErrMsg, ResultTable);
                            ClearGuidInDictionary(TibcoMessageDictionary[MessageID].ToString());
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
        /// 清除逾時的已接收的TibcoMessage
        /// </summary>
        public void ClearTimeOutTibcoReceivedMessage()
        {
            int TimeOut = Convert.ToInt32(config.TibcoReceivedMessageReservedSeconds);
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
                        OnTibcoResponseMismatched(new TibcoResponseMismatchedEventArgs(_ErrMsg));
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

        void RequestTibcoFixAdapter_TibcoResponseMismatched(object sender, TibcoResponseMismatchedEventArgs e)
        {
            if (log.IsInfoEnabled) log.Info(e.MismatchedMessage);
        }

        private void RunOnTibcoMessageHandleFinished(string ErrorMessage, DataRow MessageRow)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnTibcoMessageHandleFinished, new TibcoMessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
            else
            {
                OnTibcoMessageHandleFinished(new TibcoMessageHandleFinishedEventArgs(ErrorMessage, MessageRow));
            }
        }

        private void RunOnTibcoResponseFinished(string ErrorMessage, DataTable ResponseResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnTibcoResponseFinished, new TibcoResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
            else
            {
                OnTibcoResponseFinished(new TibcoResponseFinishedEventArgs(ErrorMessage, ResponseResultTable));
            }
        }
    }
}
