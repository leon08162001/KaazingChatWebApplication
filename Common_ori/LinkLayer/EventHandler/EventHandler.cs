namespace Common.LinkLayer
{
    public delegate void MessageAsynSendFinishedEventHandler(object sender, MessageAsynSendFinishedEventArgs e);
    public delegate void MessageHandleFinishedEventHandler(object sender, MessageHandleFinishedEventArgs e);
    public delegate void BatchFinishedEventHandler(object sender, BatchFinishedEventArgs e);
    public delegate void BatchMismatchedEventHandler(object sender, BatchMismatchedEventArgs e);
    public delegate void ResponseFinishedEventHandler(object sender, ResponseFinishedEventArgs e);
    public delegate void ResponseMismatchedEventHandler(object sender, ResponseMismatchedEventArgs e);
}