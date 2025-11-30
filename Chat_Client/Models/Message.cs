using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_Client.Models
{
    public class Message
    {
        public string Text { get; set; }
        public bool IsIncoming { get; set; }
    }
}
