using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Domain.Enums;
using DjinniAIReplyBot.Domain.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Application.Commands;

public class LanguageCommand : BaseCommand, IListener
{
    public override string Name => "/language";
    private readonly ICommandListenerManager _listenerManager;
    private readonly Dictionary<long, string> _userLanguages;

    public LanguageCommand(IServiceProvider serviceProvider, ICommandListenerManager listenerManager) : base(serviceProvider)
    {
        _listenerManager = listenerManager;
        _userLanguages = new Dictionary<long, string>();
    }

    public override async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        if(!(await ValidateUserAccess(chatId))) return;
        
        _listenerManager.StartListen(this, chatId);

        _userLanguages[chatId] = string.Empty;

        var languageKeyboard = new InlineKeyboardMarkup([
            [InlineKeyboardButton.WithCallbackData("English", "lang_en")],
            [InlineKeyboardButton.WithCallbackData("Ukrainian", "lang_ua")]
        ]);

        await TelegramClient.SendMessageAsync(chatId, "Choose reply generation language:", languageKeyboard);
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

    public Task<bool> ResetUpdate(Update update)
    {
        if (string.IsNullOrEmpty(update.Message?.Text))
            return Task.FromResult(false);

        var chatId = update.Message.Chat.Id;
        _userLanguages.Remove(chatId);
        _listenerManager.StopListen(chatId);
       
        return Task.FromResult(true);
    }

    private async Task HandleLanguageSelection(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message != null)
        {
            long chatId = callbackQuery.Message.Chat.Id;

            if (!_userLanguages.ContainsKey(chatId))
                return;

            _userLanguages[chatId] = callbackQuery.Data == "lang_en" ? "English" : "Ukrainian";

            await TelegramClient.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);
            await TelegramClient.SendMessageAsync(chatId, $"Chosen language: {_userLanguages[chatId]}");

            await ScopedAccessor.UseUserConfigurationRepositoryAsync(async repository =>
            {
                var userConfiguration = await repository.GetUserConfigurationAsync(chatId);
                if(userConfiguration == null)
                    return;
                
                userConfiguration.ReplyLanguage = _userLanguages[chatId] == "English" ? ReplyLanguage.En : ReplyLanguage.Ua;
                await repository.UpdateUserConfigurationAsync(userConfiguration);
                await repository.SaveChangesAsync();
            });
            
            _userLanguages.Remove(chatId);
            _listenerManager.StopListen(chatId);
        }
    }
}
