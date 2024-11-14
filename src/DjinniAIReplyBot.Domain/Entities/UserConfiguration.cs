using DjinniAIReplyBot.Domain.Enums;

namespace DjinniAIReplyBot.Domain.Entities;

public class UserConfiguration
{
    public long ChatId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ReplyLanguage ReplyLanguage { get; set; }
    public string ParsedResume { get; set; } = string.Empty;
    public string? AdditionalConfiguration { get; set; }
    public bool IsAccepted { get; set; }
}