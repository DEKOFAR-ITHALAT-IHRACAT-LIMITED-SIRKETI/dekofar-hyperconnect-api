using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.MediaDownloader.DTOs
{
    public class MediaPreviewRequest
    {
        public List<string> Urls { get; set; } = new();
    }
}
