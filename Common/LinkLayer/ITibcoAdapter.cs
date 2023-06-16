using System;
using System.Threading;
namespace Common.LinkLayer
{
    public interface ITibcoAdapter
    {
        event BaseTibcoAdapter.TibcoMessageAsynSendFinishedEventHandler TibcoMessageAsynSendFinished;
        event BaseTibcoAdapter.TibcoMessageHandleFinishedEventHandler TibcoMessageHandleFinished;

        /// <summary>
        /// 觸發事件時是否回到UI Thread
        /// </summary>
        bool IsEventInUIThread { get; set; }
        /// <summary>
        /// multicast Port
        /// </summary>
        string Service { get; set; }
        /// <summary>
        /// multicast IP
        /// </summary>
        string Network { get; set; }
        /// <summary>
        /// Tibco RVD服務程式IP
        /// </summary>
        string Daemon { get; set; }
        /// <summary>
        /// 監聽的主題
        /// </summary>
        string ListenName { get; set; }
        /// <summary>
        /// 發送訊息的ID(GUID型式)
        /// </summary>
        string MessageID { get; set; }
        /// <summary>
        /// 接收的主題
        /// </summary>
        string SendName { get; set; }
        /// <summary>
        /// 取得UI執行緒同步上下文
        /// </summary>
        SynchronizationContext UISyncContext { get;}

        void Start();
        void Close();
        void RemoveAllEvents();
        void initialize(string name);
        void processTibcoMessage(TIBCO.Rendezvous.Message msg);
        void SendTibcoMessage(string RequestTag, System.Collections.Generic.List<MessageField> SingleTibcoMessage);
        void SendTibcoMessage(string RequestTag, System.Collections.Generic.List<System.Collections.Generic.List<MessageField>> MultiTibcoMessage);
        void SendAsynTibcoMessage(string RequestTag, System.Collections.Generic.List<System.Collections.Generic.List<MessageField>> MultiTibcoMessage);
    }
}
