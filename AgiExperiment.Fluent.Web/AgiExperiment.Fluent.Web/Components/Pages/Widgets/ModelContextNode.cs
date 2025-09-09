using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace AgiExperiment.Fluent.Web.Components.Pages.Widgets;

public class ModelContextNode : NodeModel
{
    public ModelContextNode(Point position = null) : base(position) { }

    public string Answer { get; set; }
}
