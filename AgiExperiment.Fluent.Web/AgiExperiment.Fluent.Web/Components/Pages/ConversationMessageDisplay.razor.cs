using Microsoft.AspNetCore.Components;

namespace AgiExperiment.Fluent.Web.Components.Pages
{
    public partial class ConversationMessageDisplay
    {
        [Parameter] public bool EditMode { get; set; }

        [Parameter] public string? InitialSystemPrompt { get; set; }

        [Parameter] public required ConversationMessage Message { get; set; }

        [Parameter]
        public int MessagesCount { get; set; }

        [Inject]
        public ConversationInterop? Interop { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (Interop != null) await Interop.SetupCopyButtons();
            }
        }
    }
}
