namespace DjinniAIReplyBot.Domain.Exceptions;

public class ChatGptClientException : Exception
{
    public ChatGptClientException(string message) : base(message) { }
    
    public ChatGptClientException(string message, Exception innerException)
        : base(message, innerException) { }
}
