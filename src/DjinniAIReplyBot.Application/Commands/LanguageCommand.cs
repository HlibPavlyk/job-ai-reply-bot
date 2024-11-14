using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Repositories;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Application.Commands;

public class LanguageCommand : ICommand, IListener
{
    public string Name => "/language";
    private readonly ITelegramService _client;
    private readonly ICommandListenerManager _listenerManager;
    private readonly IUserConfigurationRepository _userConfigurationRepository;
    private readonly Dictionary<long, string> _userLanguages;

    public LanguageCommand(IServiceProvider serviceProvider, ICommandListenerManager listenerManager)
    {
        using var scope = serviceProvider.CreateScope();
        _client = scope.ServiceProvider.GetRequiredService<ITelegramService>();
        _userConfigurationRepository = scope.ServiceProvider.GetRequiredService<IUserConfigurationRepository>();
        
        _listenerManager = listenerManager;
        _userLanguages = new Dictionary<long, string>();
    }

    public async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        _listenerManager.StartListen(this, chatId);

        _userLanguages[chatId] = string.Empty;

        var languageKeyboard = new InlineKeyboardMarkup([
            [InlineKeyboardButton.WithCallbackData("English", "lang_en")],
            [InlineKeyboardButton.WithCallbackData("Ukrainian", "lang_ua")]
        ]);

        await _client.SendMessageAsync(chatId, "Choose reply generation language:", languageKeyboard);
    }

    public async Task GetUpdate(Update update)
    {
        if (update.CallbackQuery?.Data != null)
        {
            await HandleLanguageSelection(update.CallbackQuery);
        }
        else if (update.Message != null)
        {
            throw new UserNotificationException(update.Message.Chat.Id, "Invalid language selection. Please try again.");
        }
    }

    private async Task HandleLanguageSelection(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message != null)
        {
            long chatId = callbackQuery.Message.Chat.Id;

            if (!_userLanguages.ContainsKey(chatId))
                return;

            _userLanguages[chatId] = callbackQuery.Data == "lang_en" ? "English" : "Ukrainian";

            await _client.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);
            await _client.SendMessageAsync(chatId, $"Chosen language: {_userLanguages[chatId]}");

            _userLanguages.Remove(chatId);
            _listenerManager.StopListen(chatId);
        }
    }
}
