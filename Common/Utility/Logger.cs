//using Android.Media;
//using Android.Util;
using Java.IO;
using Java.Lang;
using System;
using System.Linq;
using System.Reflection;

namespace Common.Utility
{
    public class Logger
    {
        static readonly string LogDateFormat = "yyyy-MM-dd HH:mm:ss fff";
        public enum LogLevel
        {
            Info,
            Error
        }
        public static void Info(string logMessageTag, string logMessage)
        {
            if (!Log.IsLoggable(logMessageTag.Length > 23 ? logMessageTag.Substring(0, 23) : logMessageTag, LogPriority.Info))
            {
                return;
            }
            int logResult = Log.Info(logMessageTag, logMessage);
            if (logResult > 0)
                logToFile(logMessageTag, logMessage, LogLevel.Info);
        }
        public static void Error(string logMessageTag, string logMessage)
        {
            if (!Log.IsLoggable(logMessageTag.Length > 23 ? logMessageTag.Substring(0, 23) : logMessageTag, LogPriority.Error))
            {
                return;
            }
            int logResult = Log.Error(logMessageTag, logMessage);
            if (logResult > 0)
                logToFile(logMessageTag, logMessage, LogLevel.Error);
        }

        public static void Error(string logMessageTag, string logMessage, Throwable throwableException)
        {
            if (!Log.IsLoggable(logMessageTag.Length > 23 ? logMessageTag.Substring(0, 23) : logMessageTag, LogPriority.Error))
                return;

            int logResult = Log.Error(logMessageTag, logMessage, throwableException);
            if (logResult > 0)
                logToFile(logMessageTag, logMessage + "\r\n" + Log.GetStackTraceString(throwableException), LogLevel.Error);
        }

        private static void logToFile(string logMessageTag, string logMessage, LogLevel logLevel)
        {
            try
            {
                File logFile = new File(Config.Context.GetExternalFilesDir(@Config.logDir), "Log_" + System.DateTime.Today.ToString("yyyyMMdd") + ".txt");
                string formatLogMessage = string.Format("{0} - [{1}] {2} {3}\r\n", System.DateTime.Now.ToString(LogDateFormat), logMessageTag, System.Enum.GetName(typeof(LogLevel), logLevel), logMessage);
                if (!logFile.Exists())
                    logFile.CreateNewFile();
                BufferedWriter writer = new BufferedWriter(new FileWriter(logFile, true));
                writer.Write(formatLogMessage);
                writer.Close();

                MediaScannerConnection.ScanFile(Config.Context, new string[] { logFile.ToString() }, null, null);
            }
            catch (System.Exception e)
            {
                Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "Unable to log exception to file.");
            }
        }

        public static void deleteLogFile(int preservedDaysForLog)
        {
            System.IO.DirectoryInfo logDir = new System.IO.DirectoryInfo(Config.Context.GetExternalFilesDir(@Config.logDir).AbsolutePath);
            var deletedLogFiles = logDir.GetFiles().Where(file => file.CreationTime.AddDays(preservedDaysForLog) < DateTime.Today).ToList();
            foreach (System.IO.FileInfo file in deletedLogFiles)
            {
                file.Delete();
            }
        }
    }
}