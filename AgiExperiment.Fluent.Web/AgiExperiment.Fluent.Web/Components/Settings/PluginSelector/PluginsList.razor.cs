using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using AgiExperiment.AI.Cortex.Settings.PluginSelector;
using AgiExperiment.AI.Cortex.Settings;
using Microsoft.FluentUI.AspNetCore.Components;


namespace AgiExperiment.Fluent.Web.Components.Settings.PluginSelector;

public partial class PluginsList : IDialogContentComponent<List<PluginSelection>>
{
    //List<PluginSelection>? BrowserData { get; set; }

    //[CascadingParameter]
    //private Task<AuthenticationState>? AuthenticationState { get; set; }
    //public string UserId { get; set; } = null!;

    //[Inject]
    //public required SettingsStateNotificationService SettingsStateNotificationService { get; set; }

    //[Inject] 
    //public required PluginsConfigurationService PluginsConfigurationService { get; set; }

    //[Inject]
    //public required PluginsRepository PluginsRepository { get; set; }

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }
    
    [Parameter]
    public List<PluginSelection> Content { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //await GetSelectionsFromLocalStorage();

            //if (BrowserData != null)
            //{
            //    foreach (var plugin in BrowserData)
            //    {
            //        var e = Content.FirstOrDefault(o => o.Name == plugin.Name);
            //        if (e != null)
            //        {
            //            e.Selected = true;
            //        }
            //    }
            //}
        }
    }

    protected override async Task OnInitializedAsync()
    {
        //if (AuthenticationState != null)
        //{
        //    var authState = await AuthenticationState;
        //    var user = authState?.User;
        //    if (user?.Identity is not null && user.Identity.IsAuthenticated)
        //    {
        //        UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        //    }
        //}

        //StateHasChanged();
    }

    //private async Task GetSelectionsFromLocalStorage()
    //{
    //    BrowserData = await PluginsConfigurationService.GetConfig();
    //}
    
    private async Task OnSelectionChanged()
    {
        Dialog!.TogglePrimaryActionButton(true);
    }
}
