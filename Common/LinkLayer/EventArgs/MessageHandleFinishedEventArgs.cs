using System;
using System.Data;

namespace Common.LinkLayer
{
    /// <summary>
    ///收到一筆Message並完成資料處理時的事件參數類別
    /// </summary>
    public class MessageHandleFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataRow _MessageRow;
        public MessageHandleFinishedEventArgs(string errorMessage, DataRow MessageRow)
        {
            _errorMessage = errorMessage;
            _MessageRow = MessageRow;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
        public DataRow MessageRow
        {
            get { return _MessageRow; }
            set { _MessageRow = value; }
        }
    }
}
