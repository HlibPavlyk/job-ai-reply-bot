using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace DjinniAIReplyBot.Infrastructure.ExternalServices;

public class ChatGptClient : IChatGptClient
{
    private readonly ChatClient _chatClient;
    //private const string ChatGptModelName = "gpt-3.5-turbo-0125";
    private const string ChatGptModelName = "gpt-4o";
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
        ResetOrCreateChatContext(chatId);
        var chatContext = _chatContexts[chatId];
        
        var chatMessages = messages.ToList();
        chatContext.AddRange(chatMessages);
        
        return await SendChatMessageAsync(chatId);
    }

    public async Task<string?> ContinueChatResponseAsync(long chatId, string userRevision)
  
    {
        if (!_chatContexts.TryGetValue(chatId, out var chatContext))
        {
            throw new InvalidOperationException("Chat context not found for the given chat id.");
        }

        if (chatContext.Count > 10)
        {
            throw new ChatGptClientException("A lot of messages have been exchanged. Please try generating a new response");
        }

        chatContext.Add(new UserChatMessage(
            $"Based on the previously generated response, please make the following changes or improvements: {userRevision}"
        ));

        return await SendChatMessageAsync(chatId);
    }
    
    private async Task<string?> SendChatMessageAsync(long chatId)
    {
        if (!_chatContexts.TryGetValue(chatId, out var chatContext))
        {
            throw new InvalidOperationException("Chat context not found for the given chat id.");
        }
        
        var chatCompletion = await _chatClient.CompleteChatAsync(chatContext);
        var assistantMessage = chatCompletion.Value.Content.FirstOrDefault()?.Text;

        if (!string.IsNullOrEmpty(assistantMessage))
        {
            chatContext.Add(new AssistantChatMessage(assistantMessage));
        }

        return assistantMessage;
    }

    private void ResetOrCreateChatContext(long chatId)
    {
        if (_chatContexts.ContainsKey(chatId))
        {
            _chatContexts.Remove(chatId);
        }
        _chatContexts[chatId] = new List<ChatMessage>();
    }
    

}












