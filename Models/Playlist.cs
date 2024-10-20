using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZiraceVideoPlayer.Models
{
    public class Playlist
    {
        public required string Name { get; set; }
        public List<Video> Videos { get; set; } = new List<Video>();
    }
}
