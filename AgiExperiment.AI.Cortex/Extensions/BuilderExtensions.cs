using AgiExperiment.AI.Cortex.Memories;
using AgiExperiment.AI.Cortex.Pipeline;
using AgiExperiment.AI.Cortex.Pipeline.Interceptors;
using AgiExperiment.AI.Cortex.Settings;
using AgiExperiment.AI.Cortex.Settings.PluginSelector;
using AgiExperiment.AI.Domain.Data;
using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgiExperiment.AI.Cortex.Extensions
{
    public static class BuilderExtensions
    {
        public static IServiceCollection AddAgiExperiment(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IConfiguration>(config);

            services.AddMemoryCache();

            services.Configure<PipelineOptions>(config.GetSection("PipelineOptions")); ;

            services.AddDbContextFactory<AiExperimentDBContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("AgiDataConnection"), o =>
                {
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            });

            services.AddScoped<KernelService>();
            services.AddScoped<FlowService>();

            services.AddScoped<ScriptRepository>();
            services.AddScoped<DiagramRepository>();
            services.AddScoped<QuickProfileRepository>();
            services.AddScoped<ConversationsRepository>();
            services.AddScoped<StateRepository>();

            services.AddScoped<SampleDataSeeder>();
            services.AddScoped<ChatWrapper>();

            services.AddScoped<IQuickProfileHandler, QuickProfileHandler>();
            services.AddScoped<IInterceptorHandler, InterceptorHandler>();
            services.AddScoped<IInterceptor, JsonStateInterceptor>();
            services.AddScoped<IInterceptor, StateFileSaveInterceptor>();
            services.AddSingleton<StateHasChangedInterceptorService>();
            services.AddScoped<IInterceptor, StateHasChangedInterceptor>();
            services.AddScoped<IInterceptor, EmbeddingsInterceptor>();
            services.AddScoped<PluginsRepository>();
            services.AddScoped<InterceptorRepository>();

            services.AddScoped<ModelConfigurationService>();
            services.AddScoped<InterceptorConfigurationService>();
            services.AddScoped<PluginsConfigurationService>();

            services.AddSingleton<ConversationTreeState>();

            services.AddScoped<UserStorageService>();

            services.AddSingleton<CurrentConversationState>();
            services.AddTransient<FunctionCallingFilter>();

            services.AddScoped<MemoriesService>();

            return services;
        }
    }
}
