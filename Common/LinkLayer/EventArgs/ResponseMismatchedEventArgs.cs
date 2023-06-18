using System;

namespace Common.LinkLayer
{
    /// <summary>
    /// MessageHeader's Count與MessageBody's DataRow Count不符合事件參數類別
    /// </summary>
    public class ResponseMismatchedEventArgs : EventArgs
    {
        private string _MismatchedMessage;
        public ResponseMismatchedEventArgs()
        {
            _MismatchedMessage = "";
        }
        public ResponseMismatchedEventArgs(string MismatchedMessage)
        {
            _MismatchedMessage = MismatchedMessage;
        }
        public string MismatchedMessage
        {
            get { return _MismatchedMessage; }
            set { _MismatchedMessage = value; }
        }
    }
}
