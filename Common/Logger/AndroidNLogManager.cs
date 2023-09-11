//-----------------------------------------------------------------------
// <summary>安卓Log配置管理類</summary>
//-----------------------------------------------------------------------
using NLog.Config;

namespace Common.LogHelper
{
    /// <summary>
    /// 安卓Log配置管理類
    /// </summary>
    public class AndroidNLogManager : LogManager
    {
        /// <summary>
        /// 安卓自定義NLog配置
        /// </summary>
        public override void SetConfig(string logPath)
        {
            #region "SD卡路徑寫入"
            //var sdCard =Android.OS.Environment.ExternalStorageDirectory;
            //var logDirectory = new Java.IO.File(sdCard.AbsolutePath);
            //logDirectory.Mkdirs();
            //var path = logDirectory.AbsoluteFile.ToString();
            //Console.WriteLine(path);
            //LoggingConfiguration config = ConfigLog(path);

            //LogManager.Configuration = config;
            #endregion

            //var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            LoggingConfiguration config = ConfigLog(logPath);
            NLog.LogManager.Configuration = config;
        }
    }
}
