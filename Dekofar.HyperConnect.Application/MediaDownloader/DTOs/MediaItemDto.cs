using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.MediaDownloader.DTOs
{
    public class MediaItemDto
    {
        public string Id { get; set; } = default!;       // frontend seçim için
        public string PageUrl { get; set; } = default!;
        public string MediaUrl { get; set; } = default!;
        public string Type { get; set; } = default!;     // "image" | "video"
        public string? OriginalFormat { get; set; }      // jpg, png, webp, mp4, webm...
        public string? SuggestedFormat { get; set; }     // "jpg"/"png"/"mp4"
        public int? Width { get; set; }                  // bilinirse
        public int? Height { get; set; }
        public long? SizeBytes { get; set; }
    }
}
