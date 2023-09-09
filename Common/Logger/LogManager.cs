//-----------------------------------------------------------------------
// <summary>NLog的配置管理類</summary>
//-----------------------------------------------------------------------
using NLog;
using NLog.Config;
using NLog.Targets;
using System;

namespace Common.LogHelper
{
    /// <summary>
    /// NLog的配置管理類
    /// </summary>
    public abstract class LogManager
    {
        /// <summary>
        /// 默認的NLog配置信息
        /// </summary>
        /// <param name="path">Log存儲路徑</param>
        /// <returns>Log配置信息</returns>
        public virtual LoggingConfiguration ConfigLog(string path)
        {
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget("FileLoger");
            string dateString = Convert.ToInt16(DateTime.Now.Year) + DateTime.Now.ToString("/MM/dd");
            string fileDateString = dateString.Replace("/", "-");
            fileTarget.FileName = path + "/" + fileDateString;
            //fileTarget.FileName = path + "/" + fileDateString + ".txt";

            //fileTarget.FileName = path + "/logs/" + "${shortdate}.txt";
            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            config.AddTarget(fileTarget);
            config.LoggingRules.Add(fileRule);

            return config;
        }

        /// <summary>
        /// 設置存儲信息。IOS和安卓可以自定義各自的配置信息
        /// </summary>
        public abstract void SetConfig(string logPath);
    }
}
