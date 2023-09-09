using Common.LinkLayer;

namespace Common
{
    public enum WebSocketAdapterType
    {
        GenericWebSocketAdapter,
        BatchWebSocketAdapter,
        RequestWebSocketAdapter
    }

    public class WebSocketTopicFactory
    {
        public static IWebSocketAdapter GetWebSocketAdapterSingleton(WebSocketAdapterType WebSocketAdapterType)
        {
            switch (WebSocketAdapterType)
            {
                case WebSocketAdapterType.GenericWebSocketAdapter:
                    return GenericWebSocketAdapter.getSingleton();
                case WebSocketAdapterType.BatchWebSocketAdapter:
                    return BatchWebSocketAdapter.getSingleton();
                case WebSocketAdapterType.RequestWebSocketAdapter:
                    return RequestWebSocketAdapter.getSingleton();
                default:
                    return BatchWebSocketAdapter.getSingleton();
            }
        }

        public static IWebSocketAdapter GetWebSocketAdapterInstance(WebSocketAdapterType WebSocketAdapterType)
        {
            switch (WebSocketAdapterType)
            {
                case WebSocketAdapterType.GenericWebSocketAdapter:
                    return new GenericWebSocketAdapter();
                case WebSocketAdapterType.BatchWebSocketAdapter:
                    return new BatchWebSocketAdapter();
                case WebSocketAdapterType.RequestWebSocketAdapter:
                    return new RequestWebSocketAdapter();
                default:
                    return new BatchWebSocketAdapter();
            }
        }
    }
}
