using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Application.Commands;

public class StartCommand : BaseCommand, IListener
{
    public override string Name => "/start";
    private readonly ICommandListenerManager _listenerManager;
    private readonly Dictionary<long, bool?> _isUserAccepted;
    private readonly long _authorChatId;

    public StartCommand(IServiceProvider serviceProvider, ICommandListenerManager listenerManager) : base(serviceProvider)
    {
        _listenerManager = listenerManager;
        _isUserAccepted = new Dictionary<long, bool?>();
      
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _authorChatId = long.Parse(configuration["AuthorChatId"] ??
                                   throw new InvalidOperationException("Author chat id is not configured."));
       
    }

    public override async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        string? userName = update.Message.Chat.Username;

        await ScopedAccessor.UseUserConfigurationRepositoryAsync(async repository =>
        {
            var userConfiguration = await repository.GetUserConfigurationAsync(chatId);
            
            if (userConfiguration != null)
            {
                if (userConfiguration.IsAccepted)
                {
                    await TelegramClient.SendMessageAsync(chatId, "You have already been accepted and can use the bot.");
                }
                else
                {
                    await TelegramClient.SendMessageAsync(chatId, "You don't have permission to use the bot.");
                }
            }
            else
            {
                await TelegramClient.SendMessageAsync(chatId, "Welcome to the bot! Please wait for the author to accept you.");

                if (string.IsNullOrEmpty(userName))
                {
                    await TelegramClient.SendMessageAsync(chatId, "You should have a username to use the bot.");
                    return;
                }
                userName = '@' + userName;
        

                _listenerManager.StartListen(this, chatId);
                _isUserAccepted[chatId] = userConfiguration?.IsAccepted;

                var acceptKeyboard = new InlineKeyboardMarkup([
                    [InlineKeyboardButton.WithCallbackData("Accept", $"accept:{userName}:{chatId}"),],
                    [InlineKeyboardButton.WithCallbackData("Reject", $"reject:{userName}:{chatId}")]
                ]);

                await TelegramClient.SendMessageAsync(_authorChatId, $"Do you want to accept the user {userName}?", acceptKeyboard);
            }
        });
        

       
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
            await TelegramClient.SendMessageAsync(chatId, "You should wait for the author to accept you.");
        }
    }

    private async Task HandleUserAccept(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data == null || callbackQuery.Message == null) return;

        long chatId = callbackQuery.Message.Chat.Id;
        
        var dataParts = callbackQuery.Data.Split(':');
        if (dataParts.Length < 3 || !long.TryParse(dataParts[2], out var targetChatId)) return;

        var action = dataParts[0];
        var targetUserName = dataParts[1];

        if (!_isUserAccepted.ContainsKey(targetChatId)) return;
        
        var userConfiguration = new UserConfiguration
        {
            ChatId = targetChatId,
            UserName = targetUserName
        };

        if (action == "accept")
        {
            userConfiguration.IsAccepted = true;
            await TelegramClient.SendMessageAsync(chatId, $"You have accepted the user {targetUserName}.");
            await TelegramClient.SendMessageAsync(targetChatId, "You have been accepted and can now use the bot.");

        }
        else if (action == "reject")
        {
            await TelegramClient.SendMessageAsync(chatId, $"You have rejected the user {targetUserName}.");
            await TelegramClient.SendMessageAsync(targetChatId, "You have been rejected and cannot use the bot.");
        }
        
        await ScopedAccessor.UseUserConfigurationRepositoryAsync(async repository =>
        {
            await repository.AddUserConfigurationAsync(userConfiguration);
            await repository.SaveChangesAsync();
        });

        _listenerManager.StopListen(targetChatId);
        _isUserAccepted.Remove(targetChatId);
        await TelegramClient.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);
    }
    
   
}
