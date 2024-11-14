using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Repositories;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Helpers;
using DjinniAIReplyBot.Application.Services;
using DjinniAIReplyBot.Infrastructure.ExternalServices;
using DjinniAIReplyBot.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace DjinniAIReplyBot.Infrastructure.Extensions;

public static class DependencyContainerExtensions
{
    public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbEfConnection(configuration);
        
        services.AddSingleton<ScopedServiceAccessor>();
        services.AddScoped<IUserConfigurationRepository, UserConfigurationRepository>();
        
        services.AddSingleton<TelegramBotClient>(_ =>
        {
            var token = configuration["TelegramBotToken"];
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Telegram bot token is not configured.");
            }
            return new TelegramBotClient(token);
        });
        
        services.AddSingleton<ITelegramService, TelegramService>();
        services.AddSingleton<CommandExecutor>();
        services.AddSingleton<ICommandListenerManager>(provider => provider.GetRequiredService<CommandExecutor>());
        
    }
}
