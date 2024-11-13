using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Abstractions.Telegram;

public interface ITelegramUpdateListener
{
    Task GetUpdate(Update update);
}