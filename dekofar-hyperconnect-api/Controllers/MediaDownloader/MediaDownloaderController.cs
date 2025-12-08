using Dekofar.HyperConnect.Application.MediaDownloader.DTOs;
using Dekofar.HyperConnect.Application.MediaDownloader.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.HyperConnect.Api.Controllers.MediaDownloader
{
    [ApiController]
    [Route("api/media-downloader")]   // 🔴 DEĞİŞTİ: eskisi muhtemelen "api/media" idi
    public class MediaDownloaderController : ControllerBase
    {
        private readonly IMediaDownloaderService _mediaDownloaderService;

        public MediaDownloaderController(IMediaDownloaderService mediaDownloaderService)
        {
            _mediaDownloaderService = mediaDownloaderService;
        }

        [HttpPost("preview")]
        public async Task<ActionResult<IReadOnlyList<MediaItemDto>>> Preview(
            [FromBody] MediaPreviewRequest request,
            CancellationToken cancellationToken)
        {
            if (request?.Urls == null || request.Urls.Count == 0)
                return BadRequest("En az bir URL göndermelisiniz.");

            var result = await _mediaDownloaderService.PreviewAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("download-images")]
        public async Task<IActionResult> DownloadImages(
            [FromBody] MediaDownloadRequest request,
            CancellationToken cancellationToken)
        {
            var stream = await _mediaDownloaderService.DownloadImagesAsync(request, cancellationToken);
            return File(stream, "application/zip", "images.zip");
        }

        [HttpPost("download-videos")]
        public async Task<IActionResult> DownloadVideos(
            [FromBody] MediaDownloadRequest request,
            CancellationToken cancellationToken)
        {
            var stream = await _mediaDownloaderService.DownloadVideosAsync(request, cancellationToken);
            return File(stream, "application/zip", "videos.zip");
        }
    }
}
