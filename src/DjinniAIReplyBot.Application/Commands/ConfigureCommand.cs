using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Helpers;
using Telegram.Bot.Types;
using UglyToad.PdfPig.Core;

namespace DjinniAIReplyBot.Application.Commands;

public class ConfigureCommand : BaseCommand, IListener
{
    public override string Name => "/configure";
    private readonly ICommandListenerManager _listenerManager;
    private readonly Dictionary<long, string> _userResumeConfigurations;
    
    public ConfigureCommand(IServiceProvider serviceProvider, ICommandListenerManager listenerManager) : base(serviceProvider)
    {
        _listenerManager = listenerManager;
        _userResumeConfigurations = new Dictionary<long, string>();
    }
  
    public override async Task Execute(Update update)
    {
        if (update.Message?.Text == null) return;

        long chatId = update.Message.Chat.Id;
        if(!(await ValidateUserAccess(chatId))) return;
        
        _listenerManager.StartListen(this, chatId);

        _userResumeConfigurations[chatId] = string.Empty;

        await TelegramClient.SendMessageAsync(chatId, "Please upload your resume in PDF format.");
    }

    public async Task GetUpdate(Update update)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;
        
        if (!_userResumeConfigurations.TryGetValue(chatId, out var userResumeConfig)) return;

        if (update.Message?.Document != null && string.IsNullOrEmpty(userResumeConfig))
        {
            await HandleDocument(update.Message);
        }
        else if (update.Message?.Text != null && !string.IsNullOrEmpty(userResumeConfig))
        {
            await HandleAdditionalData(update.Message);
        }
        else
        {
            await TelegramClient.SendMessageAsync(chatId, "Invalid input. Please send a valid data.");
        }
    }

    public Task<bool> ResetUpdate(Update update)
    {
        if (string.IsNullOrEmpty(update.Message?.Text))
            return Task.FromResult(false);

        var chatId = update.Message.Chat.Id;
        _userResumeConfigurations.Remove(chatId);
        _listenerManager.StopListen(chatId);
       
        return Task.FromResult(true);
    }

    private async Task HandleDocument(Message message)
    {
        if (message.Document == null) return;
        var chatId = message.Chat.Id;
        var document = message.Document;

        if (document.MimeType != "application/pdf")
        {
            await TelegramClient.SendMessageAsync(chatId, "Only PDF files are supported. Please upload a valid PDF.");
            return;
        }

        var fileId = document.FileId;
        var fileBytes = await TelegramClient.GetDocumentAsync(fileId);
        await TelegramClient.SendMessageAsync(chatId, "Your resume has been received. Processing the resume...");

        try
        {
            string parsedText = PdfTools.ParsePdfToString(fileBytes);
            
            var parsedGptText = await ChatGptClient.ParseResumeAsync(chatId, parsedText);
            //var parsedGptText = parsedText;
            if (string.IsNullOrEmpty(parsedGptText))
            {
                await TelegramClient.SendMessageAsync(chatId, "Your resume could not be processed by the AI. Please provide your resume as text.");
                return;
            }
            
            if (!_userResumeConfigurations.ContainsKey(chatId))
                return;
            _userResumeConfigurations[chatId] = parsedGptText;
            
            await TelegramClient.SendMessageAsync(chatId, "Your resume has been processed successfully. Please provide any additional information as text or press /skip.");
        }
        catch (PdfDocumentFormatException e)
        {
            await TelegramClient.SendMessageAsync(chatId, e.Message);
        }
      
    }

    private async Task HandleAdditionalData(Message message)
    {
        if (message.Text == null) return;
        
        var chatId = message.Chat.Id;
        var additionalData = message.Text == "/skip" ? null : message.Text;

        await TelegramClient.SendMessageAsync(chatId, 
            additionalData == null 
                ? "Additional information skipped." 
                : "Additional information received.");
        
        if (!_userResumeConfigurations.TryGetValue(chatId, out var userConfiguration))
            return;

        await ScopedAccessor.UseUserConfigurationRepositoryAsync(async repository =>
        {
            var dbUserConfig = await repository.GetUserConfigurationAsync(chatId);
            if (dbUserConfig == null)
                return;
            
            dbUserConfig.ParsedResume = userConfiguration;
            dbUserConfig.AdditionalConfiguration = additionalData;
            await repository.SaveChangesAsync();
        });
        
        _listenerManager.StopListen(chatId);
        _userResumeConfigurations.Remove(chatId);
        await TelegramClient.SendMessageAsync(chatId, "Configuration completed successfully. You can see configuration details using /profile.");
    }
}