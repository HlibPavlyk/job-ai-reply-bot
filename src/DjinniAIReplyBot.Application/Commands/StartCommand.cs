using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Helpers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Application.Commands;

public class StartCommand : ICommand, IListener
{
    public string Name => "/start";
    private readonly ITelegramService _client;
    private readonly ICommandListenerManager _listenerManager;
    private readonly Dictionary<long, bool> _isUserAccepted;
    private readonly long _authorChatId;

    public StartCommand(ITelegramService client, ICommandListenerManager listenerManager)
    {
        _client = client;
        _listenerManager = listenerManager;
        _isUserAccepted = new Dictionary<long, bool>();
        _authorChatId = long.Parse(AppConfig.Configuration["AuthorChatId"] 
            ?? throw new InvalidOperationException("Author chat id is not configured."));
    }

    public async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        string? userName = update.Message.Chat.Username;

        await _client.SendMessageAsync(chatId, "Welcome to the bot! Please wait for the author to accept you.");

        if (string.IsNullOrEmpty(userName))
        {
            await _client.SendMessageAsync(chatId, "You should have a username to use the bot.");
            return;
        }

        _listenerManager.StartListen(this, chatId);
        _isUserAccepted[chatId] = false;

        var acceptKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Accept", $"accept_@{userName}_{chatId}") },
            new[] { InlineKeyboardButton.WithCallbackData("Reject", $"reject_@{userName}_{chatId}") }
        });

        await _client.SendMessageAsync(_authorChatId, $"Do you want to accept the user @{userName}?", acceptKeyboard);
    }

    public async Task GetUpdate(Update update)
    {
        if (update.CallbackQuery?.Data != null)
        {
            await HandleUserAccept(update.CallbackQuery);
        }
        else if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            if (_isUserAccepted.TryGetValue(chatId, out var isAccepted))
            {
                if (!isAccepted)
                {
                    await _client.SendMessageAsync(chatId, "You should wait for the author to accept you.");
                    return;
                }
            }
            else
            {
                await _client.SendMessageAsync(chatId, "You don't have permission to use the bot.");
            }
        }
    }

    private async Task HandleUserAccept(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data == null || callbackQuery.Message == null) return;

        long chatId = callbackQuery.Message.Chat.Id;
        var dataParts = callbackQuery.Data.Split('_');
        if (dataParts.Length < 3 || !long.TryParse(dataParts[2], out var targetChatId)) return;

        var action = dataParts[0];
        var targetUserName = dataParts[1];

        if (!_isUserAccepted.ContainsKey(targetChatId)) return;

        if (action == "accept")
        {
            _isUserAccepted[targetChatId] = true;

            await _client.SendMessageAsync(chatId, $"You have accepted the user {targetUserName}.");
            await _client.SendMessageAsync(targetChatId, "You have been accepted and can now use the bot.");

            _listenerManager.StopListen(targetChatId);
        }
        else if (action == "reject")
        {
            await _client.SendMessageAsync(chatId, $"You have rejected the user {targetUserName}.");
            await _client.SendMessageAsync(targetChatId, "You have been rejected and cannot use the bot.");
            _isUserAccepted.Remove(targetChatId);
        }

        await _client.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);
    }
}
