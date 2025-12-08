using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using Dekofar.HyperConnect.Application.MediaDownloader.DTOs;
using Dekofar.HyperConnect.Application.MediaDownloader.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dekofar.HyperConnect.Infrastructure.Services.MediaDownloader;

public class MediaDownloaderService : IMediaDownloaderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MediaDownloaderService> _logger;

    // Cache key prefix
    private const string CacheKeyPrefix = "media-preview-";
    // Cache süresi: 30 dk (istersen appsettings'e alırız)
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public MediaDownloaderService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<MediaDownloaderService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    #region Public API

    public async Task<IReadOnlyList<MediaItemDto>> PreviewAsync(
        MediaPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();

        var allItems = new List<MediaItemDto>();

        foreach (var pageUrl in request.Urls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            try
            {
                var html = await client.GetStringAsync(pageUrl, cancellationToken);

                var mediaUrls = ScrapeMediaUrls(pageUrl, html);

                var mediaItemsForPage = await BuildMediaItemsForPageAsync(
                    pageUrl,
                    mediaUrls,
                    client,
                    cancellationToken);

                allItems.AddRange(mediaItemsForPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while scraping media from {PageUrl}", pageUrl);
            }
        }

        // Cache'e yaz
        CacheMediaItems(allItems);

        return allItems;
    }

    public Task<Stream> DownloadImagesAsync(
        MediaDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        var ids = request.ImageIds ?? new List<string>();
        return BuildZipForIdsAsync(ids, expectedType: "image", fileNamePrefix: "image", cancellationToken);
    }

    public Task<Stream> DownloadVideosAsync(
        MediaDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        var ids = request.VideoIds ?? new List<string>();
        return BuildZipForIdsAsync(ids, expectedType: "video", fileNamePrefix: "video", cancellationToken);
    }

    #endregion

    #region Internal Models

    private sealed class MediaCacheItem
    {
        public string PageUrl { get; set; } = default!;
        public string MediaUrl { get; set; } = default!;
        public string Type { get; set; } = default!;  // "image" | "video"
        public string? OriginalFormat { get; set; }
        public string? SuggestedFormat { get; set; }
    }

    #endregion

    #region Scraper + Filter

    private static (List<string> imageUrls, List<string> videoUrls) ScrapeMediaUrls(
        string pageUrl,
        string html)
    {
        var imageUrls = new List<string>();
        var videoUrls = new List<string>();

        if (string.IsNullOrWhiteSpace(html))
            return (imageUrls, videoUrls);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        Uri? baseUri = null;
        try
        {
            baseUri = new Uri(pageUrl);
        }
        catch
        {
            // baseUri null kalırsa sadece absolute src'ler kullanılır
        }

        // ---- IMG SCRAPE + FILTER ----
        var imgNodes = doc.DocumentNode.SelectNodes("//img");
        if (imgNodes != null)
        {
            foreach (var img in imgNodes)
            {
                // Bazı sitelerde gerçek src data-src / data-original içinde
                var src =
                    img.GetAttributeValue("src", null)
                    ?? img.GetAttributeValue("data-src", null)
                    ?? img.GetAttributeValue("data-original", null);

                if (string.IsNullOrWhiteSpace(src))
                    continue;

                var absoluteUrl = MakeAbsoluteUrl(baseUri, src);
                if (absoluteUrl is null)
                    continue;

                // Heuristik filtre: ikon/logo/badge vs.
                if (IsLikelyJunkImage(img, absoluteUrl))
                    continue;

                imageUrls.Add(absoluteUrl);
            }
        }

        // ---- VIDEO SCRAPE ----
        var videoNodes = doc.DocumentNode.SelectNodes("//video");
        if (videoNodes != null)
        {
            foreach (var video in videoNodes)
            {
                // <video src="...">
                var videoSrc = video.GetAttributeValue("src", null);
                if (!string.IsNullOrWhiteSpace(videoSrc))
                {
                    var abs = MakeAbsoluteUrl(baseUri, videoSrc);
                    if (abs != null)
                        videoUrls.Add(abs);
                }

                // <video><source src="..."></video>
                var sourceNodes = video.SelectNodes(".//source");
                if (sourceNodes == null) continue;

                foreach (var source in sourceNodes)
                {
                    var src = source.GetAttributeValue("src", null);
                    if (string.IsNullOrWhiteSpace(src))
                        continue;

                    var abs = MakeAbsoluteUrl(baseUri, src);
                    if (abs != null)
                        videoUrls.Add(abs);
                }
            }
        }

        // Tekilleştir
        imageUrls = imageUrls.Distinct().ToList();
        videoUrls = videoUrls.Distinct().ToList();

        return (imageUrls, videoUrls);
    }

    private static string? MakeAbsoluteUrl(Uri? baseUri, string src)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(src))
                return null;

            src = src.Trim();

            // protokol-relative: //cdn.site.com/image.jpg
            if (src.StartsWith("//"))
            {
                if (baseUri == null)
                    return "https:" + src; // fallback

                return $"{baseUri.Scheme}:{src}";
            }

            // Zaten absolute ise
            if (Uri.TryCreate(src, UriKind.Absolute, out var absolute))
                return absolute.ToString();

            // Relative ise baseUri üzerinden absolute yap
            if (baseUri != null && Uri.TryCreate(baseUri, src, out var relative))
                return relative.ToString();
        }
        catch
        {
            // yut, null dön
        }

        return null;
    }

    private static bool IsLikelyJunkImage(HtmlNode imgNode, string absoluteUrl)
    {
        // 1) Boyut filtresi (HTML attribute üzerinden)
        var widthAttr = imgNode.GetAttributeValue("width", null);
        var heightAttr = imgNode.GetAttributeValue("height", null);

        if (TryParsePixelValue(widthAttr, out var width) && width > 0 && width < 64)
            return true;

        if (TryParsePixelValue(heightAttr, out var height) && height > 0 && height < 64)
            return true;

        // 2) class / alt / title / aria-label filtreleri
        var classAttr = imgNode.GetAttributeValue("class", string.Empty) ?? string.Empty;
        var altAttr = imgNode.GetAttributeValue("alt", string.Empty) ?? string.Empty;
        var ariaLabel = imgNode.GetAttributeValue("aria-label", string.Empty) ?? string.Empty;
        var titleAttr = imgNode.GetAttributeValue("title", string.Empty) ?? string.Empty;

        var meta = $"{classAttr} {altAttr} {ariaLabel} {titleAttr}".ToLowerInvariant();

        string[] junkKeywords =
        {
            "icon",
            "logo",
            "badge",
            "avatar",
            "placeholder",
            "sprite",
            "thumb",
            "thumbnail",
            "emoji"
        };

        if (junkKeywords.Any(k => meta.Contains(k)))
            return true;

        // 3) URL pattern'leri
        var url = absoluteUrl.ToLowerInvariant();

        string[] urlPatterns =
        {
            "sprite",
            "icon",
            "logo",
            "placeholder"
        };

        if (urlPatterns.Any(p => url.Contains(p)))
            return true;

        return false;
    }

    private static bool TryParsePixelValue(string? value, out int result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // "32px" -> "32"
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digits))
            return false;

        return int.TryParse(digits, out result);
    }

    private async Task<List<MediaItemDto>> BuildMediaItemsForPageAsync(
        string pageUrl,
        (List<string> imageUrls, List<string> videoUrls) mediaUrls,
        HttpClient client,
        CancellationToken cancellationToken)
    {
        var items = new List<MediaItemDto>();

        // imageUrls + videoUrls üzerinden HEAD isteği atıp meta dolduruyoruz
        var imageTasks = mediaUrls.imageUrls.Select(url =>
            BuildMediaItemAsync(pageUrl, url, "image", client, cancellationToken));
        var videoTasks = mediaUrls.videoUrls.Select(url =>
            BuildMediaItemAsync(pageUrl, url, "video", client, cancellationToken));

        var allTasks = imageTasks.Concat(videoTasks);
        var results = await Task.WhenAll(allTasks);

        items.AddRange(results.Where(r => r is not null)!);

        return items;
    }

    private async Task<MediaItemDto?> BuildMediaItemAsync(
        string pageUrl,
        string mediaUrl,
        string type,
        HttpClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, mediaUrl);
            using var response = await client.SendAsync(headRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var contentType = response.Content.Headers.ContentType;
            var contentLength = response.Content.Headers.ContentLength;

            var originalFormat = GetFormatFromContentTypeOrUrl(contentType, mediaUrl);
            var suggestedFormat = GetSuggestedFormat(type, originalFormat);

            var id = Guid.NewGuid().ToString("N");

            return new MediaItemDto
            {
                Id = id,
                PageUrl = pageUrl,
                MediaUrl = mediaUrl,
                Type = type,
                OriginalFormat = originalFormat,
                SuggestedFormat = suggestedFormat,
                SizeBytes = contentLength,
                // Width/Height: şimdilik null, sonra gelişmiş byte inspection eklenebilir
                Width = null,
                Height = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while building media item for {MediaUrl}", mediaUrl);
            return null;
        }
    }

    private static string? GetFormatFromContentTypeOrUrl(
        MediaTypeHeaderValue? contentType,
        string mediaUrl)
    {
        if (contentType?.MediaType is not null)
        {
            var parts = contentType.MediaType.Split('/');
            if (parts.Length == 2)
            {
                return parts[1].ToLowerInvariant();
            }
        }

        var uri = new Uri(mediaUrl, UriKind.Absolute);
        var ext = Path.GetExtension(uri.LocalPath);
        if (!string.IsNullOrEmpty(ext))
        {
            return ext.Trim('.').ToLowerInvariant();
        }

        return null;
    }

    private static string? GetSuggestedFormat(string type, string? originalFormat)
    {
        if (type == "image")
        {
            if (originalFormat is "jpg" or "jpeg" or "png")
                return originalFormat == "jpeg" ? "jpg" : originalFormat;

            return "jpg";
        }

        if (type == "video")
        {
            return "mp4";
        }

        return null;
    }

    #endregion

    #region Cache & Download

    private void CacheMediaItems(IEnumerable<MediaItemDto> items)
    {
        foreach (var item in items)
        {
            var cacheKey = CacheKeyPrefix + item.Id;

            var cacheItem = new MediaCacheItem
            {
                PageUrl = item.PageUrl,
                MediaUrl = item.MediaUrl,
                Type = item.Type,
                OriginalFormat = item.OriginalFormat,
                SuggestedFormat = item.SuggestedFormat
            };

            _cache.Set(cacheKey, cacheItem, CacheDuration);
        }
    }

    private async Task<Stream> BuildZipForIdsAsync(
        IEnumerable<string> ids,
        string expectedType,
        string fileNamePrefix,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();

        var memoryStream = new MemoryStream();
        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var id in ids.Distinct())
            {
                var cacheKey = CacheKeyPrefix + id;
                if (!_cache.TryGetValue<MediaCacheItem>(cacheKey, out var cacheItem))
                    continue;

                if (!string.Equals(cacheItem.Type, expectedType, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    using var response = await client.GetAsync(cacheItem.MediaUrl, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                        continue;

                    await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                    if (expectedType == "image")
                    {
                        // 🔄 ImageSharp ile hedef formata convert ederek ekle
                        await AddImageToZipAsync(zip, cacheItem, id, fileNamePrefix, sourceStream, cancellationToken);
                    }
                    else
                    {
                        // 🎥 Videolar için şimdilik convert yok, raw olarak ekliyoruz
                        await AddRawToZipAsync(zip, cacheItem, id, fileNamePrefix, sourceStream, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error while downloading/adding media {MediaUrl} to zip",
                        cacheItem.MediaUrl);
                }
            }
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private static async Task AddImageToZipAsync(
        ZipArchive zip,
        MediaCacheItem cacheItem,
        string id,
        string fileNamePrefix,
        Stream sourceStream,
        CancellationToken cancellationToken)
    {
        // Hedef format: suggestedFormat "png" ise PNG, aksi halde JPG
        var targetFormat = cacheItem.SuggestedFormat?.ToLowerInvariant() == "png"
            ? "png"
            : "jpg";

        var fileName = $"{fileNamePrefix}_{id}.{targetFormat}";

        var entry = zip.CreateEntry(fileName, CompressionLevel.Fastest);
        await using var entryStream = entry.Open();

        // ImageSharp ile streamden yükle → hedef formata dönüştürüp kaydet
        using var image = await Image.LoadAsync(sourceStream, cancellationToken);

        if (targetFormat == "png")
        {
            await image.SaveAsPngAsync(entryStream, encoder: new PngEncoder(), cancellationToken: cancellationToken);
        }
        else
        {
            await image.SaveAsJpegAsync(entryStream, encoder: new JpegEncoder(), cancellationToken: cancellationToken);
        }
    }

    private static async Task AddRawToZipAsync(
        ZipArchive zip,
        MediaCacheItem cacheItem,
        string id,
        string fileNamePrefix,
        Stream sourceStream,
        CancellationToken cancellationToken)
    {
        var extension = cacheItem.SuggestedFormat ?? cacheItem.OriginalFormat ?? "bin";
        var fileName = $"{fileNamePrefix}_{id}.{extension}";

        var entry = zip.CreateEntry(fileName, CompressionLevel.Fastest);
        await using var entryStream = entry.Open();
        await sourceStream.CopyToAsync(entryStream, cancellationToken);
    }

    #endregion
}
