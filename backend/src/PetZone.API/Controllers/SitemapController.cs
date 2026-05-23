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
        var listings = await listingsDb.Listings
            .Where(l => l.Status == ListingStatus.Active)
            .Select(l => new { l.Id, l.CreatedAt, l.Photos })
            .ToListAsync(ct);

        var volunteerIds = await volunteersDb.Volunteers
            .Where(v => !v.IsDeleted)
            .Select(v => v.Id)
            .ToListAsync(ct);

        var pets = await volunteersDb.Volunteers
            .Where(v => !v.IsDeleted)
            .SelectMany(v => v.Pets.Where(p => !p.IsDeleted && p.Status == HelpStatus.LookingForHome))
            .Select(p => new
            {
                p.Id,
                p.CreatedAt,
                Photos = p.Photos.Select(ph => ph.FilePath).ToList(),
            })
            .ToListAsync(ct);

        var newsPostIds = await volunteersDb.NewsPosts
            .Select(n => new { n.Id, n.VolunteerId, LastMod = n.UpdatedAt ?? n.CreatedAt })
            .ToListAsync(ct);

        var xml = new System.Text.StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        // image namespace lets Google Images index the listing/pet photos
        // from the same sitemap (https://developers.google.com/search/docs/crawling-indexing/sitemaps/image-sitemaps)
        xml.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml" xmlns:image="http://www.google.com/schemas/sitemaps-image/1.1">""");

        // Species landing pages — high priority, change frequently
        string[] speciesSlugs = ["dogs", "cats", "rabbits", "parrots"];
        string[] citySlugs = ["kyiv", "kharkiv", "lviv", "odesa", "dnipro", "zaporizhzhia"];
        foreach (var species in speciesSlugs)
        {
            AppendUrlAllLangs(xml, $"/pets/{species}", 0.9, "daily", null);
            foreach (var city in citySlugs)
                AppendUrlAllLangs(xml, $"/pets/{species}/{city}", 0.8, "daily", null);
        }

        foreach (var l in listings)
        {
            AppendUrlAllLangs(xml, $"/listings/{l.Id}", 0.8, "weekly", l.CreatedAt, l.Photos);
        }

        foreach (var p in pets)
        {
            AppendUrlAllLangs(xml, $"/pets/{p.Id}", 0.7, "weekly", p.CreatedAt, p.Photos);
        }

        foreach (var v in volunteerIds)
        {
            AppendUrlAllLangs(xml, $"/volunteers/{v}", 0.6, "monthly", null);
        }

        foreach (var n in newsPostIds)
        {
            AppendUrlAllLangs(xml, $"/news/{n.VolunteerId}/{n.Id}", 0.5, "monthly", n.LastMod);
        }

        xml.AppendLine("</urlset>");

        return Content(xml.ToString(), "application/xml");
    }

    // Emits one <url> block per language — Google recommends each language version
    // has its own <loc> so canonicalization works without relying on JS rendering.
    // Optional `images` list adds <image:image> entries for Google Images.
    private static void AppendUrlAllLangs(
        System.Text.StringBuilder xml,
        string path,
        double priority,
        string changefreq,
        DateTime? lastmod,
        IReadOnlyList<string>? images = null)
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
            if (images is { Count: > 0 })
            {
                // Google allows up to 1000 images per <url>. We're well under,
                // but the /api/files/{name}/redirect URL stays stable as the
                // canonical image URL — it 302s to a presigned MinIO link.
                foreach (var img in images)
                {
                    xml.AppendLine("    <image:image>");
                    xml.AppendLine($"      <image:loc>{SiteUrl}/api/files/{Uri.EscapeDataString(img)}/redirect</image:loc>");
                    xml.AppendLine("    </image:image>");
                }
            }
            xml.AppendLine("  </url>");
        }
    }
}