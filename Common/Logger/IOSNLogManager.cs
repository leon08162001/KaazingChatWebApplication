//-----------------------------------------------------------------------
// <summary>IOSLog配置管理類</summary>
//-----------------------------------------------------------------------
using NLog.Config;

namespace Common.LogHelper
{
    /// <summary>
    /// IOSLog配置管理類
    /// </summary>
    public class IOSNLogManager : LogManager
    {
        /// <summary>
        /// IOS自定義NLog配置
        /// </summary>
        public override void SetConfig(string logPath)
        {
            //var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //var documentPath = Path.Combine(path, "..", "Library");

            LoggingConfiguration config = ConfigLog(logPath);

            NLog.LogManager.Configuration = config;
        }
    }
}
