using AgiExperiment.AI.Cortex.Common;
using AgiExperiment.AI.Cortex.Pipeline;
using AgiExperiment.AI.Cortex.Pipeline.Interceptors;
using AgiExperiment.AI.Cortex.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.SemanticKernel;
using System.Security.Claims;

namespace AgiExperiment.Fluent.Web.Components.Pages
{
    public partial class ConversationPage
    {
        [Parameter]
        public string? NewDestinationPrefix { get; set; }

        [Parameter]
        public Guid? ConversationId { get; set; }

        [Parameter]
        public Kernel? KernelToUse { get; set; }

        private Guid _loadedConversationId = default;

        [Parameter]
        public Guid? MessageId { get; set; }


        [Parameter]
        public string? UserId { get; set; } = null!;


        [Inject]
        public required KernelService KernelService { get; set; }

        [Inject]
        public required ILoggerFactory LoggerFactory { get; set; }
        private ILogger<ConversationPage> _logger;

        [Inject]
        public IDbContextFactory<AiExperimentDBContext> DbContextFactory { get; set; } = null!;

        [Inject]
        public required IInterceptorHandler InterceptorHandler { get; set; }

        [Inject]
        public required ConversationInterop Interop { get; set; }

        public Conversation Conversation = new();

        private ModelConfiguration? _modelConfiguration;

        bool promptIsReady;

        string? selectedModelValue;

        private Kernel _kernel = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        [Inject] public required ModelConfigurationService _modelConfigurationService { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationState { get; set; }

        [Inject]
        public required UserStorageService UserStorage { get; set; }

        [Inject]
        public required ConversationsRepository ConversationsRepository { get; set; }

        [Inject]
        public IOptions<PipelineOptions> PipelineOptions { get; set; } = null!;

        public string prompt = string.Empty;

        [Inject]
        ILocalStorageService LocalStorageService { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public required IDialogService DialogService { get; set; } = null!;

        [Inject]
        IToastService ToastService { get; set; } = null!;


        private FluentTextArea? promptTextArea;
        private FluentStack? conversationStack;

        //private IJSObjectReference? module;

        //private QuickProfileSelector? _profileSelectorStart;

        //private QuickProfileSelector? _profileSelectorEnd;

        bool _formUploading;
        FluentInputFile? myFileUploader = default!;
        int? progressPercent;
        string? progressTitle;

        string _fileExtensions = @"
          application/msword,
          application/vnd.openxmlformats-officedocument.wordprocessingml.document,
          application/vnd.ms-excel,
          application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,
          application/vnd.ms-powerpoint,
          application/vnd.openxmlformats-officedocument.presentationml.presentation,
          application/pdf,
          text/markdown,
          text/x-markdown,
          text/html,
          image/jpeg,
          image/png,
          image/tiff,
          application/json";

        List<Option<string>> modelOptions = new()
        {
            { new Option<string> { Value = "OpenAI", Text = "OpenAI" } },
            { new Option<string> { Value = "AzureOpenAI", Text = "AzureOpenAI"} },
            { new Option<string> { Value = "Ollama", Text = "Ollama" } },
            { new Option<string> { Value = "XAi", Text = "XAi" } },
            { new Option<string> { Value = "Gemini", Text = "Gemini" } },
            { new Option<string> { Value = "GitHub", Text = "GitHub" } },
            { new Option<string> { Value = "DeepSeek", Text = "DeepSeek" } },
            { new Option<string> { Value = "Anthropic", Text = "Anthropic" } },
        };

        FluentInputFileEventArgs[] Files = Array.Empty<FluentInputFileEventArgs>();
        List<MessageAttachment> attachments = new List<MessageAttachment>();

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

            //BotSystemInstruction ??= PipelineOptions.Value.Bot.BotSystemInstruction;

            //InterceptorHandler.OnUpdate += UpdateAndRedraw;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _modelConfiguration = await _modelConfigurationService.GetConfig();
                selectedModelValue = _modelConfiguration.Provider.ToString();
            }

            if ((ConversationId == null && _loadedConversationId == default)
                || (ConversationId != _loadedConversationId))
            {
                await SetupConversation();
            }

            if (firstRender)
            {
                await Interop.SetupCopyButtons();
                await Interop.ScrollToBottom("convStackId");
            }

            await Interop.FocusElement(promptTextArea.Element);
        }

        async Task SetupConversation()
        {
            if (ConversationId == null)
            {
                Conversation = CreateDefaultConversation();
                ConversationId = Guid.Empty;
                _loadedConversationId = Guid.Empty;
                StateHasChanged();
                return;
            }

            if (ConversationId != _loadedConversationId)
            {
                var loaded = await ConversationsRepository.GetConversation(ConversationId);
                if (loaded != null)
                {
                    _loadedConversationId = loaded.Id ?? default;
                    if (loaded.UserId != UserId)
                    {
                        throw new UnauthorizedAccessException();
                    }
                    if (loaded.BranchedFromMessageId != null)
                    {
                        var hive = ConversationsRepository.GetMergedBranchRootConversation((Guid)loaded.BranchedFromMessageId);

                        if (hive != null)
                        {
                            loaded.HiveConversation = hive;

                        }
                    }
                    Conversation = loaded;
                }
                else
                {
                    //NavigationManager.NavigateTo("/conversation");
                }
            }

            StateHasChanged();
        }

        private Conversation CreateDefaultConversation()
        {
            var c = new Conversation
            {
                Model = !string.IsNullOrEmpty(_modelConfiguration?.Model) ? _modelConfiguration!.Model : PipelineOptions.Value.Providers.OpenAI.ChatModel!,
                UserId = UserId
            };

            var message = new ConversationMessage("system", "You are a helpful assistant.");

            c.AddMessage(message);
            return c;
        }

        public async Task SendMessage(string message, string role = "user")
        {
            if (role == "user")
            {
                prompt += message;
                await SendConversation();
            }
            else
            {
                throw new NotImplementedException("Other types of messages than user is not supported");
            }
        }

        private async Task SendConversation()
        {
            await SendConversation(false);
        }

        private async Task CancelSend()
        {
            await _cancellationTokenSource.CancelAsync();
        }

        private async Task SendConversation(bool rerun)
        {
            // todo: is this necessary anymore? k
            if (IsBusy) return;

            IsBusy = true;

            _cancellationTokenSource = new CancellationTokenSource(5 * 60 * 1000);

            _modelConfiguration = await _modelConfigurationService.GetConfig();

            _kernel = KernelToUse ?? await KernelService.CreateKernelAsync(provider: _modelConfiguration.Provider, model: _modelConfiguration!.Model);

            var interceptorKeyExists = await LocalStorageService.ContainKeyAsync(Constants.InterceptorsKey);
            var interceptorNames = interceptorKeyExists ? await LocalStorageService.GetItemAsync<List<string>>(Constants.InterceptorsKey) : [];

            prompt = prompt?.TrimEnd('\n');

            if (!Conversation.HasStarted())
            {
                var selected = /*_profileSelectorStart != null ? _profileSelectorStart.SelectedProfiles :*/ new List<QuickProfile>();

                string startMsg = string.Join(" ", selected.Select(p => p.Content));
                if (!string.IsNullOrEmpty(startMsg))
                    startMsg += "\n\n";

                if (!rerun)
                {
                    var userMessage = new ConversationMessage("user", startMsg + prompt!);
                    foreach (var item in attachments)
                    {
                        userMessage.MessageAttachments.Add(item);
                    }
                    Conversation.AddMessage(userMessage);
                    Conversation.DateStarted = DateTime.UtcNow;
                }

            }
            else if (!rerun)
            {
                var userMessage = new ConversationMessage("user", prompt!);
                foreach (var item in attachments)
                {
                    userMessage.MessageAttachments.Add(item);
                }
                Conversation.AddMessage(userMessage);

            }

            prompt = "";
            attachments.Clear();
            StateHasChanged();
            await Interop.ScrollToBottom("convStackId");

            try
            {
                var c = Conversation;
                Conversation = await InterceptorHandler.Send(_kernel,
                    Conversation,
                    enabledInterceptors: null,
                    enabledInterceptorNames: interceptorNames,
                    OnStreamCompletion,
                    cancellationToken: _cancellationTokenSource.Token);

                await Send();

                if (Conversation.InitStage())
                {
                    var selectedEnd = new List<QuickProfile>();
                    //var selectedEnd = _profileSelectorEnd != null
                    //    ? _profileSelectorEnd.SelectedProfiles
                    //    : new List<QuickProfile>();
                    if (selectedEnd.Any())
                        foreach (var profile in selectedEnd)
                        {
                            Conversation.AddMessage(new ConversationMessage("user", profile.Content));


                            StateHasChanged();
                            await Send();
                        }
                }
            }

            catch (OperationCanceledException)
            {
                var res = await DialogService.ShowErrorAsync("The operation was cancelled");
                Conversation.Messages.RemoveAt(Conversation.Messages.Count - 1);
            }

            catch (Exception e)
            {
                var res = await DialogService.ShowErrorAsync(e.StackTrace,
                    "An error occurred. Please try again/later. " + e.Message);
                Conversation.Messages.RemoveAt(Conversation.Messages.Count - 1);
            }
            finally
            {
                semaphoreSlim.Release();
            }


            IsBusy = false;
            StateHasChanged();
            await Interop.FocusElement(promptTextArea.Element);
            await Interop.ScrollToBottom("convStackId");
        }

        private async Task Send()
        {
            try
            {
                if (!Conversation.StopRequested)
                {
                    _modelConfiguration ??= await _modelConfigurationService.GetConfig();

                    Conversation.AddMessage("assistant", "");
                    StateHasChanged();
                    await Interop.ScrollToBottom("convStackId");


                    var chatRequestSettings = new ChatRequestSettings();
                    chatRequestSettings.ExtensionData["max_tokens"] = _modelConfiguration!.MaxTokens;
                    chatRequestSettings.ExtensionData["temperature"] = _modelConfiguration!.Temperature;

                    Conversation = await
                        KernelService.ChatCompletionAsStreamAsync(_kernel, Conversation, chatRequestSettings, OnStreamCompletion, cancellationToken: _cancellationTokenSource.Token);

                }


                await using var ctx = await DbContextFactory.CreateDbContextAsync();

                bool isNew = false;
                bool wasSummarized = false;

                if (Conversation.Id == null || Conversation.Id == default(Guid))
                {
                    Conversation.UserId = UserId;
                    Conversation.Model = _modelConfiguration!.Model!;
                    isNew = true;

                    //if (_profileSelectorStart != null)
                    //{
                    //    foreach (var p in _profileSelectorStart.SelectedProfiles)
                    //    {
                    //        ctx.Attach(p);
                    //        Conversation.QuickProfiles.Add(p);
                    //    }
                    //}

                    ctx.Conversations.Add(Conversation);
                }
                else
                {
                    ctx.Attach(Conversation);
                }


                if (Conversation.Summary == null)
                {
                    var last = Conversation.Messages.First(m => m.Role == ConversationRole.User).Content;
                    Conversation.Summary = last.Substring(0, last.Length >= 75 ? 75 : last.Length);
                    wasSummarized = true;
                }

                await ctx.SaveChangesAsync();

                Conversation =
                    await InterceptorHandler.Receive(_kernel, Conversation,
                       enabledInterceptorNames: await LocalStorageService.GetItemAsync<List<string>>(Constants.InterceptorsKey));

                if (/*!BotMode &&*/ wasSummarized)
                {
                    StateHasChanged();
                }


                if (isNew)
                {
                    NavigationManager.NavigateTo(
                        (/*BotMode || */!string.IsNullOrEmpty(NewDestinationPrefix)) ? NewDestinationPrefix + "/" + Conversation.Id
                                : "/conversation/" + Conversation.Id,
                        false);
                }

                StateHasChanged();

            }
            catch (OperationCanceledException)
            {
                var res = await DialogService.ShowErrorAsync("The operation was cancelled");
                Conversation.Messages.RemoveAt(Conversation.Messages.Count - 1);
            }
            catch (Exception e)
            {
                var res = await DialogService.ShowErrorAsync(e.StackTrace,
                    "An error occurred. Please try again/later. " + e.Message);
                Conversation.Messages.RemoveAt(Conversation.Messages.Count - 1);
            }
            finally
            {
                IsBusy = false;
                semaphoreSlim.Release();

            }

            prompt = "";
            promptIsReady = false;
        }

        private async Task<string> OnStreamCompletion(string s)
        {
            Conversation.Messages.Last().Content += s;
            await Interop.ScrollToBottom("convStackId");

            StateHasChanged();
            return s;
        }

        public bool IsBusy { get; set; }
        public bool SendButtonDisabled => SendDisabled();

        public bool SendDisabled()
        {
            if (IsBusy)
            {
                return true;
            }


            //if (!Conversation.IsStarted() && _profileSelectorStart != null && _profileSelectorStart.SelectedProfiles.Any())
            //{
            //    return false;
            //}

            if (promptIsReady) return false;

            return true;

        }

        private async Task OnPromptInput(ChangeEventArgs args)
        {
            promptIsReady = !string.IsNullOrEmpty(args.Value?.ToString());
        }

        private async Task OnPromptKeyUp(KeyboardEventArgs obj)
        {
            if (obj.Key == "Enter" && obj.ShiftKey == false)
            {
                await Interop.Blurelement(promptTextArea.Element);
                StateHasChanged();
                await SendConversation();
            }
        }

        private async Task OnSelectedModelChanged() 
        {
            var config = await _modelConfigurationService.GetConfig();
            var selectedProvider = Enum.Parse<ChatModelsProvider>(selectedModelValue);

            switch (selectedProvider)
            {
                case ChatModelsProvider.OpenAI:
                    config = new ModelConfiguration()
                    {
                        Provider = ChatModelsProvider.OpenAI,
                        Model = PipelineOptions.Value.Providers.OpenAI.ChatModel,
                        MaxTokens = PipelineOptions.Value.MaxTokens,
                        MaxPlannerTokens = PipelineOptions.Value.MaxPlannerTokens,
                        Temperature = 0.0f,
                        EmbeddingsModel = PipelineOptions.Value.Providers.OpenAI.EmbeddingsModel,
                        EmbeddingsProvider = EmbeddingsModelProvider.OpenAI
                    };
                    break;
                case ChatModelsProvider.AzureOpenAI:
                    config = new ModelConfiguration()
                    {
                        Provider = ChatModelsProvider.AzureOpenAI,
                        Model = PipelineOptions.Value.Providers.AzureOpenAI.ChatModel,
                        MaxTokens = PipelineOptions.Value.MaxTokens,
                        MaxPlannerTokens = PipelineOptions.Value.MaxPlannerTokens,
                        Temperature = 0.0f,
                        EmbeddingsModel = PipelineOptions.Value.Providers.AzureOpenAI.EmbeddingsModel,
                        EmbeddingsProvider = EmbeddingsModelProvider.AzureOpenAI
                    };
                    break;
                case ChatModelsProvider.Ollama:
                    config = new ModelConfiguration()
                    {
                        Provider = ChatModelsProvider.Ollama,
                        Model = PipelineOptions.Value.Providers.Ollama.ChatModel,
                        MaxTokens = PipelineOptions.Value.MaxTokens,
                        MaxPlannerTokens = PipelineOptions.Value.MaxPlannerTokens,
                        Temperature = 0.0f,
                        EmbeddingsModel = PipelineOptions.Value.Providers.Ollama.EmbeddingsModel,
                        EmbeddingsProvider = EmbeddingsModelProvider.Ollama
                    };
                    break;
                case ChatModelsProvider.XAi:
                    config = new ModelConfiguration()
                    {
                        Provider = ChatModelsProvider.XAi,
                        Model = PipelineOptions.Value.Providers.XAi.ChatModel,
                        MaxTokens = PipelineOptions.Value.MaxTokens,
                        MaxPlannerTokens = PipelineOptions.Value.MaxPlannerTokens,
                        Temperature = 0.0f,
                        EmbeddingsModel = PipelineOptions.Value.Providers.XAi.EmbeddingsModel,
                        EmbeddingsProvider = EmbeddingsModelProvider.XAi
                    };
                    break;
                case ChatModelsProvider.Gemini:
                    config = new ModelConfiguration()
                    {
                        Provider = ChatModelsProvider.Gemini,
                        Model = PipelineOptions.Value.Providers.Gemini.ChatModel,
                        MaxTokens = PipelineOptions.Value.MaxTokens,
                        MaxPlannerTokens = PipelineOptions.Value.MaxPlannerTokens,
                        Temperature = 0.0f,
                        EmbeddingsModel = PipelineOptions.Value.Providers.Gemini.EmbeddingsModel,
                        EmbeddingsProvider = EmbeddingsModelProvider.Gemini
                    };
                    break;
                case ChatModelsProvider.GitHub:
                    config = new ModelConfiguration()
                    {
                        Provider = ChatModelsProvider.GitHub,
                        Model = PipelineOptions.Value.Providers.GitHub.ChatModel,
                        MaxTokens = PipelineOptions.Value.MaxTokens,
                        MaxPlannerTokens = PipelineOptions.Value.MaxPlannerTokens,
                        Temperature = 0.0f,
                        EmbeddingsModel = PipelineOptions.Value.Providers.GitHub.EmbeddingsModel,
                        EmbeddingsProvider = EmbeddingsModelProvider.GitHub
                    };
                    break;
                case ChatModelsProvider.DeepSeek:
                    config = new ModelConfiguration()
                    {
                        Provider = ChatModelsProvider.DeepSeek,
                        Model = PipelineOptions.Value.Providers.DeepSeek.ChatModel,
                        MaxTokens = PipelineOptions.Value.MaxTokens,
                        MaxPlannerTokens = PipelineOptions.Value.MaxPlannerTokens,
                        Temperature = 0.0f,
                        EmbeddingsModel = PipelineOptions.Value.Providers.DeepSeek.EmbeddingsModel,
                        EmbeddingsProvider = EmbeddingsModelProvider.DeepSeek
                    };
                    break;
                case ChatModelsProvider.Anthropic:
                    config = new ModelConfiguration()
                    {
                        Provider = ChatModelsProvider.Anthropic,
                        Model = PipelineOptions.Value.Providers.Anthropic.ChatModel,
                        MaxTokens = PipelineOptions.Value.MaxTokens,
                        MaxPlannerTokens = PipelineOptions.Value.MaxPlannerTokens,
                        Temperature = 0.0f,
                        EmbeddingsModel = PipelineOptions.Value.Providers.Anthropic.EmbeddingsModel,
                        EmbeddingsProvider = EmbeddingsModelProvider.Anthropic
                    };
                    break;
                default:
                    break;
            }

            await _modelConfigurationService.SaveConfig(config);
        }

        void OnCompleted(IEnumerable<FluentInputFileEventArgs> files)
        {
            Files = files.ToArray();
            progressPercent = myFileUploader!.ProgressPercent;
            progressTitle = myFileUploader!.ProgressTitle;

            // For the demo, delete these files.
            foreach (var file in Files)
            {
                var attached = new MessageAttachment
                {
                    Name = file.Name,
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "text/html" : file.ContentType,
                    Content = File.ReadAllBytes(file.LocalFile.FullName)
                };

                attachments.Add(attached);
                file.LocalFile?.Delete();
            }
            _formUploading = false;
            StateHasChanged();
        }

        private async Task RemoveAttachment(MessageAttachment attachment)
        {
            IDialogReference dialog = await DialogService.ShowConfirmationAsync(
                $"Are you sure you want to remove '{attachment.Name}'?",
                "Yes",
                "No",
                "Remove Attachment?");

            DialogResult? result = await dialog.Result;

            // If cancelled, return
            if (result.Cancelled)
            {
                return;
            }

            attachments.Remove(attachment);
            var message = $"'{attachment.Name}' removed";
            ToastService.ShowToast(ToastIntent.Info, message);
        }
    }
}
