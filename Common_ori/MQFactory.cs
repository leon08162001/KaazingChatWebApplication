using Common.LinkLayer;

namespace Common
{
    public enum AdapterType
    {
        BatchMQAdapter1,
        RequestMQAdapter1,
        BatchEMSAdapter1,
        RequestEMSAdapter1
    }

    public class MQFactory
    {
        public static IMQAdapter1 GetAdapterSingleton(AdapterType AdapterType)
        {
            switch (AdapterType)
            {
                case AdapterType.BatchMQAdapter1:
                    return BatchMQAdapter1.getSingleton();
                case AdapterType.RequestMQAdapter1:
                    return RequestMQAdapter1.getSingleton();
                case AdapterType.BatchEMSAdapter1:
                    return BatchEMSAdapter1.getSingleton();
                case AdapterType.RequestEMSAdapter1:
                    return RequestEMSAdapter1.getSingleton();
                default:
                    return BatchMQAdapter1.getSingleton();
            }
        }

        public static IMQAdapter1 GetAdapterInstance(AdapterType AdapterType)
        {
            switch (AdapterType)
            {
                case AdapterType.BatchMQAdapter1:
                    return new BatchMQAdapter1();
                case AdapterType.RequestMQAdapter1:
                    return new RequestMQAdapter1();
                case AdapterType.BatchEMSAdapter1:
                    return new BatchEMSAdapter1();
                case AdapterType.RequestEMSAdapter1:
                    return new RequestEMSAdapter1();
                default:
                    return new BatchMQAdapter1();
            }
        }
    }
}
