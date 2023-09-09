using System;
using System.Data;

namespace Common.LinkLayer
{
    /// <summary>
    /// 處理完批次資料的事件參數類別
    /// </summary>
    public class BatchFinishedEventArgs : EventArgs
    {
        private string _errorMessage;
        private DataTable _BatchResultTable;
        public BatchFinishedEventArgs()
        {
            _errorMessage = "";
        }
        public BatchFinishedEventArgs(string errorMessage, DataTable BatchResultTable)
        {
            _errorMessage = errorMessage;
            _BatchResultTable = BatchResultTable;
        }
        public string errorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
        public DataTable BatchResultTable
        {
            get { return _BatchResultTable; }
            set { _BatchResultTable = value; }
        }
    }
}
