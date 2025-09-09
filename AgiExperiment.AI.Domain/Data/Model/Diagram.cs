namespace AgiExperiment.AI.Domain.Data.Model
{
    public class Diagram
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? UserId { get; set; }

        public string Name { get; set; } = null!;

        public string SystemMessage { get; set; } = "You are a helpful assistant";
        
        public string Metadata { get; set; } = "";

        public List<DiagramNode> DiagramNodes { get; set; } = new();
        public List<DiagramNodeLink> DiagramNodeLinks { get; set; } = new();
    }
}
