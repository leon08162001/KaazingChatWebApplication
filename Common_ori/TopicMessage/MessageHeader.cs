using System;

namespace Common.TopicMessage
{
    public class MessageHeader
    {
        private int _Count;
        private DateTime _CreatedTime;

        public MessageHeader(int Count, DateTime CreatedTime)
        {
            _Count = Count;
            _CreatedTime = CreatedTime;
        }
        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }
        public DateTime CreatedTime
        {
            get { return _CreatedTime; }
            set { _CreatedTime = value; }
        }
    }
}
