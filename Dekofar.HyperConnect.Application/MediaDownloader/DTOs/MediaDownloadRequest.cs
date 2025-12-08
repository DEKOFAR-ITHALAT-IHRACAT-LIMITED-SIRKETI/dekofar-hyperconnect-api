using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.MediaDownloader.DTOs
{
    public class MediaDownloadRequest
    {
        public List<string> ImageIds { get; set; } = new();
        public List<string> VideoIds { get; set; } = new();
    }
}
