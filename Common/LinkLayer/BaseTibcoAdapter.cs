using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using TIBCO.Rendezvous;
using Amib.Threading;
using Common.Dictionary;

namespace Common.LinkLayer
{
    /// <summary>
    ///收到一筆TibcoMessage並完成資料處理時的事件參數類別
    /// </summary>
    public class TibcoMessageHandleFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataRow _MessageRow;
        public TibcoMessageHandleFinishedEventArgs(string errorMessage, DataRow MessageRow)
        {
            _errorMessage = errorMessage;
            _MessageRow = MessageRow;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
        public DataRow MessageRow
        {
            get { return _MessageRow; }
            set { _MessageRow = value; }
        }
    }

    /// <summary>
    ///非同步發送TibcoMessage完成時的事件參數類別
    /// </summary>
    public class TibcoMessageAsynSendFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        public TibcoMessageAsynSendFinishedEventArgs(string errorMessage)
        {
            _errorMessage = errorMessage;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
    }
    public abstract class BaseTibcoAdapter : ITibcoAdapter
    {
        protected TIBCO.Rendezvous.Transport _transport = null;
        protected TIBCO.Rendezvous.Listener _listener = null;
        protected TIBCO.Rendezvous.Queue _queue = null;
        protected TIBCO.Rendezvous.Dispatcher _dispatcher = null;

        protected string tibco_service = string.Empty;
        protected string tibco_network = string.Empty;
        protected string tibco_daemon = string.Empty;
        protected string tibco_sendsubject = string.Empty;
        protected string tibo_listensubject = string.Empty;
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected string _MessageID;

        protected SynchronizationContext _UISyncContext;

        protected bool _IsEventInUIThread = false;             //觸發事件時是否回到UI Thread預設為false

        public delegate void TibcoMessageHandleFinishedEventHandler(object sender, TibcoMessageHandleFinishedEventArgs e);
        List<TibcoMessageHandleFinishedEventHandler> TibcoMessageHandleFinishedEventDelegates = new List<TibcoMessageHandleFinishedEventHandler>();
        private event TibcoMessageHandleFinishedEventHandler _TibcoMessageHandleFinished;
        public event TibcoMessageHandleFinishedEventHandler TibcoMessageHandleFinished
        {
            add
            {
                _TibcoMessageHandleFinished += value;
                TibcoMessageHandleFinishedEventDelegates.Add(value);
            }
            remove
            {
                _TibcoMessageHandleFinished -= value;
                TibcoMessageHandleFinishedEventDelegates.Remove(value);
            }
        }

        public delegate void TibcoMessageAsynSendFinishedEventHandler(object sender, TibcoMessageAsynSendFinishedEventArgs e);
        List<TibcoMessageAsynSendFinishedEventHandler> TibcoMessageAsynSendFinishedEventDelegates = new List<TibcoMessageAsynSendFinishedEventHandler>();
        private event TibcoMessageAsynSendFinishedEventHandler _TibcoMessageAsynSendFinished;
        public event TibcoMessageAsynSendFinishedEventHandler TibcoMessageAsynSendFinished
        {
            add
            {
                _TibcoMessageAsynSendFinished += value;
                TibcoMessageAsynSendFinishedEventDelegates.Add(value);
            }
            remove
            {
                _TibcoMessageAsynSendFinished -= value;
                TibcoMessageAsynSendFinishedEventDelegates.Remove(value);
            }
        }

        /// <summary>
        /// 收到一筆TibcoMessage並完成資料處理時事件
        /// </summary>
        protected virtual void OnTibcoMessageHandleFinished(object state)
        {
            TibcoMessageHandleFinishedEventArgs e = state as TibcoMessageHandleFinishedEventArgs;
            if (_TibcoMessageHandleFinished != null)
            {
                _TibcoMessageHandleFinished(this, e);
            }
        }
        /// <summary>
        /// 非同步發送Message完成時事件
        /// </summary>
        protected virtual void OnTibcoMessageSendFinished(object state)
        {
            TibcoMessageAsynSendFinishedEventArgs e = state as TibcoMessageAsynSendFinishedEventArgs;
            if (_TibcoMessageAsynSendFinished != null)
            {
                _TibcoMessageAsynSendFinished(this, e);
            }
        }

        public string Service
        {
            set { tibco_service = value; }
            get { return tibco_service; }
        }

        public string Network
        {
            set { tibco_network = value; }
            get { return tibco_network; }
        }

        public string Daemon
        {
            set { tibco_daemon = value; }
            get { return tibco_daemon; }
        }

        public string ListenName
        {
            set { tibo_listensubject = value; }
            get { return tibo_listensubject; }
        }

        public string SendName
        {
            set { tibco_sendsubject = value; }
            get { return tibco_sendsubject; }
        }
        public string MessageID
        {
            get { return _MessageID; }
            set
            {
                _MessageID = value;
            }
        }
        /// <summary>
        /// 觸發事件時是否回到UI Thread(預設false)
        /// </summary>
        public bool IsEventInUIThread
        {
            get { return _IsEventInUIThread; }
            set { _IsEventInUIThread = value; }
        }

        public SynchronizationContext UISyncContext
        {
            get { return _UISyncContext; }
        }

        public BaseTibcoAdapter()
        {
                       
        }

        public BaseTibcoAdapter(string tibcoService, string tibcoNetwork, string tibcoDaemon)
        {
            this.tibco_service = tibcoService;
            this.tibco_network = tibcoNetwork;
            this.tibco_daemon = tibcoDaemon;
        }

        public BaseTibcoAdapter(string tibcoService, string tibcoNetwork, string tibcoDaemon, string ListenName, string SendName)
        {
            this.tibco_service = tibcoService;
            this.tibco_network = tibcoNetwork;
            this.tibco_daemon = tibcoDaemon;
            this.tibo_listensubject = ListenName;
            this.tibco_sendsubject = SendName;
        }

       public void initialize(string name)
        {
            try
            {
                this._transport = new TIBCO.Rendezvous.NetTransport(tibco_service, tibco_network, tibco_daemon);
                this._queue = new TIBCO.Rendezvous.Queue();
                this._queue.Name = name;
            }
            catch (TIBCO.Rendezvous.RendezvousException ex)
            {
                if (log.IsErrorEnabled) log.Error("TibcoAdapter initialize() Error",ex);
            }
        }
       public void Start()
        {
           try
            {
                //if (log.IsDebugEnabled) log.Debug("BaseTibcoAdapter: Start() subject " + tibo_listensubject);
                _listener = new Listener(_queue, _transport, tibo_listensubject, null);
                _listener.MessageReceived += new MessageReceivedEventHandler(_listener_messageReceivedEventHandler);
                _dispatcher = new Dispatcher(_queue);
                _UISyncContext = SynchronizationContext.Current;
            }
            catch (TIBCO.Rendezvous.RendezvousException ex)
            {
                if (log.IsErrorEnabled) log.Error("TibcoAdapter start() Error",ex);
            } 
        }

        public void Close()
        {
            try
            {
                if (this._dispatcher != null)
                    this._dispatcher.Destroy();
            }
            catch (TIBCO.Rendezvous.RendezvousException rendezvousException)
            {
                throw rendezvousException;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("BaseTibcoAdapter stop() Error", ex);
            }
        }

        public virtual void RemoveAllEvents()
        {
            foreach (TibcoMessageHandleFinishedEventHandler eh in TibcoMessageHandleFinishedEventDelegates)
            {
                _TibcoMessageHandleFinished -= eh;
            }
            TibcoMessageHandleFinishedEventDelegates.Clear();
            foreach (TibcoMessageAsynSendFinishedEventHandler eh in TibcoMessageAsynSendFinishedEventDelegates)
            {
                _TibcoMessageAsynSendFinished -= eh;
            }
            TibcoMessageAsynSendFinishedEventDelegates.Clear();
        }

        public void _listener_messageReceivedEventHandler(Object o, MessageReceivedEventArgs arg)
        {
            if (arg.Message.FieldCount < 1)
            {
                if (log.IsWarnEnabled) log.Warn("TibcoMessage Format is InCorrect");
            }
            else
            {
                processTibcoMessage(arg.Message);
            }
        }

        public virtual void processTibcoMessage(TIBCO.Rendezvous.Message msg)
        {

        }

        public void SendTibcoMessage(string MessageIDTag, List<MessageField> SingleTibcoMessage)
        {
            string ErrorMsg = "";
            try
            {
                this._MessageID = System.Guid.NewGuid().ToString();
                //SendCountMessage(MessageIDTag, _MessageID, 1);
                Message msg = new Message();
                msg.Reset();
                msg.SendSubject = this.tibco_sendsubject;
                msg.AddField(MessageIDTag, this._MessageID);
                //加入總筆數tag
                msg.AddField("10038", "1");
                foreach (MessageField prop in SingleTibcoMessage)
                {
                    msg.AddField(prop.Name, prop.Value);
                }
                if (this._transport != null)
                {
                    this._transport.Send(msg);
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "SendTibcoMessage: Error happened(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error("SendTibcoMessage() Error", ex);
            }
            finally
            {
                if (_UISyncContext != null & IsEventInUIThread)
                {
                    _UISyncContext.Post(OnTibcoMessageSendFinished, new TibcoMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnTibcoMessageSendFinished(new TibcoMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
        }

        public void SendTibcoMessage(string MessageIDTag, List<List<MessageField>> MultiTibcoMessage)
        {
            string ErrorMsg = "";
            try
            {
                this._MessageID = System.Guid.NewGuid().ToString();
                //SendCountMessage(MessageIDTag, _MessageID, MultiTibcoMessage.Count);
                foreach (List<MessageField> SingleTibcoMessage in MultiTibcoMessage)
                {
                    Message msg = new Message();
                    msg.Reset();
                    msg.SendSubject = this.tibco_sendsubject;
                    msg.AddField(MessageIDTag, this._MessageID);
                    //加入總筆數tag
                    msg.AddField("10038", MultiTibcoMessage.Count().ToString());
                    foreach (MessageField prop in SingleTibcoMessage)
                    {
                        msg.AddField(prop.Name, prop.Value);
                    }
                    if (this._transport != null)
                    {
                        this._transport.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "SendTibcoMessage() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error("SendTibcoMessage() Error", ex);
            }
            finally
            {
                if (_UISyncContext != null & IsEventInUIThread)
                {
                    _UISyncContext.Post(OnTibcoMessageSendFinished, new TibcoMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnTibcoMessageSendFinished(new TibcoMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
        }

        public void SendAsynTibcoMessage(string MessageIDTag, List<List<MessageField>> MultiTibcoMessage)
        {
            ThreadStart SendThreadStart = new ThreadStart(
                delegate()
                {
                    lock (this)
                    {
                        this._MessageID = System.Guid.NewGuid().ToString();
                        //SendCountMessage(MessageIDTag, _MessageID, MultiTibcoMessage.Count);
                        SendAsyn(MessageIDTag, MultiTibcoMessage);
                    }
                });
            Thread SendThread = new Thread(SendThreadStart);
            SendThread.Start();
        }

        protected void SendAsyn(string MessageIDTag, List<List<MessageField>> MultiTibcoMessage)
        {
            string ErrorMsg = "";
            try
            {
                foreach (List<MessageField> SingleTibcoMessage in MultiTibcoMessage)
                {
                    Message msg = new Message();
                    msg.Reset();
                    msg.SendSubject = this.tibco_sendsubject;
                    msg.AddField(MessageIDTag, this._MessageID);
                    //加入總筆數tag
                    msg.AddField("10038", MultiTibcoMessage.Count().ToString());
                    foreach (MessageField prop in SingleTibcoMessage)
                    {
                        msg.AddField(prop.Name, prop.Value);
                    }
                    if (this._transport != null)
                    {
                        this._transport.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "SendAsyn() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
            }
            finally
            {
                if (_UISyncContext != null & IsEventInUIThread)
                {
                    _UISyncContext.Post(OnTibcoMessageSendFinished, new TibcoMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnTibcoMessageSendFinished(new TibcoMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
        }

        /// <summary>
        /// 發送筆數的Message
        /// </summary>
        /// <param name="MessageIDTag"></param>
        /// <param name="MessageID"></param>
        /// <param name="MessageCount"></param>
        private void SendCountMessage(string MessageIDTag, string MessageID, int MessageCount)
        {
            Message msg = new Message();
            msg.Reset();
            msg.SendSubject = this.tibco_sendsubject;
            List<MessageField> MessageCountRow = new List<MessageField>();
            MessageField MessageMessageIDField = new MessageField();
            MessageMessageIDField.Name = MessageIDTag;
            MessageMessageIDField.Value = MessageID;
            MessageCountRow.Add(MessageMessageIDField);
            MessageField MessageCountRowField = new MessageField();
            MessageCountRowField.Name = "10038";
            MessageCountRowField.Value = MessageCount.ToString();
            MessageCountRow.Add(MessageCountRowField);
            foreach (MessageField prop in MessageCountRow)
            {
                msg.AddField(prop.Name, prop.Value);
            }
            if (this._transport != null)
            {
                this._transport.Send(msg);
            }
        }
    }
}
