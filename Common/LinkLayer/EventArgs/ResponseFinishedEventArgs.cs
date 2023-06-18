using System;
using System.Data;

namespace Common.LinkLayer
{
    /// <summary>
    /// 處理完所有回應相同RequestID資料的事件參數類別
    /// </summary>
    public class ResponseFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _ResponseResultTable;
        public ResponseFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public ResponseFinishedEventArgs(string errorMessage, DataTable ResponseResultTable)
        {
            _errorMessage = errorMessage;
            _ResponseResultTable = ResponseResultTable;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
        public DataTable ResponseResultTable
        {
            get { return _ResponseResultTable; }
            set { _ResponseResultTable = value; }
        }
    }
}
