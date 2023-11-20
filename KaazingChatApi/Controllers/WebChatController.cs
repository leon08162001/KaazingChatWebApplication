using Common;
using Common.LinkLayer;
using Common.Utility;
using Dapper;
using KaazingChatWebApplication.Connection;
using KaazingChatWebApplication.Helper;
using KaazingChatWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Net.Sockets;
using System.Reflection;

namespace KaazingChatApi.Controllers
{
    /// <summary>
    /// Message Broker的訊息管道類型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 使用主題管道
        /// </summary>
        Topic = 1,
        /// <summary>
        /// 使用佇列管道
        /// </summary>
        Queue = 2
    }
    /// <summary>
    /// 聊天發送的訊息類型
    /// </summary>
    public enum AjaxMessageType
    {
        /// <summary>
        /// 已讀訊息
        /// </summary>
        read = 1,
        /// <summary>
        /// 已接收訊息
        /// </summary>
        receive = 2,
        /// <summary>
        /// 傳送檔案訊息
        /// </summary>
        file = 3,
        /// <summary>
        /// 傳送串流訊息(視訊用)
        /// </summary>
        stream = 4
    }
    /// <summary>
    /// 聊天內容的日期類型
    /// </summary>
    public enum MessageDate
    {
        /// <summary>
        /// 今日
        /// </summary>
        Today = 1,
        /// <summary>
        /// 過往
        /// </summary>
        History = 2
    }
    /// <summary>
    /// 網頁聊天對話所使用的web api
    /// </summary>
    //[Route("KaazingChatWebApi/api/WebChat")]
    public class WebChatController : ControllerBase
    {
        /// <summary>
        /// 傳送給MQ server的對話訊息類別
        /// </summary>
        public class MessageModel
        {
            /// <summary>
            /// 發送者
            /// </summary>
            public string sender { get; set; }
            /// <summary>
            /// 發送之訊息
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 發送次數
            /// </summary>
            public int times { get; set; }
            /// <summary>
            /// 主題或佇列名稱
            /// </summary>
            public string topicOrQueueName { get; set; }
            /// <summary>
            /// Message Broker的訊息管道類型
            /// </summary>
            public MessageType messageType { get; set; }
            /// <summary>
            /// 聊天發送的訊息類型
            /// </summary>
            public AjaxMessageType ajaxMessageType { get; set; }
            /// <summary>
            /// MQ server服務位置
            /// </summary>
            public string mqUrl { get; set; }
        }
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //IApplicationContext applicationContext = ContextRegistry.GetContext();
        //Config config;
        IWebSocketAdapter JefferiesExcuReport = WebSocketTopicFactory.GetWebSocketAdapterInstance(WebSocketAdapterType.BatchWebSocketAdapter);
        IMQAdapter JefferiesExcuReport1 = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);
        IEMSAdapter JefferiesExcuReport2 = TopicEMSFactory.GetEMSAdapterInstance(EMSAdapterType.BatchEMSAdapter);
        private ConnectionFactory cf = new ConnectionFactory();

        [HttpGet]
        //[Route("Get")]
        public string Get()
        {
            return "Hello World";
        }

        /// <summary>
        /// 傳送聊天對話訊息到MQ server
        /// </summary>
        /// <param name="Msg">傳送給MQ server的聊天對話訊息，類別為MessageModel</param>
        /// <response code="200">傳送聊天對話訊息到MQ server成功，回應MessageId = \"0000\", Message = \"\"。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("SendTalkMessageToServer")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "傳送聊天對話訊息到MQ server成功，回應MessageId = \"0000\", Message = \"\"。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult SendTalkMessageToServer(MessageModel Msg)
        {
            IActionResult? apiResult = null;
            //if (Debugger.IsAttached == false)
            //    Debugger.Launch();
            //config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = Msg.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.UseSSL = Config.IsUseSSL;
            JefferiesExcuReport.DestinationFeature = Msg.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            //JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
            JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
            try
            {
                JefferiesExcuReport.Start();
                //多個人
                if (Msg.topicOrQueueName.IndexOf(",") != -1)
                {
                    string[] sendNames = Msg.topicOrQueueName.Split(new char[] { ',' });
                    foreach (string sendName in sendNames)
                    {
                        JefferiesExcuReport.ReStartSender(sendName.Trim());
                        for (int i = 0; i < Msg.times; i++)
                        {
                            JefferiesExcuReport.SendMessage(Msg.message);
                            if (log.IsInfoEnabled) log.InfoFormat("{0} is sending a message to {1}({2})", Msg.sender, sendName.Split(new char[] { '.' })[1].Trim(), Msg.message);
                        }
                    }
                    if (log.IsInfoEnabled) log.InfoFormat("SendTalkMessageToServer from {0}(Count:{1})", Assembly.GetExecutingAssembly().GetName().Name, Msg.times.ToString());
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.ReStartSender(Msg.topicOrQueueName);
                    for (int i = 0; i < Msg.times; i++)
                    {
                        JefferiesExcuReport.SendMessage(Msg.message);
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
                        if (log.IsInfoEnabled) log.InfoFormat("{0} is sending a message to {1}({2})", Msg.sender, Msg.topicOrQueueName.Split(new char[] { '.' })[1].Trim(), Msg.message);
                    }
                    if (log.IsInfoEnabled) log.InfoFormat("SendTalkMessageToServer from {0}(Count:{1})", Assembly.GetExecutingAssembly().GetName().Name, Msg.times.ToString());
                }
                //test code begin
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = BadRequest(ex.Message);
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }

        /// <summary>
        /// 傳送其他非聊天對話訊息到MQ server
        /// </summary>
        /// <param name="Msg"></param>
        /// <response code="200">傳送其他非聊天對話訊息到MQ server成功，回應MessageId = \"0000\", Message = \"\"。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("SendAjaxMessageToServer")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "傳送其他非聊天對話訊息到MQ server成功，回應MessageId = \"0000\", Message = \"\"。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult SendAjaxMessageToServer(MessageModel Msg)
        {
            ActionResult? apiResult = null;
            //config = (Config)applicationContext.GetObject("Config");
            string ajaxMessageType = Msg.ajaxMessageType == AjaxMessageType.read ? "readed" : Msg.ajaxMessageType == AjaxMessageType.receive ? "received" : Msg.ajaxMessageType == AjaxMessageType.file ? "file" : "stream";
            JefferiesExcuReport.WebSocketUri = Msg.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.UseSSL = Config.IsUseSSL;
            JefferiesExcuReport.DestinationFeature = Msg.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = Msg.topicOrQueueName;
            JefferiesExcuReport.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
            JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
            try
            {
                JefferiesExcuReport.Start();
                //多個人
                if (Msg.topicOrQueueName.IndexOf(",") != -1)
                {
                    string[] sendNames = Msg.topicOrQueueName.Split(new char[] { ',' });
                    foreach (string sendName in sendNames)
                    {
                        JefferiesExcuReport.ReStartSender(sendName.Trim());
                        JefferiesExcuReport.SendMessage(Msg.message);
                        if (log.IsInfoEnabled) log.InfoFormat("{0} is sending a " + ajaxMessageType + " message to {1}({2})", Msg.sender, sendName.Split(new char[] { '.' })[1].Trim(), Msg.message);
                    }
                }
                //只有一個人
                else
                {
                    JefferiesExcuReport.SendMessage(Msg.message);
                    if (log.IsInfoEnabled) log.InfoFormat("{0} is sending a " + ajaxMessageType + " message to {1}({2})", Msg.sender, Msg.topicOrQueueName.Split(new char[] { '.' })[1].Trim(), Msg.message);
                }
                if (log.IsInfoEnabled) log.InfoFormat("SendReadMessageToServer from {0}", Assembly.GetExecutingAssembly().GetName().Name);
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = BadRequest(ex.Message);
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }

        /// <summary>
        /// 傳送其他非聊天對話訊息到MQ server(舊API)
        /// </summary>
        /// <param name="Msg"></param>
        /// <response code="200">傳送已讀訊息到MQ server成功，回應MessageId = \"0000\", Message = \"\"。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("SendReadMessageToServerOld")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "傳送已讀訊息到MQ server成功，回應MessageId = \"0000\", Message = \"\"。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult SendReadMessageToServerOld(MessageModel Msg)
        {
            IActionResult? apiResult = null;
            //config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = Msg.mqUrl.Replace("ws://", "").Replace("wss://", "");
            JefferiesExcuReport.UseSSL = Config.IsUseSSL;
            JefferiesExcuReport.DestinationFeature = Msg.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = Msg.topicOrQueueName;
            JefferiesExcuReport.UserName = AesHelper.AesDecrpt(Config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
            JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(Config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
            try
            {
                JefferiesExcuReport.Start();
                JefferiesExcuReport.SendMessage(Msg.message);
                if (log.IsInfoEnabled) log.InfoFormat("SendReadMessageToServer from {0}", Assembly.GetExecutingAssembly().GetName().Name);
                apiResult = Ok(new { MessageId = "0000", Message = "" });
            }
            catch (Exception ex)
            {
                //throw ex;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = BadRequest(ex.Message);
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }

        /// <summary>
        /// 上傳檔案給MQ Server發送
        /// </summary>
        /// <response code="200">上傳檔案給MQ Server發送成功，回應OK。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("UploadFile")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "上傳檔案給MQ Server發送成功，回應OK。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult UploadFile()
        {
            IActionResult? apiResult = null;
            HttpContext.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Request.Form["sender"].ToString();
                String topicOrQueueName = HttpContext.Request.Form["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Request.Form["messageType"].ToString());
                String mqUrl = HttpContext.Request.Form["mqUrl"].ToString();
                int times = Convert.ToInt32(HttpContext.Request.Form["times"].ToString());
                IFormFileCollection Files = HttpContext.Request.Form.Files;

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
                                using (BinaryReader br = new BinaryReader(Files[i].OpenReadStream(), System.Text.Encoding.UTF8, true))
                                {
                                    br.BaseStream.Position = 0;
                                    byte[] bytes = br.ReadBytes((int)Files[i].Length);
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
                            using (BinaryReader br = new BinaryReader(Files[i].OpenReadStream(), System.Text.Encoding.UTF8, true))
                            {
                                br.BaseStream.Position = 0;
                                byte[] bytes = br.ReadBytes((int)Files[i].Length);
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
                //apiResult = Ok(new { MessageId = "0000", Message = "" });
                apiResult = Ok();
            }
            catch (Exception ex)
            {
                //throw ex;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = BadRequest(ex.Message);
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }

        /// <summary>
        /// 上傳檔案給MQ Server發送(舊API)
        /// </summary>
        /// <response code="200">上傳檔案給MQ Server發送成功，回應OK。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("UploadFile1")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "上傳檔案給MQ Server發送成功，回應OK。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult UploadFile1()
        {
            IActionResult? apiResult = null;
            HttpContext.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Request.Form["sender"].ToString();
                String topicOrQueueName = HttpContext.Request.Form["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Request.Form["messageType"].ToString());
                IFormFileCollection Files = HttpContext.Request.Form.Files;

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
                    byte[] bytes = new byte[Files[i].OpenReadStream().Length];
                    Files[i].OpenReadStream().Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport1.SendFile(Files[i].FileName, bytes, sender);
                    if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                }
                //apiResult = Ok(new { MessageId = "0000", Message = "" });
                apiResult = Ok();
            }
            catch (Exception ex)
            {
                //throw ex;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = BadRequest(ex.Message);
            }
            finally
            {
                JefferiesExcuReport1.Close();
            }
            return apiResult;
        }

        /// <summary>
        /// 上傳檔案給MQ Server發送(舊API)
        /// </summary>
        /// <response code="200">上傳檔案給MQ Server發送成功，回應OK。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("UploadFile2")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "上傳檔案給MQ Server發送成功，回應OK。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult UploadFile2()
        {
            IActionResult? apiResult = null;
            HttpContext.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Request.Form["sender"].ToString();
                String topicOrQueueName = HttpContext.Request.Form["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Request.Form["messageType"].ToString());
                IFormFileCollection Files = HttpContext.Request.Form.Files;

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
                    byte[] bytes = new byte[Files[i].OpenReadStream().Length];
                    Files[i].OpenReadStream().Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport2.SendFile(Files[i].FileName, bytes, sender);
                    if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                }
                //apiResult = Ok(new { MessageId = "0000", Message = "" });
                apiResult = Ok();
            }
            catch (Exception ex)
            {
                //throw ex;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = BadRequest(ex.Message);
            }
            finally
            {
                JefferiesExcuReport2.Close();
            }
            return apiResult;
        }

        /// <summary>
        /// 上傳串流給MQ Server發送
        /// </summary>
        /// <response code="200">上傳串流給MQ Server發送成功，回應OK。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("UploadStream")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "上傳串流給MQ Server發送成功，回應OK。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult UploadStream()
        {
            IActionResult? apiResult = null;
            HttpContext.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Request.Form["sender"].ToString();
                String topicOrQueueName = HttpContext.Request.Form["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Request.Form["messageType"].ToString());
                String mqUrl = HttpContext.Request.Form["mqUrl"].ToString();
                String mimetype = HttpContext.Request.Form["mimetype"].ToString();
                IFormFile? File = HttpContext.Request.Form.Files["stream"];
                String videoName = StringValues.IsNullOrEmpty(HttpContext.Request.Form["videoname"]) ? HttpContext.Request.Form["videoname"].ToString() : "";

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
                            using (BinaryReader br = new BinaryReader(File.OpenReadStream(), System.Text.Encoding.UTF8, true))
                            {
                                br.BaseStream.Position = 0;
                                byte[] bytes = br.ReadBytes((int)File.Length);
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
                        using (BinaryReader br = new BinaryReader(File.OpenReadStream(), System.Text.Encoding.UTF8, true))
                        {
                            br.BaseStream.Position = 0;
                            byte[] bytes = br.ReadBytes((int)File.Length);
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
                //apiResult = Ok(new { MessageId = "0000", Message = "" });
                apiResult = Ok();
            }
            catch (Exception ex)
            {
                //throw ex;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                apiResult = BadRequest(ex.Message);
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }

        /// <summary>
        /// 取得websocket負載平衡主機服務url(舊API)
        /// </summary>
        /// <response code="200">取得websocket負載平衡主機服務url成功，回應單個運行中的負載平衡主機服務url；否則回應空字串。</response>
        [HttpPost]
        //[Route("GetWebSocketLoadBalancerUrlOld")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "取得websocket負載平衡主機服務url成功，回應單個運行中的負載平衡主機服務url；否則回應空字串。")]
        public IActionResult GetWebSocketLoadBalancerUrlOld()
        {
            //config = (Config)applicationContext.GetObject("Config");
            string WebSocketUrl = string.Empty;
            IActionResult? apiResult = null;
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
            apiResult = Ok(new { WebSocketUrl });
            //apiResult = ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, WebSocketUrl));
            return apiResult;
        }

        /// <summary>
        /// 取得websocket負載平衡主機服務url
        /// </summary>
        /// <response code="200">取得websocket負載平衡主機服務url成功，回應 List&lt;string&gt; 型態的多個運行中的負載平衡主機服務url；否則回應空的List&lt;string&gt; 型態。</response>
        [HttpPost]
        //[Route("GetWebSocketLoadBalancerUrl")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "取得websocket負載平衡主機服務url成功，回應 List<string> 型態的多個運行中的負載平衡主機服務url；否則回應空的List<string> 型態。")]
        public IActionResult GetWebSocketLoadBalancerUrl()
        {
            List<string> availWebSocketUrls = new List<string>();
            //config = (Config)applicationContext.GetObject("Config");
            IActionResult? apiResult = null;
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
            apiResult = Ok(new { availWebSocketUrls });
            //apiResult = ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, availWebSocketUrls));
            return apiResult;
        }

        /// <summary>
        /// 更新聊天紀錄
        /// </summary>
        /// <param name="Message">新增或更新至Sql server的聊天對話訊息，類別為Chat</param>
        /// <response code="200">更新聊天紀錄成功，回應MessageId = \"0000\", Message = \"\"。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("ChatUpdate")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "更新聊天紀錄成功，回應MessageId = \"0000\", Message = \"\"。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult ChatUpdate(Chat Message)
        {
            IActionResult? apiResult = null;
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
                apiResult = BadRequest(ex.Message);
            }
            return apiResult;
        }

        /// <summary>
        /// 離開系統時更新聊天紀錄
        /// </summary>
        /// <response code="200">離開系統時更新聊天紀錄成功，回應MessageId = \"0000\", Message = \"\"。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("ChatUpdateWhenExit")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "離開系統時更新聊天紀錄成功，回應MessageId = \"0000\", Message = \"\"。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult ChatUpdateWhenExit()
        {
            IActionResult? apiResult = null;
            try
            {
                Chat Message = new Chat();
                Message.id = HttpContext.Request.Form["id"].ToString();
                Message.name = HttpContext.Request.Form["name"].ToString();
                Message.receiver = HttpContext.Request.Form["receiver"].ToString();
                Message.htmlMessage = HttpContext.Request.Form["htmlMessage"].ToString();
                Message.date = Convert.ToDateTime(HttpContext.Request.Form["date"].ToString());
                Message.oprTime = Convert.ToDateTime(HttpContext.Request.Form["oprTime"].ToString());
                Message.id = HttpContext.Request.Form["id"].ToString();
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
                apiResult = BadRequest(ex.Message);
            }
            return apiResult;
        }

        /// <summary>
        /// 取得今日聊天紀錄
        /// </summary>
        /// <param name="Message">取得Sql server的聊天對話訊息，類別為Chat</param>
        /// <response code="200">取得今日聊天紀錄成功，回應MessageId = \"0000\", Message = \"\"。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("GetChatToday")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "取得今日聊天紀錄成功，回應MessageId = \"0000\", Message = \"\"。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult GetChatToday(Chat Message)
        {
            IActionResult? apiResult = null;
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
                apiResult = BadRequest(ex.Message);
            }
            return apiResult;
        }

        /// <summary>
        /// 取得歷史聊天紀錄
        /// </summary>
        /// <param name="Message">取得Sql server的聊天對話訊息，類別為Chat</param>
        /// <response code="200">取得歷史聊天紀錄成功，回應MessageId = \"0000\", Message = \"\"。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("GetChatHistory")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "取得歷史聊天紀錄成功，回應MessageId = \"0000\", Message = \"\"。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult GetChatHistory(Chat Message)
        {
            IActionResult? apiResult = null;
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
                apiResult = BadRequest(ex.Message);
            }
            return apiResult;
        }

        /// <summary>
        /// 取得所有交談過之朋友清單
        /// </summary>
        /// <param name="Chat">Chat型別</param>
        /// <response code="200">取得所有聊天好友清單成功，回應MessageId = \"0000\", Message = \"\"。</response>
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("GetAllTalkFriends")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Description = "取得所有聊天好友清單成功，回應MessageId = \"0000\", Message = \"\"。")]
        //[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "執行中發生例外錯誤，回應錯誤訊息。")]
        public IActionResult GetAllTalkFriends(Chat Chat)
        {
            IActionResult? apiResult = null;
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
                apiResult = BadRequest(ex.Message);
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
            using (FileStream fs = new FileStream(Path.Combine(folderInfo.FullName, VideoName), System.IO.File.Exists(Path.Combine(folderInfo.FullName, VideoName)) ? FileMode.Append : FileMode.OpenOrCreate))
            {
                fs.Write(streamByteAry, 0, streamByteAry.Length);
            }
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
