//-----------------------------------------------------------------------
// <summary>Log提供者類型枚舉</summary>
//-----------------------------------------------------------------------
namespace Common.LogHelper
{
    /// <summary>
    /// Log提供者類型枚舉
    /// </summary>
    public enum LogProviderType
    {
        /// <summary>
        /// NLog第三方庫
        /// </summary>
        NLog,
        /// <summary>
        /// Log4Net第三方庫
        /// </summary>
        Log4Net,
        /// <summary>
        /// 自定義Log庫
        /// </summary>
        Custom
    }
}
