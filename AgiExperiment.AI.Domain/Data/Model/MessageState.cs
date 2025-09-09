namespace AgiExperiment.AI.Domain.Data.Model;

public class MessageState : StateDataBase {
    public Guid? MessageId { get; set; }
    public ConversationMessage? Message { get; set; }
}