using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Threading;

namespace Common.Utility
{
    public class CustomizedQueueCountUpdateEventArgs<T> : EventArgs
    {
        private int _Count;
        private readonly T _Queue;
        private DateTime _EventTime;
        public CustomizedQueueCountUpdateEventArgs(int Count, T Queue, DateTime EventTime)
        {
            _Count = Count;
            _Queue = Queue;
            _EventTime = EventTime;
        }
        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }
        public T Queue
        {
            get { return _Queue; }
        }
        public DateTime EventTime
        {
            get { return _EventTime; }
        }
    }
    public delegate void RecvCallBack(int pQueueLength, object msgItem);
    /// <summary>
    /// Summary description for InterQueue.
    /// </summary>
    public class CustomizedQueue<T> : Queue<T>
    {
        private SynchronizationContext _UISyncContext;
        private bool _IsEventInUIThread = true;             //觸發事件時是否回到UI Thread預設為true
        //Timer CountUpdateEventTimer;
        private ManualResetEvent m_Signal = new ManualResetEvent(false);
        private bool exit = false;
        public string name = "";

        public delegate void CustomizedQueueCountUpdateEventHandler(object sender, CustomizedQueueCountUpdateEventArgs<T> e);
        public event CustomizedQueueCountUpdateEventHandler CustomizedQueueCountUpdated;

        public CustomizedQueue(string queue_name)
        {
            name = queue_name;
            _UISyncContext = SynchronizationContext.Current;
            //TimerCallback TCB = new TimerCallback(state => { TriggerCustomizedQueueCountUpdated(); });
            //CountUpdateEventTimer = new Timer(TCB, DateTime.Now, 0, 100);
        }

        public CustomizedQueue()
        {
            _UISyncContext = SynchronizationContext.Current;
            //TimerCallback TCB = new TimerCallback(state => { TriggerCustomizedQueueCountUpdated(); });
            //CountUpdateEventTimer = new Timer(TCB, DateTime.Now, 0, 100);
        }

        public bool Exit
        {
            set
            {
                exit = value;
                m_Signal.Set();
            }
            get
            {
                return exit;
            }
        }

        /// <summary>
        /// 觸發事件時是否回到UI Thread(預設true)
        /// </summary>
        public bool IsEventInUIThread
        {
            get { return _IsEventInUIThread; }
            set { _IsEventInUIThread = value; }
        }

        public SynchronizationContext UISyncContext
        {
            get { return _UISyncContext; }
        }

        ~CustomizedQueue()
        {
            Exit = true;
        }

        public void StopDispatcher()
        {
            Exit = true;
        }

        public void StartDispatcher(RecvCallBack recvCallBack)
        {
            Exit = false;
            while (!Exit)
            {
                T du = (T)this.Dequeue();
                if (du == null)
                    break;
                recvCallBack(this.Count(), du);
            }
        }

        public new void Enqueue(T obj)
        {
            if (obj != null)
            {
                lock (this)
                {
                    base.Enqueue(obj);
                    TriggerCustomizedQueueCountUpdated(obj);
                }
            }
            m_Signal.Set();
        }

        public new T Dequeue()
        {
            T qItem = default(T);
            try
            {
                if (m_Signal.WaitOne(-1, Exit) && !Exit)
                {
                    lock (this)
                    {
                        if (base.Count > 0)
                        {
                            qItem = (T)base.Dequeue();
                            TriggerCustomizedQueueCountUpdated(qItem);
                        }
                        if (base.Count <= 0)
                        {
                            m_Signal.Reset();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return qItem;
        }

        public new int Count()
        {
            lock (this)
            {
                return base.Count;
            }
        }

        public new void Clear()
        {
            T qItem = default(T);
            lock (this)
            {
                m_Signal.Reset();
                base.Clear();
                TriggerCustomizedQueueCountUpdated(qItem);
            }
        }

        public bool Contains(IComparable newobj)
        {
            lock (this)
            {
                foreach (T obj in this)
                {
                    if (newobj.CompareTo(obj) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual void TriggerCustomizedQueueCountUpdated(T obj)
        {
            if (_UISyncContext != null & IsEventInUIThread)
            {
                _UISyncContext.Post(OnCustomizedQueueCountUpdated, new CustomizedQueueCountUpdateEventArgs<T>(base.Count, obj, DateTime.Now));
            }
            else
            {
                OnCustomizedQueueCountUpdated(new CustomizedQueueCountUpdateEventArgs<T>(base.Count, obj, DateTime.Now));
            }
        }

        /// <summary>
        /// 取出佇列或存入佇列時引發佇列存量更新事件
        /// </summary>
        protected virtual void OnCustomizedQueueCountUpdated(object state)
        {
            CustomizedQueueCountUpdateEventArgs<T> e = state as CustomizedQueueCountUpdateEventArgs<T>;
            if (CustomizedQueueCountUpdated != null)
            {
                CustomizedQueueCountUpdated(this, e);
            }
        }
    }
}
