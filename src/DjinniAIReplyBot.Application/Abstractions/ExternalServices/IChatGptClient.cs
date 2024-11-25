using OpenAI.Chat;

namespace DjinniAIReplyBot.Application.Abstractions.ExternalServices;

public interface IChatGptClient
{
    Task<string?> GenerateNewChatResponseAsync(long chatId, IEnumerable<ChatMessage> messages);
    Task<string?> ContinueChatResponseAsync(long chatId, string userRevision);
}