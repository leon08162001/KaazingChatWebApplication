using Common;
using Common.LinkLayer;
//using KaazingChatWebService.Utility;
using Newtonsoft.Json;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Services;

namespace KaazingChatWebApplication
{
    public enum MessageType
    {
        Topic = 1,
        Queue = 2
    }
    /// <summary>
    ///ChatService 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    [System.Web.Script.Services.ScriptService]
    public class ChatService : System.Web.Services.WebService
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IApplicationContext applicationContext = ContextRegistry.GetContext();
        IWebSocketAdapter JefferiesExcuReport = WebSocketTopicFactory.GetWebSocketAdapterInstance(WebSocketAdapterType.BatchWebSocketAdapter);

        [WebMethod]
        public bool SendTalkMessageToServer(string message, int times, string topicOrQueueName, MessageType messageType)
        {
            //if (Debugger.IsAttached == false)
            //    Debugger.Launch();

            //Config config = (Config)applicationContext.GetObject("Config");
            bool SendMessageResult;
            JefferiesExcuReport.WebSocketUri = Config.KaazingWebSocket_network + ":" + Config.KaazingWebSocket_service + "/jms";
            //JefferiesExcuReport.WebSocketUri = config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service;
            JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = topicOrQueueName;
            JefferiesExcuReport.UserName = Config.KaazingWebSocketUserID;
            JefferiesExcuReport.PassWord = Config.KaazingWebSocketPwd;
            try
            {
                JefferiesExcuReport.Start();

                //test code begin
                for (int i = 0; i < times; i++)
                {
                    JefferiesExcuReport.SendMessage(message);
                    if (log.IsInfoEnabled) log.InfoFormat("Send JefferiesExcuReport Text Message from {0}(Count:{1})", Assembly.GetExecutingAssembly().GetName().Name, (i + 1).ToString());
                }

                //bool isArrayJson = ClientJS.IsArrayJson(jsonMessage);
                //bool isObjectJson = ClientJS.IsObjectJson(jsonMessage);
                //if (isArrayJson)
                //{
                //    for (int h = 0; h < times; h++)
                //    {
                //        int i = 0;
                //        List<List<MessageField>> MultiMqMessage = new List<List<MessageField>>();
                //        //List<Person> PersonList = JsonConvert.DeserializeObject<List<Person>>(jsonMessage);
                //        List<JefferiesExcuSendTag> JefferiesExcuSendTagList = JsonConvert.DeserializeObject<List<JefferiesExcuSendTag>>(jsonMessage);
                //        //foreach (Person p in PersonList)
                //        foreach (JefferiesExcuSendTag p in JefferiesExcuSendTagList)
                //        {
                //            //string fixString = FixStringBuilder.ConvertToFixString<Person>(p);
                //            string fixString = FixStringBuilder.ConvertToFixString<JefferiesExcuSendTag>(p);
                //            Dictionary<string, string> DicMap = Util.ToMessageMap(fixString, ((char)1).ToString(), "=");
                //            List<MessageField> MessageFields = new List<MessageField>();
                //            //加入資料序號
                //            MessageField MessageSeqenceField = new MessageField();
                //            MessageSeqenceField.Name = "9999";
                //            //MessageSeqenceField.Value = (i + 1).ToString().PadLeft(PersonList.Count().ToString().Length, '0');
                //            MessageSeqenceField.Value = (i + 1).ToString().PadLeft(JefferiesExcuSendTagList.Count().ToString().Length, '0');
                //            MessageFields.Add(MessageSeqenceField);
                //            //加入資料序號
                //            foreach (string Dic in DicMap.Keys)
                //            {
                //                MessageField MessageField = new MessageField();
                //                MessageField.Name = Dic;
                //                MessageField.Value = DicMap[Dic];
                //                MessageFields.Add(MessageField);
                //            }
                //            MultiMqMessage.Add(MessageFields);
                //            i++;
                //        }
                //        JefferiesExcuReport.SendMessage("710", MultiMqMessage);
                //        if (log.IsInfoEnabled) log.InfoFormat("Send JefferiesExcuReport Message from KaazingChatWebService(Count:{0})", i.ToString());
                //    }
                //}
                //if (isObjectJson)
                //{
                //    Person Person = JsonConvert.DeserializeObject<Person>(jsonMessage);
                //    string s = FixStringBuilder.ConvertToFixString<Person>(Person);
                //}
                //test code end
                SendMessageResult = true;
            }
            catch (Exception ex)
            {
                SendMessageResult = false;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return SendMessageResult;
        }
        [WebMethod]
        public bool SendReadMessageToServer(string message, string topicOrQueueName, MessageType messageType)
        {
            //Config config = (Config)applicationContext.GetObject("Config");
            bool SendMessageResult;
            JefferiesExcuReport.WebSocketUri = Config.KaazingWebSocket_network + ":" + Config.KaazingWebSocket_service + "/jms";
            JefferiesExcuReport.DestinationFeature = messageType == MessageType.Topic ? DestinationFeature.Topic : DestinationFeature.Queue;
            JefferiesExcuReport.SendName = topicOrQueueName;
            JefferiesExcuReport.UserName = Config.KaazingWebSocketUserID;
            JefferiesExcuReport.PassWord = Config.KaazingWebSocketPwd;
            try
            {
                JefferiesExcuReport.Start();
                JefferiesExcuReport.SendMessage(message);
                if (log.IsInfoEnabled) log.InfoFormat("Send JefferiesExcuReport Text Message from {0}", Assembly.GetExecutingAssembly().GetName().Name);
                SendMessageResult = true;
            }
            catch (Exception ex)
            {
                SendMessageResult = false;
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
                JefferiesExcuReport.Close();
            }
            return SendMessageResult;
        }
    }
}
