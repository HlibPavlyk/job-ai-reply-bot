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
    
    public async Task EditMessageReplyMarkupAsync(long chatId, int messageId, InlineKeyboardMarkup? replyMarkup = null)
    {
        await _bot.EditMessageReplyMarkup(chatId, messageId, replyMarkup);
    }
    
    public async Task DeleteMessageAsync(long chatId, int messageId)
    {
        await _bot.DeleteMessage(chatId, messageId);
    }
}