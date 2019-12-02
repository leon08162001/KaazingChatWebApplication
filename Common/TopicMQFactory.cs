using Common.LinkLayer;

namespace Common
{
    public enum MQAdapterType
    {
        GenericMQAdapter,
        BatchMQAdapter,
        RequestMQAdapter
    }

    public class TopicMQFactory
    {
        public static IMQAdapter GetMQAdapterSingleton(MQAdapterType MQAdapterType)
        {
            switch (MQAdapterType)
            {
                case MQAdapterType.GenericMQAdapter:
                    return GenericMQAdapter.getSingleton();
                case MQAdapterType.BatchMQAdapter:
                    return BatchMQAdapter.getSingleton();
                case MQAdapterType.RequestMQAdapter:
                    return RequestMQAdapter.getSingleton();
                default:
                    return BatchMQAdapter.getSingleton();
            }
        }

        public static IMQAdapter GetMQAdapterInstance(MQAdapterType MQAdapterType)
        {
            switch (MQAdapterType)
            {
                case MQAdapterType.GenericMQAdapter:
                    return new GenericMQAdapter();
                case MQAdapterType.BatchMQAdapter:
                    return new BatchMQAdapter();
                case MQAdapterType.RequestMQAdapter:
                    return new RequestMQAdapter();
                default:
                    return new BatchMQAdapter();
            }
        }
    }
}
