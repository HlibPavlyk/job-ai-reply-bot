namespace DjinniAIReplyBot.Application.Abstractions.Telegram;

public interface ICommandListenerManager
{
    long? CurrentChatId { get; }
    void StartListen(IListener listener, long chatId);
    void StopListen();
}