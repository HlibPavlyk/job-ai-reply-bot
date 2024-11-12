using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Application.Abstractions.ExternalServices;

public interface ITelegramService
{
    Task SendMessageAsync(long chatId, string message);
    Task SendMessageAsync(long chatId, string message, InlineKeyboardMarkup replyMarkup);
}