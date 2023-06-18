﻿using Apache.NMS;

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
        void processMessage(IMessage message);
    }
}
