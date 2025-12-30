using AgiExperiment.AI.Domain.Data;
using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AgiExperiment.Tests.Data;

[TestFixture]
public class ConversationsRepositoryTests
{
    private Mock<IDbContextFactory<AiExperimentDBContext>> _mockDbContextFactory;
    private Mock<QuickProfileRepository> _mockQuickProfileRepository;
    private AiExperimentDBContext _dbContext;
    private ConversationsRepository _repository;

    [SetUp]
    public void SetUp()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<AiExperimentDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AiExperimentDBContext(options);
        
        _mockDbContextFactory = new Mock<IDbContextFactory<AiExperimentDBContext>>();
        _mockDbContextFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dbContext);
        
        _mockDbContextFactory
            .Setup(f => f.CreateDbContext())
            .Returns(_dbContext);

        _mockQuickProfileRepository = new Mock<QuickProfileRepository>(
            _mockDbContextFactory.Object);

        _repository = new ConversationsRepository(
            _mockDbContextFactory.Object,
            _mockQuickProfileRepository.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public async Task SaveConversation_ShouldAddConversationToDatabase()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user",
            Summary = "Test conversation",
            DateStarted = DateTime.Now
        };
        conversation.AddMessage("system", "You are a helpful assistant");
        conversation.AddMessage("user", "Hello!");

        // Act
        var result = await _repository.SaveConversation(conversation);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.Not.Null);
        var saved = await _dbContext.Conversations.FindAsync(result.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved.Summary, Is.EqualTo("Test conversation"));
    }

    [Test]
    public async Task GetConversationsByUserId_ShouldReturnUserConversations()
    {
        // Arrange
        var userId = "test-user";
        var conversation1 = new Conversation
        {
            Model = "gpt-4",
            UserId = userId,
            Summary = "Conversation 1",
            DateStarted = DateTime.Now.AddDays(-1)
        };
        var conversation2 = new Conversation
        {
            Model = "gpt-4",
            UserId = userId,
            Summary = "Conversation 2",
            DateStarted = DateTime.Now
        };
        var conversation3 = new Conversation
        {
            Model = "gpt-4",
            UserId = "other-user",
            Summary = "Other user conversation",
            DateStarted = DateTime.Now
        };

        await _dbContext.Conversations.AddRangeAsync(conversation1, conversation2, conversation3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetConversationsByUserId(userId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(c => c.UserId == userId), Is.True);
        Assert.That(result[0].DateStarted, Is.GreaterThan(result[1].DateStarted)); // Ordered by date descending
    }

    [Test]
    public async Task GetConversationsByUserId_ShouldExcludeConversationsWithoutSummary()
    {
        // Arrange
        var userId = "test-user";
        var conversationWithSummary = new Conversation
        {
            Model = "gpt-4",
            UserId = userId,
            Summary = "Has summary",
            DateStarted = DateTime.Now
        };
        var conversationWithoutSummary = new Conversation
        {
            Model = "gpt-4",
            UserId = userId,
            Summary = null,
            DateStarted = DateTime.Now
        };

        await _dbContext.Conversations.AddRangeAsync(conversationWithSummary, conversationWithoutSummary);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetConversationsByUserId(userId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Summary, Is.EqualTo("Has summary"));
    }

    [Test]
    public async Task GetConversationsByUserIdSimple_ShouldReturnLimitedFields()
    {
        // Arrange
        var userId = "test-user";
        for (int i = 0; i < 15; i++)
        {
            var conversation = new Conversation
            {
                Model = "gpt-4",
                UserId = userId,
                Summary = $"Conversation {i}",
                DateStarted = DateTime.Now.AddDays(-i)
            };
            await _dbContext.Conversations.AddAsync(conversation);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetConversationsByUserIdSimple(userId, 10);

        // Assert
        Assert.That(result.Count, Is.EqualTo(10));
        Assert.That(result.All(c => c.Id != null), Is.True);
        Assert.That(result.All(c => c.Summary != null), Is.True);
    }

    [Test]
    public async Task UpdateConversation_ShouldModifyExistingConversation()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user",
            Summary = "Original summary",
            DateStarted = DateTime.Now
        };
        await _dbContext.Conversations.AddAsync(conversation);
        await _dbContext.SaveChangesAsync();

        // Act
        conversation.Summary = "Updated summary";
        await _repository.UpdateConversation(conversation);

        // Assert
        var updated = await _dbContext.Conversations.FindAsync(conversation.Id);
        Assert.That(updated.Summary, Is.EqualTo("Updated summary"));
    }

    [Test]
    public async Task DeleteConversationsByUserId_ShouldRemoveAllUserConversations()
    {
        // Arrange
        var userId = "test-user";
        var conversation1 = new Conversation
        {
            Model = "gpt-4",
            UserId = userId,
            Summary = "Conv 1",
            DateStarted = DateTime.Now
        };
        var conversation2 = new Conversation
        {
            Model = "gpt-4",
            UserId = userId,
            Summary = "Conv 2",
            DateStarted = DateTime.Now
        };

        await _dbContext.Conversations.AddRangeAsync(conversation1, conversation2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _repository.DeleteConversationsByUserId(userId);

        // Assert
        var remaining = await _dbContext.Conversations
            .Where(c => c.UserId == userId)
            .ToListAsync();
        Assert.That(remaining.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetConversation_ShouldReturnConversationWithMessages()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user",
            Summary = "Test conversation",
            DateStarted = DateTime.Now
        };
        conversation.AddMessage("system", "System message");
        conversation.AddMessage("user", "User message");

        await _dbContext.Conversations.AddAsync(conversation);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetConversation(conversation.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Messages.Count, Is.EqualTo(2));
        Assert.That(result.Messages[0].Content, Is.EqualTo("System message"));
    }

    [Test]
    public async Task GetConversation_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetConversation(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateMessageContent_ShouldModifyMessageContent()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user",
            Summary = "Test",
            DateStarted = DateTime.Now
        };
        conversation.AddMessage("user", "Original content");
        await _dbContext.Conversations.AddAsync(conversation);
        await _dbContext.SaveChangesAsync();

        var messageId = conversation.Messages[0].Id.Value;

        // Act
        await _repository.UpdateMessageContent(messageId, "Updated content");

        // Assert
        var updated = await _dbContext.Messages.FindAsync(messageId);
        Assert.That(updated.Content, Is.EqualTo("Updated content"));
    }

    [Test]
    public async Task GetMessage_ShouldReturnMessageById()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user",
            Summary = "Test",
            DateStarted = DateTime.Now
        };
        conversation.AddMessage("user", "Test message");
        await _dbContext.Conversations.AddAsync(conversation);
        await _dbContext.SaveChangesAsync();

        var messageId = conversation.Messages[0].Id.Value;

        // Act
        var result = await _repository.GetMessage(messageId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content, Is.EqualTo("Test message"));
        Assert.That(result.Conversation, Is.Not.Null);
    }
}
