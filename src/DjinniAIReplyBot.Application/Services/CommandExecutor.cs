using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Services;

public class CommandExecutor : ICommandListenerManager
{
    private readonly List<ICommand> _commands;
    private readonly Dictionary<long, IListener> _listeners;

    public CommandExecutor(ITelegramService client)
    {
        _commands = GetCommands(client);
        _listeners = new Dictionary<long, IListener>();
    }

    private List<ICommand> GetCommands(ITelegramService client)
    {
        var types = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(ICommand).IsAssignableFrom(type))
            .Where(type => type.IsClass);

        List<ICommand> commands = new List<ICommand>();
        foreach (var type in types)
        {
            ICommand? command;
            if (typeof(IListener).IsAssignableFrom(type))
            {
                command = Activator.CreateInstance(type, client, this) as ICommand;
            }
            else
            {
                command = Activator.CreateInstance(type, client) as ICommand;
            }

            if (command != null)
            {
                commands.Add(command);
            }
        }
        return commands;
    }

    public async Task GetUpdate(Update update)
    {
        long? chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;

        if (chatId == null) return;

        if (_listeners.TryGetValue(chatId.Value, out var listener))
        {
            await listener.GetUpdate(update);
        }
        else
        {
            await ExecuteCommand(update);
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
