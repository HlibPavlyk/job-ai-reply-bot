using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Commands;

public class StartCommand : ICommand
{
    private readonly ITelegramService _client;

    public string Name => "/start";

    public StartCommand(ITelegramService client)
    {
        _client = client;
    }

    public async Task Execute(Update update)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;
            await _client.SendMessageAsync(chatId, "Hello user!");
        }
    }
}