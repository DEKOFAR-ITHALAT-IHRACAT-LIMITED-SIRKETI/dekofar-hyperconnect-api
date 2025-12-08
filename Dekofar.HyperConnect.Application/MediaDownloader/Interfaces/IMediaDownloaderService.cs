using Dekofar.HyperConnect.Application.MediaDownloader.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.MediaDownloader.Interfaces
{
    public interface IMediaDownloaderService
    {
        Task<IReadOnlyList<MediaItemDto>> PreviewAsync(MediaPreviewRequest request, CancellationToken cancellationToken = default);

        Task<Stream> DownloadImagesAsync(MediaDownloadRequest request, CancellationToken cancellationToken = default);

        Task<Stream> DownloadVideosAsync(MediaDownloadRequest request, CancellationToken cancellationToken = default);
    }
}
