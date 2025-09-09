using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.SemanticKernel;

namespace AgiExperiment.AI.Cortex.Pipeline;

public interface IQuickProfileHandler
{
    Task<Conversation> Send(Kernel kernel, Conversation conversation, IEnumerable<QuickProfile>? beforeProfiles = null);
    Task<Conversation> Receive(Kernel kernel, ChatWrapper chatWrapper, Conversation conversation,
        IEnumerable<QuickProfile>? profiles = null);
}