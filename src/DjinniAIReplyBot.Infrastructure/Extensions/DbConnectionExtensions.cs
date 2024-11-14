using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DjinniAIReplyBot.Infrastructure.Extensions;

public static class DbConnectionExtensions
{
    public static void AddDbEfConnection(this IServiceCollection service, IConfiguration configuration)
    { 
        service.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }
   
}