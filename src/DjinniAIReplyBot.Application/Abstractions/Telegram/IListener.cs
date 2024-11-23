using DjinniAIReplyBot.Application.Services;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Abstractions.Telegram;

public interface IListener
{
    Task GetUpdate(Update update);
    Task<bool> ResetUpdate(Update update);
    
}