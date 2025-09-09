using Blazor.Diagrams;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Geometry;
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Web;
using AgiExperiment.Fluent.Web.Components.Pages.Widgets;
using AgiExperiment.AI.Domain.Data.Model;
using Blazor.Diagrams.Core.Models.Base;
using AgiExperiment.AI.Cortex.Pipeline;

namespace AgiExperiment.Fluent.Web.Components.Pages
{
    public partial class DiagramPage
    {
        private static readonly Random _random = new Random();
        protected readonly BlazorDiagram BlazorDiagram = new BlazorDiagram();
        private int? _draggedType;

        [Inject]
        public required ILoggerFactory LoggerFactory { get; set; }
        private ILogger<ConversationPage> _logger;

        [Parameter]
        public string? UserId { get; set; } = null!;

        [Parameter]
        public Guid? DiagramId { get; set; }

        public Diagram Diagram = new();

        private Guid _loadedDiagramId = default;

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationState { get; set; }

        [Inject]
        public DiagramRepository DiagramsRepository { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;
        
        [Inject]
        public required FlowService FlowService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _logger = LoggerFactory.CreateLogger<ConversationPage>();

            if (UserId == null && AuthenticationState != null)
            {
                var authState = await AuthenticationState;
                var user = authState?.User;
                if (user?.Identity is not null && user.Identity.IsAuthenticated)
                {
                    UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                }
            }

            BlazorDiagram.RegisterComponent<BotAnswerNode, BotAnswerWidget>();
            BlazorDiagram.RegisterComponent<ModelContextNode, ModelContextWidget>();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                BlazorDiagram.Options.Groups.Enabled = true;
                //BlazorDiagram.Nodes.Add(new NodeModel(new Point(300, 50)));
                //BlazorDiagram.Nodes.Add(new NodeModel(new Point(300, 400)));

                BlazorDiagram.Options.Links.Factory = (d, s, ta) =>
                {
                    var link = new LinkModel(new SinglePortAnchor(s as PortModel)
                    {
                        UseShapeAndAlignment = false
                    }, ta)
                    {
                        TargetMarker = LinkMarker.Arrow
                    };
                    return link;
                };
            }

            if ((DiagramId == null && _loadedDiagramId == default)
                || (DiagramId != _loadedDiagramId))
            {
                await SetupDiagram();
            }
        }

        async Task SetupDiagram()
        {
            if (DiagramId == null)
            {
                Diagram = CreateDefaultDiagram();
                DiagramId = Guid.Empty;
                _loadedDiagramId = Guid.Empty;
                StateHasChanged();
                return;
            }

            if (DiagramId != _loadedDiagramId)
            {
                var loaded = await DiagramsRepository.GetDiagram(DiagramId.Value);
                if (loaded != null)
                {
                    _loadedDiagramId = loaded?.Id ?? default;
                    if (loaded.UserId != UserId)
                    {
                        throw new UnauthorizedAccessException();
                    }
                    Diagram = loaded;
                    LoadBlazorDiagram();
                }
                else
                {
                    //NavigationManager.NavigateTo("/conversation");
                }
            }
            StateHasChanged();
        }

        private Diagram CreateDefaultDiagram()
        {
            var newDiagram = new Diagram()
            {
                Name = "New Diagram",
                UserId = UserId
            };
            return newDiagram;
        }

        protected void AddNode()
        {
            var x = _random.Next(0, (int)BlazorDiagram.Container.Width - 120);
            var y = _random.Next(0, (int)BlazorDiagram.Container.Height - 100);
            BlazorDiagram.Nodes.Add(new NodeModel(new Point(x, y)));
        }

        protected void RemoveNode()
        {
            var i = _random.Next(0, BlazorDiagram.Nodes.Count);
            BlazorDiagram.Nodes.Remove(BlazorDiagram.Nodes[i]);
        }

        protected void AddPort()
        {
            var node = BlazorDiagram.Nodes.FirstOrDefault(n => n.Selected);
            if (node == null)
                return;

            foreach (PortAlignment portAlignment in Enum.GetValues(typeof(PortAlignment)))
            {
                if (node.GetPort(portAlignment) == null)
                {
                    node.AddPort(portAlignment);
                    node.Refresh();
                    break;
                }
            }
        }

        protected void RemovePort()
        {
            var node = BlazorDiagram.Nodes.FirstOrDefault(n => n.Selected);
            if (node == null)
                return;

            if (node.Ports.Count == 0)
                return;

            var i = _random.Next(0, node.Ports.Count);
            var port = node.Ports[i];

            BlazorDiagram.Links.Remove(port.Links.ToArray());
            node.RemovePort(port);
            node.Refresh();
        }

        protected void AddLink()
        {
            var selectedNodes = BlazorDiagram.Nodes.Where(n => n.Selected).ToArray();
            if (selectedNodes.Length != 2)
                return;

            var node1 = selectedNodes[0];
            var node2 = selectedNodes[1];

            if (node1 == null || node1.Ports.Count == 0 || node2 == null || node2.Ports.Count == 0)
                return;

            var sourcePort = node1.Ports[_random.Next(0, node1.Ports.Count)];
            var targetPort = node2.Ports[_random.Next(0, node2.Ports.Count)];
            BlazorDiagram.Links.Add(new LinkModel(sourcePort, targetPort));
        }

        private void OnDragStart(int key)
        {
            // Can also use transferData, but this is probably "faster"
            _draggedType = key;
        }

        private NodeModel CreateNodeModel(Point position)
        {
            NodeModel node;
            switch (_draggedType)
            {
                case 1:
                    node = new BotAnswerNode(position);
                    node.AddPort(PortAlignment.Top);
                    node.AddPort(PortAlignment.Bottom);
                    break;
                case 2:
                    node = new ModelContextNode(position);
                    node.AddPort(PortAlignment.Top);
                    node.AddPort(PortAlignment.Bottom);
                    break;
                default:
                    node = new NodeModel(position);
                    node.AddPort(PortAlignment.Bottom);
                    node.Title = "You are helpful assistant!";
                    break;
            }

            return node;
        }

        private void OnDrop(DragEventArgs e)
        {
            if (_draggedType == null) // Unkown item
                return;

            var position = BlazorDiagram.GetRelativeMousePoint(e.ClientX, e.ClientY);
            var node = CreateNodeModel(position);
            BlazorDiagram.Nodes.Add(node);
            _draggedType = null;
        }

        protected async Task SaveDiagram()
        {
            var isNew = Diagram.Id == default;

            await DiagramsRepository.ClearDiagram(Diagram);
            ApplyBlazorDiagram();

            await DiagramsRepository.SaveDiagram(Diagram);

            if (isNew)
            {
                NavigationManager.NavigateTo($"/diagram/{Diagram.Id}");
            }
        }

        private void ApplyBlazorDiagram()
        {
            Dictionary<ILinkable, DiagramNodePort> portModelToDataMap = new();
            foreach (var node in BlazorDiagram.Nodes)
            {
                var diagramNode = new DiagramNode
                {
                    Diagram = Diagram,
                    PositionX = node.Position.X,
                    PositionY = node.Position.Y,
                    Template = node is BotAnswerNode ? nameof(BotAnswerNode) : "",
                    Message = node is BotAnswerNode botAnswerNode ? botAnswerNode.Answer : node.Title,
                    Metadata = "{}",
                };

                foreach (var port in node.Ports)
                {
                    var diagramNodePort = new DiagramNodePort
                    {
                        DiagramNode = diagramNode,
                        //Alignment = port.Alignment,
                        PositionX = port.Position.X,
                        PositionY = port.Position.Y,
                        IsInput= port.Alignment == PortAlignment.Left || port.Alignment == PortAlignment.Top,
                        Template = "",
                        Metadata = "",
                    };
                    diagramNode.DiagramNodePorts.Add(diagramNodePort);
                    portModelToDataMap[port] = diagramNodePort;
                }

                Diagram.DiagramNodes.Add(diagramNode);
            }

            foreach (var link in BlazorDiagram.Links)
            {
                var diagramNodeLink = new DiagramNodeLink
                {
                    Diagram = Diagram,
                    SourceNodePort = portModelToDataMap[link.Source.Model],
                    TargetNodePort = portModelToDataMap[link.Target.Model],
                    SourcePositionX = link.Source.GetPlainPosition().X,
                    SourcePositionY = link.Source.GetPlainPosition().Y,
                    TargetPositionX = link.Target.GetPlainPosition().X,
                    TargetPositionY = link.Target.GetPlainPosition().Y,
                    Template = "",
                    Metadata = "",
                };

                Diagram.DiagramNodeLinks.Add(diagramNodeLink);
            }
        }
        
        void LoadBlazorDiagram()
        {
            Dictionary<NodeModel, DiagramNode> nodeModelToDataMap = new();
            Dictionary<DiagramNodePort, PortModel> portDataToModelMap = new();

            foreach (var node in Diagram.DiagramNodes)
            {
                NodeModel nodeModel;
                if (string.IsNullOrEmpty(node.Template))
                {
                    nodeModel = new NodeModel(new Point(node.PositionX, node.PositionY)) 
                    {
                        Title = node.Message
                    };
                }
                else
                {
                    nodeModel = new BotAnswerNode(new Point(node.PositionX, node.PositionY)) 
                    { 
                        Answer = node.Message 
                    };
                }
                BlazorDiagram.Nodes.Add(nodeModel);
                nodeModelToDataMap[nodeModel] = node;

                foreach (var port in node.DiagramNodePorts)
                {
                    var portModel = new PortModel(nodeModel, port.IsInput ? PortAlignment.Top : PortAlignment.Bottom)
                    {
                        Position = new Point(port.PositionX, port.PositionY)
                    };
                    nodeModel.AddPort(portModel);
                    portDataToModelMap[port] = portModel;
                }
            }

            foreach (var link in Diagram.DiagramNodeLinks)
            {
                if (portDataToModelMap.TryGetValue(link.SourceNodePort, out var sourcePort) &&
                    portDataToModelMap.TryGetValue(link.TargetNodePort, out var targetPort))
                {
                    var linkModel = new LinkModel(sourcePort, targetPort);
                    linkModel.TargetMarker = LinkMarker.Arrow;
                    BlazorDiagram.Links.Add(linkModel);
                }
            }
        }

        protected async Task RunDiagram()
        {
            var isNew = Diagram.Id == default;

            await DiagramsRepository.ClearDiagram(Diagram);
            ApplyBlazorDiagram();

            Diagram.Metadata = string.Format(@"{0}", new { Status = "RUN", DiagramId = Diagram.Id });

            await DiagramsRepository.SaveDiagram(Diagram);

            await FlowService.RunFlowAsync(Diagram);
        }
    }
}