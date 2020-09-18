using System.Collections.Generic;

namespace PlaylistSpotify.Models
{
    public class Playlist
    {
        public Playlist() { }
        public Playlist(string Path, string Url)
        {
            this.Device = Path;
            this.Url = Url;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public string Device { get; set; }
        public string PathFolder { get; set; }
        public string PathUrlFile { get; set; }
        public List<Music> Music { get; set; }
    }
}