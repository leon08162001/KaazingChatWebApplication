using Common.Dictionary;
using Common.Utility;
using System.Data;

namespace Common.HandlerLayer
{
    public class JefferiesExcuReportHandler : TopicTypeHandler
    {
        private static readonly object Mutex = new object();
        private static volatile JefferiesExcuReportHandler _singleton;
        private JefferiesExcuReportHandler _singletonInstance;

        public JefferiesExcuReportHandler()
            : base()
        {
            _TopicType = TopicType.JefferiesExcuReport;
            _ResponseTag = typeof(JefferiesExcuResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
            _singletonInstance = this;
        }

        private JefferiesExcuReportHandler(string MaxThreads)
            : base(MaxThreads)
        {
            _TopicType = TopicType.JefferiesExcuReport;
            _ResponseTag = typeof(JefferiesExcuResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
        }

        private JefferiesExcuReportHandler(string MaxThreads, bool EnabledThreadPool)
            : base(MaxThreads, EnabledThreadPool)
        {
            _TopicType = TopicType.JefferiesExcuReport;
            _ResponseTag = typeof(JefferiesExcuResponseTag);
            this.WorkItemQueue = new CustomizedQueue<DataTable>(this.GetType().Name.ToString());
            this.WorkDispatcher.Name = this.GetType().Name.ToString();
        }

        public static JefferiesExcuReportHandler getSingleton(bool EnabledThreadPool)
        {
            if (_singleton == null)
            {
                lock (Mutex)
                {

                    if (_singleton == null)
                        _singleton = new JefferiesExcuReportHandler(Config.jefferiesExcuReportMaxThreads, EnabledThreadPool);
                }
            }
            return _singleton;
        }
        public static void Release()
        {
            _singleton = null;
        }
        public void ReleaseInstance()
        {
            _singletonInstance = null;
        }
    }
}
