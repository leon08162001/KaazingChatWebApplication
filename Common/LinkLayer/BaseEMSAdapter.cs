using Common.HandlerLayer;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using TIBCO.EMS;

namespace Common.LinkLayer
{
    /// <summary>
    ///收到一筆EMSMessage並完成資料處理時的事件參數類別
    /// </summary>
    public class EMSMessageHandleFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataRow _MessageRow;
        public EMSMessageHandleFinishedEventArgs(string errorMessage, DataRow MessageRow)
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
    ///非同步發送EMSMessage完成時的事件參數類別
    /// </summary>
    public class EMSMessageAsynSendFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        public EMSMessageAsynSendFinishedEventArgs(string errorMessage)
        {
            _errorMessage = errorMessage;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
    }
    [Serializable]

    public abstract class BaseEMSAdapter : Common.LinkLayer.IEMSAdapter
    {
        protected readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected string _Uri = string.Empty;
        protected string _ListenName = string.Empty;
        protected string _SendName = string.Empty;
        protected string _UserName = string.Empty;
        protected string _PassWord = string.Empty;
        protected string _MacAddress = string.Empty;
        protected bool _UseSharedConnection = true;
        protected DBAction _SenderDBAction = DBAction.None;
        protected DBAction _ReceiverDBAction = DBAction.None;
        protected DestinationFeature _DestinationFeature = DestinationFeature.Topic;

        //使用_UseSharedConnection=true共享連線時下面的_Factory和_Connection會一直是null
        protected ConnectionFactory _Factory = null;
        protected Connection _Connection = null;
        protected Session _Session = null;
        protected MessageConsumer _Consumer = null;

        protected MessageProducer _Producer;

        protected string _MessageID;

        protected SynchronizationContext _UISyncContext;

        protected TopicTypeHandler _Handler = null;

        protected bool _IsEventInUIThread = false;             //觸發事件時是否回到UI Thread預設為false
        protected bool _UseSSL = false;

        protected Timer HeartBeatTimer;
        protected int _HeartBeatInterval = 60;
        protected int _sendAmounnts = 0;
        protected double _MessageTimeOut = 0;
        protected string _Selector = "";
        protected bool _IsDurableConsumer = false;
        protected string _ClientID = "";

        public delegate void EMSMessageHandleFinishedEventHandler(object sender, EMSMessageHandleFinishedEventArgs e);
        List<EMSMessageHandleFinishedEventHandler> EMSMessageHandleFinishedEventDelegates = new List<EMSMessageHandleFinishedEventHandler>();
        private event EMSMessageHandleFinishedEventHandler _EMSMessageHandleFinished;
        public event EMSMessageHandleFinishedEventHandler EMSMessageHandleFinished
        {
            add
            {
                _EMSMessageHandleFinished += value;
                EMSMessageHandleFinishedEventDelegates.Add(value);
            }
            remove
            {
                _EMSMessageHandleFinished -= value;
                EMSMessageHandleFinishedEventDelegates.Remove(value);
            }
        }

        public delegate void EMSMessageAsynSendFinishedEventHandler(object sender, EMSMessageAsynSendFinishedEventArgs e);
        List<EMSMessageAsynSendFinishedEventHandler> EMSMessageAsynSendFinishedEventDelegates = new List<EMSMessageAsynSendFinishedEventHandler>();
        private event EMSMessageAsynSendFinishedEventHandler _EMSMessageAsynSendFinished;
        public event EMSMessageAsynSendFinishedEventHandler EMSMessageAsynSendFinished
        {
            add
            {
                _EMSMessageAsynSendFinished += value;
                EMSMessageAsynSendFinishedEventDelegates.Add(value);
            }
            remove
            {
                _EMSMessageAsynSendFinished -= value;
                EMSMessageAsynSendFinishedEventDelegates.Remove(value);
            }
        }

        /// <summary>
        /// 收到一筆EMSMessage並完成資料處理時事件
        /// </summary>
        protected virtual void OnEMSMessageHandleFinished(object state)
        {
            EMSMessageHandleFinishedEventArgs e = state as EMSMessageHandleFinishedEventArgs;
            if (_EMSMessageHandleFinished != null)
            {
                _EMSMessageHandleFinished(this, e);
            }
        }
        /// <summary>
        /// 非同步發送Message完成時事件
        /// </summary>
        protected virtual void OnEMSMessageSendFinished(object state)
        {
            EMSMessageAsynSendFinishedEventArgs e = state as EMSMessageAsynSendFinishedEventArgs;
            if (_EMSMessageAsynSendFinished != null)
            {
                _EMSMessageAsynSendFinished(this, e);
            }
        }

        public string Uri
        {
            set { _Uri = value; }
            get { return _Uri; }
        }

        public DestinationFeature DestinationFeature
        {
            set { _DestinationFeature = value; }
            get { return _DestinationFeature; }
        }

        public string ListenName
        {
            set { _ListenName = value; }
            get { return _ListenName; }
        }

        public string SendName
        {
            set { _SendName = value; }
            get { return _SendName; }
        }


        public string UserName
        {
            set { _UserName = value; }
        }

        public string PassWord
        {
            set { _PassWord = value; }
        }

        public string MacAddress
        {
            set { _MacAddress = value; }
            get { return _MacAddress; }
        }
        public int CurrentSendAmounts
        {
            get { return _sendAmounnts; }
        }
        public bool UseSharedConnection
        {
            set { _UseSharedConnection = value; }
            get { return _UseSharedConnection; }
        }
        public DBAction SenderDBAction
        {
            set { _SenderDBAction = value; }
            get { return _SenderDBAction; }
        }
        public DBAction ReceiverDBAction
        {
            set { _ReceiverDBAction = value; }
            get { return _ReceiverDBAction; }
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
        public bool IsDurableConsumer
        {
            get { return _IsDurableConsumer; }
            set { _IsDurableConsumer = value; }
        }
        public bool UseSSL
        {
            get { return _UseSSL; }
            set { _UseSSL = value; }
        }
        /// <summary>
        /// 心跳訊息間隔(秒)
        /// </summary>
        public int HeartBeatInterval
        {
            set { _HeartBeatInterval = value; }
            get { return _HeartBeatInterval; }
        }
        public double MessageTimeOut
        {
            set { _MessageTimeOut = value; }
            get { return _MessageTimeOut; }
        }
        public string Selector
        {
            get { return _Selector; }
            set
            {
                _Selector = value;
            }
        }
        public SynchronizationContext UISyncContext
        {
            get { return _UISyncContext; }
        }
        public TopicTypeHandler Handler
        {
            get { return _Handler; }
            set { _Handler = value; }
        }

        public BaseEMSAdapter()
        {
        }

        public BaseEMSAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            _Uri = Uri;
            _DestinationFeature = DestinationFeature;
            _ListenName = ListenName;
            _SendName = SendName;
        }
        public BaseEMSAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            _Uri = Uri;
            _DestinationFeature = DestinationFeature;
            _ListenName = ListenName;
            _SendName = SendName;
            _UserName = UserName;
            _PassWord = Pwd;
        }

        public void Start(string ClientID = "", bool IsDurableConsumer = false)
        {
            string SingleUrl = "";
            string Urls = "";
            _IsDurableConsumer = IsDurableConsumer;
            _ClientID = ClientID;
            string urls;
            string ports;
            urls = _Uri.Split(new char[] { ':' })[0];
            ports = _Uri.Split(new char[] { ':' })[1];
            //代表只有1個IP
            if (urls.IndexOf(",") == -1)
            {
                SingleUrl = urls;
            }
            //代表多個IP
            else
            {
                SingleUrl = urls.Split(new char[] { ',' })[0];
                Urls = urls;
            }
            if (!SingleUrl.Equals("") && SingleUrl.IndexOf(":") > -1)
            {
                string ip = SingleUrl.Substring(0, SingleUrl.IndexOf(":"));
                _MacAddress = Util.GetMacAddress();
            }


            // Example connection strings:
            //failover Sample
            //tcp://localhost:7222,tcp://localhost:7222
            //Tibems.SetExceptionOnFTSwitch(true); 
            try
            {
                Tibems.SetExceptionOnFTSwitch(true);
                if (_UseSharedConnection)
                {
                    if (Urls.Equals(""))
                    {
                        EMSSharedConnection.Open(SingleUrl, ports, _UserName, _PassWord, _UseSSL, _IsDurableConsumer, _ClientID);
                    }
                    else
                    {
                        EMSSharedConnection.Open(Urls, ports, _UserName, _PassWord, _UseSSL, _IsDurableConsumer, _ClientID);
                    }
                    _Session = EMSSharedConnection.GetConnection().CreateSession(false, SessionMode.AutoAcknowledge);
                    _Connection = EMSSharedConnection.GetConnection();
                    _Connection.ExceptionHandler += new EMSExceptionHandler(_Connection_ExceptionHandler);
                    StartListener();
                    StartSender();
                    _UISyncContext = SynchronizationContext.Current;
                    //InitialHeartBeat();
                }
                else
                {
                    //if (_Connection == null)
                    //{
                    if (Urls.Equals(""))
                    {
                        _Factory = new ConnectionFactory(Util.GetEMSFailOverConnString(SingleUrl, ports, _UseSSL));
                    }
                    else
                    {
                        _Factory = new ConnectionFactory(Util.GetEMSFailOverConnString(Urls, ports, _UseSSL));
                    }
                    _Factory.SetReconnAttemptCount(1200);     // 1200retries
                    _Factory.SetReconnAttemptDelay(5000);  // 5seconds
                    _Factory.SetReconnAttemptTimeout(20000); // 20seconds
                    if (IsDurableConsumer && !string.IsNullOrEmpty(ClientID)) _Factory.SetClientID(ClientID);
                    try
                    {
                        if (_UserName != "" && _PassWord != "")
                        {
                            _Connection = _Factory.CreateConnection(_UserName, _PassWord);
                        }
                        else
                        {
                            _Connection = _Factory.CreateConnection();
                        }
                        _Connection.ExceptionHandler += new EMSExceptionHandler(_Connection_ExceptionHandler);
                    }
                    catch (TIBCO.EMS.EMSException ex)
                    {
                        if (log.IsErrorEnabled) log.Error("BaseEMSAdapter Start() Error", ex);
                        throw ex;
                    }
                    try
                    {
                        _Connection.Start();
                    }
                    catch (TIBCO.EMS.EMSException ex)
                    {
                        if (log.IsErrorEnabled) log.Error("BaseEMSAdapter Start() Error", ex);
                        throw ex;
                    }
                    _Session = _Connection.CreateSession(false, SessionMode.AutoAcknowledge);
                    StartListener();
                    StartSender();
                    _UISyncContext = SynchronizationContext.Current;
                    //InitialHeartBeat();
                    //}
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("BaseEMSAdapter Start() Error", ex);
                throw ex;
            }
        }

        public void Close()
        {
            try
            {
                if (_Session != null)
                {
                    _Producer = null;
                    _Consumer = null;
                    _Session.Close();
                    _Session = null;
                }
                if (!_UseSharedConnection)
                {
                    if (_Connection != null)
                    {
                        if (!_Connection.IsClosed)
                        {
                            _Connection.Stop();
                            _Connection.Close();
                            _Connection = null;
                        }
                    }
                }
                EndHeartBeat();
            }
            catch (TIBCO.EMS.EMSException ex)
            {
                if (log.IsErrorEnabled) log.Error("BaseEMSAdapter Close() Error", ex);
                throw ex;
            }
        }

        public void Restart(string ClientID = "", bool IsDurableConsumer = false)
        {
            try
            {
                Close();
                Start(ClientID, IsDurableConsumer);
                //InitialHeartBeat();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual void RemoveAllEvents()
        {
            foreach (EMSMessageHandleFinishedEventHandler eh in EMSMessageHandleFinishedEventDelegates)
            {
                _EMSMessageHandleFinished -= eh;
            }
            EMSMessageHandleFinishedEventDelegates.Clear();
            foreach (EMSMessageAsynSendFinishedEventHandler eh in EMSMessageAsynSendFinishedEventDelegates)
            {
                _EMSMessageAsynSendFinished -= eh;
            }
            EMSMessageAsynSendFinishedEventDelegates.Clear();
        }

        public void listener_messageReceivedEventHandler(object sender, EMSMessageEventArgs arg)
        {
            processEMSMessage(arg.Message);
        }

        public abstract void processEMSMessage(Message message);
        public void SendMessage(string Text)
        {
            string ErrorMsg = "";
            try
            {
                if (_Session != null)
                {
                    if (!_Session.IsClosed)
                    {

                        TextMessage msg = _Session.CreateTextMessage();
                        msg.Text = Text;
                        long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                        _Producer.Send(msg, MessageDeliveryMode.NonPersistent, 9, MessageOut);
                        _sendAmounnts += 1;
                      }
                    else
                    {
                        //throw new Exception("Network connection or TibcoEMSService Has been closed!");
                        if (log.IsInfoEnabled) log.Info("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendMessage() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnEMSMessageSendFinished, new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnEMSMessageSendFinished(new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
        }
        public bool SendEMSMessage(string MessageIDTag, List<MessageField> SingleEMSMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0)
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                this._MessageID = System.Guid.NewGuid().ToString();
                //註解發送筆數資訊
                //SendCountMessage(MessageIDTag, _MessageID, 1);
                if (_Session != null)
                {
                    if (!_Session.IsClosed)
                    {

                        Message msg = _Session.CreateMessage();
                        msg.SetStringProperty(MessageIDTag, this._MessageID);
                        //MacAddress(99)
                        if (!string.IsNullOrEmpty(_MacAddress))
                        {
                            msg.SetStringProperty("99", _MacAddress);
                        }
                        //加入總筆數tag
                        msg.SetStringProperty("10038", "1");
                        foreach (MessageField prop in SingleEMSMessage)
                        {
                            msg.SetStringProperty(prop.Name, prop.Value);
                        }
                        long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                        _Producer.Send(msg, MessageDeliveryMode.NonPersistent, 9, MessageOut);
                        isSend = true;
                        _sendAmounnts += 1;
                        if (DelayedPerWhenNumber > 0 && DelayedMillisecond > 0)
                        {
                            SlowDownProducer(DelayedPerWhenNumber, DelayedMillisecond);
                        }
                    }
                    else
                    {
                        //throw new Exception("Network connection or TibcoEMSService Has been closed!");
                        if (log.IsInfoEnabled) log.Info("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendEMSMessage() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnEMSMessageSendFinished, new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnEMSMessageSendFinished(new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public bool SendEMSMessage(string MessageIDTag, List<List<MessageField>> MultiEMSMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0)
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                this._MessageID = System.Guid.NewGuid().ToString();
                //SendCountMessage(MessageIDTag, _MessageID, MultiEMSMessage.Count);
                if (_Session != null)
                {
                    if (!_Session.IsClosed)
                    {

                        foreach (List<MessageField> SingleEMSMessage in MultiEMSMessage)
                        {
                            Message msg = _Session.CreateMessage();
                            msg.SetStringProperty(MessageIDTag, this._MessageID);
                            //MacAddress(99)
                            if (!string.IsNullOrEmpty(_MacAddress))
                            {
                                msg.SetStringProperty("99", _MacAddress);
                            }
                            //加入總筆數tag
                            msg.SetStringProperty("10038", MultiEMSMessage.Count().ToString());
                            foreach (MessageField prop in SingleEMSMessage)
                            {
                                msg.SetStringProperty(prop.Name, prop.Value);
                            }
                            long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                            _Producer.Send(msg, MessageDeliveryMode.NonPersistent, 9, MessageOut);
                            isSend = true;
                            _sendAmounnts += 1;
                            if (DelayedPerWhenNumber > 0 && DelayedMillisecond > 0)
                            {
                                SlowDownProducer(DelayedPerWhenNumber, DelayedMillisecond);
                            }
                        }
                    }
                    else
                    {
                        //throw new Exception("Network connection or TibcoEMSService Has been closed!");
                        if (log.IsInfoEnabled) log.Info("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendEMSMessage() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnEMSMessageSendFinished, new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnEMSMessageSendFinished(new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public void SendAsynEMSMessage(string MessageIDTag, List<List<MessageField>> MultiEMSMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0)
        {
            ThreadStart SendThreadStart = new ThreadStart(
                delegate ()
                {
                    lock (this)
                    {
                        this._MessageID = System.Guid.NewGuid().ToString();
                        //註解發送筆數資訊
                        //SendCountMessage(MessageIDTag, _MessageID, MultiEMSMessage.Count);
                        SendAsyn(_Session, MessageIDTag, MultiEMSMessage, DelayedPerWhenNumber, DelayedMillisecond);
                    }
                });
            Thread SendThread = new Thread(SendThreadStart);
            SendThread.Start();
        }
        public bool SendFile(string FileName, string FilePath, string ID = "")
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                if (!_Session.IsClosed)
                {
                    var bytes = default(byte[]);
                    using (StreamReader sr = new StreamReader(FilePath))
                    {
                        using (var memstream = new MemoryStream())
                        {
                            var buffer = new byte[512];
                            var bytesRead = default(int);
                            while ((bytesRead = sr.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                                memstream.Write(buffer, 0, bytesRead);
                            bytes = memstream.ToArray();
                        }
                    }

                    BytesMessage msg = _Session.CreateBytesMessage();
                    msg.WriteBytes(bytes);
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("filename", FileName);
                    msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + FileName));
                    msg.MsgType = "file";

                    long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                    _Producer.Send(msg, MessageDeliveryMode.NonPersistent, 2, MessageOut);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendFile() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnEMSMessageSendFinished, new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnEMSMessageSendFinished(new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public bool SendFile(string FileName, byte[] FileBytes, string ID = "")
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                if (!_Session.IsClosed)
                {
                    BytesMessage msg = _Session.CreateBytesMessage();
                    msg.WriteBytes(FileBytes);
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("filename", FileName);
                    msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + FileName));
                    msg.MsgType = "file";
                    long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                    _Producer.Send(msg, MessageDeliveryMode.NonPersistent, 2, MessageOut);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendFile() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnEMSMessageSendFinished, new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnEMSMessageSendFinished(new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public void ReStartListener(string ListenName)
        {
            try
            {
                if (ListenName != "" && _Session != null)
                {
                    if (_Consumer != null)
                    {
                        _Consumer.Close();
                    }
                    if (_DestinationFeature == DestinationFeature.Topic)
                    {
                        if (_IsDurableConsumer)
                        {
                            if (_Selector.Equals(""))
                            {
                                _Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _Connection.ClientID, null, false);
                            }
                            else
                            {
                                _Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _Connection.ClientID, _Selector, false);
                            }
                        }
                        else
                        {
                            if (_Selector.Equals(""))
                            {
                                _Consumer = _Session.CreateConsumer(_Session.CreateTopic(_ListenName));
                            }
                            else
                            {
                                _Consumer = _Session.CreateConsumer(_Session.CreateTopic(_ListenName), _Selector);
                            }
                        }
                        _Consumer.MessageHandler += new EMSMessageHandler(listener_messageReceivedEventHandler);
                    }
                    else if (_DestinationFeature == DestinationFeature.Queue)
                    {
                        if (_Selector.Equals(""))
                        {
                            _Consumer = _Session.CreateConsumer(_Session.CreateQueue(ListenName));
                        }
                        else
                        {
                            _Consumer = _Session.CreateConsumer(_Session.CreateQueue(ListenName), _Selector);
                        }
                        _Consumer.MessageHandler += new EMSMessageHandler(listener_messageReceivedEventHandler);
                    }
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseEMSAdapter ReStartListener() Error", exception);
            }
        }

        public void ReStartSender(string SendName)
        {
            try
            {
                if (SendName != "" && _Session != null)
                {
                    //if (_Producer != null)
                    //{
                    //    _Producer.Close();
                    //}
                    if (_DestinationFeature == DestinationFeature.Topic)
                    {
                        _Producer = _Session.CreateProducer(_Session.CreateTopic(SendName));
                    }
                    else if (_DestinationFeature == DestinationFeature.Queue)
                    {
                        _Producer = _Session.CreateProducer(_Session.CreateQueue(SendName));
                    }
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseEMSAdapter ReStartSender() Error", exception);
            }
        }

        public void CloseSharedConnection()
        {
            try
            {
                if (_UseSharedConnection)
                {
                    EMSSharedConnection.Close();
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseEMSAdapter CloseSharedConnection() Error", exception);
            }
        }

        protected void SendAsyn(Session Session, string MessageIDTag, List<List<MessageField>> MultiEMSMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0)
        {
            string ErrorMsg = "";
            try
            {
                //if ((_Session as TIBCO.EMS.Session).Connection.IsDisconnected())
                //{
                //    Start();
                //}
                if (_Session != null)
                {
                    if (!_Session.IsClosed)
                    {
                        foreach (List<MessageField> SingleEMSMessage in MultiEMSMessage)
                        {
                            Message msg = Session.CreateMessage();
                            msg.SetStringProperty(MessageIDTag, this._MessageID);
                            //MacAddress(99)
                            if (!string.IsNullOrEmpty(_MacAddress))
                            {
                                msg.SetStringProperty("99", _MacAddress);
                            }
                            //加入總筆數tag
                            msg.SetStringProperty("10038", MultiEMSMessage.Count().ToString());
                            foreach (MessageField prop in SingleEMSMessage)
                            {
                                msg.SetStringProperty(prop.Name, prop.Value);
                            }
                            long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                            _Producer.Send(msg, MessageDeliveryMode.NonPersistent, 9, MessageOut);
                            _sendAmounnts += 1;
                            if (DelayedPerWhenNumber > 0 && DelayedMillisecond > 0)
                            {
                                SlowDownProducer(DelayedPerWhenNumber, DelayedMillisecond);
                            }
                        }
                    }
                    else
                    {
                        //throw new Exception("Network connection or TibcoEMSService Has been closed!");
                        if (log.IsInfoEnabled) log.Info("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendAsyn() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnEMSMessageSendFinished, new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnEMSMessageSendFinished(new EMSMessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
        }
        /// <summary>
        /// 發送筆數的Message
        /// </summary>
        /// <param name="MessageIDTag"></param>
        /// <param name="MessageID"></param>
        /// <param name="MessageCount"></param>
        private bool SendCountMessage(string MessageIDTag, string MessageID, int MessageCount)
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                //if ((_Session as TIBCO.EMS.Session).Connection.IsDisconnected())
                //{
                //    Start();
                //}
                if (_Session != null)
                {
                    if (!_Session.IsClosed)
                    {
                        Message msg = _Session.CreateMessage();
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
                            msg.SetStringProperty(prop.Name, prop.Value);
                        }
                        //MacAddress(99)
                        if (!string.IsNullOrEmpty(_MacAddress))
                        {
                            msg.SetStringProperty("99", _MacAddress);
                        }
                        long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                        _Producer.Send(msg, MessageDeliveryMode.NonPersistent, 9, MessageOut);
                        isSend = true;
                    }
                    else
                    {
                        //throw new Exception("Network connection or TibcoEMSService Has been closed!");
                        if (log.IsInfoEnabled) log.Info("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendCountMessage() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
            }
            return isSend;
        }

        private void StartListener()
        {
            try
            {
                if (_ListenName != "" && _Session != null)
                {
                    if (_DestinationFeature == DestinationFeature.Topic)
                    {
                        if (_IsDurableConsumer)
                        {
                            if (_Selector.Equals(""))
                            {
                                _Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _Connection.ClientID, null, false);
                            }
                            else
                            {
                                _Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _Connection.ClientID, _Selector, false);
                            }
                        }
                        else
                        {
                            if (_Selector.Equals(""))
                            {
                                _Consumer = _Session.CreateConsumer(_Session.CreateTopic(_ListenName));
                            }
                            else
                            {
                                _Consumer = _Session.CreateConsumer(_Session.CreateTopic(_ListenName), _Selector);
                            }
                        }
                        _Consumer.MessageHandler += new EMSMessageHandler(listener_messageReceivedEventHandler);
                    }
                    else if (_DestinationFeature == DestinationFeature.Queue)
                    {
                        if (_Selector.Equals(""))
                        {
                            _Consumer = _Session.CreateConsumer(_Session.CreateQueue(_ListenName));
                        }
                        else
                        {
                            _Consumer = _Session.CreateConsumer(_Session.CreateQueue(_ListenName), _Selector);
                        }
                        _Consumer.MessageHandler += new EMSMessageHandler(listener_messageReceivedEventHandler);
                    }
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseEMSAdapter StartListener() Error", exception);
            }
        }

        private void StartSender()
        {
            try
            {
                if (_SendName != "" && _Session != null)
                {
                    if (_DestinationFeature == DestinationFeature.Topic)
                    {
                        _Producer = _Session.CreateProducer(_Session.CreateTopic(_SendName));
                    }
                    else if (_DestinationFeature == DestinationFeature.Queue)
                    {
                        _Producer = _Session.CreateProducer(_Session.CreateQueue(_SendName));
                    }
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseEMSAdapter StartSender() Error", exception);
            }
        }

        private void InitialHeartBeat()
        {
            try
            {
                TimerCallback TCB = new TimerCallback(state => { SetHeartBeat(); });
                HeartBeatTimer = new Timer(TCB, DateTime.Now, 0, 1000 * _HeartBeatInterval);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void EndHeartBeat()
        {
            HeartBeatTimer = null;
        }

        private void SetHeartBeat()
        {
            string ErrorMsg = "";
            try
            {
                if (_Session != null)
                {
                    if (!_Session.IsClosed)
                    {
                        Message msg = _Session.CreateMessage();
                        //MacAddress(99)
                        if (!string.IsNullOrEmpty(_MacAddress))
                        {
                            msg.SetStringProperty("99", _MacAddress);
                        }
                        msg.SetStringProperty("0", "HeartBeat");
                    }
                    else
                    {
                        //throw new Exception("Network connection or TibcoEMSService Has been closed!");
                        if (log.IsInfoEnabled) log.Info("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SetHeartBeat() Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
            }
        }
        private void SlowDownProducer(int DelayedPerWhenNumber, int DelayedMillisecond)
        {
            if (_sendAmounnts % DelayedPerWhenNumber == 0)
            {
                Thread.Sleep(DelayedMillisecond);
            }
        }
        private void SlowDownProducer1()
        {
            //int iSlowDownNum = this.SenderDBAction == DBAction.Query ? 15 : this.SenderDBAction == DBAction.Add ? 12 : this.SenderDBAction == DBAction.Update ? 12 : this.SenderDBAction == DBAction.Delete ? 20 :
            //                   30;
            //if (_sendAmounnts % iSlowDownNum == 0)
            //{
            //    //Thread.Sleep(10);
            //    Thread.Sleep(20);
            //}
            int iSlowDownNum;
            if (this.SenderDBAction == DBAction.Query)
            {
                iSlowDownNum = 12;
                if (_sendAmounnts % iSlowDownNum == 0)
                {
                    Thread.Sleep(35);
                }
            }
            else if (this.SenderDBAction == DBAction.Add)
            {
                iSlowDownNum = 25;
                if (_sendAmounnts % iSlowDownNum == 0)
                {
                    Thread.Sleep(20);
                }
            }
            else if (this.SenderDBAction == DBAction.Update)
            {
                iSlowDownNum = 30;
                if (_sendAmounnts % iSlowDownNum == 0)
                {
                    Thread.Sleep(15);
                }

            }
            else if (this.SenderDBAction == DBAction.Delete)
            {
                iSlowDownNum = 25;
                if (_sendAmounnts % iSlowDownNum == 0)
                {
                    Thread.Sleep(20);
                }
            }
            else if (this.SenderDBAction == DBAction.None)
            {
                iSlowDownNum = 30;
                if (_sendAmounnts % iSlowDownNum == 0)
                {
                    Thread.Sleep(20);
                }
            }
        }
        private void _Connection_ExceptionHandler(object sender, EMSExceptionEventArgs args)
        {
            EMSException ex = args.Exception;
            if (ex.Message.Equals("Connection unknown by server"))
            {
                Restart();
                string ConnActiveUrl = Tibems.GetConnectionActiveURL(_Connection);
                if (log.IsErrorEnabled) log.ErrorFormat(ex.Message + "(Connection has performed fault-tolerant switch to {0})", ConnActiveUrl, ex);
            }
            else
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
            }
        }
    }
}
