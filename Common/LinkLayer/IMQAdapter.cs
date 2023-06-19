using Apache.NMS;
using Common.HandlerLayer;
using System;
using System.Threading;

namespace Common.LinkLayer
{
    public interface IMQAdapter
    {
        event BaseMQAdapter.MessageAsynSendFinishedEventHandler MessageAsynSendFinished;
        event BaseMQAdapter.MessageHandleFinishedEventHandler MessageHandleFinished;

        /// <summary>
        /// 訊息傳遞模式
        /// </summary>
        MsgDeliveryMode DeliveryMode { get; set; }
        /// <summary>
        /// 使用ActiveMQ提供的功能種類(Topic、VirtualTopic、MirroredQueues,若使用ApolloMQ目前僅支援Topic)
        /// </summary>
        DestinationFeature DestinationFeature { get; set; }
        /// <summary>
        /// 觸發事件時是否回到UI Thread
        /// </summary>
        bool IsEventInUIThread { get; set; }
        /// <summary>
        /// 是否是持久消費者
        /// </summary>
        bool IsDurableConsumer { get; set; }
        /// <summary>
        /// 是否使用ssl
        /// </summary>
        bool UseSSL { get; set; }
        /// <summary>
        /// 監聽的主題
        /// </summary>
        string ListenName { get; set; }
        /// <summary>
        /// 使用MirroredQueue時的固定前置詞名稱
        /// </summary>
        string MirroredQueuePrefix { get; set; }
        /// <summary>
        /// 發送訊息的ID(GUID型式)
        /// </summary>
        string MessageID { get; set; }
        /// <summary>
        /// 發送的主題
        /// </summary>
        string SendName { get; set; }
        /// <summary>
        /// Message Broker服務程式的IP位置
        /// </summary>
        string Uri { get; set; }
        /// <summary>
        /// 登入Message Broker的用戶名稱
        /// </summary>
        string UserName { set; }
        /// <summary>
        /// 登入Message Broker的用戶密碼
        /// </summary>
        string PassWord { set; }
        /// <summary>
        /// Message Broker所在的電腦MacAddress
        /// </summary>
        string MacAddress { get; set; }
        /// <summary>
        /// 目前已發送筆數
        /// </summary>
        int CurrentSendAmounts { get; }
        /// <summary>
        /// 是否使用共享的連線
        /// </summary>
        bool UseSharedConnection { get; set; }
        /// <summary>
        /// 傳送端傳送訊息後的資料庫目的動作
        /// </summary>
        DBAction SenderDBAction { get; set; }
        /// <summary>
        /// 接收端接收訊息後的資料庫目的動作
        /// </summary>
        DBAction ReceiverDBAction { get; set; }
        /// <summary>
        /// 使用VirtualTopic時的固定前置詞名稱
        /// </summary>
        string VirtualTopicPrefix { get; set; }
        /// <summary>
        /// 使用VirtualTopic時的訊息消費者數量
        /// </summary>
        int VirtualTopicConsumers { get; set; }
        /// <summary>
        /// 訊息保留在broker上的時間(單位:天)
        /// </summary>
        double MessageTimeOut { get; set; }
        /// <summary>
        /// 訊息篩選
        /// </summary>
        string Selector { get; set; }
        /// <summary>
        /// 訊息接收後保留在記憶體時間(秒)
        /// </summary>
        int ReceivedMessageTimeOut { get; set; }
        /// <summary>
        /// 取得UI執行緒同步上下文
        /// </summary>
        SynchronizationContext UISyncContext { get; }
        TopicTypeHandler Handler { get; set; }
        Type DataType { get; set; }
        /// <summary>
        /// Qurue和Topic時,不須指定任何參數;VirtualTopic需指定第一個參數;Durable Topic則需指定兩個參數;MirrorQueue時若Durable則需指定兩個參數,若不Durable不須指定任何參數
        /// </summary>
        /// <param name="ClientID"></param>
        /// <param name="IsDurableConsumer"></param>
        void Start(string ClientID = "", bool IsDurableConsumer = false);
        void Close();
        void processMessage(Apache.NMS.IMessage message);
        /// <summary>
        /// Qurue和Topic時,不須指定任何參數;VirtualTopic需指定第一個參數;Durable Topic則需指定兩個參數
        /// </summary>
        /// <param name="ClientID"></param>
        /// <param name="IsDurableConsumer"></param>
        void Restart(string ClientID = "", bool IsDurableConsumer = false);
        void RemoveAllEvents();
        bool SendMessage(string RequestTag, System.Collections.Generic.List<MessageField> SingleMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0);
        bool SendMessage(string RequestTag, System.Collections.Generic.List<System.Collections.Generic.List<MessageField>> MultiMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0);
        void SendAsynMessage(string RequestTag, System.Collections.Generic.List<System.Collections.Generic.List<MessageField>> MultiMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0);
        bool SendFile(string FileName, string FilePath, string ID = "");
        bool SendFileByChunks(string FileName, string FilePath, string ID = "");
        bool SendFile(string FileName, byte[] FileBytes, string ID = "");
        bool SendFileByChunks(string FileName, byte[] FileBytes, string ID = "");
        bool SendBase64File(string FileName, string FilePath, string ID = "");
        bool SendBase64File(string FileName, byte[] FileBytes, string ID = "");
        void ReStartListener(string ListenName);
        void ReStartSender(string SendName);
        bool SendMessage(string Text);
        /// <summary>
        /// 關閉共享連線(當UseSharedConnection=true時才有作用)
        /// </summary>
        void CloseSharedConnection();
        /// <summary>
        /// 檢查Message Broker服務是否運作
        /// </summary>
        /// <returns></returns>
        bool CheckMessageBrokerAlive();
    }
}
