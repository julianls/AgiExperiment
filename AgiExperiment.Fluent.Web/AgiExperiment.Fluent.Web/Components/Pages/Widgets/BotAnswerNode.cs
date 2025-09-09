using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace AgiExperiment.Fluent.Web.Components.Pages.Widgets;

public class BotAnswerNode : NodeModel
{
    public BotAnswerNode(Point position = null) : base(position) { }

    public string Answer { get; set; }
}
