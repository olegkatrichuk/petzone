using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Infrastructure;

namespace PetZone.API.Controllers;

// RSS 2.0 feed for the blog. Lives in PetZone.API rather than the Volunteers
// presentation layer because RSS is a delivery format, not domain logic — it
// just renders a DB projection as XML. Cached 1h.
[AllowAnonymous]
[ApiController]
[Route("v1/blog/rss")]
public class BlogFeedController(VolunteersDbContext volunteersDb) : ControllerBase
{
    private const string SiteUrl = "https://getpetzone.com";
    private const int MaxItems = 50;

    [HttpGet]
    [ResponseCache(Duration = 3600)]
    public async Task<ContentResult> GetFeed(
        [FromQuery] string? lang,
        CancellationToken ct)
    {
        var query = volunteersDb.BlogPosts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(lang))
            query = query.Where(p => p.Language == lang);

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Take(MaxItems)
            .Select(p => new
            {
                p.Slug,
                p.Language,
                p.Title,
                p.Summary,
                p.CoverImageUrl,
                p.CreatedAt,
            })
            .ToListAsync(ct);

        // Feed-level lang: pinned to the filter if any, else uk (default).
        var feedLang = !string.IsNullOrEmpty(lang) ? lang : "uk";
        var feedUrl = $"{SiteUrl}/{feedLang}/blog";
        var rssSelfUrl = $"{SiteUrl}/api/v1/blog/rss" + (string.IsNullOrEmpty(lang) ? "" : $"?lang={lang}");

        var xml = new System.Text.StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        xml.AppendLine("""<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:content="http://purl.org/rss/1.0/modules/content/">""");
        xml.AppendLine("  <channel>");
        xml.AppendLine($"    <title>{XmlEscape($"PetZone Blog{(lang is null ? "" : $" ({lang})")}")}</title>");
        xml.AppendLine($"    <link>{XmlEscape(feedUrl)}</link>");
        xml.AppendLine($"""    <atom:link href="{XmlEscape(rssSelfUrl)}" rel="self" type="application/rss+xml"/>""");
        xml.AppendLine($"    <description>{XmlEscape("PetZone — pet adoption tips, volunteer stories, platform updates")}</description>");
        xml.AppendLine($"    <language>{XmlEscape(feedLang)}</language>");
        xml.AppendLine($"    <lastBuildDate>{ToRfc822(DateTime.UtcNow)}</lastBuildDate>");
        xml.AppendLine($"    <generator>PetZone</generator>");

        foreach (var p in posts)
        {
            var url = $"{SiteUrl}/{p.Language}/blog/{p.Slug}";
            xml.AppendLine("    <item>");
            xml.AppendLine($"      <title>{XmlEscape(p.Title)}</title>");
            xml.AppendLine($"      <link>{XmlEscape(url)}</link>");
            xml.AppendLine($"      <guid isPermaLink=\"true\">{XmlEscape(url)}</guid>");
            xml.AppendLine($"      <pubDate>{ToRfc822(p.CreatedAt)}</pubDate>");
            xml.AppendLine($"      <description>{XmlEscape(p.Summary)}</description>");
            if (!string.IsNullOrEmpty(p.CoverImageUrl))
                xml.AppendLine($"""      <enclosure url="{XmlEscape(p.CoverImageUrl)}" type="image/jpeg"/>""");
            xml.AppendLine("    </item>");
        }

        xml.AppendLine("  </channel>");
        xml.AppendLine("</rss>");

        return Content(xml.ToString(), "application/rss+xml; charset=utf-8");
    }

    private static string XmlEscape(string s) =>
        System.Net.WebUtility.HtmlEncode(s);

    // RFC-822 for RSS <pubDate> — e.g. "Wed, 23 May 2026 12:34:56 GMT"
    private static string ToRfc822(DateTime utc) =>
        utc.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'",
            System.Globalization.CultureInfo.InvariantCulture);
}
