//-----------------------------------------------------------------------
// <summary>Log功能接口</summary>
//-----------------------------------------------------------------------
namespace Common.LogHelper
{
    /// <summary>
    /// Log功能接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 寫入一般信息
        /// </summary>
        /// <param name="logMessage">需要寫入Log的信息</param>
        void LogInfo(string logMessage);

        /// <summary>
        /// 寫入跟蹤信息
        /// </summary>
        /// <param name="logMessage">需要寫入Log的信息</param>
        void LogTrace(string logMessage);

        /// <summary>
        /// 寫入調試信息
        /// </summary>
        /// <param name="logMessage">需要寫入Log的信息</param>
        void LogDebug(string logMessage);

        /// <summary>
        /// 寫入警告信息
        /// </summary>
        /// <param name="logMessage">需要寫入Log的信息</param>
        void LogWarn(string logMessage);

        /// <summary>
        /// 寫入錯誤信息
        /// </summary>
        /// <param name="logMessage">需要寫入Log的信息</param>
        void LogError(string logMessage);

        /// <summary>
        /// 寫入崩潰信息
        /// </summary>
        /// <param name="logMessage">需要寫入Log的信息</param>
        void LogFatal(string logMessage);
    }
}
