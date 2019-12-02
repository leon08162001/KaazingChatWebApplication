using Common.TopicMessage;
using Common.Utility;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Common.LinkLayer
{
    /// <summary>
    /// Tibco處理完批次資料的事件參數類別
    /// </summary>
    public class TibcoBatchFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _BatchResultTable;
        public TibcoBatchFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public TibcoBatchFinishedEventArgs(string errorMessage, DataTable BatchResultTable)
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
    public class TibcoBatchMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public TibcoBatchMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public TibcoBatchMismatchedEventArgs(string MismatchedMessage)
        {
            _MismatchedMessage = MismatchedMessage;
        }
        public string MismatchedMessage
        {
            get { return _MismatchedMessage; }
            set { _MismatchedMessage = value; }
        }
    }

    public class BatchTibcoFixAdapter : BaseTibcoAdapter
    {
        // Delegate
        public delegate void TibcoBatchFinishedEventHandler(object sender, TibcoBatchFinishedEventArgs e);
        List<TibcoBatchFinishedEventHandler> TibcoBatchFinishedEventDelegates = new List<TibcoBatchFinishedEventHandler>();
        private event TibcoBatchFinishedEventHandler _TibcoBatchFinished;
        public event TibcoBatchFinishedEventHandler TibcoBatchFinished
        {
            add
            {
                _TibcoBatchFinished += value;
                TibcoBatchFinishedEventDelegates.Add(value);
            }
            remove
            {
                _TibcoBatchFinished -= value;
                TibcoBatchFinishedEventDelegates.Remove(value);
            }
        }

        protected delegate void TibcoBatchMismatchedEventHandler(object sender, TibcoBatchMismatchedEventArgs e);
        List<TibcoBatchMismatchedEventHandler> TibcoBatchMismatchedEventDelegates = new List<TibcoBatchMismatchedEventHandler>();
        private event TibcoBatchMismatchedEventHandler _TibcoBatchMismatched;
        protected event TibcoBatchMismatchedEventHandler TibcoBatchMismatched
        {
            add
            {
                _TibcoBatchMismatched += value;
                TibcoBatchMismatchedEventDelegates.Add(value);
            }
            remove
            {
                _TibcoBatchMismatched -= value;
                TibcoBatchMismatchedEventDelegates.Remove(value);
            }
        }

        IApplicationContext applicationContext = ContextRegistry.GetContext();
        Config config;

        /// <summary>
        /// Tibco完成所有批次資料處理時事件
        /// </summary>
        protected virtual void OnTibcoBatchFinished(object state)
        {
            TibcoBatchFinishedEventArgs e = state as TibcoBatchFinishedEventArgs;
            if (_TibcoBatchFinished != null)
            {
                _TibcoBatchFinished(this, e);
            }
        }
        /// <summary>
        /// MessageHeader's Count與MessageBody's DataRow Count不符合時事件(每次在接收訊息一開始呼叫ClearTimeOutTibcoReceivedMessage時觸發)
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnTibcoBatchMismatched(object state)
        {
            TibcoBatchMismatchedEventArgs e = state as TibcoBatchMismatchedEventArgs;
            if (_TibcoBatchMismatched != null)
            {
                _TibcoBatchMismatched(this, e);
            }
        }

        protected bool _IsBatchFinished = false;
        protected Type _FixTagType;
        protected Dictionary<string, string> _DicTagType = new Dictionary<string, string>();

        //註解紀錄傳送筆數資訊的Dictionary
        //protected Dictionary<string, MessageHeader> DicMessageHeader = new Dictionary<string,MessageHeader>();
        protected Dictionary<string, MessageBody> DicMessageBody = new Dictionary<string, MessageBody>();

        private static BatchTibcoFixAdapter singleton;

        public BatchTibcoFixAdapter() : base() 
        {
            try
            {
                config = (Config)applicationContext.GetObject("Config");
                this.TibcoBatchMismatched += new TibcoBatchMismatchedEventHandler(BatchTibcoFixAdapter_TibcoBatchMismatched);
                TIBCO.Rendezvous.Environment.Open();
                TIBCO.Rendezvous.Environment.StringEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            }
            catch (TIBCO.Rendezvous.RendezvousException exception)
            {
                if (log.IsErrorEnabled) log.Error("BatchTibcoFixAdapter BatchTibcoFixAdapter(): " + exception.ToString());
                throw exception;
            }
        }

        public BatchTibcoFixAdapter(string tibcoService, string tibcoNetwork, string tibcoDaemon)
            : base(tibcoService, tibcoNetwork, tibcoDaemon) 
        {
            try
            {
                config = (Config)applicationContext.GetObject("Config");
                this.TibcoBatchMismatched += new TibcoBatchMismatchedEventHandler(BatchTibcoFixAdapter_TibcoBatchMismatched);
                TIBCO.Rendezvous.Environment.Open();
                TIBCO.Rendezvous.Environment.StringEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            }
            catch (TIBCO.Rendezvous.RendezvousException exception)
            {
                if (log.IsErrorEnabled) log.Error("BatchTibcoFixAdapter BatchTibcoFixAdapter(): " + exception.ToString());
                throw exception;
            }
        }

        public BatchTibcoFixAdapter(string tibcoService, string tibcoNetwork, string tibcoDaemon, string ListenName, string SendName)
            : base(tibcoService, tibcoNetwork, tibcoDaemon, ListenName, SendName) 
        {
            try
            {
                config = (Config)applicationContext.GetObject("Config");
                this.TibcoBatchMismatched += new TibcoBatchMismatchedEventHandler(BatchTibcoFixAdapter_TibcoBatchMismatched);
                TIBCO.Rendezvous.Environment.Open();
                TIBCO.Rendezvous.Environment.StringEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            }
            catch (TIBCO.Rendezvous.RendezvousException exception)
            {
                if (log.IsErrorEnabled) log.Error("BatchTibcoFixAdapter BatchTibcoFixAdapter(): " + exception.ToString());
                throw exception;
            }
        }

        public static BatchTibcoFixAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new BatchTibcoFixAdapter();
            }
            return singleton;
        }

        public static BatchTibcoFixAdapter getSingleton(string tibcoService, string tibcoNetwork, string tibcoDaemon)
        {
            if (singleton == null)
            {
                singleton = new BatchTibcoFixAdapter(tibcoService, tibcoNetwork, tibcoDaemon);
            }
            return singleton;
        }

        public static BatchTibcoFixAdapter getSingleton(string tibcoService, string tibcoNetwork, string tibcoDaemon, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new BatchTibcoFixAdapter(tibcoService, tibcoNetwork, tibcoDaemon, ListenName, SendName);
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
            foreach (TibcoBatchFinishedEventHandler eh in TibcoBatchFinishedEventDelegates)
            {
                _TibcoBatchFinished -= eh;
            }
            TibcoBatchFinishedEventDelegates.Clear();
            foreach (TibcoBatchMismatchedEventHandler eh in TibcoBatchMismatchedEventDelegates)
            {
                _TibcoBatchMismatched -= eh;
            }
            TibcoBatchMismatchedEventDelegates.Clear();
        }

        public override void processTibcoMessage(TIBCO.Rendezvous.Message message)
        {
            try
            {
                ClearTimeOutTibcoReceivedMessage();
                string Message = "";
                string _ErrMsg = "";
                Dictionary<string, string> TibcoMessageDictionary = new Dictionary<string, string>();
                for (uint i = 0; i < message.FieldCount; i++)
                {
                    TIBCO.Rendezvous.MessageField field = message.GetFieldByIndex(i);
                    TibcoMessageDictionary.Add(field.Name, field.Value.ToString());
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
                        _IsBatchFinished = true;
                        RunOnTibcoBatchFinished(_ErrMsg, ResultTable);
                        ClearGuidInDictionary(TibcoMessageDictionary[MessageID].ToString());
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
                        OnTibcoBatchMismatched(new TibcoBatchMismatchedEventArgs(_ErrMsg));
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

        void BatchTibcoFixAdapter_TibcoBatchMismatched(object sender, TibcoBatchMismatchedEventArgs e)
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

        private void RunOnTibcoBatchFinished(string ErrorMessage, DataTable BatchResultTable)
        {
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnTibcoBatchFinished, new TibcoBatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
            else
            {
                OnTibcoBatchFinished(new TibcoBatchFinishedEventArgs(ErrorMessage, BatchResultTable));
            }
        }
    }
}
