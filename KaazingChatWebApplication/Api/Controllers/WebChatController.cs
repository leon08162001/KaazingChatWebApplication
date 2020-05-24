using Common;
using Common.LinkLayer;
using KaazingChatWebApplication.Connection;
using KaazingChatWebApplication.Models;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Dapper;
using System.Linq;
using KaazingChatWebApplication.Helper;
using System.IO;

namespace KaazingTestWebApplication.Controllers
{
    public enum MessageType
    {
        Topic = 1,
        Queue = 2
    }
    public enum MessageDate
    {
        Today = 1,
        History = 2
    }
    [RoutePrefix("api/WebChat")]
    public class WebChatController : ApiController
    {
        public class MessageModel
        {
            public string message { get; set; }
            public int times { get; set; }
            public string topicOrQueueName { get; set; }
            public MessageType messageType { get; set; }
            public string mqUrl { get; set; }
        }
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IApplicationContext applicationContext = ContextRegistry.GetContext();
        Config config;
        IWebSocketAdapter JefferiesExcuReport = WebSocketTopicFactory.GetWebSocketAdapterInstance(WebSocketAdapterType.BatchWebSocketAdapter);
        IMQAdapter JefferiesExcuReport1 = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);
        IEMSAdapter JefferiesExcuReport2 = TopicEMSFactory.GetEMSAdapterInstance(EMSAdapterType.BatchEMSAdapter);
        private ConnectionFactory cf = new ConnectionFactory();


        [Route("Get")]
        public string Get()
        {
            return "Hello World";
        }
        [HttpPost]
        [Route("SendTalkMessageToServer")]
        public IHttpActionResult SendTalkMessageToServer(MessageModel Message)
        {
            IHttpActionResult apiResult = null;
            //if (Debugger.IsAttached == false)
            //    Debugger.Launch();
            config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = Message.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.DestinationFeature = Message.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            //JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = config.KaazingWebSocketUserID;
            JefferiesExcuReport.PassWord = config.KaazingWebSocketPwd;
            try
            {
                JefferiesExcuReport.Start();
                //多個人
                if (Message.topicOrQueueName.IndexOf(",") != -1)
                {
                    string[] sendNames = Message.topicOrQueueName.Split(new char[] { ',' });
                    foreach (string sendName in sendNames)
                    {
                        JefferiesExcuReport.ReStartSender(sendName.Trim());
                        for (int i = 0; i < Message.times; i++)
                        {
                            JefferiesExcuReport.SendMessage(Message.message);
                            if (log.IsInfoEnabled) log.InfoFormat("SendTalkMessageToServer from {0}(Count:{1})", Assembly.GetExecutingAssembly().GetName().Name, (i + 1).ToString());
                        }
                    }
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.ReStartSender(Message.topicOrQueueName);
                    for (int i = 0; i < Message.times; i++)
                    {
                        JefferiesExcuReport.SendMessage(Message.message);
                        if (log.IsInfoEnabled) log.InfoFormat("SendTalkMessageToServer from {0}(Count:{1})", Assembly.GetExecutingAssembly().GetName().Name, (i + 1).ToString());
                    }
                }
                //test code begin
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }
        [HttpPost]
        [Route("SendReadMessageToServer")]
        public IHttpActionResult SendReadMessageToServer(MessageModel Message)
        {
            IHttpActionResult apiResult = null;
            config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = Message.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.DestinationFeature = Message.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = config.KaazingWebSocketUserID;
            JefferiesExcuReport.PassWord = config.KaazingWebSocketPwd;
            try
            {
                JefferiesExcuReport.Start();
                //多個人
                if (Message.topicOrQueueName.IndexOf(",") != -1)
                {
                    string[] sendNames = Message.topicOrQueueName.Split(new char[] { ',' });
                    foreach (string sendName in sendNames)
                    {
                        JefferiesExcuReport.ReStartSender(sendName.Trim());
                        JefferiesExcuReport.SendMessage(Message.message);
                    }
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.SendMessage(Message.message);
                }
                if (log.IsInfoEnabled) log.InfoFormat("SendReadMessageToServer from {0}", Assembly.GetExecutingAssembly().GetName().Name);
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }
        [HttpPost]
        [Route("SendReadMessageToServerOld")]
        public IHttpActionResult SendReadMessageToServerOld(MessageModel Message)
        {
            IHttpActionResult apiResult = null;
            config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = Message.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.DestinationFeature = Message.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = config.KaazingWebSocketUserID;
            JefferiesExcuReport.PassWord = config.KaazingWebSocketPwd;
            try
            {
                JefferiesExcuReport.Start();
                JefferiesExcuReport.SendMessage(Message.message);
                if (log.IsInfoEnabled) log.InfoFormat("SendReadMessageToServer from {0}", Assembly.GetExecutingAssembly().GetName().Name);
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }
        [HttpPost]
        [Route("UploadFile")]
        public IHttpActionResult UploadFile()
        {
            IHttpActionResult apiResult = null;
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Current.Request["sender"].ToString();
                String topicOrQueueName = HttpContext.Current.Request["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Current.Request["messageType"].ToString());
                String mqUrl = HttpContext.Current.Request["mqUrl"].ToString();
                HttpFileCollection Files = HttpContext.Current.Request.Files;

                config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport.WebSocketUri = mqUrl.Replace("ws://", "").Replace("wss://", "");
                JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                //JefferiesExcuReport.SendName = topicOrQueueName;
                JefferiesExcuReport.UserName = config.KaazingWebSocketUserID;
                JefferiesExcuReport.PassWord = config.KaazingWebSocketPwd;
                JefferiesExcuReport.Start();
                //多個人
                if (topicOrQueueName.IndexOf(",") != -1)
                {
                    string[] sendNames = topicOrQueueName.Split(new char[] { ',' });
                    foreach (string sendName in sendNames)
                    {
                        JefferiesExcuReport.ReStartSender(sendName.Trim());
                        for (var i = 0; i < Files.Count; i++)
                        {
                            long sequence = 1;
                            byte[] buffer = new byte[2048000];
                            int offset = 0;
                            long remaining = Files[i].InputStream.Length;
                            byte[] lstBuffer = new byte[remaining % buffer.Length];
                            long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                            while (remaining > 0)
                            {
                                int read = 0;
                                if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                                {
                                    read = Files[i].InputStream.Read(buffer, 0, buffer.Length);
                                    JefferiesExcuReport.SendFile(Files[i].FileName, buffer, sequence, totalSequence, sender);
                                }
                                else if (sequence == totalSequence && remaining % buffer.Length > 0)
                                {
                                    read = Files[i].InputStream.Read(lstBuffer, 0, lstBuffer.Length);
                                    JefferiesExcuReport.SendFile(Files[i].FileName, lstBuffer, sequence, totalSequence, sender);
                                }
                                remaining -= read;
                                sequence++;
                            }
                            if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                            Files[i].InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                        }
                    }
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.ReStartSender(topicOrQueueName);
                    for (var i = 0; i < Files.Count; i++)
                    {
                        long sequence = 1;
                        byte[] buffer = new byte[2048000];
                        int offset = 0;
                        long remaining = Files[i].InputStream.Length;
                        byte[] lstBuffer = new byte[remaining % buffer.Length];
                        long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                        while (remaining > 0)
                        {
                            int read = 0;
                            if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                            {
                                read = Files[i].InputStream.Read(buffer, 0, buffer.Length);
                                JefferiesExcuReport.SendFile(Files[i].FileName, buffer, sequence, totalSequence, sender);
                            }
                            else if(sequence == totalSequence && remaining % buffer.Length > 0)
                            {
                                read = Files[i].InputStream.Read(lstBuffer, 0, lstBuffer.Length);
                                JefferiesExcuReport.SendFile(Files[i].FileName, lstBuffer, sequence, totalSequence, sender);
                            }
                            remaining -= read;
                            sequence++;
                        }
                        if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                        Files[i].InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                    }
                }
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }
        [HttpPost]
        [Route("UploadFile1")]
        public IHttpActionResult UploadFile1()
        {
            IHttpActionResult apiResult = null;
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Current.Request["sender"].ToString();
                String topicOrQueueName = HttpContext.Current.Request["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Current.Request["messageType"].ToString());
                HttpFileCollection Files = HttpContext.Current.Request.Files;

                config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport1.Uri = config.KaazingWebSocket_network + ":" + config.Mq_port;
                JefferiesExcuReport1.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport1.SendName = topicOrQueueName;
                JefferiesExcuReport1.UserName = config.KaazingWebSocketUserID;
                JefferiesExcuReport1.PassWord = config.KaazingWebSocketPwd;
                JefferiesExcuReport1.UseSSL = config.IsUseSSL;
                JefferiesExcuReport1.Start();
                for (var i = 0; i < Files.Count; i++)
                {
                    byte[] bytes = new byte[Files[i].InputStream.Length];
                    Files[i].InputStream.Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport1.SendFile(Files[i].FileName, bytes, sender);
                    if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                }
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            finally
            {
                JefferiesExcuReport1.Close();
            }
            return apiResult;
        }
        [HttpPost]
        [Route("UploadFile2")]
        public IHttpActionResult UploadFile2()
        {
            IHttpActionResult apiResult = null;
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Current.Request["sender"].ToString();
                String topicOrQueueName = HttpContext.Current.Request["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Current.Request["messageType"].ToString());
                HttpFileCollection Files = HttpContext.Current.Request.Files;

                config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport2.Uri = config.KaazingWebSocket_network + ":" + config.Ems_port;
                JefferiesExcuReport2.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport2.SendName = topicOrQueueName;
                JefferiesExcuReport2.UserName = config.KaazingWebSocketUserID;
                JefferiesExcuReport2.PassWord = config.KaazingWebSocketPwd;
                JefferiesExcuReport2.UseSSL = false;
                JefferiesExcuReport2.Start();
                for (var i = 0; i < Files.Count; i++)
                {
                    byte[] bytes = new byte[Files[i].InputStream.Length];
                    Files[i].InputStream.Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport2.SendFile(Files[i].FileName, bytes, sender);
                    if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                }
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            finally
            {
                JefferiesExcuReport2.Close();
            }
            return apiResult;
        }
        [HttpPost]
        [Route("UploadStream")]
        public IHttpActionResult UploadStream()
        {
            IHttpActionResult apiResult = null;
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Current.Request["sender"].ToString();
                String topicOrQueueName = HttpContext.Current.Request["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Current.Request["messageType"].ToString());
                String mqUrl = HttpContext.Current.Request["mqUrl"].ToString();
                String mimetype = HttpContext.Current.Request["mimetype"].ToString();
                HttpPostedFile File = HttpContext.Current.Request.Files["stream"];
                String videoName = HttpContext.Current.Request["videoname"] != null ? HttpContext.Current.Request["videoname"].ToString() : "";

                config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport.WebSocketUri = mqUrl.Replace("ws://", "").Replace("wss://", "");
                JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                //JefferiesExcuReport.SendName = topicOrQueueName;
                JefferiesExcuReport.UserName = config.KaazingWebSocketUserID;
                JefferiesExcuReport.PassWord = config.KaazingWebSocketPwd;
                JefferiesExcuReport.Start();

                //多個人
                if (topicOrQueueName.IndexOf(",") != -1)
                {
                    string[] sendNames = topicOrQueueName.Split(new char[] { ',' });
                    foreach (string sendName in sendNames)
                    {
                        JefferiesExcuReport.ReStartSender(sendName.Trim());
                        if (File != null)
                        {
                            long sequence = 1;
                            byte[] buffer = new byte[2048000];
                            int offset = 0;
                            long remaining = File.InputStream.Length;
                            byte[] lstBuffer = new byte[remaining % buffer.Length];
                            long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                            while (remaining > 0)
                            {
                                int read = 0;
                                if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                                {
                                    read = File.InputStream.Read(buffer, 0, buffer.Length);
                                    JefferiesExcuReport.SendStream("STREAM." + mimetype.Split(new char[] {'/' })[1], buffer, sequence, totalSequence, sender);
                                }
                                else if (sequence == totalSequence && remaining % buffer.Length > 0)
                                {
                                    read = File.InputStream.Read(lstBuffer, 0, lstBuffer.Length);
                                    JefferiesExcuReport.SendStream("STREAM." + mimetype.Split(new char[] { '/' })[1], lstBuffer, sequence, totalSequence, sender);
                                }
                                remaining -= read;
                                sequence++;
                            }
                            if (log.IsInfoEnabled) log.InfoFormat("Send Stream({0}) from {1}", File.FileName, Assembly.GetExecutingAssembly().GetName().Name);
                            File.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                        }
                    }
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.ReStartSender(topicOrQueueName);
                    if (File != null)
                    {
                        long sequence = 1;
                        byte[] buffer = new byte[2048000];
                        int offset = 0;
                        long remaining = File.InputStream.Length;
                        byte[] lstBuffer = new byte[remaining % buffer.Length];
                        long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                        while (remaining > 0)
                        {
                            int read = 0;
                            if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                            {
                                read = File.InputStream.Read(buffer, 0, buffer.Length);
                                if (!videoName.Equals(""))
                                {
                                    WriteVideoStreamToFile(buffer, videoName);
                                }
                                JefferiesExcuReport.SendStream("STREAM." + mimetype.Split(new char[] { '/' })[1], buffer, sequence, totalSequence, sender);
                            }
                            else if (sequence == totalSequence && remaining % buffer.Length > 0)
                            {
                                read = File.InputStream.Read(lstBuffer, 0, lstBuffer.Length);
                                if (!videoName.Equals(""))
                                {
                                    WriteVideoStreamToFile(lstBuffer, videoName);
                                }
                                JefferiesExcuReport.SendStream("STREAM." + mimetype.Split(new char[] { '/' })[1], lstBuffer, sequence, totalSequence, sender);
                            }
                            remaining -= read;
                            sequence++;
                        }
                        if (log.IsInfoEnabled) log.InfoFormat("Send Stream({0}) from {1}", File.FileName, Assembly.GetExecutingAssembly().GetName().Name);
                        File.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                    }
                }
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }
        private void WriteVideoStreamToFile(byte[] streamByteAry, string VideoName){
            using (FileStream fs = new FileStream(@"D:\VideoStream\" + VideoName, File.Exists(@"D:\VideoStream\" + VideoName) ? FileMode.Append : FileMode.OpenOrCreate)){
                fs.Write(streamByteAry, 0, streamByteAry.Length);
            }
        }
        [HttpPost]
        [Route("GetWebSocketLoadBalancerUrlOld")]
        public IHttpActionResult GetWebSocketLoadBalancerUrlOld()
        {
            config = (Config)applicationContext.GetObject("Config");
            string WebSocketUrl = string.Empty;
            IHttpActionResult apiResult = null;
            try
            {
                List<string> LoadBalancerUrls = new List<string>();
                if (config.KaazingWebSocket_network.IndexOf(",") != -1)
                {
                    foreach (var ip in config.KaazingWebSocket_network.Split(new char[] { ',' }))
                    {
                        LoadBalancerUrls.Add(ip + ":" + config.KaazingWebSocket_service);
                    }
                }
                else
                {
                    LoadBalancerUrls.Add(config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service);
                }
                foreach (var lbUrl in LoadBalancerUrls)
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        string ip = lbUrl.IndexOf(':') != -1 && lbUrl.Split(new char[] { ':' }).Length == 2 ? lbUrl.Split(new char[] { ':' })[0] : "";
                        string port = lbUrl.IndexOf(':') != -1 && lbUrl.Split(new char[] { ':' }).Length == 2 ? lbUrl.Split(new char[] { ':' })[1] : "";
                        int n;
                        bool chkPort = int.TryParse(port, out n);
                        if (!chkPort || (ip.Equals("") || port.Equals("")))
                        {
                            WebSocketUrl = "";
                            break;
                        }
                        try
                        {
                            tcpClient.Connect(ip, Convert.ToInt16(port));
                        }
                        catch (Exception ex)
                        {
                        }
                        if (tcpClient.Connected)
                        {
                            tcpClient.Close();
                            WebSocketUrl = config.IsUseSSL ? "wss://" + ip + ":" + port + "/jms" : "ws://" + ip + ":" + port + "/jms";
                            break;
                        }
                        WebSocketUrl = "";
                    }
                }
            }
            catch (Exception ex)
            {
                WebSocketUrl = "";
            }
            //apiResult = Ok(new { success = true });
            apiResult = ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, WebSocketUrl));
            return apiResult;
        }
        [HttpPost]
        [Route("GetWebSocketLoadBalancerUrl")]
        public IHttpActionResult GetWebSocketLoadBalancerUrl()
        {
            List<string> availWebSocketUrls = new List<string>();
            config = (Config)applicationContext.GetObject("Config");
            IHttpActionResult apiResult = null;
            try
            {
                List<string> LoadBalancerUrls = new List<string>();
                if (config.KaazingWebSocket_network.IndexOf(",") != -1)
                {
                    foreach (var ip in config.KaazingWebSocket_network.Split(new char[] { ',' }))
                    {
                        LoadBalancerUrls.Add(ip + ":" + config.KaazingWebSocket_service);
                    }
                }
                else
                {
                    LoadBalancerUrls.Add(config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service);
                }
                foreach (var lbUrl in LoadBalancerUrls)
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        string ip = lbUrl.IndexOf(':') != -1 && lbUrl.Split(new char[] { ':' }).Length == 2 ? lbUrl.Split(new char[] { ':' })[0] : "";
                        string port = lbUrl.IndexOf(':') != -1 && lbUrl.Split(new char[] { ':' }).Length == 2 ? lbUrl.Split(new char[] { ':' })[1] : "";
                        int n;
                        bool chkPort = int.TryParse(port, out n);
                        if (!chkPort || (ip.Equals("") || port.Equals("")))
                        {
                            break;
                        }
                        try
                        {
                            tcpClient.Connect(ip, Convert.ToInt16(port));
                        }
                        catch (Exception ex)
                        {
                        }
                        if (tcpClient.Connected)
                        {
                            tcpClient.Close();
                            availWebSocketUrls.Add(config.IsUseSSL ? "wss://" + ip + ":" + port + "/jms" : "ws://" + ip + ":" + port + "/jms");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            //apiResult = Ok(new { success = true });
            apiResult = ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, availWebSocketUrls));
            return apiResult;
        }
        [HttpPost]
        [Route("ChatUpdate")]
        public IHttpActionResult ChatUpdate(Chat Message)
        {
            IHttpActionResult apiResult = null;
            try
            {
                var cn = cf.CreateConnection();
                //若這次請求接收者為多人時(以逗號分隔)
                if (Message.receiver.IndexOf(",") != -1)
                {
                    Message.receiver = SortString(Message.receiver, ',');
                    //若快取不存在,需查資料表取得與之前相符的接收者
                    if (!MemoryCacher.Exist("geniuneReceiver"))
                    {
                        string geniuneReceiver = GetGeniuneReceiverBySenderID(Message, MessageDate.Today);
                        Message.receiver = string.IsNullOrEmpty(geniuneReceiver) ? Message.receiver : geniuneReceiver;
                        MemoryCacher.Add("geniuneReceiver", Message.receiver, DateTimeOffset.Now.AddMinutes(30));
                    }
                    //若存在快取,比對快取接收者與這次請求接收者是否相同(不同代表這次請求接收者有所變動需重新查資料表)
                    else
                    {
                        string[] geniuneReceivers = MemoryCacher.GetValue("geniuneReceiver").ToString().Split(new char[] { ',' });
                        string[] messageReceivers = Message.receiver.Split(new char[] { ',' });
                        if (ScrambledEquals<string>(messageReceivers, geniuneReceivers))
                        {
                            Message.receiver = MemoryCacher.GetValue("geniuneReceiver").ToString();
                        }
                        else
                        {
                            string geniuneReceiver = GetGeniuneReceiverBySenderID(Message, MessageDate.Today);
                            Message.receiver = string.IsNullOrEmpty(geniuneReceiver) ? Message.receiver : geniuneReceiver;
                            MemoryCacher.Update("geniuneReceiver", Message.receiver, DateTimeOffset.Now.AddMinutes(30));
                        }
                    }
                }
                var sql = "select count(*) from [dbo].[ChatDialogue] where id=@id and receiver=@receiver and [date]=@date";
                int rowCount = cn.ExecuteScalar<int>(sql, Message);
                if (rowCount == 0)
                {
                    sql = "INSERT INTO [dbo].[ChatDialogue]([id],[name],[receiver],[htmlMessage],[date],[oprTime],[oprIpAddress])" +
                          "values(@id,@name,@receiver,@htmlMessage,@date,@oprTime,@oprIpAddress)";
                }
                else
                {
                    sql = "UPDATE [ChatDialogue] set htmlMessage=@htmlMessage,oprTime=@oprTime,oprIpAddress=@oprIpAddress where id=@id AND receiver=@receiver AND date=@date";
                }
                int iRows = cn.Execute(sql, Message);
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            return apiResult;
        }
        [HttpPost]
        [Route("ChatUpdateWhenExit")]
        public IHttpActionResult ChatUpdateWhenExit()
        {
            IHttpActionResult apiResult = null;
            try
            {
                Chat Message = new Chat();
                Message.id = HttpContext.Current.Request["id"].ToString();
                Message.name = HttpContext.Current.Request["name"].ToString();
                Message.receiver = HttpContext.Current.Request["receiver"].ToString();
                Message.htmlMessage = HttpContext.Current.Request["htmlMessage"].ToString();
                Message.date = Convert.ToDateTime(HttpContext.Current.Request["date"].ToString());
                Message.oprTime = Convert.ToDateTime(HttpContext.Current.Request["oprTime"].ToString());
                Message.id = HttpContext.Current.Request["id"].ToString();
                var cn = cf.CreateConnection();
                //若這次請求接收者為多人時(以逗號分隔)
                if (Message.receiver.IndexOf(",") != -1)
                {
                    Message.receiver = SortString(Message.receiver, ',');
                    //若快取不存在,需查資料表取得與之前相符的接收者
                    if (!MemoryCacher.Exist("geniuneReceiver"))
                    {
                        string geniuneReceiver = GetGeniuneReceiverBySenderID(Message, MessageDate.Today);
                        Message.receiver = string.IsNullOrEmpty(geniuneReceiver) ? Message.receiver : geniuneReceiver;
                        MemoryCacher.Add("geniuneReceiver", Message.receiver, DateTimeOffset.Now.AddMinutes(30));
                    }
                    //若存在快取,比對快取接收者與這次請求接收者是否相同(不同代表這次請求接收者有所變動需重新查資料表)
                    else
                    {
                        string[] geniuneReceivers = MemoryCacher.GetValue("geniuneReceiver").ToString().Split(new char[] { ',' });
                        string[] messageReceivers = Message.receiver.Split(new char[] { ',' });
                        if (ScrambledEquals<string>(messageReceivers, geniuneReceivers))
                        {
                            Message.receiver = MemoryCacher.GetValue("geniuneReceiver").ToString();
                        }
                        else
                        {
                            string geniuneReceiver = GetGeniuneReceiverBySenderID(Message, MessageDate.Today);
                            Message.receiver = string.IsNullOrEmpty(geniuneReceiver) ? Message.receiver : geniuneReceiver;
                            MemoryCacher.Update("geniuneReceiver", Message.receiver, DateTimeOffset.Now.AddMinutes(30));
                        }
                    }
                }
                var sql = "select count(*) from [dbo].[ChatDialogue] where id=@id and receiver=@receiver and [date]=@date";
                int rowCount = cn.ExecuteScalar<int>(sql, Message);
                if (rowCount == 0)
                {
                    sql = "INSERT INTO [dbo].[ChatDialogue]([id],[name],[receiver],[htmlMessage],[date],[oprTime],[oprIpAddress])" +
                          "values(@id,@name,@receiver,@htmlMessage,@date,@oprTime,@oprIpAddress)";
                }
                else
                {
                    sql = "UPDATE [ChatDialogue] set htmlMessage=@htmlMessage,oprTime=@oprTime,oprIpAddress=@oprIpAddress where id=@id AND receiver=@receiver AND date=@date";
                }
                int iRows = cn.Execute(sql, Message);
                apiResult = Ok(new { success = true });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            return apiResult;
        }
        [HttpPost]
        [Route("GetChatToday")]
        public IHttpActionResult GetChatToday(Chat Message)
        {
            IHttpActionResult apiResult = null;
            try
            {
                var cn = cf.CreateConnection();
                //若這次請求接收者為多人時(以逗號分隔)
                if (Message.receiver.IndexOf(",") != -1)
                {
                    Message.receiver = SortString(Message.receiver, ',');
                    //若快取不存在,需查資料表取得與之前相符的接收者
                    if (!MemoryCacher.Exist("geniuneReceiver"))
                    {
                        string geniuneReceiver = GetGeniuneReceiverBySenderID(Message, MessageDate.Today);                       
                        Message.receiver = string.IsNullOrEmpty(geniuneReceiver) ? Message.receiver : geniuneReceiver;
                        MemoryCacher.Add("geniuneReceiver", Message.receiver, DateTimeOffset.Now.AddMinutes(30));
                    }
                    //若存在快取,比對快取接收者與這次請求接收者是否相同(不同代表這次請求接收者有所變動需重新查資料表)
                    else
                    {
                        string[] geniuneReceivers = MemoryCacher.GetValue("geniuneReceiver").ToString().Split(new char[] { ',' });
                        string[] messageReceivers = Message.receiver.Split(new char[] { ',' });
                        if (ScrambledEquals<string>(messageReceivers, geniuneReceivers))
                        {
                            Message.receiver = MemoryCacher.GetValue("geniuneReceiver").ToString();
                        }
                        else
                        {
                            string geniuneReceiver = GetGeniuneReceiverBySenderID(Message, MessageDate.Today);
                            Message.receiver = string.IsNullOrEmpty(geniuneReceiver) ? Message.receiver : geniuneReceiver;
                            MemoryCacher.Update("geniuneReceiver", Message.receiver, DateTimeOffset.Now.AddMinutes(30));
                        }
                    }
                }
                var sql = "select id,htmlMessage,[date] from [dbo].[ChatDialogue] where id=@id and [receiver]=@receiver and [date]=@date order by [date] desc";
                var chatToday = cn.Query<Chat>(sql, Message).ToList();
                apiResult = Ok(chatToday);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            return apiResult;
        }
        [HttpPost]
        [Route("GetChatHistory")]
        public IHttpActionResult GetChatHistory(Chat Message)
        {
            IHttpActionResult apiResult = null;
            try
            {
                var cn = cf.CreateConnection();
                //若這次請求接收者為多人時(以逗號分隔)
                if (Message.receiver.IndexOf(",") != -1)
                {
                    Message.receiver = SortString(Message.receiver, ',');
                    //若快取不存在,需查資料表取得與之前相符的接收者
                    if (!MemoryCacher.Exist("geniuneReceiver"))
                    {
                        string geniuneReceiver = GetGeniuneReceiverBySenderID(Message, MessageDate.History);
                        Message.receiver = string.IsNullOrEmpty(geniuneReceiver) ? Message.receiver : geniuneReceiver;
                        MemoryCacher.Add("geniuneReceiver", Message.receiver, DateTimeOffset.Now.AddMinutes(30));
                    }
                    //若快取存在,比對快取接收者與這次請求接收者是否相同(不同代表這次請求接收者有所變動需重新查資料表)
                    else
                    {
                        string[] geniuneReceivers = MemoryCacher.GetValue("geniuneReceiver").ToString().Split(new char[] { ',' });
                        string[] messageReceivers = Message.receiver.Split(new char[] { ',' });
                        if (ScrambledEquals<string>(messageReceivers, geniuneReceivers))
                        {
                            Message.receiver = MemoryCacher.GetValue("geniuneReceiver").ToString();
                        }
                        else
                        {
                            string geniuneReceiver = GetGeniuneReceiverBySenderID(Message, MessageDate.History);
                            Message.receiver = string.IsNullOrEmpty(geniuneReceiver) ? Message.receiver : geniuneReceiver;
                            MemoryCacher.Update("geniuneReceiver", Message.receiver, DateTimeOffset.Now.AddMinutes(30));
                        }
                    }
                }
                var sql = "select id,htmlMessage,[date] from [dbo].[ChatDialogue] where id=@id and [receiver]=@receiver and [date]<@date order by [date] desc";
                var chatHistory = cn.Query<Chat>(sql, Message).ToList();
                apiResult = Ok(chatHistory);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            return apiResult;
        }
        private string GetGeniuneReceiverBySenderID(Chat Message, MessageDate MessageDate)
        {
            string result = string.Empty;
            if (Message.receiver.IndexOf(",") != -1)
            {
                string sql = string.Empty;
                string GeniuneReceiver = string.Empty;
                var cn = cf.CreateConnection();
                if (MessageDate == MessageDate.Today)
                {
                    sql = "select id,receiver,htmlMessage,[date] from [dbo].[ChatDialogue] where id=@id and [date]=@date order by receiver";
                }
                else if(MessageDate == MessageDate.History)
                {
                    sql = "select id,receiver,htmlMessage,[date] from [dbo].[ChatDialogue] where id=@id and [date]<@date order by receiver";
                }
                var chats = cn.Query<Chat>(sql, Message).ToList();
                string[] messageReceivers = Message.receiver.Split(new char[] { ',' });
                foreach (Chat chat in chats)
                {
                    if (chat.receiver.IndexOf(",") != -1)
                    {
                        string[] chatReceivers = chat.receiver.Split(new char[] { ',' });
                        if(ScrambledEquals<string>(messageReceivers, chatReceivers))
                        {
                            result = chat.receiver;
                            break;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return result;
        }
        private bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach(T s in list1)
            {
                if(cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
        private string SortString(string srcString, char seperator)
        {
            string[] aryString = srcString.Split(new char[] { seperator });
            Array.Sort(aryString, StringComparer.InvariantCultureIgnoreCase);
            return string.Join(",", aryString);
        }
    }
}
