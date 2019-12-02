using Common.HandlerLayer;
using System.Threading;

namespace Common.LinkLayer
{
    public enum DBAction
    {
        Query, Add, Update, Delete, None
    }
    public interface IEMSAdapter
    {
        //event BaseMQAdapter.MQMessageAsynSendFinishedEventHandler MQMessageAsynSendFinished;
        //event BaseMQAdapter.MQMessageHandleFinishedEventHandler MQMessageHandleFinished;

        /// <summary>
        /// 使用Tibco EMS提供的功能種類(Queue和Topic)
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
        /// 發送訊息的ID(GUID型式)
        /// </summary>
        string MessageID { get; set; }
        /// <summary>
        /// 發送的主題
        /// </summary>
        string SendName { get; set; }
        /// <summary>
        /// EMS Broker服務程式的IP位置
        /// </summary>
        string Uri { get; set; }
        /// <summary>
        /// 登入EMS Broker的用戶名稱
        /// </summary>
        string UserName { set; }
        /// <summary>
        /// 登入EMS Broker的用戶密碼
        /// </summary>
        string PassWord { set; }
        /// <summary>
        /// EMSAdapter所在的電腦MacAddress
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
        /// 訊息保留在broker上的時間(單位:天)
        /// </summary>
        double MessageTimeOut { get; set; }
        /// <summary>
        /// 訊息篩選
        /// </summary>
        string Selector { get; set; }
        /// <summary>
        /// 取得UI執行緒同步上下文
        /// </summary>
        SynchronizationContext UISyncContext { get; }

        TopicTypeHandler Handler { get; set; }

        void Start(string ClientID = "", bool IsDurableConsumer = false);
        void Close();
        void processEMSMessage(TIBCO.EMS.Message message);
        void Restart(string ClientID = "", bool IsDurableConsumer = false);
        void RemoveAllEvents();
        bool SendEMSMessage(string RequestTag, System.Collections.Generic.List<MessageField> SingleEMSMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0);
        bool SendEMSMessage(string RequestTag, System.Collections.Generic.List<System.Collections.Generic.List<MessageField>> MultiEMSMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0);
        void SendAsynEMSMessage(string RequestTag, System.Collections.Generic.List<System.Collections.Generic.List<MessageField>> MultiEMSMessage, int DelayedPerWhenNumber = 0, int DelayedMillisecond = 0);
        bool SendFile(string FileName, string FilePath, string ID = "");
        bool SendFile(string FileName, byte[] FileBytes, string ID = "");
        void ReStartListener(string ListenName);
        void ReStartSender(string SendName);
        void SendMessage(string Text);
        /// <summary>
        /// 關閉共享連線(當UseSharedConnection=true時才有作用)
        /// </summary>
        void CloseSharedConnection();
    }
}
