using Common.Dictionary;
using Common.Utility;
using Spring.Context.Support;
using System.Data;

namespace Common.HandlerLayer
{
    public class OTAExportHandler : TopicTypeHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object Mutex = new object();
        private static volatile OTAExportHandler _singleton;

        private OTAExportHandler(string MaxThreads)
            : base(MaxThreads)
        {
            _TopicType = TopicType.OTAExport;
            _ResponseTag = typeof(OTAExportResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
        }

        private OTAExportHandler(string MaxThreads, bool EnabledThreadPool)
            : base(MaxThreads, EnabledThreadPool)
        {
            _TopicType = TopicType.OTAExport;
            _ResponseTag = typeof(OTAExportResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
        }
        public static OTAExportHandler getSingleton(bool EnabledThreadPool)
        {
                if (_singleton == null)
                {
                    lock (Mutex)
                    {
                        if (_singleton == null)
                            _singleton = new OTAExportHandler(((Config)ContextRegistry.GetContext().GetObject("Config")).otaExportMaxThreads, EnabledThreadPool);
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
