using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.SemanticKernel;

namespace AgiExperiment.AI.Cortex.Pipeline.Interceptors;

public interface IInterceptor
{
    string Name { get; }
    bool Internal { get; }
    Task<Conversation> Receive(Kernel kernel, Conversation conversation, Func<string, Task<string>>? onComplete = null, CancellationToken cancellationToken = default);
    Task<Conversation> Send(Kernel kernel, Conversation conversation, Func<string, Task<string>>? onComplete = null, CancellationToken cancellationToken = default);


}