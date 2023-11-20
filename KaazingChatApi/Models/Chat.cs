using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KaazingChatWebApplication.Models
{
    public class Chat
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? receiver { get; set; }
        public string? htmlMessage { get; set; }
        public DateTime  date { get; set; }
        public DateTime oprTime { get; set; }
        public string oprIpAddress { get; set; }
    }
}