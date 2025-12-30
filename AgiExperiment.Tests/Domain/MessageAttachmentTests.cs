using AgiExperiment.AI.Domain.Data.Model;
using System.Text;

namespace AgiExperiment.Tests.Domain;

[TestFixture]
public class MessageAttachmentTests
{
    [Test]
    public void IsTextContent_WithTextContentType_ShouldReturnTrue()
    {
        // Arrange
        var attachment = new MessageAttachment
        {
            ContentType = "text/plain",
            Content = Encoding.UTF8.GetBytes("Some text")
        };

        // Assert
        Assert.That(attachment.IsTextContent, Is.True);
    }

    [Test]
    public void IsTextContent_WithJsonContentType_ShouldReturnTrue()
    {
        // Arrange
        var attachment = new MessageAttachment
        {
            ContentType = "application/json",
            Content = Encoding.UTF8.GetBytes("{\"key\": \"value\"}")
        };

        // Assert
        Assert.That(attachment.IsTextContent, Is.True);
    }

    [Test]
    public void IsImageContent_WithImageContentType_ShouldReturnTrue()
    {
        // Arrange
        var attachment = new MessageAttachment
        {
            ContentType = "image/png",
            Content = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
        };

        // Assert
        Assert.That(attachment.IsImageContent, Is.True);
    }

    [Test]
    public void IsImageContent_WithJpegContentType_ShouldReturnTrue()
    {
        // Arrange
        var attachment = new MessageAttachment
        {
            ContentType = "image/jpeg",
            Content = Array.Empty<byte>()
        };

        // Assert
        Assert.That(attachment.IsImageContent, Is.True);
    }

    [Test]
    public void IsAudioContent_WithAudioContentType_ShouldReturnTrue()
    {
        // Arrange
        var attachment = new MessageAttachment
        {
            ContentType = "audio/mpeg",
            Content = Array.Empty<byte>()
        };

        // Assert
        Assert.That(attachment.IsAudioContent, Is.True);
    }

    [Test]
    public void IsAudioContent_WithWavContentType_ShouldReturnTrue()
    {
        // Arrange
        var attachment = new MessageAttachment
        {
            ContentType = "audio/wav",
            Content = Array.Empty<byte>()
        };

        // Assert
        Assert.That(attachment.IsAudioContent, Is.True);
    }

    [Test]
    public void ContentTypeFlags_WithPdfContentType_ShouldAllBeFalse()
    {
        // Arrange
        var attachment = new MessageAttachment
        {
            ContentType = "application/pdf",
            Content = Array.Empty<byte>()
        };

        // Assert
        Assert.That(attachment.IsTextContent, Is.False);
        Assert.That(attachment.IsImageContent, Is.False);
        Assert.That(attachment.IsAudioContent, Is.False);
    }

    [Test]
    public void MessageAttachment_ShouldHaveNullableProperties()
    {
        // Act
        var attachment = new MessageAttachment
        {
            Id = null,
            ConversationMessage = null
        };

        // Assert
        Assert.That(attachment.Id, Is.Null);
        Assert.That(attachment.ConversationMessage, Is.Null);
    }

    [Test]
    public void MessageAttachment_ShouldStoreContent()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("Test content");
        var name = "test.txt";
        var contentType = "text/plain";

        // Act
        var attachment = new MessageAttachment
        {
            Name = name,
            ContentType = contentType,
            Content = content
        };

        // Assert
        Assert.That(attachment.Name, Is.EqualTo(name));
        Assert.That(attachment.ContentType, Is.EqualTo(contentType));
        Assert.That(attachment.Content, Is.EqualTo(content));
    }

    [Test]
    public void MessageAttachment_ShouldLinkToConversationMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new ConversationMessage("user", "Question");
        message.Id = messageId;

        // Act
        var attachment = new MessageAttachment
        {
            ConversationMessageId = messageId,
            ConversationMessage = message,
            ContentType = "text/plain",
            Content = Encoding.UTF8.GetBytes("Data")
        };

        // Assert
        Assert.That(attachment.ConversationMessageId, Is.EqualTo(messageId));
        Assert.That(attachment.ConversationMessage, Is.EqualTo(message));
    }
}
