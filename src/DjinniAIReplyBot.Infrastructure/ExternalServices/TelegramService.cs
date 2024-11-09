using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using Telegram.Bot;

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
}