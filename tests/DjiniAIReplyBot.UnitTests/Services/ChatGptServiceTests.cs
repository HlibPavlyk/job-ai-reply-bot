using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Services;
using DjinniAIReplyBot.Domain.Enums;
using Moq;
using OpenAI.Chat;

namespace DjiniAIReplyBot.UnitTests.Services;

public class ChatGptServiceTests
{
    private readonly Mock<IChatGptClient> _chatGptClientMock;
    private readonly ChatGptService _chatGptService;

    public ChatGptServiceTests()
    {
        _chatGptClientMock = new Mock<IChatGptClient>();
        _chatGptService = new ChatGptService(_chatGptClientMock.Object);
    }

    
    
    [Fact]
    public async Task ParseResumeAsync_ShouldCallChatGptClientWithCorrectMessages()
    {
        // Arrange
        var chatId = 12345L;
        var resumeText = "Sample resume text";
        var expectedResponse = "Parsed resume";
        
        _chatGptClientMock
            .Setup(client => client.GenerateNewChatResponseAsync(chatId, It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _chatGptService.ParseResumeAsync(chatId, resumeText);

        // Assert
        Assert.Equal(expectedResponse, result);

        _chatGptClientMock.Verify(client =>
            client.GenerateNewChatResponseAsync(chatId, It.Is<List<ChatMessage>>(messages =>
                messages[0].Content[0].Text.Contains("HR assistant specialized in extracting and formatting resumes") &&
                messages[1].Content[0].Text.Contains($"Here is the resume to process: {resumeText}"))),
            Times.Once);
    }

    [Fact]
    public async Task GenerateCoverLetterAsync_ShouldCallChatGptClientWithCorrectMessages()
    {
        // Arrange
        var chatId = 12345L;
        var jobDescription = "Sample job description";
        var resumeText = "Sample resume text";
        var replyLanguage = ReplyLanguage.En;
        var additionalNotes = "Some additional notes";
        var expectedResponse = "Generated cover letter";

        _chatGptClientMock
            .Setup(client => client.GenerateNewChatResponseAsync(chatId, It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _chatGptService.GenerateCoverLetterAsync(chatId, jobDescription, resumeText, replyLanguage, additionalNotes);

        // Assert
        Assert.Equal(expectedResponse, result);

        _chatGptClientMock.Verify(client =>
            client.GenerateNewChatResponseAsync(chatId, It.Is<List<ChatMessage>>(messages =>
                messages[0].Content[0].Text.Contains("professional assistant who generates highly personalized cover letters") &&
                messages[1].Content[0].Text.Contains($"Job Description: {jobDescription}") &&
                messages[2].Content[0].Text.Contains($"Resume: {resumeText}") &&
                messages[3].Content[0].Text.Contains("English") &&
                messages[4].Content[0].Text.Contains($"Additional Notes from the user: {additionalNotes}"))),
            Times.Once);
    }

    [Fact]
    public async Task GenerateCoverLetterAsync_ShouldNotIncludeAdditionalNotesWhenNull()
    {
        // Arrange
        var chatId = 12345L;
        var jobDescription = "Sample job description";
        var resumeText = "Sample resume text";
        var replyLanguage = ReplyLanguage.Ua;
        var expectedResponse = "Generated cover letter";

        _chatGptClientMock
            .Setup(client => client.GenerateNewChatResponseAsync(chatId, It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _chatGptService.GenerateCoverLetterAsync(chatId, jobDescription, resumeText, replyLanguage);

        // Assert
        Assert.Equal(expectedResponse, result);

        _chatGptClientMock.Verify(client =>
            client.GenerateNewChatResponseAsync(chatId, It.Is<List<ChatMessage>>(messages =>
                !messages.Any(m => m.Content[0].Text.Contains("Additional Notes")) &&
                messages[3].Content[0].Text.Contains("Ukrainian"))),
            Times.Once);
    }

    [Fact]
    public async Task RegenerateCoverLetterAsync_ShouldCallContinueChatResponseAsync()
    {
        // Arrange
        var chatId = 12345L;
        var userRevision = "User revision text";
        var expectedResponse = "Regenerated cover letter";

        _chatGptClientMock
            .Setup(client => client.ContinueChatResponseAsync(chatId, userRevision))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _chatGptService.RegenerateCoverLetterAsync(chatId, userRevision);

        // Assert
        Assert.Equal(expectedResponse, result);

        _chatGptClientMock.Verify(client =>
            client.ContinueChatResponseAsync(chatId, userRevision),
            Times.Once);
    }

    [Fact]
    public async Task RegenerateCoverLetterAsync_ShouldThrowExceptionForEmptyUserRevision()
    {
        // Arrange
        var chatId = 12345L;
        var userRevision = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _chatGptService.RegenerateCoverLetterAsync(chatId, userRevision));

        Assert.Equal("User revision cannot be null or empty. (Parameter 'userRevision')", exception.Message);

        _chatGptClientMock.Verify(client =>
            client.ContinueChatResponseAsync(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never);
    }
}