using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AgiExperiment.AI.Domain.Data
{
    public class StateRepository
    {
        private IDbContextFactory<AiExperimentDBContext> _dbContextFactory;

        public StateRepository(IDbContextFactory<AiExperimentDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<StateDataBase?> GetState(string stateType, Guid stateId)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync();

            StateDataBase? state = null;
            if (stateType == "message")
            {
                state = await ctx.StateData.FindAsync(stateId);
            }
            else
            {
                state = await ctx.TreeStateData.FindAsync(stateId);
            }

            return state;
        }
    }
}
