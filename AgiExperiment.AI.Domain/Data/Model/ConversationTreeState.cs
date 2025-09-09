﻿namespace AgiExperiment.AI.Domain.Data.Model;

public class ConversationTreeState : StateDataBase
{
    public Guid? ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
}