using AgiExperiment.AI.Cortex.Pipeline;
using AgiExperiment.AI.Domain.Data;
using Microsoft.Extensions.Options;

namespace AgiExperiment.Tests.Pipeline;

[TestFixture]
public class KernelServiceTests
{
    [Test]
    public void Constructor_ShouldInitializeWithOptions()
    {
        // Arrange
        var pipelineOptions = new PipelineOptions
        {
            Providers = new ModelsProvidersOptions
            {
                OpenAI = new OpenAIModelsOptions
                {
                    ApiKey = "test-api-key",
                    ChatModel = "gpt-4",
                    EmbeddingsModel = "text-embedding-ada-002"
                }
            }
        };
        var options = Options.Create(pipelineOptions);

        // Act
        var service = new KernelService(options);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public async Task CreateKernelAsync_WithValidOpenAIProvider_ShouldCreateKernel()
    {
        // Arrange
        var options = Options.Create(new PipelineOptions
        {
            Providers = new ModelsProvidersOptions
            {
                OpenAI = new OpenAIModelsOptions
                {
                    ApiKey = "test-key",
                    ChatModel = "gpt-4",
                    EmbeddingsModel = "text-embedding-ada-002"
                }
            }
        });
        var service = new KernelService(options);

        // Act
        var kernel = await service.CreateKernelAsync();

        // Assert
        Assert.That(kernel, Is.Not.Null);
    }

    [Test]
    public async Task CreateKernelAsync_WithEmptyModel_ShouldUseDefaultModel()
    {
        // Arrange
        var options = Options.Create(new PipelineOptions
        {
            Providers = new ModelsProvidersOptions
            {
                OpenAI = new OpenAIModelsOptions
                {
                    ApiKey = "test-key",
                    ChatModel = "gpt-4",
                    EmbeddingsModel = "text-embedding-ada-002"
                }
            }
        });
        var service = new KernelService(options);

        // Act
        var kernel = await service.CreateKernelAsync("");

        // Assert
        Assert.That(kernel, Is.Not.Null);
    }

    [Test]
    public async Task CreateKernelAsync_WithSpecificModel_ShouldCreateKernel()
    {
        // Arrange
        var options = Options.Create(new PipelineOptions
        {
            Providers = new ModelsProvidersOptions
            {
                OpenAI = new OpenAIModelsOptions
                {
                    ApiKey = "test-key",
                    ChatModel = "gpt-4",
                    EmbeddingsModel = "text-embedding-ada-002"
                }
            }
        });
        var service = new KernelService(options);

        // Act
        var kernel = await service.CreateKernelAsync("gpt-3.5-turbo");

        // Assert
        Assert.That(kernel, Is.Not.Null);
    }
}
