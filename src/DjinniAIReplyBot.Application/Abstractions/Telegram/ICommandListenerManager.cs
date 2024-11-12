namespace DjinniAIReplyBot.Application.Abstractions.Telegram;

public interface ICommandListenerManager
{
    void StartListen(IListener listener, long chatId);
    void StopListen(long chatId);
}