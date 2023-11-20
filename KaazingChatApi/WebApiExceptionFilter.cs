using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace KaazingTestWebApplication
{
    public class WebApiExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            var actionArgs = context.ActionDescriptor.RouteValues;
            int errorCode;
            string actionValue = String.Empty;
            string sErrorMsg = "";
            foreach (string key in actionArgs.Keys)
            {
                actionValue += key + "：" + actionArgs[key] + ";";
            }
            if (ex is KaazingChatWebApiException)
            {
                errorCode = (int)(ex as KaazingChatWebApiException).ErrorCode;
                sErrorMsg = ex.GetType().Name + ":" + (ex as KaazingChatWebApiException).ErrorMessage;
            }
            else
            {
                errorCode = (int)(ex as Exception).HResult;
                sErrorMsg = ex.GetType().Name + ":" + "An unhandled exception was thrown by Custom Web API exception handler(" + ex.Message + ")";
            }
            if (log.IsErrorEnabled && !actionValue.Equals(String.Empty))
            {
                
                log.Error("Request Url－" + context.HttpContext.Request.GetDisplayUrl());
                log.Error("Request Method－" + context.HttpContext.Request.Method);
                log.Error("Request Route－" + actionValue);
            }
            if (log.IsErrorEnabled) log.Error(sErrorMsg, ex);

            var response = new {
                MessageId = errorCode,
                Message = sErrorMsg
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
            context.ExceptionHandled = true;
            base.OnException(context);
        }
        private static Dictionary<string, TValue> ToDictionary<TValue>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }
        private static string GetDictContents<TKey, TValue>(IDictionary<TKey, TValue> data)
        {
            string result = string.Empty;
            foreach (var pair in data)
            {
                result += pair.Key + " = " + pair.Value + Environment.NewLine;
            }
            result = result.TrimEnd(Environment.NewLine.ToCharArray());
            return result;
        }
        private static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}