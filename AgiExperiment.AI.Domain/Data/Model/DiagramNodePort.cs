namespace AgiExperiment.AI.Domain.Data.Model
{
    public class DiagramNodePort
    {
        public Guid Id { get; set; }
        public bool IsInput { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public string Template { get; set; } = "";
        public string Metadata { get; set; } = "";
        public DiagramNode? DiagramNode { get; set; } = default!;
        public Guid DiagramNodeId { get; set; }
        public List<DiagramNodeLink> SourceNodeLinks { get; set; } = new();
        public List<DiagramNodeLink> TargetNodeLinks { get; set; } = new();
    }
}
