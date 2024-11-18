using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Application.Commands;

public class JokeCommand : BaseCommand
{
    public override string Name => "/joke";
    
    
    public JokeCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        if(!(await ValidateUserAccess(chatId))) return;


        var joke = await ChatGptClient.TestJokeGenerationAsync(chatId);
        await TelegramClient.SendMessageAsync(chatId, "Here is a joke for you: " + joke);
    }
}