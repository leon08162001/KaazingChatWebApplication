using Common.LinkLayer;

namespace Common
{
    public enum EMSAdapterType
    {
        GenericEMSAdapter,
        BatchEMSAdapter,
        RequestEMSAdapter
    }

    public class TopicEMSFactory
    {
        public static IEMSAdapter GetEMSAdapterSingleton(EMSAdapterType EMSAdapterType)
        {
            switch (EMSAdapterType)
            {
                case EMSAdapterType.GenericEMSAdapter:
                    return GenericEMSAdapter.getSingleton();
                case EMSAdapterType.BatchEMSAdapter:
                    return BatchEMSAdapter.getSingleton();
                case EMSAdapterType.RequestEMSAdapter:
                    return RequestEMSAdapter.getSingleton();
                default:
                    return BatchEMSAdapter.getSingleton();
            }
        }

        public static IEMSAdapter GetEMSAdapterInstance(EMSAdapterType EMSAdapterType)
        {
            switch (EMSAdapterType)
            {
                case EMSAdapterType.GenericEMSAdapter:
                    return new GenericEMSAdapter();
                case EMSAdapterType.BatchEMSAdapter:
                    return new BatchEMSAdapter();
                case EMSAdapterType.RequestEMSAdapter:
                    return new RequestEMSAdapter();
                default:
                    return new BatchEMSAdapter();
            }
        }
    }
}
