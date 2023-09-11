//-----------------------------------------------------------------------
// <summary>Log信息構建類</summary>
//-----------------------------------------------------------------------
using System;

namespace Common.LogHelper
{
    /// <summary>
    /// Log內容構建類
    /// </summary>
    public class LogMessageBuild
    {
        /// <summary>
        /// 構建Log內容
        /// </summary>
        /// <param name="ex">異常信息</param>
        /// <returns>Log內容</returns>
        public static string BuildExceptionMessage(Exception ex, string addtionalMessage = "")
        {
            string innerMessage = ex.InnerException == null ? "Null" : ex.InnerException.Message;

            string strErrMsg = ex.Message + "\r\n";
            strErrMsg = strErrMsg + "Exception Type:" + ex.GetType().ToString() + " \r\n";
            strErrMsg = strErrMsg + "Source:" + ex.Source.ToString() + "\r\n";
            if (ex.InnerException != null)
            {
                strErrMsg = strErrMsg + "InnerExcptionMessage:" + ex.InnerException.Message + "\r\n";
            }
            strErrMsg = strErrMsg + "StackTrace:" + ex.StackTrace + "\r\n";
            if (addtionalMessage.Equals(""))
            {
                strErrMsg = strErrMsg + "Message:" + ex.Message + "\r\n";
            }
            else
            {
                strErrMsg = strErrMsg + "Message:" + ex.Message + "(" + addtionalMessage + ")" + "\r\n";
            }
            strErrMsg = strErrMsg + "ErrorMessage:" + ex.ToString() + "\r\n";
            strErrMsg = strErrMsg + "inner Message:" + innerMessage;

            return strErrMsg;
            //return string.Format("Exception type is {0},Message is {1},inner Message is {2},source is {3}",ex.GetType(),ex.Message, innerMessage, ex.Source);
        }

        /// <summary>
        /// 構建Log內容
        /// </summary>
        /// <param name="ex">異常信息</param>
        /// <param name="type">調用Log的類型</param>
        /// <returns>Log內容</returns>
        public static string BuildExceptionMessage(Exception ex, Type type, string addtionalMessage = "")
        {
            string innerMessage = ex.InnerException == null ? "Null" : ex.InnerException.Message;

            string strErrMsg = ex.Message + "\r\n";
            strErrMsg = strErrMsg + "Exception Type:" + type.ToString() + " \r\n";
            strErrMsg = strErrMsg + "Source:" + ex.Source.ToString() + "\r\n";
            if (ex.InnerException != null)
            {
                strErrMsg = strErrMsg + "InnerExcptionMessage:" + ex.InnerException.Message + "\r\n";
            }
            strErrMsg = strErrMsg + "StackTrace:" + ex.StackTrace + "\r\n";
            if (addtionalMessage.Equals(""))
            {
                strErrMsg = strErrMsg + "Message:" + ex.Message + "\r\n";
            }
            else
            {
                strErrMsg = strErrMsg + "Message:" + ex.Message + "(" + addtionalMessage + ")" + "\r\n";
            }
            strErrMsg = strErrMsg + "ErrorMessage:" + ex.ToString() + "\r\n";
            strErrMsg = strErrMsg + "inner Message:" + innerMessage;

            return strErrMsg;
            //return string.Format("Exception type is {0},Message is {1},inner Message is {2},error file is {3}", ex.GetType(), ex.Message, innerMessage, type.GetType());
        }
        public static string BuildExceptionMessage(string errorMessage, Type type)
        {
            string strErrMsg = "";
            strErrMsg = strErrMsg + "Exception Type:" + type.ToString() + " \r\n";
            strErrMsg = strErrMsg + "Message:" + errorMessage + "\r\n";
            return strErrMsg;
            //return string.Format("Exception type is {0},Message is {1},inner Message is {2},error file is {3}", ex.GetType(), ex.Message, innerMessage, type.GetType());
        }
    }
}
