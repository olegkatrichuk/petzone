using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain;

namespace NotificationService.Infrastructure.Configurations;

public class UserNotificationSettingsConfiguration
    : IEntityTypeConfiguration<UserNotificationSettings>
{
    public void Configure(EntityTypeBuilder<UserNotificationSettings> builder)
    {
        builder.ToTable("user_notification_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.SendEmail).IsRequired();
        builder.Property(s => s.SendTelegram).IsRequired();
        builder.Property(s => s.SendWeb).IsRequired();

        builder.HasIndex(s => s.UserId).IsUnique();
    }
}