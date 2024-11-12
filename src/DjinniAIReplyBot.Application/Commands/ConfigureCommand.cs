using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Domain.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Application.Commands;

public class ConfigureCommand : ICommand, IListener
{
    public string Name => "/config";
    private readonly ITelegramService _client;
    private readonly ICommandListenerManager _listenerManager;
    private string? _language;
    private string? _name;
    private long? _chatId;

    public ConfigureCommand(ITelegramService client, ICommandListenerManager listenerManager)
    {
        _client = client;
        _listenerManager = listenerManager;
    }

    public async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        _chatId = update.Message.Chat.Id;
        _listenerManager.StartListen(this, _chatId.Value);

        // Створення клавіатури для вибору мови
        var languageKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("English", "lang_en") },
            new[] { InlineKeyboardButton.WithCallbackData("Ukrainian", "lang_ua") }
        });

        await _client.SendMessageAsync(_chatId.Value, "Choose language:", languageKeyboard);
    }

    public async Task GetUpdate(Update update)
    {
        if (update.CallbackQuery != null && update.CallbackQuery.Data != null)
        {
            // Обробка натискання кнопки вибору мови
            await HandleLanguageSelection(update.CallbackQuery);
        }
        else if (update.Message?.Text != null)
        {
            // Обробка введення імені
            await HandleNameInput(update.Message);
        }
    }

    private async Task HandleLanguageSelection(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message != null)
        {
            long chatId = callbackQuery.Message.Chat.Id;

            if (chatId != _listenerManager.CurrentChatId)
                throw new UserNotificationException("You should wait for your turn. Another user is configuring now.");

            // Збереження вибраної мови
            _language = callbackQuery.Data == "lang_en" ? "English" : "Ukrainian";
        
            // Надсилаємо запит на введення імені після вибору мови
            await _client.SendMessageAsync(chatId, "Enter your name");
        }
    }

    private async Task HandleNameInput(Message message)
    {
        long chatId = message.Chat.Id;

        if (chatId != _listenerManager.CurrentChatId)
            throw new UserNotificationException("You should wait for your turn. Another user is configuring now.");

        _name = message.Text;
        await _client.SendMessageAsync(chatId, $"Thank you! We will not bother you anymore.\nYour data: {_name} {_language}");

        // Скидання стану і завершення прослуховування
        _language = null;
        _name = null;
        _listenerManager.StopListen();
    }
}
