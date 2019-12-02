using Common.Dictionary;
using Common.Utility;
using Spring.Context.Support;
using System.Data;

namespace Common.HandlerLayer
{
    public class OTA1ExportHandler : TopicTypeHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object Mutex = new object();
        private static volatile OTA1ExportHandler _singleton;

        private OTA1ExportHandler(string MaxThreads)
            : base(MaxThreads)
        {
            _TopicType = TopicType.OTA1Export;
            _ResponseTag = typeof(OTAExportResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
        }

        private OTA1ExportHandler(string MaxThreads, bool EnabledThreadPool)
            : base(MaxThreads, EnabledThreadPool)
        {
            _TopicType = TopicType.OTA1Export;
            _ResponseTag = typeof(OTAExportResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
        }
        public static OTA1ExportHandler getSingleton(bool EnabledThreadPool)
        {
                if (_singleton == null)
                {
                    lock (Mutex)
                    {
                        if (_singleton == null)
                            _singleton = new OTA1ExportHandler(((Config)ContextRegistry.GetContext().GetObject("Config")).ota1ExportMaxThreads, EnabledThreadPool);
                    }
                }
                return _singleton;
        }
        public static void Release()
        {
            _singleton = null;
        }
    }
}
