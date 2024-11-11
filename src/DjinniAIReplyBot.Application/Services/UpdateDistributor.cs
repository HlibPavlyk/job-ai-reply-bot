using DjinniAIReplyBot.Application.Abstractions.Telegram;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Services;

public class UpdateDistributor<T> where T : ITelegramUpdateListener
{
    private readonly Dictionary<long, T> _listeners = new();
    private readonly Func<T> _listenerFactory;

    public UpdateDistributor(Func<T> listenerFactory)
    {
        _listenerFactory = listenerFactory;
    }

    public async Task GetUpdate(Update update)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;
            if (!_listeners.TryGetValue(chatId, out var listener))
            {
                listener = _listenerFactory();
                _listeners.Add(chatId, listener);
            }

            await listener.GetUpdate(update);
        }
    }
}
