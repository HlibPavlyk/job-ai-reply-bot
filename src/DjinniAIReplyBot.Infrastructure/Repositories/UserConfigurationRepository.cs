using DjinniAIReplyBot.Application.Abstractions.Repositories;
using DjinniAIReplyBot.Application.Helpers;
using DjinniAIReplyBot.Domain.Entities;

namespace DjinniAIReplyBot.Infrastructure.Repositories;

public class UserConfigurationRepository : IUserConfigurationRepository
{
    private readonly ApplicationDbContext _context;

    public UserConfigurationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserConfiguration?> GetUserConfigurationAsync(long chatId)
    {
       return await _context.UserConfigurations
           .FindAsync(chatId);
    }

    public async Task AddUserConfigurationAsync(UserConfiguration userConfiguration)
    {
        await _context.UserConfigurations
            .AddAsync(userConfiguration);
    }

    public Task UpdateUserConfigurationAsync(UserConfiguration userConfiguration)
    {
        _context.UserConfigurations
            .Update(userConfiguration);
        return Task.CompletedTask;
        
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}