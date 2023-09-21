using Apache.NMS;
using Common.HandlerLayer;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TIBCO.EMS;

namespace Common.LinkLayer
{
    [Serializable]
    public abstract class BaseEMSAdapter1 : Common.LinkLayer.IMQAdapter1, Common.LinkLayer.ITibcoEMSAdapter
    {
        //protected readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected string _Uri = string.Empty;
        protected string _ListenName = string.Empty;
        protected string _SendName = string.Empty;
        protected string _UserName = string.Empty;
        protected string _PassWord = string.Empty;
        protected string _MacAddress = string.Empty;
        protected bool _UseSharedConnection = true;
        protected DBAction _SenderDBAction = DBAction.None;
        protected DBAction _ReceiverDBAction = DBAction.None;
        protected MessageDeliveryMode _DeliveryMode = MessageDeliveryMode.NonPersistent;
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
        protected Type _DataType;

        protected bool _IsEventInUIThread = false;             //觸發事件時是否回到UI Thread預設為false
        protected bool _UseSSL = false;
        protected List<string> _CertsPath = new List<string>();

        protected Timer HeartBeatTimer;
        protected int _HeartBeatInterval = 60;
        protected int _sendAmounnts = 0;
        protected double _MessageTimeOut = 0;
        protected string _Selector = "";
        protected int _ReceivedMessageTimeOut = 20;
        protected bool _IsDurableConsumer = false;
        protected string _ClientID = "";

        List<MessageAsynSendFinishedEventHandler> MessageAsynSendFinishedEventDelegates = new List<MessageAsynSendFinishedEventHandler>();
        private event MessageAsynSendFinishedEventHandler _MessageAsynSendFinished;
        public event MessageAsynSendFinishedEventHandler MessageAsynSendFinished
        {
            add
            {
                _MessageAsynSendFinished += value;
                MessageAsynSendFinishedEventDelegates.Add(value);
            }
            remove
            {
                _MessageAsynSendFinished -= value;
                MessageAsynSendFinishedEventDelegates.Remove(value);
            }
        }

        List<MessageHandleFinishedEventHandler> MessageHandleFinishedEventDelegates = new List<MessageHandleFinishedEventHandler>();
        private event MessageHandleFinishedEventHandler _MessageHandleFinished;
        public event MessageHandleFinishedEventHandler MessageHandleFinished
        {
            add
            {
                _MessageHandleFinished += value;
                MessageHandleFinishedEventDelegates.Add(value);
            }
            remove
            {
                _MessageHandleFinished -= value;
                MessageHandleFinishedEventDelegates.Remove(value);
            }
        }

        /// <summary>
        /// 收到一筆Message並完成資料處理時事件
        /// </summary>
        protected virtual void OnMessageHandleFinished(object state)
        {
            MessageHandleFinishedEventArgs e = state as MessageHandleFinishedEventArgs;
            if (_MessageHandleFinished != null)
            {
                _MessageHandleFinished(this, e);
            }
        }
        /// <summary>
        /// 非同步發送Message完成時事件
        /// </summary>
        protected virtual void OnMessageSendFinished(object state)
        {
            MessageAsynSendFinishedEventArgs e = state as MessageAsynSendFinishedEventArgs;
            if (_MessageAsynSendFinished != null)
            {
                _MessageAsynSendFinished(this, e);
            }
        }

        public string Uri
        {
            set { _Uri = value; }
            get { return _Uri; }
        }
        public MessageDeliveryMode DeliveryMode
        {
            set { _DeliveryMode = value; }
            get { return _DeliveryMode; }
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
        public List<string> CertsPath
        {
            get { return _CertsPath; }
            set { _CertsPath = value; }
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
        public int ReceivedMessageTimeOut
        {
            get
            {
                return _ReceivedMessageTimeOut;
            }
            set
            {
                _ReceivedMessageTimeOut = value;
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
        public Type DataType
        {
            set { _DataType = value; }
            get { return _DataType; }
        }

        public BaseEMSAdapter1()
        {
        }

        public BaseEMSAdapter1(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            _Uri = Uri;
            _DestinationFeature = DestinationFeature;
            _ListenName = ListenName;
            _SendName = SendName;
        }
        public BaseEMSAdapter1(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            _Uri = Uri;
            _DestinationFeature = DestinationFeature;
            _ListenName = ListenName;
            _SendName = SendName;
            _UserName = UserName;
            _PassWord = Pwd;
        }
        public bool CheckMessageBrokerAlive()
        {
            string urls;
            string ports;
            string url = "";
            urls = _Uri.Split(new char[] { ':' })[0];
            ports = _Uri.Split(new char[] { ':' })[1];
            //代表url只有1個IP
            if (urls.IndexOf(",") == -1)
            {
                bool result = false;
                //代表只有1個port
                if (ports.IndexOf(",") == -1)
                {
                    url = "tcp://" + urls + ":" + ports;
                    _Factory = new ConnectionFactory(url);
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
                        result = true;
                    }
                    catch (EMSException ex)
                    {
                        Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, "CheckMessageBrokerAlive() Error");
                        result = false;
                    }
                }
                //代表多個port
                else
                {
                    foreach (string port in ports.Split(new char[] { ',' }))
                    {
                        url = "tcp://" + urls + ":" + port;
                        _Factory = new ConnectionFactory(url);
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
                            result = true;
                            break;
                        }
                        catch (EMSException ex)
                        {
                            Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, "CheckMessageBrokerAlive() Error");
                            continue;
                        }
                    }
                }
                return result;
            }
            //代表url有多個IP
            else
            {
                bool result = false;
                List<string> lstUrl = urls.Split(new char[] { ',' }).ToList<string>();
                int i = 0;
                foreach (string _url in lstUrl)
                {
                    //代表只有1個port
                    if (ports.IndexOf(",") == -1)
                    {
                        url = "tcp://" + _url + ":" + ports;
                    }
                    //代表多個port
                    else
                    {
                        string[] lstPort = ports.Split(new char[] { ',' });
                        url = "tcp://" + _url + ":" + ports.Split(new char[] { ',' })[i];
                        i++;
                    }
                    _Factory = new ConnectionFactory(url);
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
                        result = true;
                        break;
                    }
                    catch (NMSException ex)
                    {
                        Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, "CheckMessageBrokerAlive() Error");
                        continue;
                    }
                }
                return result;
            }
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
            }


            // Example connection strings:
            //failover Sample
            //tcp://localhost:7222,tcp://localhost:7222
            //Tibems.SetExceptionOnFTSwitch(true); 
            try
            {
                Tibems.SetExceptionOnFTSwitch(true);
                //若使用持久消費者,將強制不使用共享連線
                _UseSharedConnection = _IsDurableConsumer ? false : _UseSharedConnection;
                if (_UseSharedConnection)
                {
                    if (Urls.Equals(""))
                    {
                        EMSSharedConnection.Open(SingleUrl, ports, _UserName, _PassWord, _UseSSL, CertsPath, _IsDurableConsumer, _ClientID);
                    }
                    else
                    {
                        EMSSharedConnection.Open(Urls, ports, _UserName, _PassWord, _UseSSL, CertsPath, _IsDurableConsumer, _ClientID);
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
                        if (_UseSSL)
                        {
                            EMSSharedConnection.SSLSetting(ref _Factory, SingleUrl, CertsPath);
                        }
                    }
                    else
                    {
                        _Factory = new ConnectionFactory(Util.GetEMSFailOverConnString(Urls, ports, _UseSSL));
                        if (_UseSSL)
                        {
                            EMSSharedConnection.SSLSetting(ref _Factory, Urls, CertsPath);
                        }
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
                        Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex);
                        throw ex;
                    }
                    try
                    {
                        _Connection.Start();
                    }
                    catch (TIBCO.EMS.EMSException ex)
                    {
                        Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex);
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
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex);
                throw ex;
            }
        }

        public void Close()
        {
            try
            {
                if (_Session != null)
                {
                    if (!_Connection.IsClosed)
                    {
                        _Producer = null;
                        _Consumer = null;
                        _Session.Close();
                        _Session = null;
                    }
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
                else
                {
                    CloseSharedConnection();
                }
                EndHeartBeat();
            }
            catch (TIBCO.EMS.EMSException ex)
            {
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex);
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
            foreach (MessageHandleFinishedEventHandler eh in MessageHandleFinishedEventDelegates)
            {
                _MessageHandleFinished -= eh;
            }
            MessageHandleFinishedEventDelegates.Clear();
            foreach (MessageAsynSendFinishedEventHandler eh in MessageAsynSendFinishedEventDelegates)
            {
                _MessageAsynSendFinished -= eh;
            }
            MessageAsynSendFinishedEventDelegates.Clear();
        }

        public void listener_messageReceivedEventHandler(object sender, EMSMessageEventArgs arg)
        {
            processMessage(arg.Message);
        }

        public abstract void processMessage(Message message);
        public bool SendMessage(string Text)
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                if (_Session != null)
                {
                    if (!_Session.IsClosed)
                    {

                        TextMessage msg = _Session.CreateTextMessage();
                        msg.MsgType = "text";
                        msg.Text = Text;
                        long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                        Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>(string.Format("Sending a message(message：{0}", Text));
                        _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                        _sendAmounnts += 1;
                    }
                    else
                    {
                        //throw new Exception("Network connection or TibcoEMSService Has been closed!");
                        Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendMessage() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public bool SendMessage(string MessageIDTag, List<MessageField> SingleMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0)
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
                        msg.MsgType = "map";
                        msg.SetStringProperty(MessageIDTag, this._MessageID);
                        //MacAddress(99)
                        if (!string.IsNullOrEmpty(_MacAddress))
                        {
                            msg.SetStringProperty("99", _MacAddress);
                        }
                        //加入總筆數tag
                        msg.SetStringProperty("10038", "1");
                        foreach (MessageField prop in SingleMessage)
                        {
                            msg.SetStringProperty(prop.Name, prop.Value);
                        }
                        long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                        _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                        isSend = true;
                        _sendAmounnts += 1;
                        if (DelayedPerWhenNumber > 0 && DelayedMillisecond > 0)
                        {
                            SlowDownProducer(DelayedPerWhenNumber, DelayedMillisecond);
                        }
                    }
                    else
                    {
                        Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendMessage() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }

        public bool SendMessage(string MessageIDTag, List<List<MessageField>> MultiMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0)
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                this._MessageID = System.Guid.NewGuid().ToString();
                //SendCountMessage(MessageIDTag, _MessageID, MultiMessage.Count);
                if (_Session != null)
                {
                    if (!_Session.IsClosed)
                    {

                        foreach (List<MessageField> SingleMessage in MultiMessage)
                        {
                            Message msg = _Session.CreateMessage();
                            msg.MsgType = "map";
                            msg.SetStringProperty(MessageIDTag, this._MessageID);
                            //MacAddress(99)
                            if (!string.IsNullOrEmpty(_MacAddress))
                            {
                                msg.SetStringProperty("99", _MacAddress);
                            }
                            //加入總筆數tag
                            msg.SetStringProperty("10038", MultiMessage.Count().ToString());
                            foreach (MessageField prop in SingleMessage)
                            {
                                msg.SetStringProperty(prop.Name, prop.Value);
                            }
                            long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                            _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
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
                        Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendMessage() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }

        public void SendAsynMessage(string MessageIDTag, List<List<MessageField>> MultiMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0)
        {
            ThreadStart SendThreadStart = new ThreadStart(
                delegate ()
                {
                    lock (this)
                    {
                        this._MessageID = System.Guid.NewGuid().ToString();
                        //註解發送筆數資訊
                        //SendCountMessage(MessageIDTag, _MessageID, MultiMessage.Count);
                        SendAsyn(_Session, MessageIDTag, MultiMessage, DelayedPerWhenNumber, DelayedMillisecond);
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
                            var buffer = new byte[1048576];
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
                    Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>(string.Format("Sending a file(filename：{0})", FileName));
                    _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendFile() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public bool SendFileByChunks(string FileName, string FilePath, string ID = "")
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                if (!_Session.IsClosed)
                {
                    using (StreamReader sr = new StreamReader(FilePath))
                    {
                        byte[] bytes = new byte[1048576];
                        var bytesRead = default(int);
                        long seq = 0;
                        long totalSequence = sr.BaseStream.Length % bytes.Length > 0 ? (sr.BaseStream.Length / bytes.Length) + 1 : (sr.BaseStream.Length / bytes.Length);
                        while ((bytesRead = sr.BaseStream.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            seq++;
                            if (seq == totalSequence) bytes = bytes.Take(bytesRead).ToArray();
                            BytesMessage msg = _Session.CreateBytesMessage();
                            msg.WriteBytes(bytes);
                            msg.SetStringProperty("id", ID);
                            msg.SetStringProperty("filename", FileName);
                            msg.SetStringProperty("sequence", seq.ToString());
                            msg.SetStringProperty("totalSequence", totalSequence.ToString());
                            long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                            Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>(string.Format("Sending a file by chunks(filename：{0}，sequence：{1}，totalSequence：{2})", FileName, seq.ToString(), totalSequence.ToString()));
                            _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                            isSend = true;
                        }
                    }
                }

                //SendFileByChunks(FileName, File.ReadAllBytes(FilePath), ID);
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendFileByChunks() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
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
                    Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>(string.Format("Sending a file(filename：{0})", FileName));
                    _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendFile() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public bool SendFileByChunks(string FileName, byte[] FileBytes, string ID = "")
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                if (!_Session.IsClosed)
                {
                    int buffer = 1048576;
                    byte[] bytes = new byte[buffer];
                    long seq = 0;
                    long totalSequence = FileBytes.Length % buffer > 0 ? (FileBytes.Length / buffer) + 1 : (FileBytes.Length / buffer);
                    for (var i = 0; i < totalSequence; i++)
                    {
                        seq++;
                        if (seq == totalSequence) bytes = FileBytes.Skip(i * buffer).Take(buffer).ToArray();
                        else Array.Copy(FileBytes, i * buffer, bytes, 0, buffer);
                        BytesMessage msg = _Session.CreateBytesMessage();
                        msg.WriteBytes(bytes);
                        msg.SetStringProperty("id", ID);
                        msg.SetStringProperty("filename", FileName);
                        msg.SetStringProperty("sequence", seq.ToString());
                        msg.SetStringProperty("totalSequence", totalSequence.ToString());
                        long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                        Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>(string.Format("Sending a file by chunks(filename：{0}，sequence：{1}，totalSequence：{2})", FileName, seq.ToString(), totalSequence.ToString()));
                        _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                        isSend = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendFileByChunks() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public bool SendBase64File(string FileName, string FilePath, string ID = "")
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
                            var buffer = new byte[1048576];
                            var bytesRead = default(int);
                            while ((bytesRead = sr.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                                memstream.Write(buffer, 0, bytesRead);
                            bytes = memstream.ToArray();
                        }
                    }
                    String base64File = Convert.ToBase64String(bytes);

                    TextMessage msg = _Session.CreateTextMessage();
                    msg.Text = base64File;
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("datatype", Util.GetMimeType(FilePath));
                    msg.SetStringProperty("filename", FileName);
                    msg.MsgType = "file";

                    long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                    _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendFile() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
            return isSend;
        }
        public bool SendBase64File(string FileName, byte[] FileBytes, string ID = "")
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                if (!_Session.IsClosed)
                {
                    String base64File = Convert.ToBase64String(FileBytes);
                    TextMessage msg = _Session.CreateTextMessage();
                    msg.Text = base64File;
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + FileName));
                    msg.SetStringProperty("filename", FileName);
                    msg.MsgType = "file";

                    long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                    _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendFile() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
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
                    _ListenName = ListenName;
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
                                _Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _ListenName + ".durable", null, false);
                                //_Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _Connection.ClientID, null, false);
                            }
                            else
                            {
                                _Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _ListenName + ".durable", _Selector, false);
                                //_Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _Connection.ClientID, _Selector, false);
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
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(exception);
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
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(exception);
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
                else
                {
                    Close();
                }
            }
            catch (Exception exception)
            {
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(exception);
            }
        }

        protected void SendAsyn(Session Session, string MessageIDTag, List<List<MessageField>> MultiMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0)
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
                        foreach (List<MessageField> SingleMessage in MultiMessage)
                        {
                            Message msg = Session.CreateMessage();
                            msg.MsgType = "map";
                            msg.SetStringProperty(MessageIDTag, this._MessageID);
                            //MacAddress(99)
                            if (!string.IsNullOrEmpty(_MacAddress))
                            {
                                msg.SetStringProperty("99", _MacAddress);
                            }
                            //加入總筆數tag
                            msg.SetStringProperty("10038", MultiMessage.Count().ToString());
                            foreach (MessageField prop in SingleMessage)
                            {
                                msg.SetStringProperty(prop.Name, prop.Value);
                            }
                            long MessageOut = _MessageTimeOut == 0 ? Convert.ToInt64(_MessageTimeOut) : Convert.ToInt64(_MessageTimeOut * 24 * 60 * 60 * 1000);
                            _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                            _sendAmounnts += 1;
                            if (DelayedPerWhenNumber > 0 && DelayedMillisecond > 0)
                            {
                                SlowDownProducer(DelayedPerWhenNumber, DelayedMillisecond);
                            }
                        }
                    }
                    else
                    {
                        Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendAsyn() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
            }
            finally
            {
                if (_UISyncContext != null && IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
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
                        _Producer.Send(msg, _DeliveryMode, 9, MessageOut);
                        isSend = true;
                    }
                    else
                    {
                        Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SendCountMessage() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
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
                                _Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _ListenName + ".durable", null, false);
                                //_Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _Connection.ClientID, null, false);
                            }
                            else
                            {
                                _Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _ListenName + ".durable", _Selector, false);
                                //_Consumer = _Session.CreateDurableSubscriber(_Session.CreateTopic(_ListenName), _Connection.ClientID, _Selector, false);
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
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(exception);
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
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(exception);
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
                        Common.LogHelper.Logger.LogInfo<BaseEMSAdapter1>("Network connection or TibcoEMSService Has been closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseEMSAdapter SetHeartBeat() Error(" + ex.Message + ")";
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex, ErrorMsg);
                //System.Environment.Exit(-1);
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
                if (IsDurableConsumer)
                {
                    Restart(_ClientID, IsDurableConsumer);
                }
                else
                {
                    Restart();
                }
                string ConnActiveUrl = Tibems.GetConnectionActiveURL(_Connection);
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(string.Format(ex.Message + "(Connection has performed fault-tolerant switch to {0})", ConnActiveUrl));
            }
            else
            {
                Common.LogHelper.Logger.LogError<BaseEMSAdapter1>(ex);
            }
        }
    }
}
