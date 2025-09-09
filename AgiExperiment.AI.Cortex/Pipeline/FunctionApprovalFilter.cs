using Microsoft.SemanticKernel;

namespace AgiExperiment.AI.Cortex.Pipeline
{
    public interface IFunctionApprovalService
    {
        Task<bool?> IsInvocationApproved(KernelFunction function, KernelArguments arguments);
    }

    public class FunctionApprovalFilter(IFunctionApprovalService approvalService/*, NotificationService notificationService*/) : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            if (context.Function.Description.ToLower().Contains("requires approval"))
            {
                var approval = await approvalService.IsInvocationApproved(context.Function, context.Arguments);
                if (approval.HasValue && approval.Value)
                {
                    await next(context);
                }
                else
                {
                    context.Result = new FunctionResult(context.Result, "Operation was rejected by user.");
                    //notificationService.Notify(NotificationSeverity.Warning, "Operation was rejected by user.");
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
