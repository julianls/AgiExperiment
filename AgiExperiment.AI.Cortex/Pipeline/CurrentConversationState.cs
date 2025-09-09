﻿using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.Extensions.Caching.Memory;

namespace AgiExperiment.AI.Cortex.Pipeline;

public class CurrentConversationState(IMemoryCache stateCache)
{
    private const string Key = "cstate";

    public void SetCurrentConversationForUser(Conversation conversation)
    {
        string key = $@"{Key}{conversation.UserId}";
        stateCache.Set(key, conversation);
    }

    public Conversation? GetCurrentConversation(string userId)
    {

        string key = $@"{Key}{userId}";
        stateCache.TryGetValue(key, out Conversation? conversation);
        return conversation;
    }

    public void RemoveCurrentConversation(string userId)
    {
        string key = $@"{Key}{userId}";
        stateCache.Remove(key);
    }
}