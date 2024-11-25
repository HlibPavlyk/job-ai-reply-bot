using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Services;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Commands;

public abstract class BaseCommand : ICommand
{
    
    protected readonly ScopedServiceAccessor ScopedAccessor;
    protected readonly ITelegramService TelegramClient;
    protected readonly IChatGptService ChatGptClient;

    protected BaseCommand(IServiceProvider serviceProvider)
    {
        ScopedAccessor = serviceProvider.GetRequiredService<ScopedServiceAccessor>();
        TelegramClient = serviceProvider.GetRequiredService<ITelegramService>();
        ChatGptClient = serviceProvider.GetRequiredService<IChatGptService>();
    }

    public abstract string Name { get; }
    public abstract Task Execute(Update update);

    protected async Task<bool> ValidateUserAccess(long chatId)
    {
        bool isUserValid = true;
        await ScopedAccessor.UseUserConfigurationRepositoryAsync(async repository =>
        {
            var userConfiguration = await repository.GetUserConfigurationAsync(chatId);
            if (userConfiguration == null)
            {
                await TelegramClient.SendMessageAsync(chatId, "You have not been accepted yet. Please use /start to request access.");
                isUserValid = false;
                return;
            }

            if (!userConfiguration.IsAccepted)
            {
                await TelegramClient.SendMessageAsync(chatId, "You don't have permission to use the bot.");
                isUserValid = false;
            }
        });
        return isUserValid;
    }
}
