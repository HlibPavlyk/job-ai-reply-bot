using DjinniAIReplyBot.Application.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DjinniAIReplyBot.Application.Helpers;

public class ScopedServiceAccessor
{
    private readonly IServiceProvider _serviceProvider;

    public ScopedServiceAccessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task UseUserConfigurationRepositoryAsync(Func<IUserConfigurationRepository, Task> action)
    {
        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IUserConfigurationRepository>();
        await action(client);
    }
   
}