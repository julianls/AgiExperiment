using AgiExperiment.AI.Domain.Data.Model;
using System.Text;

namespace AgiExperiment.Tests.Domain;

[TestFixture]
public class ConversationMessageTests
{
    [Test]
    public void Constructor_ShouldInitializeRoleAndContent()
    {
        // Arrange
        var role = "user";
        var content = "Hello, world!";

        // Act
        var message = new ConversationMessage(role, content);

        // Assert
        Assert.That(message.Role, Is.EqualTo(role));
        Assert.That(message.Content, Is.EqualTo(content));
    }

    [Test]
    public void ConversationMessage_ShouldInitializeEmptyCollections()
    {
        // Act
        var message = new ConversationMessage("user", "Test");

        // Assert
        Assert.That(message.MessageAttachments, Is.Not.Null);
        Assert.That(message.MessageAttachments, Is.Empty);
        Assert.That(message.BranchedConversations, Is.Not.Null);
        Assert.That(message.BranchedConversations, Is.Empty);
    }

    [Test]
    public void Date_ShouldBeSetToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var message = new ConversationMessage("assistant", "Response");
        var after = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.That(message.Date, Is.GreaterThan(before));
        Assert.That(message.Date, Is.LessThan(after));
    }

    [Test]
    public void ConversationMessage_ShouldAllowNullableProperties()
    {
        // Act
        var message = new ConversationMessage("system", "System message")
        {
            Id = null,
            ConversationId = null,
            Conversation = null,
            State = null,
            PromptTokens = null,
            CompletionTokens = null,
            ActionLog = null
        };

        // Assert
        Assert.That(message.Id, Is.Null);
        Assert.That(message.ConversationId, Is.Null);
        Assert.That(message.Conversation, Is.Null);
        Assert.That(message.State, Is.Null);
        Assert.That(message.PromptTokens, Is.Null);
        Assert.That(message.CompletionTokens, Is.Null);
        Assert.That(message.ActionLog, Is.Null);
    }

    [Test]
    public void ConversationMessage_ShouldSetTokenCounts()
    {
        // Arrange
        var message = new ConversationMessage("assistant", "Response")
        {
            PromptTokens = 150,
            CompletionTokens = 75
        };

        // Assert
        Assert.That(message.PromptTokens, Is.EqualTo(150));
        Assert.That(message.CompletionTokens, Is.EqualTo(75));
    }

    [Test]
    public void ConversationMessage_ShouldLinkToConversation()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            Model = "gpt-4",
            UserId = "test-user"
        };

        // Act
        var message = new ConversationMessage("user", "Question")
        {
            ConversationId = conversationId,
            Conversation = conversation
        };

        // Assert
        Assert.That(message.ConversationId, Is.EqualTo(conversationId));
        Assert.That(message.Conversation, Is.EqualTo(conversation));
    }
}
