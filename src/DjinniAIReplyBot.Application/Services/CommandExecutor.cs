using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Commands;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Services;

public class CommandExecutor : ICommandListenerManager
{
    private readonly List<ICommand> _commands;
    private IListener? _listener;
    public long? CurrentChatId { get; private set; }


    public CommandExecutor(ITelegramService client)
    {
        CurrentChatId = null;
        _commands =
        [
            new StartCommand(client),
            new ConfigureCommand(client, this)
        ];
    }

    public async Task GetUpdate(Update update)
    {
        if (_listener == null)
        {
            await ExecuteCommand(update);
        }
        else
        {
            await _listener.GetUpdate(update);
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
        _listener = newListener;
        CurrentChatId = chatId;
    }

    public void StopListen()
    {
        _listener = null;
        CurrentChatId = null;
    }
}
