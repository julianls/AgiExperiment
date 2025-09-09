namespace AgiExperiment.AI.Domain.Data.Model
{
    public class MessageAttachment
    {
        public Guid? Id { get; set; }

        public ConversationMessage? ConversationMessage { get; set; }

        public Guid ConversationMessageId { get; set; }

        public string Name { get; set; }

        public string ContentType { get; set; }
        
        public byte[] Content { get; set; }

        public bool IsTextContent { get { return ContentType.StartsWith("text/") || ContentType.StartsWith("application/json"); } }

        public bool IsImageContent { get{ return ContentType.StartsWith("image/"); }}

        public bool IsAudioContent { get { return ContentType.StartsWith("audio/"); } }
    }
}
