using AgiExperiment.AI.Cortex.Pipeline;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.SemanticKernel;

namespace AgiExperiment.Fluent.Web
{
    public class FunctionCallingDialogApprovalService(IDialogService dialogService) : IFunctionApprovalService
    {
        public async Task<bool?> IsInvocationApproved(KernelFunction function, KernelArguments arguments)
        {
            var confirm = await dialogService.ShowConfirmationAsync($"{function.PluginName} - {function.Name}", "Perform function call?");
            return !confirm.Result.IsCanceled;
        }
    }
}
