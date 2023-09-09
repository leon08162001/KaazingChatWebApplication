//-----------------------------------------------------------------------
// <summary>NLog第三方庫Log功能提供者</summary>
//-----------------------------------------------------------------------
namespace Common.LogHelper
{
    /// <summary>
    /// NLog第三方庫Log功能提供者
    /// </summary>
    public class NLogProvider : ILogger
    {
        /// <summary>
        /// NLogLog功能類型
        /// </summary>
        private static NLog.Logger NLoger;

        /// <summary>
        /// 靜態構造函數，根據不同的平台構造不同的Log配置文件
        /// </summary>
        public NLogProvider(string logPath)
        {
            LogManager manager;
            //#if DROID
            //            manager = new AndroidNLogManager();
            //            manager.SetConfig(logPath);
            //#endif
            //#if IOS
            //manager = new IOSNLogManager();
            //manager.SetConfig(logPath);
            //#endif
            manager = new NLogManager();
            manager.SetConfig(logPath);
            NLoger = NLog.LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// 調用NLogLog提供者寫入調試信息
        /// </summary>
        /// <param name="logMessage">寫入Log的內容</param>
        public void LogDebug(string logMessage)
        {
            NLoger.Debug(logMessage);
        }

        /// <summary>
        /// 調用NLogLog提供者寫入錯誤信息
        /// </summary>
        /// <param name="logMessage">寫入Log的內容</param>
        public void LogError(string logMessage)
        {
            NLoger.Error(logMessage);
        }

        /// <summary>
        /// 調用NLogLog提供者寫入崩潰信息
        /// </summary>
        /// <param name="logMessage">寫入Log的內容</param>
        public void LogFatal(string logMessage)
        {
            NLoger.Fatal(logMessage);
        }

        /// <summary>
        /// 調用NLogLog提供者寫入一般信息
        /// </summary>
        /// <param name="logMessage">寫入Log的內容</param>
        public void LogInfo(string logMessage)
        {
            NLoger.Info(logMessage);
        }

        /// <summary>
        /// 調用NLogLog提供者寫入跟蹤信息
        /// </summary>
        /// <param name="logMessage">寫入Log的內容</param>
        public void LogTrace(string logMessage)
        {
            NLoger.Trace(logMessage);
        }

        /// <summary>
        /// 調用NLogLog提供者寫入警告信息
        /// </summary>
        /// <param name="logMessage">寫入Log的內容</param>
        public void LogWarn(string logMessage)
        {
            NLoger.Warn(logMessage);
        }
    }
}
