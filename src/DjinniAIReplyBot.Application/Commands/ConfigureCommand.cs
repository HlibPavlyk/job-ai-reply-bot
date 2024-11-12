using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Dtos.Telegram;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Application.Commands;

public class ConfigureCommand : ICommand, IListener
{
    public string Name => "/config";
    private readonly ITelegramService _client;
    private readonly ICommandListenerManager _listenerManager;
    private readonly Dictionary<long, UserConfigState> _userStates;

    public ConfigureCommand(ITelegramService client, ICommandListenerManager listenerManager)
    {
        _client = client;
        _listenerManager = listenerManager;
        _userStates = new Dictionary<long, UserConfigState>();
    }

    public async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        _listenerManager.StartListen(this, chatId);

        _userStates[chatId] = new UserConfigState();

        var languageKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("English", "lang_en") },
            new[] { InlineKeyboardButton.WithCallbackData("Ukrainian", "lang_ua") }
        });

        await _client.SendMessageAsync(chatId, "Choose language:", languageKeyboard);
    }

    public async Task GetUpdate(Update update)
    {
        if (update.CallbackQuery?.Data != null)
        {
            await HandleLanguageSelection(update.CallbackQuery);
        }
        else if (update.Message?.Text != null)
        {
            await HandleNameInput(update.Message);
        }
    }

    private async Task HandleLanguageSelection(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message != null)
        {
            long chatId = callbackQuery.Message.Chat.Id;

            if (!_userStates.TryGetValue(chatId, out var state))
                return;

            state.Language = callbackQuery.Data == "lang_en" ? "English" : "Ukrainian";
        
            await _client.SendMessageAsync(chatId, "Enter your name");
        }
    }

    private async Task HandleNameInput(Message message)
    {
        long chatId = message.Chat.Id;

        if (!_userStates.TryGetValue(chatId, out var state))
            return;

        state.Name = message.Text;
        var userState = _userStates[chatId];

        await _client.SendMessageAsync(chatId, $"Thank you! We will not bother you anymore.\nYour data: {userState.Name} {userState.Language}");

        _userStates.Remove(chatId);
        _listenerManager.StopListen(chatId);
    }
}

