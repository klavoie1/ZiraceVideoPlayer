using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZiraceVideoPlayer.Models
{
        public class Video
        {
            public required string Title { get; set; }
            public required string FilePath { get; set; }
            public required TimeSpan Duration { get; set; }
            // Thumbnail is not Required
            public string? Thumbnail { get; set; }
        }
}
