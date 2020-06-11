using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Filters;

namespace KaazingTestWebApplication
{
    public class WebApiExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnException(HttpActionExecutedContext context)
        {
            var ex = context.Exception;
            var actionArgs = context.ActionContext.ActionArguments;
            int errorCode;
            string actionValue = String.Empty;
            string sErrorMsg = "";
            foreach (string key in actionArgs.Keys)
            {
                var dictObj = ToDictionary<object>(actionArgs[key]);
                foreach (Type iType in dictObj.GetType().GetInterfaces())
                {
                    if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    {
                        actionValue = GetDictContents<string, object>(dictObj);
                        break;
                    }
                }
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
                log.Error("Request Url－" + context.Request.RequestUri);
                log.Error("Request Method－" + context.Request.Method.Method);
                log.Error("Request Text－" + actionValue);
            }
            if (log.IsErrorEnabled) log.Error(sErrorMsg, ex);

            context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
            new
            {
                MessageId = errorCode,
                Message = sErrorMsg
            },
            new JsonMediaTypeFormatter());

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
    }
}