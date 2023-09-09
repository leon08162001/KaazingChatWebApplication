//-----------------------------------------------------------------------
// <summary>Log公共API，封裝Log操作公共方法類</summary>
//-----------------------------------------------------------------------
using System;
using System.Linq;

namespace Common.LogHelper
{
    /// <summary>
    /// Log公共API，封裝Log操作公共方法類
    /// </summary>
    public class Logger
    {
        public static string logPath = "";
        /// <summary>
        /// 寫入一般信息Log
        /// </summary>
        /// <param name="logMessage">Log信息</param>
        public static void LogInfo(string logMessage)
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogInfo(logMessage);
        }
        /// <summary>
        /// 寫入一般信息Log
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logMessage">Log信息</param>
        public static void LogInfo<T>(string logMessage)
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogInfo(LogMessageBuild.BuildExceptionMessage(logMessage, type));
        }
        /// <summary>
        /// 寫入一般信息Log
        /// </summary>
        /// <param name="logException">異常信息類</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogInfo(Exception logException, string addtionalMessage = "")
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogInfo(LogMessageBuild.BuildExceptionMessage(logException, addtionalMessage));
        }
        /// <summary>
        /// 寫入一般信息Log
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logException">異常信息類</param>
        /// <param name="addtionalMessage"></param>
        public static void LogInfo<T>(Exception logException, string addtionalMessage = "")
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogInfo(LogMessageBuild.BuildExceptionMessage(logException, type, addtionalMessage));
        }

        /// <summary>
        /// 寫入跟蹤信息
        /// </summary>
        /// <param name="logMessage">跟蹤信息</param>
        public static void LogTrace(string logMessage)
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogTrace(logMessage);
        }
        /// <summary>
        /// 寫入跟蹤信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logMessage">跟蹤信息</param>
        public static void LogTrace<T>(string logMessage)
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogTrace(LogMessageBuild.BuildExceptionMessage(logMessage, type));
        }
        /// <summary>
        /// 寫入跟蹤信息
        /// </summary>
        /// <param name="logException">異常信息類</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogTrace(Exception logException, string addtionalMessage = "")
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogTrace(LogMessageBuild.BuildExceptionMessage(logException, addtionalMessage));
        }
        /// <summary>
        /// 寫入跟蹤信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logException">異常信息類</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogTrace<T>(Exception logException, string addtionalMessage = "")
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogTrace(LogMessageBuild.BuildExceptionMessage(logException, type, addtionalMessage));
        }

        /// <summary>
        /// 寫入調試信息
        /// </summary>
        /// <param name="logMessage">調試信息</param>
        public static void LogDebug(string logMessage)
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogDebug(logMessage);
        }
        /// <summary>
        /// 寫入調試信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logMessage">調試信息</param>
        public static void LogDebug<T>(string logMessage)
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogDebug(LogMessageBuild.BuildExceptionMessage(logMessage, type));
        }
        /// <summary>
        /// 寫入調試信息
        /// </summary>
        /// <param name="logException">異常信息類</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogDebug(Exception logException, string addtionalMessage = "")
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogDebug(LogMessageBuild.BuildExceptionMessage(logException, addtionalMessage));
        }
        /// <summary>
        /// 寫入調試信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logException">異常信息類</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogDebug<T>(Exception logException, string addtionalMessage = "")
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogDebug(LogMessageBuild.BuildExceptionMessage(logException, type, addtionalMessage));
        }

        /// <summary>
        /// 寫入警告信息
        /// </summary>
        /// <param name="logMessage">警告信息</param>
        public static void LogWarn(string logMessage)
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogWarn(logMessage);
        }
        /// <summary>
        /// 寫入警告信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logMessage">警告信息</param>
        public static void LogWarn<T>(string logMessage)
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogWarn(LogMessageBuild.BuildExceptionMessage(logMessage, type));
        }
        /// <summary>
        /// 寫入警告信息
        /// </summary>
        /// <param name="logException">警告異常信息</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogWarn(Exception logException, string addtionalMessage = "")
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogWarn(LogMessageBuild.BuildExceptionMessage(logException, addtionalMessage));
        }
        /// <summary>
        /// 寫入警告信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logException">警告異常信息</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogWarn<T>(Exception logException, string addtionalMessage = "")
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogWarn(LogMessageBuild.BuildExceptionMessage(logException, type, addtionalMessage));
        }

        /// <summary>
        /// 寫入錯誤信息
        /// </summary>
        /// <param name="logMessage">錯誤信息</param>
        public static void LogError(string logMessage)
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogError(logMessage);
        }
        /// <summary>
        /// 寫入錯誤信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logMessage">錯誤信息</param>
        public static void LogError<T>(string logMessage)
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogError(LogMessageBuild.BuildExceptionMessage(logMessage, type));
        }
        /// <summary>
        /// 寫入錯誤信息
        /// </summary>
        /// <param name="logException">錯誤異常信息</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogError(Exception logException, string addtionalMessage = "")
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogError(LogMessageBuild.BuildExceptionMessage(logException, addtionalMessage));
        }
        /// <summary>
        /// 寫入錯誤信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logException">錯誤異常信息</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogError<T>(Exception logException, string addtionalMessage = "")
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogError(LogMessageBuild.BuildExceptionMessage(logException, type, addtionalMessage));
        }

        /// <summary>
        /// 寫入致命崩潰信息
        /// </summary>
        /// <param name="logMessage">崩潰信息</param>
        public static void LogFatal(string logMessage)
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogFatal(logMessage);
        }
        /// <summary>
        /// 寫入致命崩潰信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logMessage">崩潰信息</param>
        public static void LogFatal<T>(string logMessage)
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogFatal(LogMessageBuild.BuildExceptionMessage(logMessage, type));
        }
        /// <summary>
        /// 寫入致命崩潰信息
        /// </summary>
        /// <param name="logException">崩潰異常信息</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogFatal(Exception logException, string addtionalMessage = "")
        {
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogFatal(LogMessageBuild.BuildExceptionMessage(logException, addtionalMessage));
        }
        /// <summary>
        /// 寫入致命崩潰信息
        /// </summary>
        /// <typeparam name="T">調用此Log操作的類</typeparam>
        /// <param name="logException">崩潰異常信息</param>
        /// <param name="addtionalMessage">額外訊息</param>
        public static void LogFatal<T>(Exception logException, string addtionalMessage = "")
        {
            Type type = typeof(T);
            LogFactory.CreateLogger(LogProviderType.NLog, logPath).LogFatal(LogMessageBuild.BuildExceptionMessage(logException, type, addtionalMessage));
        }
        public static void DeleteLogFile(int preservedDaysForLog, string logPath)
        {
            System.IO.DirectoryInfo logDir = new System.IO.DirectoryInfo(logPath);
            var deletedLogFiles = logDir.GetFiles().Where(file => file.CreationTime.AddDays(preservedDaysForLog) < DateTime.Today).ToList();
            foreach (System.IO.FileInfo file in deletedLogFiles)
            {
                file.Delete();
            }
        }
    }
}
