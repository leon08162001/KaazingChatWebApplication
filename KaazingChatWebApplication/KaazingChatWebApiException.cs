using System;
using System.Runtime.Serialization;

namespace KaazingTestWebApplication
{
    public enum KaazingChatWebApiErrror
    {
        None,
        NoAnyRow,
        ObjectNotFound,
        SystemError,
        Other
    }
    public class KaazingChatWebApiException : Exception, ISerializable
    {
        protected KaazingChatWebApiErrror _ErrorCode = KaazingChatWebApiErrror.None;
        protected string _ErrorMessage;

        public KaazingChatWebApiErrror ErrorCode
        {
            get { return _ErrorCode; }
        }

        public string ErrorMessage
        {
            get { return _ErrorMessage; }
        }

        public KaazingChatWebApiException()
            : base()
        {
            _ErrorMessage = Enum.GetName(typeof(KaazingChatWebApiErrror), _ErrorCode);
        }
        public KaazingChatWebApiException(KaazingChatWebApiErrror ErrorCode)
        {
            _ErrorCode = ErrorCode;
            _ErrorMessage = Enum.GetName(typeof(KaazingChatWebApiErrror), _ErrorCode);
        }
        public KaazingChatWebApiException(string message)
            : base(message)
        {
            _ErrorCode = KaazingChatWebApiErrror.Other;
            _ErrorMessage = message;
        }
        public KaazingChatWebApiException(string message, Exception inner)
            : base(message, inner)
        {
            _ErrorCode = KaazingChatWebApiErrror.Other;
            _ErrorMessage = message;

        }
        // This constructor is needed for serialization.
        protected KaazingChatWebApiException(SerializationInfo info, StreamingContext context)
                        : base(info, context)
        {

        }
    }
}