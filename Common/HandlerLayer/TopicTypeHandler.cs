using Amib.Threading;
using Common.Utility;
using System;
using System.Data;
using System.Threading;

namespace Common.HandlerLayer
{
    public abstract class TopicTypeHandler
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected TopicType _TopicType;
        protected Type _ResponseTag;
        protected bool _EnabledThreadPool = true;
        protected ThreadPriority _Priority = ThreadPriority.Normal;
        protected SmartThreadPool _Stp;
        protected Thread _Thread;
        //最大執行緒數量、工作中執行緒數量、實際最大執行緒數量
        protected readonly int _MaxThreads;
        protected volatile int _WorkingThreads;
        protected volatile int _ActualMaxThreads;
        //加入佇列並分配給WorkThreads處理的機制
        protected CustomizedQueue<DataTable> _WorkItemQueue;
        protected SmartThreadPool _WorkDispatcher = new SmartThreadPool(60 * 1000, 1, 1);

        public TopicType TopicType
        {
            get { return _TopicType; }
        }

        public Type ResponseTag
        {
            get { return _ResponseTag; }
            set { _ResponseTag = value; }
        }

        public bool EnabledThreadPool
        {
            get { return _EnabledThreadPool; }
        }

        public SmartThreadPool SmartThreadPool
        {
            get { return _Stp; }
        }

        public Thread Thread
        {
            get { return _Thread; }
            set { _Thread = value; }
        }

        public ThreadPriority Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        public CustomizedQueue<DataTable> WorkItemQueue
        {
            get { return _WorkItemQueue; }
            set { _WorkItemQueue = value; }
        }

        public SmartThreadPool WorkDispatcher
        {
            get { return _WorkDispatcher; }
            set { _WorkDispatcher = value; }
        }

        public TopicTypeHandler()
        {
            _MaxThreads = 1;
            _EnabledThreadPool = true;
            _Stp = new SmartThreadPool();
            _Stp.MinThreads = 1;
            _Stp.MaxThreads = _MaxThreads;
        }

        public TopicTypeHandler(string MaxThreads)
        {
            int.TryParse(MaxThreads, out _MaxThreads);
        }

        public TopicTypeHandler(string MaxThreads, bool EnabledThreadPool)
        {
            int.TryParse(MaxThreads, out _MaxThreads);
            _EnabledThreadPool = EnabledThreadPool;
            if (_EnabledThreadPool)
            {
                _Stp = new SmartThreadPool();
                _Stp.MinThreads = 1;
                _Stp.MaxThreads = _MaxThreads;
            }
        }
        /// <summary>
        /// 檢查工作中執行緒數量是否超過最大限制執行緒數量而等待或繼續執行
        /// </summary>
        public void WaitOrContinueByMaxThreads()
        {
            if (!_EnabledThreadPool)
            {
                while (_MaxThreads > 0 && _WorkingThreads >= _MaxThreads) { }
            }
        }
        /// <summary>
        /// 紀錄此次應用程式運行中各主題所用到的最大執行緒數量
        /// </summary>
        public void LogActualMaxThreads()
        {
            //if (!_EnabledThreadPool & log.IsInfoEnabled) log.InfoFormat("ActualMaxThreads for {0} is：{1}", Enum.GetName(typeof(TopicType), _TopicType), _ActualMaxThreads);
        }
        /// <summary>
        /// 增加一個工作中執行緒數量
        /// </summary>
        public void IncreasingWorkingThreads()
        {
            _WorkingThreads += 1;
            _ActualMaxThreads = _WorkingThreads > _ActualMaxThreads ? _WorkingThreads : _ActualMaxThreads;
        }
        /// <summary>
        /// 減少一個工作中執行緒數量
        /// </summary>
        public void DecreasingWorkingThreads()
        {
            _WorkingThreads -= 1;
        }
    }
}
