using Common.HandlerLayer;
using Common.Utility;
using Kaazing.JMS;
using Kaazing.JMS.Stomp;
using Kaazing.Security;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace Common.LinkLayer
{
    public enum DestinationFeature
    {
        Topic,
        VirtualTopic,
        MirroredQueues,
        Queue
    }

    public struct MessageField
    {
        public string Name;
        public string Value;
    }

    /// <summary>
    ///收到一筆Message並完成資料處理時的事件參數類別
    /// </summary>
    public class MessageHandleFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataRow _MessageRow;
        public MessageHandleFinishedEventArgs(string errorMessage, DataRow MessageRow)
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
    ///非同步發送Message完成時的事件參數類別
    /// </summary>
    public class MessageAsynSendFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        public MessageAsynSendFinishedEventArgs(string errorMessage)
        {
            _errorMessage = errorMessage;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
    }

    public abstract class BaseWebSocketAdapter : Common.LinkLayer.IWebSocketAdapter
    {
        IApplicationContext applicationContext = ContextRegistry.GetContext();
        Config config;
        protected readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected string _WebSocketUri = string.Empty;
        protected string _ListenName = string.Empty;
        protected string _SendName = string.Empty;
        protected string _UserName = string.Empty;
        protected string _PassWord = string.Empty;
        protected string _MacAddress = string.Empty;
        protected DestinationFeature _DestinationFeature = DestinationFeature.Topic;

        protected IConnectionFactory _Factory = null;
        protected IConnection _Connection = null;
        protected ISession _Session = null;
        protected IMessageConsumer _Consumer = null;

        protected IMessageProducer _Producer;

        protected string _MessageID;

        protected SynchronizationContext _UISyncContext;

        protected TopicTypeHandler _Handler = null;

        protected bool _IsEventInUIThread = false;             //觸發事件時是否回到UI Thread預設為false

        protected Timer HeartBeatTimer;
        protected bool _IsUseHeartBeat = false;
        protected int _HeartBeatInterval = 60;

        public delegate void MessageHandleFinishedEventHandler(object sender, MessageHandleFinishedEventArgs e);
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

        public delegate void MessageAsynSendFinishedEventHandler(object sender, MessageAsynSendFinishedEventArgs e);
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

        public string WebSocketUri
        {
            set { _WebSocketUri = value; }
            get { return _WebSocketUri; }
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
        /// <summary>
        /// 是否使用HeartBeat心跳
        /// </summary>
        public bool IsUseHeartBeat
        {
            get { return _IsUseHeartBeat; }
            set { _IsUseHeartBeat = value; }
        }

        /// <summary>
        /// 心跳訊息間隔(秒)
        /// </summary>
         public int HeartBeatInterval
        {
            set { _HeartBeatInterval = value; }
            get { return _HeartBeatInterval; }
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
        public BaseWebSocketAdapter()
        {
        }

        public BaseWebSocketAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            _WebSocketUri = Uri;
            _DestinationFeature = DestinationFeature;
            _ListenName = ListenName;
            _SendName = SendName;
        }
        public BaseWebSocketAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            _WebSocketUri = Uri;
            _DestinationFeature = DestinationFeature;
            _ListenName = ListenName;
            _SendName = SendName;
            _UserName = UserName;
            _PassWord = Pwd;
        }

        public void Start()
        {
            config = (Config)applicationContext.GetObject("Config");
            if (!_WebSocketUri.Equals("") && _WebSocketUri.IndexOf(":") > -1)
            {
                string ip = _WebSocketUri.Substring(0, _WebSocketUri.IndexOf(":"));
                _MacAddress = Util.GetMacAddress(ip);
            }
            // Example connection strings:
            // ems:tcp://tibcohost:7222
            Uri connecturi = config.IsUseSSL ? new Uri("wss://" + _WebSocketUri) : new Uri("ws://" + _WebSocketUri);
            try
            {
                if (_Connection == null)
                {
                    //setup ChallengeHandler to handler Basic/Application Basic authentications
                    BasicChallengeHandler basicHandler = ChallengeHandlers.Load<BasicChallengeHandler>(typeof(BasicChallengeHandler));
                    basicHandler.LoginHandler = new KaazingSocketLogin(_UserName, _PassWord);
                    ChallengeHandlers.Default = basicHandler;

                    StompConnectionProperties SCP = new StompConnectionProperties();
                    SCP._connectionTimeout = 0;
                    _Factory = new StompConnectionFactory(connecturi, SCP);
                    try
                    {
                        //if (_UserName != "" && _PassWord != "")
                        //{
                        //    _Connection = _Factory.CreateConnection(_UserName, _PassWord);
                        //}
                        //else
                        //{
                        //    _Connection = _Factory.CreateConnection();
                        //}
                        _Connection = _Factory.CreateConnection();
                    }
                    catch (Kaazing.JMS.JMSException ex)
                    {
                        string LoginMsg = (basicHandler.LoginHandler as KaazingSocketLogin).LoginMsg;
                        if (LoginMsg != "")
                        {
                            if (log.IsErrorEnabled) log.ErrorFormat("BaseWebSocketAdapter Start() Error({0})", LoginMsg);
                            throw new Exception(LoginMsg);
                        }
                        else
                        {
                            if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter Start() Error", ex);
                            throw ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter Start() Error", ex);
                        throw ex;
                    }
                    try
                    {
                        _Connection.Start();
                    }
                    catch (Kaazing.JMS.JMSException ex)
                    {
                        if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter Start() Error", ex);
                        throw ex;
                    }
                    _Session = _Connection.CreateSession(false, SessionConstants.AUTO_ACKNOWLEDGE);
                    StartListener();
                    StartSender();
                    _UISyncContext = SynchronizationContext.Current;
                    if (_IsUseHeartBeat)
                    {
                        InitialHeartBeat();
                    }
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter Start() Error", ex);
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

                if (_Connection != null)
                {
                    //_Connection.Stop();
                    _Connection.Close();
                    _Connection = null;
                }
                if (_IsUseHeartBeat)
                {
                    EndHeartBeat();
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter Close() Error", ex);
                throw ex;
            }
        }

        public void Restart()
        {
            Close();
            Start();
            if (_IsUseHeartBeat)
            {
                InitialHeartBeat();
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

        public abstract void processMessage(IMessage message);
        public void SendMessage(string Text)
        {
            string ErrorMsg = "";
            try
            {
                if (_Session != null)
                {
                    ITextMessage msg = _Session.CreateTextMessage();
                    msg.JMSType = "text";
                    msg.Text = Text;
                    try
                    {
                        if (_Producer == null)
                        {
                            return;
                        }
                        _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, 9, 0);
                    }
                    catch (Kaazing.JMS.JMSException ex)
                    {
                        throw ex;
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorMsg = "BaseWebSocketAdapter SendMessage() Error(" + exception.Message + ")";
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter SendMessage() Error", exception);
            }
            finally
            {
                if (_UISyncContext != null & IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
        }
        public void SendMessage(string MessageIDTag, List<MessageField> SingleMessage)
        {
            string ErrorMsg = "";
            try
            {
                this._MessageID = System.Guid.NewGuid().ToString();
                int TestTag;
                if (int.TryParse(MessageIDTag, out TestTag))
                {
                    MessageIDTag = "N" + MessageIDTag;
                }
                //SendCountMessage(MessageIDTag, _MessageID, 1);
                if (_Session != null)
                {
                    IMessage msg = _Session.CreateMessage();
                    msg.SetStringProperty(MessageIDTag, this._MessageID);
                    //MacAddress(N99)
                    if (!_MacAddress.Equals(""))
                    {
                        msg.SetStringProperty("N99", _MacAddress);
                    }
                    //加入總筆數tag
                    msg.SetStringProperty("N10038", "1");
                    foreach (MessageField prop in SingleMessage)
                    {
                        string PropName = prop.Name;
                        if (int.TryParse(PropName, out TestTag))
                        {
                            PropName = "N" + PropName;
                        }
                        msg.SetStringProperty(PropName, prop.Value);
                    }
                    try
                    {
                        if(_Producer == null)
                        {
                            return;
                        }
                        _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, 9, 0);
                    }
                    catch (Kaazing.JMS.JMSException ex)
                    {
                        throw ex;
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorMsg = "BaseWebSocketAdapter SendMessage() Error(" + exception.Message + ")";
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter SendMessage() Error", exception);
            }
            finally
            {
                if (_UISyncContext != null & IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
        }
        public void SendMessage(string MessageIDTag, List<List<MessageField>> MultiMessage)
        {
            string ErrorMsg = "";
            try
            {
                this._MessageID = System.Guid.NewGuid().ToString();
                int TestTag;
                if (int.TryParse(MessageIDTag, out TestTag))
                {
                    MessageIDTag = "N" + MessageIDTag;
                }
                //SendCountMessage(MessageIDTag, _MessageID, MultiMessage.Count);
                if (_Session != null)
                {
                    foreach (List<MessageField> SingleMessage in MultiMessage)
                    {
                        if (_Session == null)
                        {
                            break;
                        }
                        IMessage msg = _Session.CreateMessage();
                        msg.SetStringProperty(MessageIDTag, this._MessageID);
                        //MacAddress(N99)
                        if (!_MacAddress.Equals(""))
                        {
                            msg.SetStringProperty("N99", _MacAddress);
                        }
                        //加入總筆數tag
                        msg.SetStringProperty("N10038", MultiMessage.Count().ToString());
                        foreach (MessageField prop in SingleMessage)
                        {
                            string PropName = prop.Name;
                            if (int.TryParse(PropName, out TestTag))
                            {
                                PropName = "N" + PropName;
                            }
                            msg.SetStringProperty(PropName, prop.Value);
                        }
                        try
                        {
                            if (_Producer == null)
                            {
                                return;
                            }
                            _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, 9, 0);
                        }
                        catch (Kaazing.JMS.JMSException ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorMsg = "BaseWebSocketAdapter SendMessage() Error(" + exception.Message + ")";
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter SendMessage() Error", exception);
            }
            finally
            {
                if (_UISyncContext != null & IsEventInUIThread)
                {
                    _UISyncContext.Post(OnMessageSendFinished, new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
                else
                {
                    OnMessageSendFinished(new MessageAsynSendFinishedEventArgs(ErrorMsg));
                }
            }
        }
        public void SendAsynMessage(string MessageIDTag, List<List<MessageField>> MultiMessage)
        {
            ThreadStart SendThreadStart = new ThreadStart(
                delegate()
                {
                    lock (this)
                    {
                        this._MessageID = System.Guid.NewGuid().ToString();
                        int TestTag;
                        if (int.TryParse(MessageIDTag, out TestTag))
                        {
                            MessageIDTag = "N" + MessageIDTag;
                        }
                        //SendCountMessage(MessageIDTag, _MessageID, MultiMessage.Count);
                        SendAsyn(_Session, MessageIDTag, MultiMessage);
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
                if (_Producer != null)
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
                    IBytesMessage msg = _Session.CreateBytesMessage();
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("filename", FileName);
                    msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + FileName));
                    msg.JMSType = "file";
                    msg.WriteBytes(bytes);
                    _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, MessageConstants.DEFAULT_DELIVERY_MODE, 0);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseWebSocketAdapter SendFile: Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
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
        public bool SendFile(string FileName, string FilePath, int BufferSize, string ID = "")
        {
            bool isSend = false;
            string ErrorMsg = "";
            byte[] buffer = new byte[BufferSize];
            try
            {
                if (_Producer != null)
                {
                    using (StreamReader sr = new StreamReader(FilePath))
                    {
                        long sequence = 1;
                        long remaining = sr.BaseStream.Length;
                        byte[] lstBuffer = new byte[remaining % buffer.Length];
                        long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                        while (remaining > 0)
                        {
                            int read = 0;
                            IBytesMessage msg = _Session.CreateBytesMessage();
                            msg.SetStringProperty("sequence", sequence.ToString());
                            msg.SetStringProperty("totalSequence", totalSequence.ToString());
                            msg.SetStringProperty("id", ID);
                            msg.SetStringProperty("filename", FileName);
                            msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + FileName));
                            msg.JMSType = "file";
                            if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                            {
                                read = sr.BaseStream.Read(buffer, 0, buffer.Length);
                                msg.WriteBytes(buffer);
                            }
                            else if (sequence == totalSequence && remaining % buffer.Length > 0)
                            {
                                read = sr.BaseStream.Read(lstBuffer, 0, lstBuffer.Length);
                                msg.WriteBytes(lstBuffer);
                            }

                            _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, MessageConstants.DEFAULT_DELIVERY_MODE, 0);
                            remaining -= read;
                            sequence++;
                        }
                    }
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseWebSocketAdapter SendFile: Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
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
                if (_Producer != null)
                {
                    IBytesMessage msg = _Session.CreateBytesMessage();
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("filename", FileName);
                    msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + FileName));
                    msg.JMSType = "file";
                    msg.WriteBytes(FileBytes);
                    _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, MessageConstants.DEFAULT_DELIVERY_MODE, 0);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseWebSocketAdapter SendFile: Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
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
        public bool SendFile(string FileName, byte[] FileBytes, long Sequence, long TotalSequence, string ID = "")
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                if (_Producer != null)
                {
                    IBytesMessage msg = _Session.CreateBytesMessage();
                    msg.SetStringProperty("sequence", Sequence.ToString());
                    msg.SetStringProperty("totalSequence", TotalSequence.ToString());
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("filename", FileName);
                    msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + FileName));
                    msg.JMSType = "file";
                    msg.WriteBytes(FileBytes);
                    _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, MessageConstants.DEFAULT_DELIVERY_MODE, 0);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseWebSocketAdapter SendFile: Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
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
        public bool SendStream(string StreamName, byte[] StreamBytes, long Sequence, long TotalSequence, string ID = "")
        {
            bool isSend = false;
            string ErrorMsg = "";
            try
            {
                if (_Producer != null)
                {
                    IBytesMessage msg = _Session.CreateBytesMessage();
                    msg.SetStringProperty("sequence", Sequence.ToString());
                    msg.SetStringProperty("totalSequence", TotalSequence.ToString());
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("streamname", StreamName);
                    msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + StreamName));
                    msg.JMSType = "stream";
                    msg.WriteBytes(StreamBytes);
                    _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, MessageConstants.DEFAULT_DELIVERY_MODE, 0);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseWebSocketAdapter SendFile: Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
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
                if (_Producer != null)
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
                    String base64File = Convert.ToBase64String(bytes);
                    ITextMessage msg = _Session.CreateTextMessage();
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("datatype", Util.GetMimeType(FilePath));
                    msg.SetStringProperty("filename", FileName);
                    msg.JMSType = "file";
                    msg.Text = base64File;
                    _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, MessageConstants.DEFAULT_DELIVERY_MODE, 0);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseWebSocketAdapter SendBase64File: Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
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
                if (_Producer != null)
                {
                    String base64File = Convert.ToBase64String(FileBytes);
                    ITextMessage msg = _Session.CreateTextMessage();
                    msg.SetStringProperty("id", ID);
                    msg.SetStringProperty("datatype", Util.GetMimeType(@"C:\" + FileName));
                    msg.SetStringProperty("filename", FileName);
                    msg.JMSType = "file";
                    msg.Text = base64File;
                    _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, MessageConstants.DEFAULT_DELIVERY_MODE, 0);
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "BaseWebSocketAdapter SendBase64File: Error(" + ex.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, ex);
                System.Environment.Exit(-1);
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
                    if (_DestinationFeature == DestinationFeature.Topic)
                    {
                        if (!ListenName.StartsWith("/topic/"))
                        {
                            ListenName = "/topic/" + ListenName;
                        }
                        _Consumer = _Session.CreateConsumer(_Session.CreateTopic(ListenName));
                        _Consumer.MessageListener = new WebSocketMessageHandler(this);
                    }
                    else if (_DestinationFeature == DestinationFeature.Queue)
                    {
                        if (!ListenName.StartsWith("/queue/"))
                        {
                            ListenName = "/queue/" + ListenName + "?consumer.exclusive=true";
                        }
                        _Consumer = _Session.CreateConsumer(_Session.CreateQueue(ListenName));
                        _Consumer.MessageListener = new WebSocketMessageHandler(this);
                    }
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter ReStartListener() Error", exception);
            }
        }

        public void ReStartSender(string SendName)
        {
            try
            {
                if (SendName != "" && _Session != null)
                {
                    if (_DestinationFeature == DestinationFeature.Topic)
                    {
                        if (!SendName.StartsWith("/topic/"))
                        {
                            SendName = "/topic/" + SendName;
                        }
                        _Producer = _Session.CreateProducer(_Session.CreateTopic(SendName));
                    }
                    else if (_DestinationFeature == DestinationFeature.Queue)
                    {
                        if (!SendName.StartsWith("/queue/"))
                        {
                            SendName = "/queue/" + SendName;
                        }
                        _Producer = _Session.CreateProducer(_Session.CreateQueue(SendName));
                    }
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter ReStartSender() Error", exception);
            }
        }

        protected void SendAsyn(ISession Session, string MessageIDTag, List<List<MessageField>> MultiMessage)
        {
            string ErrorMsg = "";
            try
            {
                if (_Session != null)
                {
                    foreach (List<MessageField> SingleMessage in MultiMessage)
                    {
                        if (_Session == null)
                        {
                            break;
                        }
                        IMessage msg = Session.CreateMessage();
                        int TestTag;
                        if (int.TryParse(MessageIDTag, out TestTag))
                        {
                            MessageIDTag = "N" + MessageIDTag;
                        }
                        msg.SetStringProperty(MessageIDTag, this._MessageID);
                        //MacAddress(N99)
                        if (!_MacAddress.Equals(""))
                        {
                            msg.SetStringProperty("N99", _MacAddress);
                        }
                        //加入總筆數tag
                        msg.SetStringProperty("N10038", MultiMessage.Count().ToString());
                        foreach (MessageField prop in SingleMessage)
                        {
                            string PropName = prop.Name;
                            if (int.TryParse(PropName, out TestTag))
                            {
                                PropName = "N" + PropName;
                            }
                            msg.SetStringProperty(PropName, prop.Value);
                        }
                        try
                        {
                            if (_Producer == null)
                            {
                                return;
                            }
                            _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, 9, 0);
                        }
                        catch (Kaazing.JMS.JMSException ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorMsg = "BaseWebSocketAdapter SendAsyn() Error(" + exception.Message + ")";
                if (log.IsErrorEnabled) log.Error(ErrorMsg, exception);
            }
            finally
            {
                if (_UISyncContext != null & IsEventInUIThread)
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
        private void SendCountMessage(string MessageIDTag, string MessageID, int MessageCount)
        {
            if (_Session != null)
            {
                IMessage msg = _Session.CreateMessage();
                List<MessageField> MessageCountRow = new List<MessageField>();
                MessageField MessageMessageIDField = new MessageField();
                int TestTag;
                if (int.TryParse(MessageIDTag, out TestTag))
                {
                    MessageIDTag = "N" + MessageIDTag;
                }
                MessageMessageIDField.Name = MessageIDTag;
                MessageMessageIDField.Value = MessageID;
                MessageCountRow.Add(MessageMessageIDField);
                MessageField MessageCountRowField = new MessageField();
                MessageCountRowField.Name = "N10038";
                MessageCountRowField.Value = MessageCount.ToString();
                MessageCountRow.Add(MessageCountRowField);
                foreach (MessageField prop in MessageCountRow)
                {
                    string PropName = prop.Name;
                    if (int.TryParse(PropName, out TestTag))
                    {
                        PropName = "N" + PropName;
                    }
                    msg.SetStringProperty(PropName, prop.Value);
                }
                //MacAddress(N99)
                if (!_MacAddress.Equals(""))
                {
                    msg.SetStringProperty("N99", _MacAddress);
                }
                try
                {
                    if (_Producer == null)
                    {
                        return;
                    }
                    _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, 9, 0);
                }
                catch (Kaazing.JMS.JMSException ex)
                {
                    throw ex;
                }
            }
        }

        private void StartListener()
        {
            try
            {
                if (_ListenName != "" && _Session != null)
                {
                    if (_DestinationFeature == DestinationFeature.Topic)
                    {
                        if (!_ListenName.StartsWith("/topic/"))
                        {
                            _ListenName = "/topic/" + _ListenName;
                        }
                        
                        _Consumer = _Session.CreateConsumer(_Session.CreateTopic(_ListenName));
                        _Consumer.MessageListener = new WebSocketMessageHandler(this);
                    }
                    else if (_DestinationFeature == DestinationFeature.Queue)
                    {
                        if (!_ListenName.StartsWith("/queue/"))
                        {
                            _ListenName = "/queue/" + _ListenName + "?consumer.exclusive=true";
                        }
                        _Consumer = _Session.CreateConsumer(_Session.CreateQueue(_ListenName));
                        _Consumer.MessageListener = new WebSocketMessageHandler(this);
                    }
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter StartListener() Error", exception);
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
                        if (!_SendName.StartsWith("/topic/"))
                        {
                            _SendName = "/topic/" + _SendName;
                        }
                        _Producer = _Session.CreateProducer(_Session.CreateTopic(_SendName));
                    }
                    else if (_DestinationFeature == DestinationFeature.Queue)
                    {
                        if (!_SendName.StartsWith("/queue/"))
                        {
                            _SendName = "/queue/" + _SendName;
                        }
                        _Producer = _Session.CreateProducer(_Session.CreateQueue(_SendName));
                    }
                }
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled) log.Error("BaseWebSocketAdapter StartSender() Error", exception);
            }
        }

        private void InitialHeartBeat()
        {
            TimerCallback TCB = new TimerCallback(state => { SetHeartBeat(); });
            HeartBeatTimer = new Timer(TCB, DateTime.Now, 0, 1000 * _HeartBeatInterval);
        }

        private void EndHeartBeat()
        {
            HeartBeatTimer = null;
        }

        private void SetHeartBeat()
        {
            if (_Producer != null && _Session != null)
            {
                IMessage msg = _Session.CreateMessage();
                //MacAddress(N99)
                if (!_MacAddress.Equals(""))
                {
                    msg.SetStringProperty("N99", _MacAddress);
                }
                msg.SetStringProperty("N0", "HeartBeat");
                try
                {
                    if (_Producer == null)
                    {
                        return;
                    }
                    _Producer.Send(msg, DeliveryModeConstants.NON_PERSISTENT, 9, 0);
                }
                catch (Kaazing.JMS.JMSException ex)
                {
                    throw ex;
                }
            }
        }
    }

    class WebSocketMessageHandler : Kaazing.JMS.IMessageListener
    {
        IWebSocketAdapter _IWebSocketAdapter;

        internal WebSocketMessageHandler(IWebSocketAdapter WebSocketAdapter)
        {
            _IWebSocketAdapter = WebSocketAdapter;
        }

        public void OnMessage(IMessage message)
        {
            _IWebSocketAdapter.processMessage(message);
        }
    }
}
