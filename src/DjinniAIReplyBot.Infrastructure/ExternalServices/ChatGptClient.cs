using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace DjinniAIReplyBot.Infrastructure.ExternalServices;

public class ChatGptClient : IChatGptClient
{
    private readonly ChatClient _chatClient;
    private const string ChatGptModelName = "gpt-3.5-turbo-0125";
    private readonly Dictionary<long, List<ChatMessage>> _chatContexts = new(); 

    public ChatGptClient(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var token = configuration["ChatGptAccessToken"];
        
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("ChatGPT token is not configured.");
        }
        var client = new OpenAIClient(token);
        _chatClient = client.GetChatClient(ChatGptModelName);
    }
   
    public async Task<string?> GenerateNewChatResponseAsync(long chatId, IEnumerable<ChatMessage> messages)
    {
        ResetChatContext(chatId);
        var chatContext = _chatContexts[chatId];
        
        var chatMessages = messages.ToList();
        chatContext.AddRange(chatMessages);
        
        return await SendChatMessageAsync(chatContext, chatContext);
    }

    public async Task<string?> ContinueChatResponseAsync(long chatId, string userRevision)
  
    {
        if (!_chatContexts.TryGetValue(chatId, out var chatContext))
        {
            throw new InvalidOperationException("Chat context not found for the given chat id.");
        }

        chatContext.Add(new UserChatMessage($"Please revise based on the following feedback: {userRevision}"));

        return await SendChatMessageAsync(chatContext, chatContext);
    }
    
    private async Task<string?> SendChatMessageAsync(IEnumerable<ChatMessage> messages, List<ChatMessage> chatContext)
    {
        var chatCompletion = await _chatClient.CompleteChatAsync(messages);
        var assistantMessage = chatCompletion.Value.Content.FirstOrDefault()?.Text ?? null;

        if (assistantMessage != null)
        {
            chatContext.Add(new AssistantChatMessage(assistantMessage));
        }

        return assistantMessage;
    }

    private void ResetChatContext(long chatId)
    {
        if (_chatContexts.ContainsKey(chatId))
        {
            _chatContexts.Remove(chatId);
        }
        _chatContexts[chatId] = new List<ChatMessage>();
    }
    

}












