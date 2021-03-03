using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YouTubeDownloader
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args[0] == null)
            {
                Console.WriteLine($"ERROR: no url provided");
                return -1;
            }
            else if(args[1] == null)
            {
                Console.WriteLine("ERROR: no destination folder provided");
                return -1;
            }
            
            var destFolder = args[1];

            if (!Directory.Exists(destFolder))
            {
                Console.WriteLine($"ERROR: directory {destFolder} not found");
                return -1;
            }

            var exeLoc = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (exeLoc == null)
            {
                Console.WriteLine("ERROR: Could not find exe location");
                return -1;
            }

            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(args[0]);
            if(video == null)
            {
                Console.WriteLine($"ERROR: could not find youtube video {args[0]}");
                return -1;
            }
            var channel = await youtube.Channels.GetAsync(video.ChannelId);

            var videoTitle = video.Title;
            // Read the video ID
            var videoId = new VideoId(args[0]);
            if(videoId.Value == null)
            {
                Console.WriteLine($"ERROR: could not find youtube video {videoId}");
                return -1;
            }

            // Get media streams & choose the best muxed stream
            var streams = await youtube.Videos.Streams.GetManifestAsync(videoId);
            var streamInfo = streams.GetAudio().WithHighestBitrate();
            if (streamInfo == null)
            {
                return -1;
            }
            var fileName = $"{PrepareForFilename(channel.Title)} - {PrepareForFilename(videoTitle)}.mp3";
            var dest = Path.Combine(destFolder, fileName);
            await youtube.Videos.Streams.DownloadAsync(streamInfo, dest, null);
            //TagLib.File mp3File = TagLib.File.Create(dest);
            //mp3File.Tag.Title = video.Title;
            //mp3File.Tag.Description = video.Description;
            //mp3File.Tag.Album = channel.Title;
            //mp3File.Save();

            Console.WriteLine(dest);
            return 0;
        }
        static string PrepareForFilename(string fname)
        {
            string ret = string.Empty;
            for (var c = 0; c < fname.Length; c++)
            {
                if ("/\\?%*:|\"<>".Contains(fname[c]))
                    ret += "";
                else
                    ret += fname[c];
            }
            return ret;
        }
    }

}
