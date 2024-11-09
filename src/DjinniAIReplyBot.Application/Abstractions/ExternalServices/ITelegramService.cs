namespace DjinniAIReplyBot.Application.Abstractions.ExternalServices;

public interface ITelegramService
{
    Task SendMessageAsync(long chatId, string message);
}