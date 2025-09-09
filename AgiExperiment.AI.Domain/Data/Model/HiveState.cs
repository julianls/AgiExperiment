namespace AgiExperiment.AI.Domain.Data.Model;

public class HiveState : StateDataBase
{
    public Guid? ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
}