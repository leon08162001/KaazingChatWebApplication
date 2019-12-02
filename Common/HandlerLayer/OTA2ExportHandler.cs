using Common.Dictionary;
using Common.Utility;
using Spring.Context.Support;
using System.Data;

namespace Common.HandlerLayer
{
    public class OTA2ExportHandler : TopicTypeHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object Mutex = new object();
        private static volatile OTA2ExportHandler _singleton;

        private OTA2ExportHandler(string MaxThreads)
            : base(MaxThreads)
        {
            _TopicType = TopicType.OTA2Export;
            _ResponseTag = typeof(OTAExportResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
        }

        private OTA2ExportHandler(string MaxThreads, bool EnabledThreadPool)
            : base(MaxThreads, EnabledThreadPool)
        {
            _TopicType = TopicType.OTA2Export;
            _ResponseTag = typeof(OTAExportResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
        }
        public static OTA2ExportHandler getSingleton(bool EnabledThreadPool)
        {
                if (_singleton == null)
                {
                    lock (Mutex)
                    {
                        if (_singleton == null)
                            _singleton = new OTA2ExportHandler(((Config)ContextRegistry.GetContext().GetObject("Config")).ota2ExportMaxThreads, EnabledThreadPool);
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
