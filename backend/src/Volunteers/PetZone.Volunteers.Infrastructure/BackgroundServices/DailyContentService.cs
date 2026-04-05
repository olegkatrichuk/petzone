using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.BackgroundServices;

public class DailyContentService(
    IServiceScopeFactory scopeFactory,
    ILogger<DailyContentService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Try to generate on startup in case today's digest is missing
        await GenerateDailyContentAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            logger.LogInformation("DailyContentService: next digest in {Hours:F1} hours", delay.TotalHours);
            await Task.Delay(delay, stoppingToken);

            await GenerateDailyContentAsync(stoppingToken);
        }
    }

    private async Task GenerateDailyContentAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();

            var today = DateTime.UtcNow.Date;
            var alreadyExists = await db.SystemNewsPosts
                .AnyAsync(p => p.PublishedAt >= today && p.Type == "DailyDigest", cancellationToken);

            if (alreadyExists)
            {
                logger.LogInformation("DailyContentService: today's digest already exists, skipping");
                return;
            }

            var weekAgo = DateTime.UtcNow.AddDays(-7);

            var lookingForHome = await db.Pets
                .CountAsync(p => !p.IsDeleted && p.Status == HelpStatus.LookingForHome, cancellationToken);

            var foundHomeThisWeek = await db.Pets
                .CountAsync(p => !p.IsDeleted && p.Status == HelpStatus.FoundHome && p.CreatedAt >= weekAgo, cancellationToken);

            var needsHelp = await db.Pets
                .CountAsync(p => !p.IsDeleted && p.Status == HelpStatus.NeedsHelp, cancellationToken);

            var totalVolunteers = await db.Volunteers
                .CountAsync(v => !v.IsDeleted, cancellationToken);

            var fact = await FetchAnimalFactAsync();

            var dateStr = today.ToString("dd.MM.yyyy");
            var title = $"📊 Дайджест PetZone — {dateStr}";
            var content = $"""
                🐾 Сьогодні на платформі:

                🔍 Шукають дім: {lookingForHome} тваринок
                🏥 Потребують допомоги: {needsHelp}
                🏠 Знайшли дім цього тижня: {foundHomeThisWeek}
                👥 Активних волонтерів: {totalVolunteers}

                ---

                💡 Факт дня:
                {fact}
                """;

            var post = SystemNewsPost.Create(title, content.Trim(), "DailyDigest");
            db.SystemNewsPosts.Add(post);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("DailyContentService: digest created for {Date}", dateStr);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DailyContentService: failed to generate daily content");
        }
    }

    private static async Task<string> FetchAnimalFactAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await http.GetFromJsonAsync<CatFactResponse>("https://catfact.ninja/fact");
            return response?.Fact ?? DefaultFact();
        }
        catch
        {
            return DefaultFact();
        }
    }

    private static string DefaultFact() =>
        "Коти сплять близько 70% свого життя — це понад 13 годин на добу!";

    private record CatFactResponse([property: JsonPropertyName("fact")] string? Fact);
}