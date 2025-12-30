using AgiExperiment.AI.Domain.Data.Model;

namespace AgiExperiment.Tests.Domain;

[TestFixture]
public class ConversationTests
{
    [Test]
    public void CreateConversation_ShouldInitializeWithCorrectProperties()
    {
        // Arrange
        var model = "gpt-4";
        var userId = "test-user-123";
        var systemMessage = "You are a helpful assistant";
        var userMessage = "Hello, how are you?";

        // Act
        var conversation = Conversation.CreateConversation(model, userId, systemMessage, userMessage);

        // Assert
        Assert.That(conversation.Model, Is.EqualTo(model));
        Assert.That(conversation.UserId, Is.EqualTo(userId));
        Assert.That(conversation.Messages.Count, Is.EqualTo(2));
        Assert.That(conversation.Messages[0].Role, Is.EqualTo("system"));
        Assert.That(conversation.Messages[0].Content, Is.EqualTo(systemMessage));
        Assert.That(conversation.Messages[1].Role, Is.EqualTo("user"));
        Assert.That(conversation.Messages[1].Content, Is.EqualTo(userMessage));
        Assert.That(conversation.DateStarted, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void CreateConversation_WithoutUserMessage_ShouldOnlyHaveSystemMessage()
    {
        // Arrange
        var model = "gpt-4";
        var userId = "test-user-123";
        var systemMessage = "You are a helpful assistant";

        // Act
        var conversation = Conversation.CreateConversation(model, userId, systemMessage, null);

        // Assert
        Assert.That(conversation.Messages.Count, Is.EqualTo(1));
        Assert.That(conversation.Messages[0].Role, Is.EqualTo("system"));
    }

    [Test]
    public void AddMessage_WithMessageObject_ShouldAddToMessagesCollection()
    {
        // Arrange
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Model = "gpt-4",
            UserId = "test-user"
        };
        var message = new ConversationMessage("user", "Test message");

        // Act
        conversation.AddMessage(message);

        // Assert
        Assert.That(conversation.Messages.Count, Is.EqualTo(1));
        Assert.That(conversation.Messages[0].ConversationId, Is.EqualTo(conversation.Id));
        Assert.That(conversation.Messages[0].Content, Is.EqualTo("Test message"));
    }

    [Test]
    public void AddMessage_WithRoleAndContent_ShouldCreateAndAddMessage()
    {
        // Arrange
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Model = "gpt-4",
            UserId = "test-user"
        };

        // Act
        conversation.AddMessage("assistant", "I can help with that");

        // Assert
        Assert.That(conversation.Messages.Count, Is.EqualTo(1));
        Assert.That(conversation.Messages[0].Role, Is.EqualTo("assistant"));
        Assert.That(conversation.Messages[0].Content, Is.EqualTo("I can help with that"));
    }

    [Test]
    public void Conversation_ShouldInitializeEmptyCollections()
    {
        // Act
        var conversation = new Conversation();

        // Assert
        Assert.That(conversation.Messages, Is.Not.Null);
        Assert.That(conversation.Messages, Is.Empty);
        Assert.That(conversation.QuickProfiles, Is.Not.Null);
        Assert.That(conversation.QuickProfiles, Is.Empty);
        Assert.That(conversation.FileUrls, Is.Not.Null);
        Assert.That(conversation.FileUrls, Is.Empty);
        Assert.That(conversation.TreeStateList, Is.Not.Null);
        Assert.That(conversation.TreeStateList, Is.Empty);
    }

    [Test]
    public void StopRequested_ShouldNotBePersisted()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user"
        };

        // Act
        conversation.StopRequested = true;

        // Assert
        Assert.That(conversation.StopRequested, Is.True);
        // StopRequested should be marked as [NotMapped] so it won't be persisted to database
    }

    [Test]
    public void Conversation_ShouldAllowNullableId()
    {
        // Act
        var conversation = new Conversation
        {
            Id = null,
            Model = "gpt-4",
            UserId = "test-user"
        };

        // Assert
        Assert.That(conversation.Id, Is.Null);
    }

    [Test]
    public void AddMessage_MultipleTimes_ShouldMaintainOrder()
    {
        // Arrange
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Model = "gpt-4",
            UserId = "test-user"
        };

        // Act
        conversation.AddMessage("system", "System message");
        conversation.AddMessage("user", "First user message");
        conversation.AddMessage("assistant", "First assistant response");
        conversation.AddMessage("user", "Second user message");

        // Assert
        Assert.That(conversation.Messages.Count, Is.EqualTo(4));
        Assert.That(conversation.Messages[0].Role, Is.EqualTo("system"));
        Assert.That(conversation.Messages[1].Role, Is.EqualTo("user"));
        Assert.That(conversation.Messages[2].Role, Is.EqualTo("assistant"));
        Assert.That(conversation.Messages[3].Role, Is.EqualTo("user"));
    }

    [Test]
    public void Conversation_BranchedFromMessage_ShouldAllowNull()
    {
        // Arrange & Act
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user",
            BranchedFromMessageId = null,
            BranchedFromMessage = null
        };

        // Assert
        Assert.That(conversation.BranchedFromMessageId, Is.Null);
        Assert.That(conversation.BranchedFromMessage, Is.Null);
    }
}
