namespace DjinniAIReplyBot.Domain.Exceptions;

public class UserNotificationException : Exception
{
    public long ChatId { get; set; }

    public UserNotificationException(long chatId, string message) : base(message)
    {
        ChatId = chatId;
    }
    
    public UserNotificationException(string message, Exception innerException) : base(message, innerException) { }
}
