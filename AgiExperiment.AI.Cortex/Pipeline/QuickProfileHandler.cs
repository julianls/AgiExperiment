﻿

using AgiExperiment.AI.Domain.Data;
using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace AgiExperiment.AI.Cortex.Pipeline;

public class QuickProfileHandler : IQuickProfileHandler
{
    private IDbContextFactory<AiExperimentDBContext> _dbContextFactory;

    public QuickProfileHandler(IDbContextFactory<AiExperimentDBContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Conversation> Send(Kernel kernel, Conversation conversation,
        IEnumerable<QuickProfile>? beforeProfiles = null)
    {

        if (!conversation.HasStarted())
        {
            var selected = beforeProfiles.Where(p => p.InsertAt == InsertAt.Before);
            if (selected.Any())
            {
                string startMsg = string.Join(" ", selected.Select(p => p.Content)) + "\n\n\n";

                conversation.Messages.Insert(1, new ConversationMessage("user", startMsg));

            }
        }

        return conversation;
    }

    public  async Task<Conversation> Receive(Kernel kernel, ChatWrapper chatWrapper, Conversation conversation,
        IEnumerable<QuickProfile>? profiles = null)
    {

        await using var ctx = await _dbContextFactory.CreateDbContextAsync();

        if (conversation.Id == null || conversation.Id == default(Guid))
        {
            foreach (var p in profiles.Where(p => p.Id != default))
            {
                ctx.Attach(p);
                conversation.QuickProfiles.Add(p);
            }
        }

        if (conversation.InitStage())
        {
            ;
            if (profiles.Any())
            {
                foreach (var profile in profiles)
                {
                    conversation.AddMessage(new ConversationMessage("user", profile.Content));
                    conversation = await chatWrapper.Send(kernel , conversation).ConfigureAwait(false);
                }
            }
        }

        return conversation;

    }
}