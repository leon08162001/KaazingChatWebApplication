using Common;
using Common.LinkLayer;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Web;
using System.Web.Services;

namespace KaazingChatWebService
{
    /// <summary>
    ///ChatService 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://leonlee.com.tw/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    [System.Web.Script.Services.ScriptService]
    public class ChatService1 : System.Web.Services.WebService
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IApplicationContext applicationContext = ContextRegistry.GetContext();
        IMQAdapter JefferiesExcuReport = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);

        //[WebMethod]
        //public bool UploadFile(string fileName, string topicOrQueueName, MessageType messageType)
        //{
        //    HttpPostedFile File = HttpContext.Current.Request.Files["fileStream"];
        //    Config config = (Config)applicationContext.GetObject("Config");
        //    bool SendFileResult;
        //    JefferiesExcuReport.WebSocketUri = config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service + "/jms";
        //    JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
        //    JefferiesExcuReport.SendName = topicOrQueueName;
        //    JefferiesExcuReport.UserName = config.KaazingWebSocketUserID;
        //    JefferiesExcuReport.PassWord = config.KaazingWebSocketPwd;
        //    try
        //    {
        //        byte[] bytes = new byte[File.InputStream.Length];
        //        File.InputStream.Read(bytes, 0, bytes.Length);
        //        JefferiesExcuReport.Start();
        //        JefferiesExcuReport.SendBase64File(fileName, bytes);
        //        if (log.IsInfoEnabled) log.Info("Send JefferiesExcuReport Text Message from KaazingChatWebService");
        //        SendFileResult = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        SendFileResult = false;
        //        if (log.IsErrorEnabled) log.Error(ex.Message, ex);
        //        throw ex;
        //    }
        //    finally
        //    {
        //        JefferiesExcuReport.Close();
        //    }
        //    return SendFileResult;
        //}

        //[WebMethod]
        //public bool UploadFile(string fileName, string topicOrQueueName, MessageType messageType, String fileStream)
        //{
        //    Config config = (Config)applicationContext.GetObject("Config");
        //    bool SendFileResult;
        //    JefferiesExcuReport.WebSocketUri = config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service + "/jms";
        //    JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
        //    JefferiesExcuReport.SendName = topicOrQueueName;
        //    JefferiesExcuReport.UserName = config.KaazingWebSocketUserID;
        //    JefferiesExcuReport.PassWord = config.KaazingWebSocketPwd;
        //    try
        //    {
        //        byte[] bytes = Convert.FromBase64String(fileStream);
        //        JefferiesExcuReport.Start();
        //        JefferiesExcuReport.SendBase64File(fileName, bytes);
        //        if (log.IsInfoEnabled) log.Info("Send JefferiesExcuReport Text Message from KaazingChatWebService");
        //        SendFileResult = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        SendFileResult = false;
        //        if (log.IsErrorEnabled) log.Error(ex.Message, ex);
        //        throw ex;
        //    }
        //    finally
        //    {
        //        JefferiesExcuReport.Close();
        //    }
        //    return SendFileResult;
        //}

        [WebMethod]
        public bool UploadFile()
        {
            bool SendFileResult;
            try
            {
                String topicOrQueueName = HttpContext.Current.Request["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Current.Request["messageType"].ToString());
                HttpFileCollection Files = HttpContext.Current.Request.Files;

                Config config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport.Uri = config.KaazingWebSocket_network + ":" + config.Mq_port;
                JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport.SendName = topicOrQueueName;
                JefferiesExcuReport.UserName = config.KaazingWebSocketUserID;
                JefferiesExcuReport.PassWord = config.KaazingWebSocketPwd;
                JefferiesExcuReport.UseSSL = config.IsUseSSL;

                for (var i = 0; i < Files.Count; i++)
                {
                    byte[] bytes = new byte[Files[i].InputStream.Length];
                    Files[i].InputStream.Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport.Start();
                    JefferiesExcuReport.SendBase64File(Files[i].FileName, bytes);
                    if (log.IsInfoEnabled) log.Info("Send JefferiesExcuReport Text Message from KaazingChatWebService");
                }
                SendFileResult = true;
            }
            catch (Exception ex)
            {
                SendFileResult = false;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return SendFileResult;
        }
    }
}
