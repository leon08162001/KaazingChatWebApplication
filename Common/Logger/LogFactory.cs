//-----------------------------------------------------------------------
// <summary>Log提供者工廠，Log可以有NLog，Log4，自定義</summary>
//-----------------------------------------------------------------------
namespace Common.LogHelper
{
    /// <summary>
    /// Log提供者工廠，Log可以有NLog，Log4，自定義（單例模式）
    /// </summary>
    public class LogFactory
    {
        /// <summary>
        /// Log功能接口
        /// </summary>
        public static ILogger ILogger = null;

        /// <summary>
        /// Lock對象
        /// </summary>
        public static readonly object _object = new object();

        /// <summary>
        /// 私有構造函數，防止用戶在外部構造對象
        /// </summary>
        private LogFactory()
        {

        }

        /// <summary>
        /// 創建並返回特定的工廠類型
        /// </summary>
        /// <param name="type">Log提供者類型</param>
        /// <returns>Log提供者</returns>
        public static ILogger CreateLogger(LogProviderType type, string logPath)
        {
            if (ILogger == null)
            {
                lock (_object)
                {
                    if (ILogger == null && type == LogProviderType.NLog)
                    {
                        ILogger = new NLogProvider(logPath);
                    }
                }
            }

            return ILogger;
        }
    }
}
