using System.Collections;
using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Models;
using DjinniAIReplyBot.Domain.Entities;
using DjinniAIReplyBot.Domain.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DjinniAIReplyBot.Application.Commands;

public class ReplyCommand : BaseCommand, IListener
{
    public override string Name => "/reply";

    private readonly ICommandListenerManager _listenerManager;
    private readonly Dictionary<long, ReplyGenerationNote> _jobDescriptionOrRevision;

    public ReplyCommand(IServiceProvider serviceProvider, ICommandListenerManager listenerManager) : base(
        serviceProvider)
    {
        _listenerManager = listenerManager;
        _jobDescriptionOrRevision = new Dictionary<long, ReplyGenerationNote>();
    }

    public override async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        if(!(await ValidateUserAccess(chatId))) return;
        
        _listenerManager.StartListen(this, chatId);

        _jobDescriptionOrRevision[chatId] = new ReplyGenerationNote();

        await TelegramClient.SendMessageAsync(chatId, "Please send the job description you'd like to reply to.");
    }
    
    public async Task GetUpdate(Update update)
    {
        if (update.CallbackQuery  != null)
        {
            await HandleReplyConfirming(update.CallbackQuery);
            return;
        }
        
        if (update.Message != null)
        {
            var chatId = update.Message.Chat.Id;
            if (_jobDescriptionOrRevision.TryGetValue(chatId, out var replyNotes))
            {
                if (string.IsNullOrEmpty(replyNotes.JobDescription))
                {
                    await HandleJobDescription(update.Message);
                    return;
                }

                if (string.IsNullOrEmpty(replyNotes.Revision))
                {
                    await HandleUserRevision(update.Message);
                    return;
                }
            }

            await TelegramClient.SendMessageAsync(chatId, "Invalid input. Please use buttons to approve or reject the reply.");
        }
    }

    public async Task<bool> ResetUpdate(Update update)
    {
        if (string.IsNullOrWhiteSpace(update.Message?.Text))
            return false;

        await ExitFromCommand(update.Message.Chat.Id);
        return true;
    }

    private async Task HandleJobDescription(Message message)
    {
        if (message.Text == null) return;
        
        var chatId = message.Chat.Id;
        if (!_jobDescriptionOrRevision.TryGetValue(chatId, out var value))
            return;
        
        value.JobDescription = message.Text;
        await TelegramClient.SendMessageAsync(chatId, "Job description received. Generating reply...");
        
        UserConfiguration? userConfigs = null;
        await ScopedAccessor.UseUserConfigurationRepositoryAsync(async repository =>
        {
            userConfigs = await repository.GetUserConfigurationAsync(chatId);
        });

        if (userConfigs?.ParsedResume == null)
        {
            await ExitFromCommand(chatId, "You need to configure your resume first. Please use /configure command.");
            return;
        }

        // var reply = "Generated reply";
        var reply = await ChatGptClient.GenerateCoverLetterAsync(chatId, value.JobDescription, userConfigs.ParsedResume,
            userConfigs.ReplyLanguage, userConfigs.AdditionalConfiguration);
            
        await OutputReply(chatId, reply);
    }
    
    private async Task HandleReplyConfirming(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message == null)
            return;
        
        var chatId = callbackQuery.Message.Chat.Id;
        
        switch (callbackQuery.Data)
        {
            case "confirm":
                await TelegramClient.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);
                await ExitFromCommand(chatId, "Reply confirmed. You can now use the bot to generate more replies.");
                break;
            case "regenerate":
            {
                if (!_jobDescriptionOrRevision.TryGetValue(chatId, out var value))
                    return;
            
                value.Revision = string.Empty;
                await TelegramClient.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);
                await TelegramClient.SendMessageAsync(chatId, "Please information you want to revise.");
                break;
            }
        }
    }

    
    private async Task OutputReply(long chatId, string? reply)
    {
        if (string.IsNullOrEmpty(reply))
        {
            await TelegramClient.SendMessageAsync(chatId, "Reply generation failed. Please try again.");
            return;
        }
        
        await TelegramClient.SendMessageAsync(chatId, reply);
        
        var languageKeyboard = new InlineKeyboardMarkup([
            [InlineKeyboardButton.WithCallbackData("Confirm", "confirm")],
            [InlineKeyboardButton.WithCallbackData("Regenerate", "regenerate")]
        ]);

        await TelegramClient.SendMessageAsync(chatId, "Do you want to confirm the reply?", languageKeyboard);
    }
    
   private async Task HandleUserRevision(Message message)
   {
       if (message.Text == null) return;
        
       var chatId = message.Chat.Id;
       if (!_jobDescriptionOrRevision.TryGetValue(chatId, out var value))
           return;
       
       value.Revision = message.Text;
       await TelegramClient.SendMessageAsync(chatId, "Revision received. Generating reply...");
       
       try
       {
           //var reply = "Revised reply";
           var reply = await ChatGptClient.RegenerateCoverLetterAsync(chatId, value.Revision);
           await OutputReply(chatId, reply);
       }
       catch (ChatGptClientException e)
       {
           await ExitFromCommand(chatId, e.Message + "with /reply command.");
       }
   }
    
   private async Task ExitFromCommand(long chatId, string? message = null)
   {
       _jobDescriptionOrRevision.Remove(chatId);
       _listenerManager.StopListen(chatId);
       
         if (message != null)
         {
              await TelegramClient.SendMessageAsync(chatId, message);
         }
   }
    
}