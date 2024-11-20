using DjinniAIReplyBot.Domain.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DjinniAIReplyBot.Application.Commands;

public class ProfileCommand : BaseCommand
{
    public override string Name => "/profile";
    
    public ProfileCommand(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override async Task Execute(Update update)
    {
        if (update.Message == null) return;

        long chatId = update.Message.Chat.Id;
        if(!(await ValidateUserAccess(chatId))) return;
        
        await ScopedAccessor.UseUserConfigurationRepositoryAsync(async repository =>
        {
            var userConfig = await repository.GetUserConfigurationAsync(chatId);
            if (userConfig == null)
                return;

            string profileText = $"👤 *Your Profile*\n\n" +
                                 $"*Username:* {userConfig.UserName}\n\n" +
                                 $"*Resume generation language:* {(userConfig.ReplyLanguage == ReplyLanguage.En ? "English" : "Ukrainian")}\n\n" +
                                 $"*Additional configuration:*\n{(string.IsNullOrEmpty(userConfig.AdditionalConfiguration) ? "No additional data" : userConfig.AdditionalConfiguration)}\n\n" +
                                 $"*Parsed resume:*\n{(string.IsNullOrEmpty(userConfig.ParsedResume) ? "No data available" : userConfig.ParsedResume)}";
        
            await TelegramClient.SendMessageAsync(chatId, profileText,  ParseMode.Markdown);
        });
    }
}