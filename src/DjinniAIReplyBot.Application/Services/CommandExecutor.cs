using DjinniAIReplyBot.Application.Abstractions.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Services;

public class CommandExecutor : ICommandListenerManager
{
    private readonly List<ICommand> _commands;
    private readonly Dictionary<long, IListener> _listeners;
    private readonly long _authorChatId;

    public CommandExecutor(IServiceProvider serviceProvider)
    {
        _commands = GetCommands(serviceProvider);
        _listeners = new Dictionary<long, IListener>();
        
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _authorChatId = long.Parse(configuration["AuthorChatId"] ??
                                   throw new InvalidOperationException("Author chat id is not configured."));
    }

    private List<ICommand> GetCommands(IServiceProvider serviceProvider)
    {
        var types = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(ICommand).IsAssignableFrom(type) && type.IsClass);

        return types
            .Select(type => typeof(IListener).IsAssignableFrom(type) 
                ? Activator.CreateInstance(type, serviceProvider, this) as ICommand 
                : Activator.CreateInstance(type, serviceProvider) as ICommand)
            .Where(command => command != null)
            .ToList()!;
    }

    public async Task GetUpdate(Update update)
    {
        long? chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
        if (chatId == null) return;

        if (chatId == _authorChatId && update.CallbackQuery?.Data != null)
        {
            await ProcessAuthorCallback(update, update.CallbackQuery.Data);
        }

        if (_listeners.TryGetValue(chatId.Value, out var listener))
        {
            await listener.GetUpdate(update);
        }
        else
        {
            await ExecuteCommand(update);
        }
    }

    private async Task ProcessAuthorCallback(Update update, string callbackData)
    {
        var dataParts = callbackData.Split('_');
        if (dataParts.Length < 2 || !long.TryParse(dataParts[^1], out var targetChatId)) return;

        if (_listeners.TryGetValue(targetChatId, out var listener))
        {
            await listener.GetUpdate(update);
        }
    }

    private async Task ExecuteCommand(Update update)
    {
        if (update.Message?.Text == null) return;

        foreach (var command in _commands)
        {
            if (command.Name == update.Message.Text)
            {
                await command.Execute(update);
                break;
            }
        }
    }

    public void StartListen(IListener newListener, long chatId)
    {
        _listeners[chatId] = newListener;
    }

    public void StopListen(long chatId)
    {
        if (_listeners.ContainsKey(chatId))
        {
            _listeners.Remove(chatId);
        }
    }
}
