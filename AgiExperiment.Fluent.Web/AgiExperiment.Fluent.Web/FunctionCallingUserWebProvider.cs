using AgiExperiment.AI.Cortex.Pipeline;
using AgiExperiment.Fluent.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace AgiExperiment.Fluent.Web
{
    public class FunctionCallingUserWebProvider(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor) : IFunctionCallingUserProvider
    {
        public async Task<string> GetUserId()
        {
            if (httpContextAccessor.HttpContext == null)
            {
                throw new InvalidOperationException("HttpContext is null. This filter requires HttpContext to be set.");
            }

            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            if (user == null)
            {
                throw new InvalidOperationException("User is null. This filter requires a user to be set.");
            }

            return user.Id;
        }
    }
}
