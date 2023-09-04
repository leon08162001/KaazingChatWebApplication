using Common.HandlerLayer;
using Kaazing.JMS;
using System;
using System.Threading;
namespace Common.LinkLayer
{
    public interface IWebSocketAdapter
    {
        event BaseWebSocketAdapter.MessageAsynSendFinishedEventHandler MessageAsynSendFinished;
        event BaseWebSocketAdapter.MessageHandleFinishedEventHandler MessageHandleFinished;

        /// <summary>
        /// 使用WebSocket提供的功能種類(Queue和Topic)
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
        /// Kaazing WebSocket Server的IP位置
        /// </summary>
        string WebSocketUri { get; set; }
        /// <summary>
        /// 登入WebSocket的用戶名稱
        /// </summary>
        string UserName { set; }
        /// <summary>
        /// 登入WebSocket的用戶密碼
        /// </summary>
        string PassWord { set; }
        /// <summary>
        /// WebSocketAdapter所在的電腦MacAddress
        /// </summary>
        string MacAddress { get; set; }
        /// <summary>
        /// 是否使用HeartBeat心跳
        /// </summary>
        bool IsUseHeartBeat { get; set; }
        /// <summary>
        /// HeartBeat心跳間隔(秒)
        /// </summary>
        int HeartBeatInterval { get; set; }
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

        void Start();
        void Close();
        void processMessage(IMessage message);
        void Restart();
        void RemoveAllEvents();
        void SendMessage(string RequestTag, System.Collections.Generic.List<MessageField> SingleMessage);
        void SendMessage(string RequestTag, System.Collections.Generic.List<System.Collections.Generic.List<MessageField>> MultiMessage);
        void SendAsynMessage(string RequestTag, System.Collections.Generic.List<System.Collections.Generic.List<MessageField>> MultiMessage);
        bool SendFile(string FileName, string FilePath, string ID = "");
        bool SendFileByChunks(string FileName, string FilePath, string ID = "");
        bool SendFile(string FileName, string FilePath, int BufferSize, string ID = "");
        bool SendFile(string FileName, byte[] FileBytes, string ID = "");
        bool SendFileByChunks(string FileName, byte[] FileBytes, string ID = "");
        bool SendFile(string FileName, byte[] FileBytes, long Sequence, long TotalSequence, string ID = "");
        bool SendStream(string StreamName, byte[] StreamBytes, long Sequence, long TotalSequence, string ID = "");
        bool SendStreamByChunks(string StreamName, byte[] StreamBytes, string ID = "");
        bool SendBase64File(string FileName, string FilePath, string ID = "");
        bool SendBase64File(string FileName, byte[] FileBytes, string ID = "");
        void ReStartListener(string ListenName);
        void ReStartSender(string SendName);
        void SendMessage(string Text);
    }
}
