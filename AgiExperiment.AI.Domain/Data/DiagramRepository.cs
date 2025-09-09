using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace AgiExperiment.AI.Domain.Data
{
    public class DiagramRepository
    {
        private IDbContextFactory<AiExperimentDBContext> _dbContextFactory;

        public DiagramRepository(IDbContextFactory<AiExperimentDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<bool> CreateDiagram(string userId, string newName)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            await ctx.Diagrams.AddAsync(new Diagram() { Name = newName, UserId = userId });
            var res = await ctx.SaveChangesAsync();
            return res == 1;
        }

        public async Task<bool> DeleteDiagram(Guid id)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var diagram = await ctx.Diagrams.FindAsync(id);
            if (diagram == null)
                return false;

            ctx.Diagrams.Remove(diagram);
            var res = await ctx.SaveChangesAsync();
            return res == 1;
        }

        public async Task<List<Diagram>> GetDiagrams(string userId)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            return await ctx.Diagrams.Where(d => d.UserId == userId).ToListAsync();
        }

        public async Task<bool> SaveDiagram(Diagram diagram)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            ctx.Diagrams.Update(diagram);
            var res = await ctx.SaveChangesAsync();
            return res > 0;
        }

        public async Task<Diagram> GetDiagram(Guid diagramId)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var diagram = await ctx.Diagrams
                .Include(d => d.DiagramNodes)
                    .ThenInclude(dn => dn.DiagramNodePorts)
                        .ThenInclude(dnp => dnp.SourceNodeLinks)
                .Include(d => d.DiagramNodes)
                    .ThenInclude(dn => dn.DiagramNodePorts)
                        .ThenInclude(dnp => dnp.TargetNodeLinks)
                .Include(d => d.DiagramNodeLinks)
                .SingleAsync(d => d.Id == diagramId);

            return diagram;
        }

        public async Task ClearDiagram(Diagram diagram)
        {
            if (diagram.Id != default)
            {
                await using var context = await _dbContextFactory.CreateDbContextAsync();
                var diagramNodes = context.DiagramNodes.Where(n => n.DiagramId == diagram.Id);
                context.DiagramNodes.RemoveRange(diagramNodes);

                var diagramNodePorts = context.DiagramNodePorts.Where(p => diagramNodes.Select(n => n.Id).Contains(p.DiagramNodeId));
                context.DiagramNodePorts.RemoveRange(diagramNodePorts);

                var diagramNodeLinks = context.DiagramNodeLinks.Where(l => l.DiagramId == diagram.Id);
                context.DiagramNodeLinks.RemoveRange(diagramNodeLinks);

                diagram.DiagramNodes.Clear();
                diagram.DiagramNodeLinks.Clear();

                await context.SaveChangesAsync();
            }
        }
    }
}
