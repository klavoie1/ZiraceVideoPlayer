using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZiraceVideoPlayer.Models
{
    [Serializable]
    public class VideoState
    {
        public string VideoPath {  get; set; }
        public double LastPosition { get; set; }
    }
}
