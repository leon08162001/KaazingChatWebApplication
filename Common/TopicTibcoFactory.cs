namespace Common
{
    public enum TibcoAdapterType
    {
        BatchTibcoFixAdapter,
        RequestTibcoFixAdapter
    }

    public class TopicTibcoFactory
    {
        //public static ITibcoAdapter GetTibcoAdapterSingleton(TibcoAdapterType TibcoAdapterType)
        //{
        //    switch (TibcoAdapterType)
        //    {
        //        case TibcoAdapterType.BatchTibcoFixAdapter:
        //            return BatchTibcoFixAdapter.getSingleton();
        //        case TibcoAdapterType.RequestTibcoFixAdapter:
        //            return RequestTibcoFixAdapter.getSingleton();
        //        default:
        //            return BatchTibcoFixAdapter.getSingleton();
        //    }
        //}

        //public static ITibcoAdapter GetTibcoAdapterInstance(TibcoAdapterType TibcoAdapterType)
        //{
        //    switch (TibcoAdapterType)
        //    {
        //        case TibcoAdapterType.BatchTibcoFixAdapter:
        //            return new BatchTibcoFixAdapter();
        //        case TibcoAdapterType.RequestTibcoFixAdapter:
        //            return new RequestTibcoFixAdapter();
        //        default:
        //            return new BatchTibcoFixAdapter();
        //    }
        //}
    }
}
