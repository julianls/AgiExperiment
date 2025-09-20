using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using AgiExperiment.AI.Cortex.Settings.McpSelector;

namespace AgiExperiment.Fluent.Web.Components.Settings.McpSelector;

public partial class McpList : IDialogContentComponent<List<McpSelection>>
{
    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [Parameter]
    public List<McpSelection> Content { get; set; } = default!;

    private async Task OnSelectionChanged()
    {
        Dialog!.TogglePrimaryActionButton(true);
        await Task.CompletedTask;
    }
}
