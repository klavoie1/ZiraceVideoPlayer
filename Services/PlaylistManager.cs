using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZiraceVideoPlayer.Models;

namespace ZiraceVideoPlayer.Services
{
    public class PlaylistManager
    {
        private const string PlaylistFilePath = "playlists.json";

        public async Task SavePlaylistsAsync(List<Playlist> playlists)
        {
            var json = JsonSerializer.Serialize(playlists);
            await File.WriteAllTextAsync(PlaylistFilePath, json);
        }

        public async Task<List<Playlist>> LoadPlaylistsAsync()
        {
            if (!File.Exists(PlaylistFilePath))
                return new List<Playlist>();

            var json = await File.ReadAllTextAsync(PlaylistFilePath);
            return JsonSerializer.Deserialize<List<Playlist>>(json);
        }
        
    }
}
