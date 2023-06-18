using System.Collections.Generic;
using TIBCO.EMS;

namespace Common.LinkLayer
{
    public interface ITibcoEMSAdapter
    {
        /// <summary>
        /// 訊息傳遞模式
        /// </summary>
        MessageDeliveryMode DeliveryMode { get; set; }
        /// <summary>
        /// 憑證檔位置
        /// </summary>
        List<string> CertsPath { get; set; }
        void processMessage(Message message);
    }
}
