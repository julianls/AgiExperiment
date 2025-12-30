using AgiExperiment.AI.Domain.Data.Model;
using AgiExperiment.AI.Cortex.Pipeline;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace AgiExperiment.Tests.Pipeline;

#pragma warning disable SKEXP0001

[TestFixture]
public class ChatExtensionsTests
{
    [Test]
    public void ToChatHistory_ShouldConvertConversationMessages()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user"
        };
        conversation.AddMessage("system", "You are helpful");
        conversation.AddMessage("user", "Hello");
        conversation.AddMessage("assistant", "Hi there");

        // Act
        var chatHistory = conversation.ToChatHistory();

        // Assert
        Assert.That(chatHistory.Count, Is.EqualTo(3));
        Assert.That(chatHistory[0].Role, Is.EqualTo(AuthorRole.System));
        Assert.That(chatHistory[0].Content, Is.EqualTo("You are helpful"));
        Assert.That(chatHistory[1].Role, Is.EqualTo(AuthorRole.User));
        Assert.That(chatHistory[1].Content, Is.EqualTo("Hello"));
        Assert.That(chatHistory[2].Role, Is.EqualTo(AuthorRole.Assistant));
        Assert.That(chatHistory[2].Content, Is.EqualTo("Hi there"));
    }

    [Test]
    public void ToChatHistory_ShouldSkipEmptyMessages()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user"
        };
        conversation.AddMessage("system", "System message");
        conversation.AddMessage("user", "   "); // Only whitespace
        conversation.AddMessage("user", "Real message");

        // Act
        var chatHistory = conversation.ToChatHistory();

        // Assert
        Assert.That(chatHistory.Count, Is.EqualTo(2));
        Assert.That(chatHistory[1].Content, Is.EqualTo("Real message"));
    }

    [Test]
    public void ToConversation_ShouldConvertChatHistoryMessages()
    {
        // Arrange
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are helpful");
        chatHistory.AddUserMessage("Hello");
        chatHistory.AddAssistantMessage("Hi there");

        // Act
        var conversation = chatHistory.ToConversation();

        // Assert
        Assert.That(conversation.Messages.Count, Is.EqualTo(3));
        Assert.That(conversation.Messages[0].Role, Is.EqualTo("system"));
        Assert.That(conversation.Messages[0].Content, Is.EqualTo("You are helpful"));
        Assert.That(conversation.Messages[1].Role, Is.EqualTo("user"));
        Assert.That(conversation.Messages[1].Content, Is.EqualTo("Hello"));
        Assert.That(conversation.Messages[2].Role, Is.EqualTo("assistant"));
        Assert.That(conversation.Messages[2].Content, Is.EqualTo("Hi there"));
    }

    [Test]
    public void ToMessageContentItemCollection_WithTextOnly_ShouldReturnTextContent()
    {
        // Arrange
        var message = new ConversationMessage("user", "Test message");

        // Act
        var collection = message.ToMessageContentItemCollection();

        // Assert
        Assert.That(collection.Count, Is.EqualTo(1));
        Assert.That(collection[0], Is.InstanceOf<TextContent>());
        var textContent = (TextContent)collection[0];
        Assert.That(textContent.Text, Is.EqualTo("Test message"));
    }

    [Test]
    public void ToMessageContentItemCollection_WithImageAttachment_ShouldIncludeImageContent()
    {
        // Arrange
        var message = new ConversationMessage("user", "Check this image");
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        message.MessageAttachments.Add(new MessageAttachment
        {
            ContentType = "image/png",
            Content = imageBytes,
            Name = "test.png"
        });

        // Act
        var collection = message.ToMessageContentItemCollection();

        // Assert
        Assert.That(collection.Count, Is.EqualTo(2));
        Assert.That(collection[0], Is.InstanceOf<TextContent>());
        Assert.That(collection[1], Is.InstanceOf<ImageContent>());
        var imageContent = (ImageContent)collection[1];
        Assert.That(imageContent.Data, Is.Not.Null);
    }

    [Test]
    public void ToMessageContentItemCollection_WithTextAttachment_ShouldIncludeTextContent()
    {
        // Arrange
        var message = new ConversationMessage("user", "Main message");
        var attachmentText = "Additional text content";
        message.MessageAttachments.Add(new MessageAttachment
        {
            ContentType = "text/plain",
            Content = Encoding.Default.GetBytes(attachmentText),
            Name = "attachment.txt"
        });

        // Act
        var collection = message.ToMessageContentItemCollection();

        // Assert
        Assert.That(collection.Count, Is.EqualTo(2));
        Assert.That(collection[0], Is.InstanceOf<TextContent>());
        Assert.That(collection[1], Is.InstanceOf<TextContent>());
        var attachmentContent = (TextContent)collection[1];
        Assert.That(attachmentContent.Text, Is.EqualTo(attachmentText));
    }

    [Test]
    public void ToMessageContentItemCollection_WithAudioAttachment_ShouldIncludeAudioContent()
    {
        // Arrange
        var message = new ConversationMessage("user", "Listen to this");
        var audioBytes = new byte[] { 0xFF, 0xFB, 0x90 }; // MP3 header
        message.MessageAttachments.Add(new MessageAttachment
        {
            ContentType = "audio/mpeg",
            Content = audioBytes,
            Name = "audio.mp3"
        });

        // Act
        var collection = message.ToMessageContentItemCollection();

        // Assert
        Assert.That(collection.Count, Is.EqualTo(2));
        Assert.That(collection[0], Is.InstanceOf<TextContent>());
        Assert.That(collection[1], Is.InstanceOf<AudioContent>());
        var audioContent = (AudioContent)collection[1];
        Assert.That(audioContent.Data, Is.Not.Null);
    }

    [Test]
    public void ToMessageContentItemCollection_WithJsonAttachment_ShouldIncludeTextContent()
    {
        // Arrange
        var message = new ConversationMessage("user", "JSON data");
        var jsonContent = "{\"key\": \"value\"}";
        message.MessageAttachments.Add(new MessageAttachment
        {
            ContentType = "application/json",
            Content = Encoding.Default.GetBytes(jsonContent),
            Name = "data.json"
        });

        // Act
        var collection = message.ToMessageContentItemCollection();

        // Assert
        Assert.That(collection.Count, Is.EqualTo(2));
        Assert.That(collection[1], Is.InstanceOf<TextContent>());
        var jsonTextContent = (TextContent)collection[1];
        Assert.That(jsonTextContent.Text, Is.EqualTo(jsonContent));
    }

    [Test]
    public void ToChatHistory_WithMultipleAttachments_ShouldHandleCorrectly()
    {
        // Arrange
        var conversation = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user"
        };
        var message = new ConversationMessage("user", "Multiple attachments");
        message.MessageAttachments.Add(new MessageAttachment
        {
            ContentType = "image/png",
            Content = new byte[] { 0x89, 0x50, 0x4E, 0x47 },
            Name = "image.png"
        });
        message.MessageAttachments.Add(new MessageAttachment
        {
            ContentType = "text/plain",
            Content = Encoding.UTF8.GetBytes("Text content"),
            Name = "text.txt"
        });
        conversation.Messages.Add(message);

        // Act
        var chatHistory = conversation.ToChatHistory();

        // Assert
        Assert.That(chatHistory.Count, Is.EqualTo(1));
        Assert.That(chatHistory[0].Items.Count, Is.GreaterThan(1));
    }

    [Test]
    public void ConversionRoundTrip_ShouldPreserveMessageContent()
    {
        // Arrange
        var original = new Conversation
        {
            Model = "gpt-4",
            UserId = "test-user"
        };
        original.AddMessage("system", "System message");
        original.AddMessage("user", "User question");
        original.AddMessage("assistant", "Assistant response");

        // Act
        var chatHistory = original.ToChatHistory();
        var converted = chatHistory.ToConversation();

        // Assert
        Assert.That(converted.Messages.Count, Is.EqualTo(original.Messages.Count));
        for (int i = 0; i < original.Messages.Count; i++)
        {
            Assert.That(converted.Messages[i].Role, Is.EqualTo(original.Messages[i].Role));
            Assert.That(converted.Messages[i].Content, Is.EqualTo(original.Messages[i].Content));
        }
    }
}

#pragma warning restore SKEXP0001
