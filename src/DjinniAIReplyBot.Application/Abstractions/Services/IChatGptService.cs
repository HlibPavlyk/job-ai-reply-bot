namespace DjinniAIReplyBot.Application.Abstractions.Services;

public interface IChatGptService
{
    Task<string?> ParseResumeAsync(long chatId, string resumeText);
    Task<string?> GenerateCoverLetterAsync(long chatId, string jobDescription, string resumeText, string replyLanguage,  string? additionalNotes = null);
    Task<string?> RegenerateCoverLetterAsync(long chatId, string userRevision);

}