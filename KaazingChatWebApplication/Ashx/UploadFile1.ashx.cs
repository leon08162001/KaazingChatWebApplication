using Common;
using Common.LinkLayer;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace KaazingChatWebApplication
{
    /// <summary>
    /// ChatService11 的摘要描述
    /// </summary>
    public class UploadFile1 : IHttpHandler
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IApplicationContext applicationContext = ContextRegistry.GetContext();
        IMQAdapter JefferiesExcuReport = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);

        public void ProcessRequest(HttpContext context)
        {
            //context.Response.ContentType = "text/plain";
            context.Response.ContentType = "application/octet-stream";
            try
            {
                String sender = HttpContext.Current.Request["sender"].ToString();
                String topicOrQueueName = HttpContext.Current.Request["topicOrQueueName"].ToString();
                MessageType messageType = (MessageType)int.Parse(HttpContext.Current.Request["messageType"].ToString());
                HttpFileCollection Files = HttpContext.Current.Request.Files;

                //Config config = (Config)applicationContext.GetObject("Config");
                JefferiesExcuReport.Uri = Config.KaazingWebSocket_network + ":" + Config.Mq_port;
                JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
                JefferiesExcuReport.SendName = topicOrQueueName;
                JefferiesExcuReport.UserName = Config.KaazingWebSocketUserID;
                JefferiesExcuReport.PassWord = Config.KaazingWebSocketPwd;
                JefferiesExcuReport.UseSSL = Config.IsUseSSL;
                JefferiesExcuReport.Start();
                for (var i = 0; i < Files.Count; i++)
                {
                    byte[] bytes = new byte[Files[i].InputStream.Length];
                    Files[i].InputStream.Read(bytes, 0, bytes.Length);
                    JefferiesExcuReport.SendFile(Files[i].FileName, bytes, sender);
                    if (log.IsInfoEnabled) log.InfoFormat("Send File({0}) from {1}", Files[i].FileName, Assembly.GetExecutingAssembly().GetName().Name);
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}