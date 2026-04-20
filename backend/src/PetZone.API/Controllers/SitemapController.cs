using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Domain;
using PetZone.Listings.Infrastructure;
using PetZone.Volunteers.Domain.Models;
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
            .SelectMany(v => v.Pets.Where(p => !p.IsDeleted && p.Status == HelpStatus.LookingForHome))
            .Select(p => new { p.Id, p.CreatedAt })
            .ToListAsync(ct);

        var xml = new System.Text.StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        xml.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml">""");

        foreach (var l in listingIds)
        {
            AppendUrlAllLangs(xml, $"/listings/{l.Id}", 0.8, "weekly", l.CreatedAt);
        }

        foreach (var p in petIds)
        {
            AppendUrlAllLangs(xml, $"/pets/{p.Id}", 0.7, "weekly", p.CreatedAt);
        }

        foreach (var v in volunteerIds)
        {
            AppendUrlAllLangs(xml, $"/volunteers/{v}", 0.6, "monthly", null);
        }

        xml.AppendLine("</urlset>");

        return Content(xml.ToString(), "application/xml");
    }

    // Emits one <url> block per language — Google recommends each language version
    // has its own <loc> so canonicalization works without relying on JS rendering.
    private static void AppendUrlAllLangs(
        System.Text.StringBuilder xml,
        string path,
        double priority,
        string changefreq,
        DateTime? lastmod)
    {
        foreach (var lang in Langs)
        {
            xml.AppendLine("  <url>");
            xml.AppendLine($"    <loc>{SiteUrl}/{lang}{path}</loc>");
            if (lastmod.HasValue)
                xml.AppendLine($"    <lastmod>{lastmod.Value:yyyy-MM-dd}</lastmod>");
            xml.AppendLine($"    <changefreq>{changefreq}</changefreq>");
            xml.AppendLine($"    <priority>{(lang == "uk" ? priority : priority - 0.1):F1}</priority>");
            foreach (var l in Langs)
                xml.AppendLine($"""    <xhtml:link rel="alternate" hreflang="{l}" href="{SiteUrl}/{l}{path}"/>""");
            xml.AppendLine($"""    <xhtml:link rel="alternate" hreflang="x-default" href="{SiteUrl}/uk{path}"/>""");
            xml.AppendLine("  </url>");
        }
    }
}