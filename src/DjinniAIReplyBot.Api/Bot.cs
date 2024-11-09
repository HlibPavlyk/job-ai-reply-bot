using Telegram.Bot;

namespace DjinniAIReplyBot.Api;

public static class Bot
{
    private static TelegramBotClient? _client;

    public static TelegramBotClient GetTelegramBot()
    {
        if (_client != null)
        {
            return _client;
        }
        _client = new TelegramBotClient("7567350520:AAEE0FHohQveHPHGI5HSnu3oGtHm6Q1Voek");
        return _client;
    }
}