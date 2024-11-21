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
            new SystemChatMessage(
                "You are an HR assistant specialized in extracting and formatting resumes for specific job applications. Focus on listing the key skills in detail. Summarize other sections, such as education and projects, briefly. If a section is not present in the resume, do not create it. Format the output as a concise and structured Telegram message."),
            new UserChatMessage(
                $"Here is the resume to process: {resumeText}")
        };




        return await _chatGptClient.GenerateNewChatResponseAsync(chatId, messages);
    }

    public async Task<string?> GenerateCoverLetterAsync(long chatId, string jobDescription, string resumeText, string replyLanguage, string? additionalNotes = null)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a professional assistant who generates highly personalized cover letters. Your goal is to highlight the candidate's relevant skills and experiences based on the provided resume and job description. Write a concise, professional, and engaging cover letter."),
            new UserChatMessage($"Job Description: {jobDescription}"),
            new UserChatMessage($"Resume: {resumeText}"),
            new UserChatMessage($"The cover letter should be written in the following language: {replyLanguage}.")
        };

        if (!string.IsNullOrEmpty(additionalNotes))
        {
            messages.Add(new UserChatMessage($"Additional Notes from the user: {additionalNotes}"));
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
}