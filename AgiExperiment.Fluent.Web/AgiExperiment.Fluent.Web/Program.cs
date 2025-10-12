using AgiExperiment.Fluent.Web;
using AgiExperiment.Fluent.Web.Components;
using AgiExperiment.Fluent.Web.Components.Account;
using AgiExperiment.Fluent.Web.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using AgiExperiment.AI.Cortex.Extensions;
using AgiExperiment.AI.Cortex.Pipeline.Interceptors;
using AgiExperiment.AI.Cortex.Settings;
using Microsoft.Extensions.FileProviders;
using AgiExperiment.AI.Cortex.Pipeline;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
//builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
{
    // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
    // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
    client.BaseAddress = new("https+http://apiservice");
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Configure gpt pipeline.
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddTransient<FunctionApprovalFilter>();
builder.Services.AddTransient<IFunctionApprovalService, FunctionCallingDialogApprovalService>();
builder.Services.AddSingleton<SettingsStateNotificationService>();

// web mode, other apps use FunctionCallingUserConsoleProvider
builder.Services.AddTransient<IFunctionCallingUserProvider, FunctionCallingUserWebProvider>();
builder.Services.AddAgiExperiment(builder.Configuration);

builder.Services.AddScoped<ConversationInterop>();

// add MCP client
builder.Services.AddScoped<McpClient>(sp =>
{
    McpClientOptions mcpClientOptions = new()
    { ClientInfo = new() { Name = "AspNetCoreSseClient", Version = "1.0.0" } };

    // can't use the service discovery for ["https +http://agiexperiment-mcp-aspnetcoresseserver"]
    // fix: read the environment value for the key 'services__agiexperiment-mcp-aspnetcoresseserver__https__0' to get the url for the aspnet core sse server
    var serviceName = "agiexperiment-mcp-aspnetcoresseserver";
    var name = $"services__{serviceName}__https__0";
    var url = Environment.GetEnvironmentVariable(name) + "/sse";

    var mcpServerConfig = new HttpClientTransport(new()
    {
        Name = "AspNetCoreSse",
        Endpoint = new Uri(url)
    });

    var mcpClient = McpClient.CreateAsync(mcpServerConfig, mcpClientOptions).GetAwaiter().GetResult();
    return mcpClient;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "MyStaticFiles")),
    RequestPath = "/StaticFiles"
}); 
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AgiExperiment.Fluent.Web.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
