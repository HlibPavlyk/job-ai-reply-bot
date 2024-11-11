using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Commands;
using DjinniAIReplyBot.Application.Services;
using DjinniAIReplyBot.Infrastructure.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace DjinniAIReplyBot.Infrastructure.Extensions;

public static class DependencyContainerExtensions
{
    public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<TelegramBotClient>(_ =>
        {
            var token = configuration["TelegramBotToken"];
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Telegram bot token is not configured.");
            }
            return new TelegramBotClient(token);
        });
        
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddScoped<ICommand, StartCommand>();
        services.AddScoped<ITelegramUpdateListener, CommandExecutor>();
        services.AddScoped<UpdateDistributor<CommandExecutor>>(provider =>
        {
            return new UpdateDistributor<CommandExecutor>(() => 
                provider.GetRequiredService<ITelegramUpdateListener>() as CommandExecutor ?? 
                throw new InvalidOperationException("CommandExecutor is not registered."));
        });

    }
}