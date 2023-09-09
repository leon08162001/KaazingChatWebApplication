using System;

namespace Common.LinkLayer
{
    /// <summary>
    ///非同步發送Message完成時的事件參數類別
    /// </summary>
    public class MessageAsynSendFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        public MessageAsynSendFinishedEventArgs(string errorMessage)
        {
            _errorMessage = errorMessage;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
    }
}
