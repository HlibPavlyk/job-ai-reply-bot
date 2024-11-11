using DjinniAIReplyBot.Application.Abstractions.Telegram;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Services;

public class CommandExecutor : ITelegramUpdateListener
{
    private readonly List<ICommand> _commands;

    public CommandExecutor(IEnumerable<ICommand> commands)
    {
        _commands = commands.ToList();
    }

    public async Task GetUpdate(Update update)
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
}