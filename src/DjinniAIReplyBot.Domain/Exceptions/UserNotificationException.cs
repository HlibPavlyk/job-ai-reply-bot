namespace DjinniAIReplyBot.Domain.Exceptions;

public class UserNotificationException : Exception
{
    public UserNotificationException(string message) : base(message) { }
    
    public UserNotificationException(string message, Exception innerException) : base(message, innerException) { }
}
