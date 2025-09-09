using AgiExperiment.AI.Cortex.Memories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.KernelMemory;

namespace AgiExperiment.Fluent.Web.Components.Pages
{
    public partial class MemoriesList : IDisposable
    {
        public record DocumemntItemModel
        {
            public string DocumentId { get; set; }
            public string Name { get; set; }
            public Citation Citation { get; set; }
            public bool Selected { get; set; }
        }

        private bool _hasLoadedFirstTime;

        private string _index = MemoriesService.IndexDefault;
        private string _imageExtensions = ".jpeg,.jpg,.png";
        private bool _isLoading;

        private double _defaultRelevance = 0.5;
        private double _relevance = 0;
        private string _searchQuery = "";

        private MemoriesService _docService = null!;

        [Parameter]
        public string? Index
        {
            get => _index;
            set
            {
                if (value != null)
                {
                    _index = value;
                    InvokeAsync(Reload);
                }
            }
        }

        [Inject]
        public required ConversationInterop Interop { get; set; }
        [Inject] public required IDialogService DialogService { get; set; }

        [Inject] public required IToastService NotificationService { get; set; }

        [Inject] public required IServiceProvider ServiceProvider { get; set; }

        private string[] ImageExtensions => _imageExtensions.Split(',');

        public IEnumerable<Citation> CitationsInKm { get; set; } = new List<Citation>();

        public List<DocumemntItemModel> Documents { get; set; } = new List<DocumemntItemModel>();

        public IEnumerable<DocumemntItemModel> SelectedDocuments;

        public void Dispose()
        {
            _docService.OnUploadFinished -= UploadIsDone;
        }

        protected override async Task OnInitializedAsync()
        {
            _docService = ServiceProvider.GetRequiredService<MemoriesService>();

            _docService.OnUploadFinished += UploadIsDone;
        }

        private void UploadIsDone(int obj)
        {
            InvokeAsync(Reload);
        }


        private async Task Reload()
        {
            if(_docService == null)
            {
                return;
            }

            _isLoading = true;
            await InvokeAsync(StateHasChanged);

            try
            {
                CitationsInKm = await _docService.SearchUserDocuments(_searchQuery, _index, _relevance, 1000);
                Documents = new List<DocumemntItemModel>();
                foreach (var item in CitationsInKm)
                {
                    var modelItem = new DocumemntItemModel();
                    modelItem.DocumentId = item.DocumentId;
                    modelItem.Name = item.SourceName;
                    modelItem.Citation = item;
                    Documents.Add(modelItem);
                }
                SelectedDocuments = Documents.Where(p => p.Selected);
            }
            catch (Exception e)
            {
                NotificationService.ShowToast(ToastIntent.Error, e.Message);
            }
            finally
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (!CitationsInKm.Any())
                    await Reload();
                _hasLoadedFirstTime = true;

                //var dimensions = await Interop.GetElementDimensions("layout-body");

                //_gridHeight = 300; //dimensions.Height - 300;

                StateHasChanged();

            }

            await base.OnAfterRenderAsync(firstRender);
        }


        private async Task DeleteFile(string documentId)
        {
            await _docService.DeleteDoc(documentId);
            Console.WriteLine($"Delete memory: {documentId}");
            await Reload();
        }

        private async Task DeleteAll()
        {
            IDialogReference dialog = await DialogService.ShowConfirmationAsync(
                "Are you sure you want to delete all files?",
                "Yes",
                "No",
                "Delete all files?");

            DialogResult? result = await dialog.Result;

            // If cancelled, return
            if (result.Cancelled)
            {
                return;
            }

            FileAreaCleaner cleaner = new(ServiceProvider.GetRequiredService<IOptions<PipelineOptions>>());

            await cleaner.DeleteAll(_index);
            await Reload();
        }

        private async Task DeleteSelected()
        {
            IDialogReference dialog = await DialogService.ShowConfirmationAsync(
                "Are you sure you want to delete selected files?",
                "Yes",
                "No",
                "Delete selected files?");

            DialogResult? result = await dialog.Result;

            // If cancelled, return
            if (result.Cancelled)
            {
                return;
            }

            foreach (var item in SelectedDocuments)
            {
                await _docService.DeleteDoc(item.DocumentId);
            }

            await Reload();
        }

        private async Task Clear(MouseEventArgs arg)
        {
            _searchQuery = "";
            _relevance = 0.0;
            await Reload();
        }

        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(_searchQuery))
                _relevance = 0.0;
            else
                _relevance = _relevance == 0 ? _defaultRelevance : _relevance;

            await Reload();
        }
    }
}
