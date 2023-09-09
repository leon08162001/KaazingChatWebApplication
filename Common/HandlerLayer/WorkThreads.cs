using Common.LinkLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Common.HandlerLayer
{
    public class WorkThreadsEventArgs<T> : EventArgs
    {
        private TopicType _TopicType;
        private int _CurrentSendAmounts;
        private T _ListMessageData;
        public WorkThreadsEventArgs(TopicType TopicType, int CurrentSendAmounts, T ListMessageData)
        {
            _TopicType = TopicType;
            _CurrentSendAmounts = CurrentSendAmounts;
            _ListMessageData = ListMessageData;
        }
        public TopicType TopicType
        {
            get { return _TopicType; }
            set { _TopicType = value; }
        }
        public int CurrentSendAmounts
        {
            get { return _CurrentSendAmounts; }
        }
        public T ListMessageData
        {
            get { return _ListMessageData; }
            set { _ListMessageData = value; }
        }
    }

    public class WorkThreadsErrorHappenedEventArgs<T> : EventArgs
    {
        private TopicType _TopicType;
        private int _CurrentSendAmounts;
        private T _ListMessageData;
        private string _errorMessage;
        public WorkThreadsErrorHappenedEventArgs(TopicType TopicType, int CurrentSendAmounts, T ListMessageData, string errorMessage)
        {
            _TopicType = TopicType;
            _CurrentSendAmounts = CurrentSendAmounts;
            _ListMessageData = ListMessageData;
            _errorMessage = errorMessage;
        }
        public TopicType TopicType
        {
            get { return _TopicType; }
            set { _TopicType = value; }
        }
        public int CurrentSendAmounts
        {
            get { return _CurrentSendAmounts; }
        }
        public T ListMessageData
        {
            get { return _ListMessageData; }
            set { _ListMessageData = value; }
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
    }

    public class WorkThreads
    {
        private static SynchronizationContext _UISyncContext;
        private static bool _IsEventInUIThread = false;             //觸發事件時是否回到UI Thread預設為false
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 觸發事件時是否回到UI Thread(預設false)
        /// </summary>
        public static bool IsEventInUIThread
        {
            get { return _IsEventInUIThread; }
            set { _IsEventInUIThread = value; }
        }

        #region WorkThreads's events

        public delegate void DoDBTaskFinishedEventHandler(object sender, WorkThreadsEventArgs<DataTable> e);
        public static event DoDBTaskFinishedEventHandler DoDBTaskFinished;

        public delegate void ConvertTagToMessageFinishedEventHandler(object sender, WorkThreadsEventArgs<DataTable> e);
        public static event ConvertTagToMessageFinishedEventHandler ConvertTagToMessageFinished;

        public delegate void SendMessageToClientFinishedEventHandler(object sender, WorkThreadsEventArgs<DataTable> e);
        public static event SendMessageToClientFinishedEventHandler SendMessageToClientFinished;

        public delegate void WorkThreadsErrorHappenedEventHandler(object sender, WorkThreadsErrorHappenedEventArgs<DataTable> e);
        public static event WorkThreadsErrorHappenedEventHandler WorkThreadsErrorHappened;

        #endregion

        /// <summary>
        /// 使用新的執行緒執行一連串的工作(For ActiveMQ & ApolloMQ)
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TResult1"></typeparam>
        /// <typeparam name="TResult2"></typeparam>
        /// <param name="MQAdapter"></param>
        /// <param name="DoDBTask"></param>
        /// <param name="ConvertTagToMessage"></param>
        /// <param name="SendMQMessage"></param>
        /// <param name="DoDBTask_arg1"></param>
        /// <param name="DoDBTask_arg2"></param>
        /// <param name="ConvertTagToMessage_arg1"></param>
        /// <param name="SendMQMessage_arg1"></param>
        public static void DoTask<T1, T2, T3, T4, TResult1, TResult2>(IMQAdapter MQAdapter,
            Func<T1, T2, TResult1> DoDBTask, Func<T3, TResult1, TResult2> ConvertTagToMessage,
            Func<T4, TResult2, bool> SendMQMessage, T1 DoDBTask_arg1, T2 DoDBTask_arg2, T3 ConvertTagToMessage_arg1,
            T4 SendMQMessage_arg1)
        //where T2 : DataTable
        //where T3 : Type
        //where T4 : struct
        //where TResult1: DataTable
        //where TResult2 : List<List<MessageField>>
        {
            _UISyncContext = MQAdapter.UISyncContext;
            Action TaskDefinition = delegate ()
            {
                try
                {
                    TResult1 Result1 = default(TResult1);
                    TResult2 Result2 = default(TResult2);
                    Result1 = DoDBTask.Invoke(DoDBTask_arg1, DoDBTask_arg2);
                    //if (_UISyncContext != null & IsEventInUIThread)
                    //{
                    //    _UISyncContext.Post(OnDoDBTaskFinished, new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, MQAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    //}
                    //else
                    //{
                    //    OnDoDBTaskFinished(new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, MQAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    //}
                    Result2 = ConvertTagToMessage.Invoke(ConvertTagToMessage_arg1, Result1);
                    //if (_UISyncContext != null & IsEventInUIThread)
                    //{
                    //    _UISyncContext.Post(OnConvertTagToMessageFinished, new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, MQAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    //}
                    //else
                    //{
                    //    OnConvertTagToMessageFinished(new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, MQAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    //}
                    SendMQMessage.Invoke(SendMQMessage_arg1, Result2);
                    if (_UISyncContext != null & IsEventInUIThread)
                    {
                        _UISyncContext.Post(OnSendMessageToClientFinished,
                            new WorkThreadsEventArgs<T2>(MQAdapter.Handler.TopicType, MQAdapter.CurrentSendAmounts,
                                DoDBTask_arg2));
                    }
                    else
                    {
                        OnSendMessageToClientFinished(new WorkThreadsEventArgs<T2>(MQAdapter.Handler.TopicType,
                            MQAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    }
                    //if (log.IsInfoEnabled) log.InfoFormat("Send {0} Message from Tibco Server Receive & Response Test", Enum.GetName(typeof(TopicType), TopicTypeHandler.TopicType));
                }
                catch (Exception ex)
                {
                    Common.LogHelper.Logger.LogError<WorkThreads>(ex);
                    if (_UISyncContext != null & IsEventInUIThread)
                    {
                        _UISyncContext.Post(OnWorkThreadsErrorHappened,
                            new WorkThreadsErrorHappenedEventArgs<T2>(MQAdapter.Handler.TopicType,
                                MQAdapter.CurrentSendAmounts, DoDBTask_arg2, ex.Message));
                    }
                    else
                    {
                        OnWorkThreadsErrorHappened(new WorkThreadsErrorHappenedEventArgs<T2>(
                            MQAdapter.Handler.TopicType, MQAdapter.CurrentSendAmounts, DoDBTask_arg2, ex.Message));
                    }
                }
                finally
                {
                    if (!MQAdapter.Handler.EnabledThreadPool)
                    {
                        DecreasingWorkingThreads(MQAdapter.Handler);
                    }
                }
            };
            if (!MQAdapter.Handler.EnabledThreadPool)
            {
                var executionContext = ExecutionContext.Capture();
                MQAdapter.Handler.Thread = new Thread(state =>
                {
                    ExecutionContext parentContext = (ExecutionContext)state;
                    ExecutionContext.Run(parentContext, _ =>
                    {
                        TaskDefinition();
                    }, null);
                });
                IncreasingWorkingThreads(MQAdapter.Handler);
                MQAdapter.Handler.Thread.Priority = MQAdapter.Handler.Priority;
                MQAdapter.Handler.Thread.Start(executionContext);
            }
            else
            {
                MQAdapter.Handler.SmartThreadPool.QueueWorkItem(TaskDefinition,
                    (Amib.Threading.WorkItemPriority)MQAdapter.Handler.Priority);
                //Amib.Threading.Action Task = new Amib.Threading.Action(() =>
                //{
                //    TaskDefinition();
                //});
                //MQAdapter.Handler.SmartThreadPool.QueueWorkItem(Task,
                //(Amib.Threading.WorkItemPriority)MQAdapter.Handler.Priority);
            }
        }

        public static void DoConvertSendTaskForJason(IMQAdapter MQAdapter, Func<Type, DataTable, List<List<MessageField>>> ConvertTableToMessage, DataTable ConvertTableToMessage_DT, string SendMQMessage_requestTag)
        {
            _UISyncContext = MQAdapter.UISyncContext;
            Action TaskDefinition = delegate ()
            {
                try
                {
                    List<List<MessageField>> Result1 = ConvertTableToMessage.Invoke((MQAdapter as BatchMQAdapter).DataType, ConvertTableToMessage_DT);
                    MQAdapter.SendMessage(SendMQMessage_requestTag, Result1);
                    if (_UISyncContext != null & IsEventInUIThread)
                    {
                        _UISyncContext.Post(OnSendMessageToClientFinished, new WorkThreadsEventArgs<DataTable>(MQAdapter.Handler.TopicType, MQAdapter.CurrentSendAmounts, ConvertTableToMessage_DT));
                    }
                    else
                    {
                        OnSendMessageToClientFinished(new WorkThreadsEventArgs<DataTable>(MQAdapter.Handler.TopicType, MQAdapter.CurrentSendAmounts, ConvertTableToMessage_DT));
                    }
                    //if (log.IsInfoEnabled) log.InfoFormat("Send {0} Message from Tibco Server Receive & Response Test", Enum.GetName(typeof(TopicType), TopicTypeHandler.TopicType));
                }
                catch (Exception ex)
                {
                    Common.LogHelper.Logger.LogError<WorkThreads>(ex);
                    if (_UISyncContext != null & IsEventInUIThread)
                    {
                        _UISyncContext.Post(OnWorkThreadsErrorHappened, new WorkThreadsErrorHappenedEventArgs<DataTable>(MQAdapter.Handler.TopicType, MQAdapter.CurrentSendAmounts, ConvertTableToMessage_DT, ex.Message));
                    }
                    else
                    {
                        OnWorkThreadsErrorHappened(new WorkThreadsErrorHappenedEventArgs<DataTable>(MQAdapter.Handler.TopicType, MQAdapter.CurrentSendAmounts, ConvertTableToMessage_DT, ex.Message));
                    }
                }
                finally
                {
                    if (!MQAdapter.Handler.EnabledThreadPool)
                    {
                        DecreasingWorkingThreads(MQAdapter.Handler);
                    }
                }
            };
            if (!MQAdapter.Handler.EnabledThreadPool)
            {
                var executionContext = ExecutionContext.Capture();
                MQAdapter.Handler.Thread = new Thread(state =>
                {
                    ExecutionContext parentContext = (ExecutionContext)state;
                    ExecutionContext.Run(parentContext, _ =>
                    {
                        TaskDefinition();
                    }, null);
                });
                IncreasingWorkingThreads(MQAdapter.Handler);
                MQAdapter.Handler.Thread.Priority = MQAdapter.Handler.Priority;
                MQAdapter.Handler.Thread.Start(executionContext);
            }
            else
            {
                MQAdapter.Handler.SmartThreadPool.QueueWorkItem(TaskDefinition, (Amib.Threading.WorkItemPriority)MQAdapter.Handler.Priority);
                //Amib.Threading.Action Task = new Amib.Threading.Action(() =>
                //{
                //    TaskDefinition();
                //});
                //MQAdapter.Handler.SmartThreadPool.QueueWorkItem(Task, (Amib.Threading.WorkItemPriority)MQAdapter.Handler.Priority);
            }
        }
        public static void DoConvertSendTaskForJason(IEMSAdapter EMSdapter, Func<Type, DataTable, List<List<MessageField>>> ConvertTableToMessage, DataTable ConvertTableToMessage_DT, string SendEMSMessage_requestTag)
        {
            _UISyncContext = EMSdapter.UISyncContext;
            Action TaskDefinition = delegate ()
            {
                try
                {
                    List<List<MessageField>> Result1 = ConvertTableToMessage.Invoke((EMSdapter as BatchEMSAdapter).DataType, ConvertTableToMessage_DT);
                    EMSdapter.SendMessage(SendEMSMessage_requestTag, Result1);
                    if (_UISyncContext != null & IsEventInUIThread)
                    {
                        _UISyncContext.Post(OnSendMessageToClientFinished, new WorkThreadsEventArgs<DataTable>(EMSdapter.Handler.TopicType, EMSdapter.CurrentSendAmounts, ConvertTableToMessage_DT));
                    }
                    else
                    {
                        OnSendMessageToClientFinished(new WorkThreadsEventArgs<DataTable>(EMSdapter.Handler.TopicType, EMSdapter.CurrentSendAmounts, ConvertTableToMessage_DT));
                    }
                    //if (log.IsInfoEnabled) log.InfoFormat("Send {0} Message from Tibco Server Receive & Response Test", Enum.GetName(typeof(TopicType), TopicTypeHandler.TopicType));
                }
                catch (Exception ex)
                {
                    Common.LogHelper.Logger.LogError<WorkThreads>(ex);
                    if (_UISyncContext != null & IsEventInUIThread)
                    {
                        _UISyncContext.Post(OnWorkThreadsErrorHappened, new WorkThreadsErrorHappenedEventArgs<DataTable>(EMSdapter.Handler.TopicType, EMSdapter.CurrentSendAmounts, ConvertTableToMessage_DT, ex.Message));
                    }
                    else
                    {
                        OnWorkThreadsErrorHappened(new WorkThreadsErrorHappenedEventArgs<DataTable>(EMSdapter.Handler.TopicType, EMSdapter.CurrentSendAmounts, ConvertTableToMessage_DT, ex.Message));
                    }
                }
                finally
                {
                    if (!EMSdapter.Handler.EnabledThreadPool)
                    {
                        DecreasingWorkingThreads(EMSdapter.Handler);
                    }
                }
            };
            if (!EMSdapter.Handler.EnabledThreadPool)
            {
                var executionContext = ExecutionContext.Capture();
                EMSdapter.Handler.Thread = new Thread(state =>
                {
                    ExecutionContext parentContext = (ExecutionContext)state;
                    ExecutionContext.Run(parentContext, _ =>
                    {
                        TaskDefinition();
                    }, null);
                });
                IncreasingWorkingThreads(EMSdapter.Handler);
                EMSdapter.Handler.Thread.Priority = EMSdapter.Handler.Priority;
                EMSdapter.Handler.Thread.Start(executionContext);
            }
            else
            {
                EMSdapter.Handler.SmartThreadPool.QueueWorkItem(TaskDefinition, (Amib.Threading.WorkItemPriority)EMSdapter.Handler.Priority);
                //Amib.Threading.Action Task = new Amib.Threading.Action(() =>
                //{
                //    TaskDefinition();
                //});
                //EMSdapter.Handler.SmartThreadPool.QueueWorkItem(Task, (Amib.Threading.WorkItemPriority)EMSdapter.Handler.Priority);
            }
        }



        /// <summary>
        /// 使用新的執行緒執行一連串的工作(For Tibco)
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TResult1"></typeparam>
        /// <typeparam name="TResult2"></typeparam>
        /// <param name="TibcoAdapter"></param>
        /// <param name="TopicType"></param>
        /// <param name="DoDBTask"></param>
        /// <param name="ConvertTagToMessage"></param>
        /// <param name="SendTibcoMessage"></param>
        /// <param name="DoDBTask_arg1"></param>
        /// <param name="DoDBTask_arg2"></param>
        /// <param name="ConvertTagToMessage_arg1"></param>
        /// <param name="SendTibcoMessage_arg1"></param>
        //public static void DoTask<T1, T2, T3, T4, TResult1, TResult2>(ITibcoAdapter TibcoAdapter, TopicTypeHandler TopicTypeHandler, Func<T1, T2, TResult1> DoDBTask, Func<T3, TResult1, TResult2> ConvertTagToMessage, Action<T4, TResult2> SendTibcoMessage, T1 DoDBTask_arg1, T2 DoDBTask_arg2, T3 ConvertTagToMessage_arg1, T4 SendTibcoMessage_arg1)
        //{
        //    //_UISyncContext = TibcoAdapter.UISyncContext;
        //    //Action TaskDefinition = delegate()
        //    //{
        //    //    try
        //    //    {
        //    //        TResult1 Result1 = default(TResult1);
        //    //        TResult2 Result2 = default(TResult2);
        //    //        Result1 = DoDBTask.Invoke(DoDBTask_arg1, DoDBTask_arg2);
        //    //        if (_UISyncContext != null & IsEventInUIThread)
        //    //        {
        //    //            _UISyncContext.Post(OnDoDBTaskFinished, new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, DoDBTask_arg2));
        //    //        }
        //    //        else
        //    //        {
        //    //            OnDoDBTaskFinished(new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, DoDBTask_arg2));
        //    //        }
        //    //        Result2 = ConvertTagToMessage.Invoke(ConvertTagToMessage_arg1, Result1);
        //    //        if (_UISyncContext != null & IsEventInUIThread)
        //    //        {
        //    //            _UISyncContext.Post(OnConvertTagToMessageFinished, new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, DoDBTask_arg2));
        //    //        }
        //    //        else
        //    //        {
        //    //            OnConvertTagToMessageFinished(new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, DoDBTask_arg2));
        //    //        }
        //    //        SendTibcoMessage.Invoke(SendTibcoMessage_arg1, Result2);
        //    //        if (_UISyncContext != null & IsEventInUIThread)
        //    //        {
        //    //            _UISyncContext.Post(OnSendMessageToClientFinished, new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, DoDBTask_arg2));
        //    //        }
        //    //        else
        //    //        {
        //    //            OnSendMessageToClientFinished(new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, DoDBTask_arg2));
        //    //        }
        //    //        //if (log.IsInfoEnabled) log.InfoFormat("Send {0} Message from Tibco Server Receive & Response Test", Enum.GetName(typeof(TopicType), TopicTypeHandler.TopicType));
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        if (log.IsErrorEnabled) log.Error(ex.Message, ex);
        //    //        if (_UISyncContext != null & IsEventInUIThread)
        //    //        {
        //    //            _UISyncContext.Post(OnWorkThreadsErrorHappened, new WorkThreadsErrorHappenedEventArgs<T2>(TopicTypeHandler.TopicType, DoDBTask_arg2, ex.Message));
        //    //        }
        //    //        else
        //    //        {
        //    //            OnWorkThreadsErrorHappened(new WorkThreadsErrorHappenedEventArgs<T2>(TopicTypeHandler.TopicType, DoDBTask_arg2, ex.Message));
        //    //        }
        //    //    }
        //    //    finally
        //    //    {
        //    //        if (!TopicTypeHandler.EnabledThreadPool)
        //    //        {
        //    //            DecreasingWorkingThreads(TopicTypeHandler);
        //    //        }
        //    //    }
        //    //};
        //    //if (!TopicTypeHandler.EnabledThreadPool)
        //    //{
        //    //    var executionContext = ExecutionContext.Capture();
        //    //    TopicTypeHandler.Thread = new Thread(state =>
        //    //    {
        //    //        ExecutionContext parentContext = (ExecutionContext)state;
        //    //        ExecutionContext.Run(parentContext, _ =>
        //    //        {
        //    //            TaskDefinition();
        //    //        }, null);
        //    //    });
        //    //    IncreasingWorkingThreads(TopicTypeHandler);
        //    //    TopicTypeHandler.Thread.Priority = TopicTypeHandler.Priority;
        //    //    TopicTypeHandler.Thread.Start(executionContext);
        //    //}
        //    //else
        //    //{
        //    //    Amib.Threading.Action Task = new Amib.Threading.Action(() =>
        //    //    {
        //    //        TaskDefinition();
        //    //    });
        //    //    TopicTypeHandler.SmartThreadPool.QueueWorkItem(Task, (Amib.Threading.WorkItemPriority)TopicTypeHandler.Priority);
        //    //}
        //}

        /// <summary>
        /// 使用新的執行緒執行一連串的工作(For Tibco EMS)
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TResult1"></typeparam>
        /// <typeparam name="TResult2"></typeparam>
        /// <param name="EMSAdapter"></param>
        /// <param name="DoDBTask"></param>
        /// <param name="ConvertTagToMessage"></param>
        /// <param name="SendMQMessage"></param>
        /// <param name="DoDBTask_arg1"></param>
        /// <param name="DoDBTask_arg2"></param>
        /// <param name="ConvertTagToMessage_arg1"></param>
        /// <param name="SendMQMessage_arg1"></param>
        public static void DoTask<T1, T2, T3, T4, TResult1, TResult2>(IEMSAdapter EMSAdapter, Func<T1, T2, TResult1> DoDBTask, Func<T3, TResult1, TResult2> ConvertTagToMessage, Func<T4, TResult2, bool> SendEMSMessage, T1 DoDBTask_arg1, T2 DoDBTask_arg2, T3 ConvertTagToMessage_arg1, T4 SendEMSMessage_arg1)
        {
            _UISyncContext = EMSAdapter.UISyncContext;
            Action TaskDefinition = delegate ()
            {
                try
                {
                    TResult1 Result1 = default(TResult1);
                    TResult2 Result2 = default(TResult2);
                    Result1 = DoDBTask.Invoke(DoDBTask_arg1, DoDBTask_arg2);
                    //if (_UISyncContext != null & IsEventInUIThread)
                    //{
                    //    _UISyncContext.Post(OnDoDBTaskFinished, new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, EMSAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    //}
                    //else
                    //{
                    //    OnDoDBTaskFinished(new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, EMSAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    //}
                    Result2 = ConvertTagToMessage.Invoke(ConvertTagToMessage_arg1, Result1);
                    //if (_UISyncContext != null & IsEventInUIThread)
                    //{
                    //    _UISyncContext.Post(OnConvertTagToMessageFinished, new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, EMSAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    //}
                    //else
                    //{
                    //    OnConvertTagToMessageFinished(new WorkThreadsEventArgs<T2>(TopicTypeHandler.TopicType, EMSAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    //}
                    SendEMSMessage.Invoke(SendEMSMessage_arg1, Result2);
                    if (_UISyncContext != null & IsEventInUIThread)
                    {
                        _UISyncContext.Post(OnSendMessageToClientFinished,
                            new WorkThreadsEventArgs<T2>(EMSAdapter.Handler.TopicType, EMSAdapter.CurrentSendAmounts,
                                DoDBTask_arg2));
                    }
                    else
                    {
                        OnSendMessageToClientFinished(new WorkThreadsEventArgs<T2>(EMSAdapter.Handler.TopicType,
                            EMSAdapter.CurrentSendAmounts, DoDBTask_arg2));
                    }
                    //if (log.IsInfoEnabled) log.InfoFormat("Send {0} Message from Tibco Server Receive & Response Test", Enum.GetName(typeof(TopicType), TopicTypeHandler.TopicType));
                }
                catch (Exception ex)
                {
                    Common.LogHelper.Logger.LogError<WorkThreads>(ex);
                    if (_UISyncContext != null & IsEventInUIThread)
                    {
                        _UISyncContext.Post(OnWorkThreadsErrorHappened,
                            new WorkThreadsErrorHappenedEventArgs<T2>(EMSAdapter.Handler.TopicType,
                                EMSAdapter.CurrentSendAmounts, DoDBTask_arg2, ex.Message));
                    }
                    else
                    {
                        OnWorkThreadsErrorHappened(
                            new WorkThreadsErrorHappenedEventArgs<T2>(EMSAdapter.Handler.TopicType,
                                EMSAdapter.CurrentSendAmounts, DoDBTask_arg2, ex.Message));
                    }
                }
                finally
                {
                    if (!EMSAdapter.Handler.EnabledThreadPool)
                    {
                        DecreasingWorkingThreads(EMSAdapter.Handler);
                    }
                }
            };
            if (!EMSAdapter.Handler.EnabledThreadPool)
            {
                var executionContext = ExecutionContext.Capture();
                EMSAdapter.Handler.Thread = new Thread(state =>
                {
                    ExecutionContext parentContext = (ExecutionContext)state;
                    ExecutionContext.Run(parentContext, _ =>
                    {
                        TaskDefinition();
                    }, null);
                });
                IncreasingWorkingThreads(EMSAdapter.Handler);
                EMSAdapter.Handler.Thread.Priority = EMSAdapter.Handler.Priority;
                EMSAdapter.Handler.Thread.Start(executionContext);
            }
            else
            {
                EMSAdapter.Handler.SmartThreadPool.QueueWorkItem(TaskDefinition, (Amib.Threading.WorkItemPriority)EMSAdapter.Handler.Priority);
                //Amib.Threading.Action Task = new Amib.Threading.Action(() =>
                //{
                //    TaskDefinition();
                //});
                //EMSAdapter.Handler.SmartThreadPool.QueueWorkItem(Task, (Amib.Threading.WorkItemPriority)EMSAdapter.Handler.Priority);
            }
        }

        /// <summary>
        /// 檢查工作中執行緒數量是否超過最大限制執行緒數量而等待或繼續執行
        /// </summary>
        /// <param name="TopicTypeHandler"></param>
        public static void WaitOrContinueByMaxThreads(TopicTypeHandler TopicTypeHandler)
        {
            TopicTypeHandler.WaitOrContinueByMaxThreads();
        }

        /// <summary>
        /// 紀錄此次應用程式運行中各主題所用到的最大執行緒數量
        /// </summary>
        /// <param name="TopicTypeHandler"></param>
        public static void LogActualMaxThreads(TopicTypeHandler TopicTypeHandler)
        {
            TopicTypeHandler.LogActualMaxThreads();
        }

        /// <summary>
        /// 處理完DB存取後引發的事件
        /// </summary>
        protected static void OnDoDBTaskFinished(object state)
        {
            WorkThreadsEventArgs<DataTable> e = state as WorkThreadsEventArgs<DataTable>;
            if (DoDBTaskFinished != null)
            {
                DoDBTaskFinished(null, e);
            }
        }

        /// <summary>
        /// 轉換Fix Tag為MQ Message後引發的事件
        /// </summary>
        protected static void OnConvertTagToMessageFinished(object state)
        {
            WorkThreadsEventArgs<DataTable> e = state as WorkThreadsEventArgs<DataTable>;
            if (ConvertTagToMessageFinished != null)
            {
                ConvertTagToMessageFinished(null, e);
            }
        }

        /// <summary>
        /// 回應訊息給Client後引發的事件
        /// </summary>
        /// <param name="state"></param>
        protected static void OnSendMessageToClientFinished(object state)
        {
            WorkThreadsEventArgs<DataTable> e = state as WorkThreadsEventArgs<DataTable>;
            if (SendMessageToClientFinished != null)
            {
                SendMessageToClientFinished(null, e);
            }
        }

        protected static void OnWorkThreadsErrorHappened(object state)
        {
            WorkThreadsErrorHappenedEventArgs<DataTable> e = state as WorkThreadsErrorHappenedEventArgs<DataTable>;
            if (WorkThreadsErrorHappened != null)
            {
                WorkThreadsErrorHappened(null, e);
            }
        }

        /// <summary>
        /// 增加一個工作中執行緒數量
        /// </summary>
        /// <param name="TopicTypeHandler"></param>
        private static void IncreasingWorkingThreads(TopicTypeHandler TopicTypeHandler)
        {
            TopicTypeHandler.IncreasingWorkingThreads();
        }

        /// <summary>
        /// 減少一個工作中執行緒數量
        /// </summary>
        /// <param name="TopicTypeHandler"></param>
        private static void DecreasingWorkingThreads(TopicTypeHandler TopicTypeHandler)
        {
            TopicTypeHandler.DecreasingWorkingThreads();
        }
    }
}
