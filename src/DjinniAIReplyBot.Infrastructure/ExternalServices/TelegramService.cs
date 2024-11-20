using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Infrastructure.ExternalServices;

public class TelegramService : ITelegramService
{
    private readonly TelegramBotClient _bot;
    private readonly string _telegramBotToken;

    public TelegramService(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var token = configuration["TelegramBotToken"];
        
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Telegram bot token is not configured.");
        }
        _telegramBotToken = token;
        _bot = new TelegramBotClient(_telegramBotToken);
        
    }

    public async Task SendMessageAsync(long chatId, string message, ParseMode parseMode = ParseMode.None)
    {
        await _bot.SendMessage(chatId, message, parseMode);
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
    
    public async Task<byte[]> GetDocumentAsync(string fileId)
    {
        var file = await _bot.GetFile(fileId);

        var fileUrl = $"https://api.telegram.org/file/bot{_telegramBotToken}/{file.FilePath}";

        using var httpClient = new HttpClient();
    
        var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

        return fileBytes;
    }

}