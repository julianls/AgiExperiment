using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgiExperiment.AI.Domain.Data.Model
{
    public class DiagramNodeLink
    {
        public Guid Id { get; set; }
        public string Template { get; set; } = "";
        public string Metadata { get; set; } = "";
        public double SourcePositionX { get; set; }
        public double SourcePositionY { get; set; }
        public double SourcePositionZ { get; set; }
        public double TargetPositionX { get; set; }
        public double TargetPositionY { get; set; }
        public double TargetPositionZ { get; set; }
        public DiagramNodePort? SourceNodePort { get; set; } = default!;
        public Guid SourceNodePortId { get; set; }
        public DiagramNodePort? TargetNodePort { get; set; } = default!;
        public Guid TargetNodePortId { get; set; }
        public Diagram? Diagram { get; set; } = default!;
        public Guid DiagramId { get; set; }
    }
}
