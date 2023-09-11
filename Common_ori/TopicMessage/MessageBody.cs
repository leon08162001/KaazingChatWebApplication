using System;
using System.Data;

namespace Common.TopicMessage
{
    public class MessageBody
    {
        private DataTable _Messages;
        private DateTime _CreatedTime;

        public MessageBody(DataTable Messages, DateTime CreatedTime)
        {
            _Messages = Messages;
            _CreatedTime = CreatedTime;
        }
        public DataTable Messages
        {
            get { return _Messages; }
            set { _Messages = value; }
        }
        public DateTime CreatedTime
        {
            get { return _CreatedTime; }
            set { _CreatedTime = value; }
        }
    }
}
