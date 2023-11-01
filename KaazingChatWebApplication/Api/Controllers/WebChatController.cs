using Common;
using Common.LinkLayer;
using Common.Utility;
using Dapper;
using KaazingChatWebApplication.Connection;
using KaazingChatWebApplication.Helper;
using KaazingChatWebApplication.Models;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Services.Description;

namespace KaazingTestWebApplication.Controllers
{
    public enum MessageType
    {
        Topic = 1,
        Queue = 2
    }
    public enum AjaxMessageType
    {
        read = 1,
        receive = 2,
        file = 3,
        stream = 4
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
            public string sender { get; set; }
            public string message { get; set; }
            public int times { get; set; }
            public string topicOrQueueName { get; set; }
            public MessageType messageType { get; set; }
            public AjaxMessageType ajaxMessageType { get; set; }
            public string mqUrl { get; set; }
        }
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //IApplicationContext applicationContext = ContextRegistry.GetContext();
        //Config config;
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
            //config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = Message.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.UseSSL = Config.IsUseSSL;
            JefferiesExcuReport.DestinationFeature = Message.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            //JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
            JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
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
                            if (log.IsInfoEnabled) log.InfoFormat("{0} is sending a message to {1}({2})", Message.sender, sendName.Split(new char[] { '.' })[1].Trim(), Message.message);
                        }
                    }
                    if (log.IsInfoEnabled) log.InfoFormat("SendTalkMessageToServer from {0}(Count:{1})", Assembly.GetExecutingAssembly().GetName().Name, Message.times.ToString());
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.ReStartSender(Message.topicOrQueueName);
                    for (int i = 0; i < Message.times; i++)
                    {
                        JefferiesExcuReport.SendMessage(Message.message);
                        //test code
                        //List<List<MessageField>> messageRows = new List<List<MessageField>>();
                        //for (int j = 0; j < 2; j++)
                        //{
                        //    List<MessageField> row = new List<MessageField>();
                        //    MessageField jmsMessage = new MessageField();
                        //    jmsMessage.Name = "message";
                        //    jmsMessage.Value = Message.message;
                        //    row.Add(jmsMessage);
                        //    messageRows.Add(row);
                        //}
                        //JefferiesExcuReport.SendMessage("ID", messageRows);

                        //List<MessageField> row = new List<MessageField>();
                        //MessageField jmsMessage = new MessageField();
                        //jmsMessage.Name = "message";
                        //jmsMessage.Value = Message.message;
                        //JefferiesExcuReport.SendMessage("ID", row);
                        //test code
                        if (log.IsInfoEnabled) log.InfoFormat("{0} is sending a message to {1}({2})", Message.sender, Message.topicOrQueueName.Split(new char[] { '.' })[1].Trim(), Message.message);
                    }
                    if (log.IsInfoEnabled) log.InfoFormat("SendTalkMessageToServer from {0}(Count:{1})", Assembly.GetExecutingAssembly().GetName().Name, Message.times.ToString());
                }
                //test code begin
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
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
        [Route("SendAjaxMessageToServer")]
        public IHttpActionResult SendAjaxMessageToServer(MessageModel Message)
        {
            IHttpActionResult apiResult = null;
            //config = (Config)applicationContext.GetObject("Config");
            string ajaxMessageType = Message.ajaxMessageType == AjaxMessageType.read ? "readed" : Message.ajaxMessageType == AjaxMessageType.receive ? "received" : Message.ajaxMessageType == AjaxMessageType.file ? "file" : "stream";
            JefferiesExcuReport.WebSocketUri = Message.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.UseSSL = Config.IsUseSSL;
            JefferiesExcuReport.DestinationFeature = Message.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
            JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
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
                        if (log.IsInfoEnabled) log.InfoFormat("{0} is sending a " + ajaxMessageType + " message to {1}({2})", Message.sender, sendName.Split(new char[] { '.' })[1].Trim(), Message.message);
                    }
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.SendMessage(Message.message);
                    if (log.IsInfoEnabled) log.InfoFormat("{0} is sending a " + ajaxMessageType + " message to {1}({2})", Message.sender, Message.topicOrQueueName.Split(new char[] { '.' })[1].Trim(), Message.message);
                }
                if (log.IsInfoEnabled) log.InfoFormat("SendReadMessageToServer from {0}", Assembly.GetExecutingAssembly().GetName().Name);
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
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
            //config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = Message.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.UseSSL = Config.IsUseSSL;
            JefferiesExcuReport.DestinationFeature = Message.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
            JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
            try
            {
                JefferiesExcuReport.Start();
                JefferiesExcuReport.SendMessage(Message.message);
                if (log.IsInfoEnabled) log.InfoFormat("SendReadMessageToServer from {0}", Assembly.GetExecutingAssembly().GetName().Name);
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
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
                int times = Convert.ToInt32(HttpContext.Current.Request["times"].ToString());
                HttpFileCollection Files = HttpContext.Current.Request.Files;

                //config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport.WebSocketUri = mqUrl.Replace("ws://", "").Replace("wss://", "");
                JefferiesExcuReport.UseSSL = Config.IsUseSSL;
                JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                //JefferiesExcuReport.SendName = topicOrQueueName;
                JefferiesExcuReport.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport.Start();
                //多個人
                if (topicOrQueueName.IndexOf(",") != -1)
                {
                    string[] sendNames = topicOrQueueName.Split(new char[] { ',' });
                    foreach (string sendName in sendNames)
                    {
                        JefferiesExcuReport.ReStartSender(sendName.Trim());
                        for (int h = 0; h < times; h++)
                        {
                            for (var i = 0; i < Files.Count; i++)
                            {
                                //舊式傳檔寫法(在此將上傳檔案以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) BEGIN
                                //long sequence = 1;
                                //byte[] buffer = new byte[1048576];
                                //int offset = 0;
                                //long remaining = Files[i].InputStream.Length;
                                //byte[] lstBuffer = new byte[remaining % buffer.Length];
                                //long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                                //while (remaining > 0)
                                //{
                                //    int read = 0;
                                //    if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                                //    {
                                //        read = Files[i].InputStream.Read(buffer, 0, buffer.Length);
                                //        JefferiesExcuReport.SendFile(Files[i].FileName, buffer, sequence, totalSequence, sender);
                                //    }
                                //    else if (sequence == totalSequence && remaining % buffer.Length > 0)
                                //    {
                                //        read = Files[i].InputStream.Read(lstBuffer, 0, lstBuffer.Length);
                                //        JefferiesExcuReport.SendFile(Files[i].FileName, lstBuffer, sequence, totalSequence, sender);
                                //    }
                                //    remaining -= read;
                                //    sequence++;
                                //}
                                //Files[i].InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                                //舊式傳檔寫法(在此將上傳檔案以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) END

                                //新式傳檔寫法(在此將上傳檔案完整丟給處理MQ的相關元件的方法,此方法內部以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) BEGIN
                                using (BinaryReader br = new BinaryReader(Files[i].InputStream, System.Text.Encoding.UTF8, true))
                                {
                                    br.BaseStream.Position = 0;
                                    byte[] bytes = br.ReadBytes(Files[i].ContentLength);
                                    JefferiesExcuReport.SendFileByChunks(Files[i].FileName, bytes, sender);
                                }
                                //新式傳檔寫法(在此將上傳檔案完整丟給處理MQ的相關元件的方法,此方法內部以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) END

                                if (log.IsInfoEnabled)
                                {
                                    log.InfoFormat("{0} is sending a file to {1}({2})", sender, sendName.Split(new char[] { '.' })[1].Trim(), Files[i].FileName);
                                    log.InfoFormat("sending a file({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                                }
                            }
                        }
                    }
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.ReStartSender(topicOrQueueName);
                    for (int h = 0; h < times; h++)
                    {
                        for (var i = 0; i < Files.Count; i++)
                        {
                            //舊式傳檔寫法(在此將上傳檔案以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) BEGIN
                            //long sequence = 1;
                            //byte[] buffer = new byte[1048576];
                            //int offset = 0;
                            //long remaining = Files[i].InputStream.Length;
                            //byte[] lstBuffer = new byte[remaining % buffer.Length];
                            //long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                            //while (remaining > 0)
                            //{
                            //    int read = 0;
                            //    if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                            //    {
                            //        read = Files[i].InputStream.Read(buffer, 0, buffer.Length);
                            //        JefferiesExcuReport.SendFile(Files[i].FileName, buffer, sequence, totalSequence, sender);
                            //    }
                            //    else if (sequence == totalSequence && remaining % buffer.Length > 0)
                            //    {
                            //        read = Files[i].InputStream.Read(lstBuffer, 0, lstBuffer.Length);
                            //        JefferiesExcuReport.SendFile(Files[i].FileName, lstBuffer, sequence, totalSequence, sender);
                            //    }
                            //    remaining -= read;
                            //    sequence++;
                            //}
                            //Files[i].InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                            //舊式傳檔寫法(在此將上傳檔案以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) EMD

                            //新式傳檔寫法(在此將上傳檔案完整丟給處理MQ的相關元件的方法,此方法內部以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) BEGIN
                            using (BinaryReader br = new BinaryReader(Files[i].InputStream, System.Text.Encoding.UTF8, true))
                            {
                                br.BaseStream.Position = 0;
                                byte[] bytes = br.ReadBytes(Files[i].ContentLength);
                                JefferiesExcuReport.SendFileByChunks(Files[i].FileName, bytes, sender);
                            }
                            //新式傳檔寫法(在此將上傳檔案完整丟給處理MQ的相關元件的方法,此方法內部以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) END

                            if (log.IsInfoEnabled)
                            {
                                log.InfoFormat("{0} is sending a file to {1}({2})", sender, topicOrQueueName.Split(new char[] { '.' })[1].Trim(), Files[i].FileName);
                                log.InfoFormat("sending a file({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                            }
                        }
                    }
                }
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
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

                //config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport1.Uri = Config.KaazingWebSocket_network + ":" + Config.Mq_port;
                JefferiesExcuReport1.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport1.SendName = topicOrQueueName;
                JefferiesExcuReport1.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport1.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport1.UseSSL = Config.IsUseSSL;
                JefferiesExcuReport1.Start();
                for (var i = 0; i < Files.Count; i++)
                {
                    byte[] bytes = new byte[Files[i].InputStream.Length];
                    Files[i].InputStream.Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport1.SendFile(Files[i].FileName, bytes, sender);
                    if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                }
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
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

                //config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport2.Uri = Config.KaazingWebSocket_network + ":" + Config.Ems_port;
                JefferiesExcuReport2.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport2.SendName = topicOrQueueName;
                JefferiesExcuReport2.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport2.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport2.UseSSL = false;
                JefferiesExcuReport2.Start();
                for (var i = 0; i < Files.Count; i++)
                {
                    byte[] bytes = new byte[Files[i].InputStream.Length];
                    Files[i].InputStream.Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport2.SendFile(Files[i].FileName, bytes, sender);
                    if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                }
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
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

                //config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport.WebSocketUri = mqUrl.Replace("ws://", "").Replace("wss://", "");
                JefferiesExcuReport.UseSSL = Config.IsUseSSL;
                JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                //JefferiesExcuReport.SendName = topicOrQueueName;
                JefferiesExcuReport.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
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
                            //舊式傳送視訊stream寫法(在此將上傳視訊stream以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) BEGIN
                            //long sequence = 1;
                            //byte[] buffer = new byte[1048576];
                            //int offset = 0;
                            //long remaining = File.InputStream.Length;
                            //byte[] lstBuffer = new byte[remaining % buffer.Length];
                            //long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                            //while (remaining > 0)
                            //{
                            //    int read = 0;
                            //    if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                            //    {
                            //        read = File.InputStream.Read(buffer, 0, buffer.Length);
                            //        JefferiesExcuReport.SendStream("STREAM." + mimetype.Split(new char[] { '/' })[1], buffer, sequence, totalSequence, sender);
                            //    }
                            //    else if (sequence == totalSequence && remaining % buffer.Length > 0)
                            //    {
                            //        read = File.InputStream.Read(lstBuffer, 0, lstBuffer.Length);
                            //        JefferiesExcuReport.SendStream("STREAM." + mimetype.Split(new char[] { '/' })[1], lstBuffer, sequence, totalSequence, sender);
                            //    }
                            //    remaining -= read;
                            //    sequence++;
                            //}
                            //if (log.IsInfoEnabled) log.InfoFormat("Send Stream({0}) from {1}", File.FileName, Assembly.GetExecutingAssembly().GetName().Name);
                            //File.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                            //舊式傳送視訊stream寫法(在此將上傳視訊stream以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) END

                            //新式傳送視訊stream寫法(在此將上傳視訊stream完整丟給處理MQ的相關元件的方法,此方法內部以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) BEGIN
                            using (BinaryReader br = new BinaryReader(File.InputStream, System.Text.Encoding.UTF8, true))
                            {
                                br.BaseStream.Position = 0;
                                byte[] bytes = br.ReadBytes(File.ContentLength);
                                JefferiesExcuReport.SendStreamByChunks("STREAM." + mimetype.Split(new char[] { '/' })[1], bytes, sender);
                            }
                            if (log.IsInfoEnabled) log.InfoFormat("Send Stream({0}) from {1}", File.FileName, Assembly.GetExecutingAssembly().GetName().Name);
                            //新式傳送視訊stream寫法(在此將上傳視訊stream完整丟給處理MQ的相關元件的方法,此方法內部以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) END
                        }
                    }
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.ReStartSender(topicOrQueueName);
                    if (File != null)
                    {
                        //舊式傳送視訊stream寫法(在此將上傳視訊stream以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) BEGIN
                        //long sequence = 1;
                        //byte[] buffer = new byte[1048576];
                        //int offset = 0;
                        //long remaining = File.InputStream.Length;
                        //byte[] lstBuffer = new byte[remaining % buffer.Length];
                        //long totalSequence = remaining % buffer.Length > 0 ? (remaining / buffer.Length) + 1 : (remaining / buffer.Length);
                        //while (remaining > 0)
                        //{
                        //    int read = 0;
                        //    if (sequence < totalSequence || (sequence == totalSequence && remaining % buffer.Length == 0))
                        //    {
                        //        read = File.InputStream.Read(buffer, 0, buffer.Length);
                        //        if (!videoName.Equals(""))
                        //        {
                        //            WriteVideoStreamToFile(config.VideoStreamFileFolder, buffer, videoName);
                        //        }
                        //        JefferiesExcuReport.SendStream("STREAM." + mimetype.Split(new char[] { '/' })[1], buffer, sequence, totalSequence, sender);
                        //    }
                        //    else if (sequence == totalSequence && remaining % buffer.Length > 0)
                        //    {
                        //        read = File.InputStream.Read(lstBuffer, 0, lstBuffer.Length);
                        //        if (!videoName.Equals(""))
                        //        {
                        //            WriteVideoStreamToFile(config.VideoStreamFileFolder, lstBuffer, videoName);
                        //        }
                        //        JefferiesExcuReport.SendStream("STREAM." + mimetype.Split(new char[] { '/' })[1], lstBuffer, sequence, totalSequence, sender);
                        //    }
                        //    remaining -= read;
                        //    sequence++;
                        //}
                        //if (log.IsInfoEnabled) log.InfoFormat("Send Stream({0}) from {1}", File.FileName, Assembly.GetExecutingAssembly().GetName().Name);
                        //File.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                        //舊式傳送視訊stream寫法(在此將上傳視訊stream以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) END

                        //新式傳送視訊stream寫法(在此將上傳視訊stream完整丟給處理MQ的相關元件的方法,此方法內部以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) BEGIN
                        using (BinaryReader br = new BinaryReader(File.InputStream, System.Text.Encoding.UTF8, true))
                        {
                            br.BaseStream.Position = 0;
                            byte[] bytes = br.ReadBytes(File.ContentLength);
                            if (!videoName.Equals(""))
                            {
                                WriteVideoStreamToFile(Config.VideoStreamFileFolder, bytes, videoName);
                            }
                            JefferiesExcuReport.SendStreamByChunks("STREAM." + mimetype.Split(new char[] { '/' })[1], bytes, sender);
                        }
                        if (log.IsInfoEnabled) log.InfoFormat("Send Stream({0}) from {1}", File.FileName, Assembly.GetExecutingAssembly().GetName().Name);
                        //新式傳送視訊stream寫法(在此將上傳視訊stream完整丟給處理MQ的相關元件的方法,此方法內部以拆檔成多個固定大小區塊的資料逐次丟給MQ傳送) END
                    }
                }
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }
        private void WriteVideoStreamToFile(string folderPath, byte[] streamByteAry, string VideoName)
        {
            DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
            if (!folderInfo.Exists)
            {
                folderInfo.Create();
            }
            using (FileStream fs = new FileStream(Path.Combine(folderInfo.FullName, VideoName), File.Exists(Path.Combine(folderInfo.FullName, VideoName)) ? FileMode.Append : FileMode.OpenOrCreate))
            {
                fs.Write(streamByteAry, 0, streamByteAry.Length);
            }
        }
        [HttpPost]
        [Route("GetWebSocketLoadBalancerUrlOld")]
        public IHttpActionResult GetWebSocketLoadBalancerUrlOld()
        {
            //config = (Config)applicationContext.GetObject("Config");
            string WebSocketUrl = string.Empty;
            IHttpActionResult apiResult = null;
            try
            {
                List<string> LoadBalancerUrls = new List<string>();
                if (Config.KaazingWebSocket_network.IndexOf(",") != -1)
                {
                    foreach (var ip in Config.KaazingWebSocket_network.Split(new char[] { ',' }))
                    {
                        LoadBalancerUrls.Add(ip + ":" + Config.KaazingWebSocket_service);
                    }
                }
                else
                {
                    LoadBalancerUrls.Add(Config.KaazingWebSocket_network + ":" + Config.KaazingWebSocket_service);
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
                            WebSocketUrl = Config.IsUseSSL ? "wss://" + ip + ":" + port + "/jms" : "ws://" + ip + ":" + port + "/jms";
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
            apiResult = ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, WebSocketUrl));
            return apiResult;
        }
        [HttpPost]
        [Route("GetWebSocketLoadBalancerUrl")]
        public IHttpActionResult GetWebSocketLoadBalancerUrl()
        {
            List<string> availWebSocketUrls = new List<string>();
            //config = (Config)applicationContext.GetObject("Config");
            IHttpActionResult apiResult = null;
            try
            {
                List<string> LoadBalancerUrls = new List<string>();
                if (Config.KaazingWebSocket_network.IndexOf(",") != -1)
                {
                    foreach (var ip in Config.KaazingWebSocket_network.Split(new char[] { ',' }))
                    {
                        LoadBalancerUrls.Add(ip + ":" + Config.KaazingWebSocket_service);
                    }
                }
                else
                {
                    LoadBalancerUrls.Add(Config.KaazingWebSocket_network + ":" + Config.KaazingWebSocket_service);
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
                            availWebSocketUrls.Add(Config.IsUseSSL ? "wss://" + ip + ":" + port + "/jms" : "ws://" + ip + ":" + port + "/jms");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
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
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
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
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
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
                var sql = "";
                if (DateTime.Today.Equals(Message.date))
                {
                    sql = "select id,htmlMessage,[date] from [dbo].[ChatDialogue] where id=@id and [receiver]=@receiver and [date]<@date order by [date] desc";
                }
                else
                {
                    sql = "select id,htmlMessage,[date] from [dbo].[ChatDialogue] where id=@id and [receiver]=@receiver and [date]<CONVERT(char(10), GetDate(),126) and [date]>=@date order by [date] desc";
                }
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
        [HttpPost]
        [Route("GetAllTalkFriends")]
        public IHttpActionResult GetAllTalkFriends(Chat Chat)
        {
            IHttpActionResult apiResult = null;
            try
            {
                var cn = cf.CreateConnection();
                if (!Chat.id.Trim().Equals(""))
                {
                    var sql = "select distinct receiver from [dbo].[ChatDialogue]" + Environment.NewLine +
                              "where[id] = @id AND(receiver is not null AND receiver<>'')" + Environment.NewLine +
                              "order by receiver";
                    var allTalkFriends = cn.Query<Chat>(sql, Chat).ToList();
                    apiResult = Ok(allTalkFriends);
                }
                else
                {
                    Chat.receiver = "";
                    var allTalkFriends = new List<Chat>();
                    allTalkFriends.Add(Chat);
                    apiResult = Ok(allTalkFriends);
                }
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
                else if (MessageDate == MessageDate.History)
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
                        if (ScrambledEquals<string>(messageReceivers, chatReceivers))
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
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
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
