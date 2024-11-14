using DjinniAIReplyBot.Domain.Entities;

namespace DjinniAIReplyBot.Application.Abstractions.Repositories;

public interface IUserConfigurationRepository
{
    Task<UserConfiguration?> GetUserConfigurationAsync(long chatId);
    Task AddUserConfigurationAsync(UserConfiguration userConfiguration);
    Task UpdateUserConfigurationAsync(UserConfiguration userConfiguration);
    Task SaveChangesAsync();
}