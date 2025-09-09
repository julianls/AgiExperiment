namespace AgiExperiment.AI.Domain.Data.Model
{
    public class DiagramNode
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = "";
        public string Role { get; set; } = "";
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public string Template { get; set; } = "";
        public string Metadata { get; set; } = "";
        public Diagram? Diagram { get; set; } = default!;
        public Guid DiagramId { get; set; }
        public List<DiagramNodePort> DiagramNodePorts { get; set; } = new();
    }
}
