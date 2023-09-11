namespace Common.LinkLayer
{
    public enum DBAction
    {
        Query, Add, Update, Delete, None
    }
    public enum DestinationFeature
    {
        Topic,
        VirtualTopic,
        MirroredQueues,
        Queue
    }

    public struct MessageField
    {
        public string Name;
        public string Value;
    }
}
