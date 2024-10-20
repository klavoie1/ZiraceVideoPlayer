using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.

namespace ZiraceVideoPlayer.Models
{
    internal class MediaBrowserViewModel
    {
        public ObservableCollection<MediaItem> MediaItems { get; set; }

        public MediaBrowserViewModel()
        {
            MediaItems = new ObservableCollection<MediaItem>(LoadMediaFromDatabase());
        }

        private List<MediaItem> LoadMediaFromDatabase()
        {
            // Example loading from a JSON file.
            string json = File.ReadAllText("mediaDatabase.json");
            return JsonConvert.DeserializeObject<List<MediaItem>>(json);
        }
    }
}
