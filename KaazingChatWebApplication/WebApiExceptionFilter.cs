using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;

namespace KaazingTestWebApplication
{
    public class WebApiExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnException(HttpActionExecutedContext context)
        {
            var ex = context.Exception;
            int errorCode;
            string sErrorMsg = "";
            if (ex is KaazingChatWebApiException)
            {
                errorCode = (int)(ex as KaazingChatWebApiException).ErrorCode;
                sErrorMsg = ex.GetType().Name + ":" + (ex as KaazingChatWebApiException).ErrorMessage;
            }
            else
            {
                errorCode = (int)(ex as Exception).HResult;
                sErrorMsg = ex.GetType().Name + ":" + "An unhandled exception was thrown by Customer Web API controller.context." + "(" + ex.Message + ")";
            }
            if (log.IsErrorEnabled) log.Error(sErrorMsg, ex);

            context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
            new
            {
                ErrorCode = errorCode,
                ErrorMessage = sErrorMsg
            },
            new JsonMediaTypeFormatter());

            base.OnException(context);
        }
    }
}