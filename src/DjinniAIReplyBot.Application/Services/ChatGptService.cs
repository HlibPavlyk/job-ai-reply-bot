using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Services;
using OpenAI.Chat;

namespace DjinniAIReplyBot.Application.Services;

public class ChatGptService : IChatGptService
{
    private readonly IChatGptClient  _chatGptClient;

    public ChatGptService(IChatGptClient chatGptClient)
    {
        _chatGptClient = chatGptClient;
    }
    
    public async Task<string?> ParseResumeAsync(long chatId, string resumeText)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a professional assistant that parses resumes."),
            new UserChatMessage($"Parse the following resume into structured text: {resumeText}")
        };

        return await _chatGptClient.GenerateNewChatResponseAsync(chatId, messages);
    }

    public async Task<string?> GenerateCoverLetterAsync(long chatId, string jobDescription, string resumeText, string replyLanguage, string? additionalNotes = null)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an assistant that generates personalized cover letters."),
            new UserChatMessage($"Here is the job description: {jobDescription}"),
            new UserChatMessage($"Here is the resume: {resumeText}"),
            new UserChatMessage($"Reply in the following language: {replyLanguage}")
            
        };

        if (!string.IsNullOrEmpty(additionalNotes))
        {
            messages.Add(new UserChatMessage($"Additional notes: {additionalNotes}"));
        }
        return await _chatGptClient.GenerateNewChatResponseAsync(chatId, messages);
    }

    public Task<string?> RegenerateCoverLetterAsync(long chatId, string userRevision)
    {
        if(string.IsNullOrEmpty(userRevision))
        {
            throw new ArgumentException("User revision cannot be null or empty.", nameof(userRevision));
        }
        
        return _chatGptClient.ContinueChatResponseAsync(chatId, userRevision);
    }

    public async Task<string?> TestJokeGenerationAsync(long chatId)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an assistant that generates jokes."),
            new UserChatMessage("Generate a joke for me.")
        };
        
        return await _chatGptClient.GenerateNewChatResponseAsync(chatId, messages);
    }
}