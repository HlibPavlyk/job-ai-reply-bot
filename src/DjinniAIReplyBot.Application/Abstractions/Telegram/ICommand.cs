using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Abstractions.Telegram;

public interface ICommand
{
    string Name { get; }

    Task Execute(Update update);
}