using Common;
using Common.LinkLayer;
using Common.Utility;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace KaazingChatWebApi.Controllers
{
    /// <summary>
    /// 訊息類型
    /// </summary>
    public enum MessageType
    {
        Topic = 1,
        Queue = 2
    }
    /// <summary>
    /// Web聊天室Api
    /// </summary>
    [RoutePrefix("api/WebChat")]
    public class WebChatController : ApiController
    {
        /// <summary>
        /// 訊息模型
        /// </summary>
        public class MessageModel
        {
            /// <summary>
            /// 訊息內容
            /// </summary>
            public string message { get;set;}
            /// <summary>
            /// 訊息發送次數
            /// </summary>
            public int times { get; set; }
            /// <summary>
            /// 主題名稱或佇列名稱
            /// </summary>
            public string topicOrQueueName { get; set; }
            /// <summary>
            /// 訊息類型(主題或佇列)
            /// </summary>
            public MessageType messageType { get; set; }

        }
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IApplicationContext applicationContext = ContextRegistry.GetContext();
        IWebSocketAdapter JefferiesExcuReport = WebSocketTopicFactory.GetWebSocketAdapterInstance(WebSocketAdapterType.BatchWebSocketAdapter);
        IMQAdapter JefferiesExcuReport1 = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);
        IEMSAdapter JefferiesExcuReport2 = TopicEMSFactory.GetEMSAdapterInstance(EMSAdapterType.BatchEMSAdapter);
        /// <summary>
        /// Get測試用
        /// </summary>
        /// <returns></returns>
        [Route("Get")]
        public string Get()
        {
            return "Hello World";
        }
        /// <summary>
        /// 使用Websocket adapter傳送發送者聊天訊息給MQ broker後再傳給收訊者
        /// </summary>
        /// <param name="Message">發送者輸入的聊天訊息</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendTalkMessageToServer")]
        public IHttpActionResult SendTalkMessageToServer(MessageModel Message)
        {
            IHttpActionResult apiResult = null;
            //if (Debugger.IsAttached == false)
            //    Debugger.Launch();
            Config config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service + "/jms";
            //JefferiesExcuReport.WebSocketUri = config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service;
            JefferiesExcuReport.DestinationFeature = Message.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = AesHelper.AesDecrpt(config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
            JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
            try
            {
                JefferiesExcuReport.Start();

                //test code begin
                for (int i = 0; i < Message.times; i++)
                {
                    JefferiesExcuReport.SendMessage(Message.message);
                    if (log.IsInfoEnabled) log.InfoFormat("Send JefferiesExcuReport Text Message from{0}(Count:{1})", Assembly.GetExecutingAssembly().GetName().Name, (i+1).ToString());
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
        /// <summary>
        /// 使用Websocket adapter傳送收訊者已讀訊息給MQ broker再傳給發送者
        /// </summary>
        /// <param name="Message">已讀訊息</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendReadMessageToServer")]
        public IHttpActionResult SendReadMessageToServer(MessageModel Message)
        {
            IHttpActionResult apiResult = null;
            Config config = (Config)applicationContext.GetObject("Config");
            JefferiesExcuReport.WebSocketUri = config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service + "/jms";
            JefferiesExcuReport.DestinationFeature = Message.messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = Message.topicOrQueueName;
            JefferiesExcuReport.UserName = AesHelper.AesDecrpt(config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
            JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
            try
            {
                JefferiesExcuReport.Start();
                JefferiesExcuReport.SendMessage(Message.message);
                if (log.IsInfoEnabled) log.InfoFormat("Send JefferiesExcuReport Text Message from {0}", Assembly.GetExecutingAssembly().GetName().Name);
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
        /// <summary>
        /// 使用Websocket adapter傳送發送者上傳檔案給MQ broker後再傳給收訊者
        /// </summary>
        /// <returns></returns>
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
                HttpFileCollection Files = HttpContext.Current.Request.Files;

                Config config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport.WebSocketUri = config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service + "/jms";
                JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport.SendName = topicOrQueueName;
                JefferiesExcuReport.UserName = AesHelper.AesDecrpt(config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport.PassWord = AesHelper.AesDecrpt(config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport.Start();
                for (var i = 0; i < Files.Count; i++)
                {
                    byte[] bytes = new byte[Files[i].InputStream.Length];
                    Files[i].InputStream.Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport.SendFile(Files[i].FileName, bytes, sender);
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
                JefferiesExcuReport.Close();
            }
            return apiResult;
        }
        /// <summary>
        /// 使用ActiveMQ adapter傳送發送者上傳檔案給MQ broker後再傳給收訊者
        /// </summary>
        /// <returns></returns>
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

                Config config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport1.Uri = config.KaazingWebSocket_network + ":" + config.Mq_port;
                JefferiesExcuReport1.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport1.SendName = topicOrQueueName;
                JefferiesExcuReport1.UserName = AesHelper.AesDecrpt(config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport1.PassWord = AesHelper.AesDecrpt(config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
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
        /// <summary>
        /// 使用TibocEMS adapter傳送發送者上傳檔案給MQ broker後再傳給收訊者
        /// </summary>
        /// <returns></returns>
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

                Config config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport2.Uri = config.KaazingWebSocket_network + ":" + config.Ems_port;
                JefferiesExcuReport2.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport2.SendName = topicOrQueueName;
                JefferiesExcuReport2.UserName = AesHelper.AesDecrpt(config.KaazingWebSocketUserID, "taipei-star-bank", "taipei-star-bank");
                JefferiesExcuReport2.PassWord = AesHelper.AesDecrpt(config.KaazingWebSocketPwd, "taipei-star-bank", "taipei-star-bank");
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
    }
}
