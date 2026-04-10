using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Domain;
using PetZone.Listings.Infrastructure;
using PetZone.Volunteers.Infrastructure;

namespace PetZone.API.Controllers;

[ApiController]
[Route("v1/sitemap")]
public class SitemapController(
    ListingsDbContext listingsDb,
    VolunteersDbContext volunteersDb) : ControllerBase
{
    private const string SiteUrl = "https://getpetzone.com";
    private static readonly string[] Langs = ["uk", "ru", "en", "de", "fr", "pl"];

    [HttpGet]
    [ResponseCache(Duration = 3600)]
    public async Task<ContentResult> GetSitemap(CancellationToken ct)
    {
        var listingIds = await listingsDb.Listings
            .Where(l => l.Status == ListingStatus.Active)
            .Select(l => new { l.Id, l.CreatedAt })
            .ToListAsync(ct);

        var volunteerIds = await volunteersDb.Volunteers
            .Where(v => !v.IsDeleted)
            .Select(v => v.Id)
            .ToListAsync(ct);

        var petIds = await volunteersDb.Volunteers
            .Where(v => !v.IsDeleted)
            .SelectMany(v => v.Pets.Where(p => !p.IsDeleted))
            .Select(p => new { p.Id, p.CreatedAt })
            .ToListAsync(ct);

        var xml = new System.Text.StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        xml.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml">""");

        foreach (var l in listingIds)
        {
            AppendUrl(xml, $"/listings/{l.Id}", 0.8, "weekly", l.CreatedAt);
        }

        foreach (var p in petIds)
        {
            AppendUrl(xml, $"/pets/{p.Id}", 0.7, "weekly", p.CreatedAt);
        }

        foreach (var v in volunteerIds)
        {
            AppendUrl(xml, $"/volunteers/{v}", 0.6, "monthly", null);
        }

        xml.AppendLine("</urlset>");

        return Content(xml.ToString(), "application/xml");
    }

    private static void AppendUrl(
        System.Text.StringBuilder xml,
        string path,
        double priority,
        string changefreq,
        DateTime? lastmod)
    {
        xml.AppendLine("  <url>");
        xml.AppendLine($"    <loc>{SiteUrl}/uk{path}</loc>");
        if (lastmod.HasValue)
            xml.AppendLine($"    <lastmod>{lastmod.Value:yyyy-MM-dd}</lastmod>");
        xml.AppendLine($"    <changefreq>{changefreq}</changefreq>");
        xml.AppendLine($"    <priority>{priority:F1}</priority>");
        foreach (var lang in Langs)
            xml.AppendLine($"""    <xhtml:link rel="alternate" hreflang="{lang}" href="{SiteUrl}/{lang}{path}"/>""");
        xml.AppendLine($"""    <xhtml:link rel="alternate" hreflang="x-default" href="{SiteUrl}/uk{path}"/>""");
        xml.AppendLine("  </url>");
    }
}