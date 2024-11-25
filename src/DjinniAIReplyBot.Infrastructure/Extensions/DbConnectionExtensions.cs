using Microsoft.AspNetCore.Builder;
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
    
    public static void MigrateDatabase(this IApplicationBuilder app)
    {
        // Creates a scope to resolve the ApplicationDbContext service and migrate the database.
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate(); 
    }
   
}