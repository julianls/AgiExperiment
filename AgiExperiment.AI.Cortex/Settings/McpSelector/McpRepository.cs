using ModelContextProtocol.Client;

namespace AgiExperiment.AI.Cortex.Settings.McpSelector;

public class McpRepository
{
    public record McpServer(string Name, string Type);

    // Predefined known MCP servers. Type can be "stdio" or "sse" for now.
    public IEnumerable<McpServer> All()
    {
        // Add more as needed.
        yield return new("Playwright", "stdio");
        yield return new("GitHub", "stdio");
        yield return new("AspNetCoreSse", "sse");
    }
}
