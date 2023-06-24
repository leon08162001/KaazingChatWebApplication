using Apache.NMS;

namespace Common.LinkLayer
{
    public interface IActiveMQAdapter
    {
        /// <summary>
        /// 訊息傳遞模式
        /// </summary>
        MsgDeliveryMode DeliveryMode { get; set; }
        /// <summary>
        /// 使用MirroredQueue時的固定前置詞名稱
        /// </summary>
        string MirroredQueuePrefix { get; set; }
        /// <summary>
        /// 使用VirtualTopic時的固定前置詞名稱
        /// </summary>
        string VirtualTopicPrefix { get; set; }
        /// <summary>
        /// 使用VirtualTopic時的訊息消費者數量
        /// </summary>
        int VirtualTopicConsumers { get; set; }
        /// <summary>
        /// 發送訊息緩衝區的大小
        /// </summary>
        int SendBufferSize { get; set; }
        /// <summary>
        /// 接收訊息緩衝區的大小
        /// </summary>
        int ReceiveBufferSize { get; set; }
        void processMessage(IMessage message);
    }
}
