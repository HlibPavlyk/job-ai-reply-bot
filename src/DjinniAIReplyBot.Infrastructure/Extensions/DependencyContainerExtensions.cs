using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Repositories;
using DjinniAIReplyBot.Application.Abstractions.Services;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Helpers;
using DjinniAIReplyBot.Application.Services;
using DjinniAIReplyBot.Infrastructure.ExternalServices;
using DjinniAIReplyBot.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DjinniAIReplyBot.Infrastructure.Extensions;

public static class DependencyContainerExtensions
{
    public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbEfConnection(configuration);
        services.AddHttpClient();
        
        services.AddSingleton<ScopedServiceAccessor>();
        services.AddScoped<IUserConfigurationRepository, UserConfigurationRepository>();
        
        services.AddSingleton<ITelegramService, TelegramService>();
        services.AddSingleton<IChatGptClient, ChatGptClient>();
        services.AddSingleton<IChatGptService, ChatGptService>();
        
        services.AddSingleton<CommandExecutor>();
        services.AddSingleton<ICommandListenerManager>(provider => provider.GetRequiredService<CommandExecutor>());
        
    }
}
