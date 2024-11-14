using DjinniAIReplyBot.Domain.Entities;
using DjinniAIReplyBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DjinniAIReplyBot.Infrastructure.EntityConfigurations;

public class UserConfConfiguration : IEntityTypeConfiguration<UserConfiguration>
{
    public void Configure(EntityTypeBuilder<UserConfiguration> builder)
    {
        builder.ToTable("user_configurations");

        builder.HasKey(u => u.ChatId);

        builder.Property(u => u.ChatId)
            .HasColumnName("chat_id")
            .IsRequired();
        
        builder.HasIndex(u => u.ChatId)
            .IsUnique();
        
        builder.Property(u => u.UserName)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(u => u.UserName)
            .IsUnique();

        builder.Property(u => u.ReplyLanguage)
            .HasColumnName("reply_language")
            .IsRequired()
            .HasMaxLength(2)
            .HasConversion<string>()
            .HasDefaultValue(ReplyLanguage.En);

        builder.Property(u => u.ParsedResume)
            .HasColumnName("parsed_resume")
            .IsRequired();

        builder.Property(u => u.AdditionalConfiguration)
            .HasColumnName("additional_configuration");
        
        builder.Property(u => u.IsAccepted)
            .HasColumnName("is_accepted")
            .IsRequired()
            .HasDefaultValue(false);
    }
}