var builder = DistributedApplication.CreateBuilder(args);

//var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.AgiExperiment_ApiService>("apiservice");

//builder.AddProject<Projects.AgiExperiment_Web>("webfrontend")
//    .WithExternalHttpEndpoints()
//    .WithReference(cache)
//    .WaitFor(cache)
//    .WithReference(apiService)
//    .WaitFor(apiService);

var mcpService = builder.AddProject<Projects.AgiExperiment_MCP_AspNetCoreSseServer>("agiexperiment-mcp-aspnetcoresseserver")
    .WithExternalHttpEndpoints();


builder.AddProject<Projects.AgiExperiment_Fluent_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(mcpService)
    //.WithReference(cache)
    //.WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

//builder.AddProject<Projects.AgiExperiment_AI_ServiceHost>("agiexperiment-ai-servicehost");

//builder.AddProject<Projects.AgiExperiment_AI_ServiceHost>("agiexperiment-ai-servicehost");

builder.Build().Run();
