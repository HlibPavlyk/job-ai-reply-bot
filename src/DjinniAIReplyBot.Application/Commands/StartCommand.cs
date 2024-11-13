using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Helpers;
using Microsoft.Extensions.Configuration;
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
        _authorChatId = long.Parse(AppConfig.Configuration ["AuthorChatId"] ??
                                   throw new InvalidOperationException("Author chat id is not configured."));
    }

    public async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        string? userName = update.Message.Chat.Username ?? update.Message.Chat.FirstName;

        // Починаємо слухати, але користувач ще не підтверджений
        _listenerManager.StartListen(this, chatId);
        _isUserAccepted[chatId] = false;

        // Надсилаємо автору запит на підтвердження
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

            // Якщо користувач ще не прийнятий
            if (!_isUserAccepted.GetValueOrDefault(chatId, false))
            {
                await _client.SendMessageAsync(chatId, "You should wait for the author to accept you.");
            }
            // Якщо користувач не має дозволу на використання бота
            else if (_isUserAccepted.TryGetValue(chatId, out var isAccepted) && !isAccepted)
            {
                await _client.SendMessageAsync(chatId, "You don't have permission to use the bot.");
            }
        }
    }

    private async Task HandleUserAccept(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data != null && callbackQuery.Message != null)
        {
            long chatId = callbackQuery.Message.Chat.Id;
            var dataParts = callbackQuery.Data.Split('_');
            if (dataParts.Length < 3) return;

            var action = dataParts[0];
            if (!long.TryParse(dataParts[1], out var targetChatId)) return;
            if (!long.TryParse(dataParts[12], out var targetUserName)) return;

            if (!_isUserAccepted.ContainsKey(targetChatId)) return;

            if (action == "accept")
            {
                // Якщо автор приймає користувача
                _isUserAccepted[targetChatId] = true;
                
                await _client.SendMessageAsync(chatId, $"You have accepted the user {targetUserName}.");
                await _client.SendMessageAsync(targetChatId, "You have been accepted and can now use the bot.");
                
                _listenerManager.StopListen(targetChatId);
            }
            else if (action == "reject")
            {
                // Якщо автор відхиляє користувача
                await _client.SendMessageAsync(targetChatId, "You have been rejected and cannot use the bot.");
                _isUserAccepted.Remove(targetChatId);
            }

            // Видаляємо повідомлення з клавіатурою у автора
            await _client.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);
        }
    }
}
