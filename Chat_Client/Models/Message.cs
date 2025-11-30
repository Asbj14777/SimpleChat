namespace Chat_Client.Models
{
    public enum MessageType
    {
        Chat,
        System
    }

    public class Message
    {
        public string Text { get; set; } = "";
        public bool IsIncoming { get; set; }
        public string Sender { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public MessageType Type { get; set; } = MessageType.Chat;
    }
}
