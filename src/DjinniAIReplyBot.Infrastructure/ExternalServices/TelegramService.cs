using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Infrastructure.ExternalServices;

public class TelegramService : ITelegramService
{
    private readonly TelegramBotClient _bot;

    public TelegramService(TelegramBotClient bot)
    {
        _bot = bot;
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        await _bot.SendMessage(chatId, message);
    }

    public async Task SendMessageAsync(long chatId, string message, InlineKeyboardMarkup replyMarkup)
    {
        await _bot.SendMessage(chatId, message, replyMarkup: replyMarkup);
    }
}