using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BombcastDownloader
{
    public partial class MainWindow : Window
    {

        public List<DownloadURL> downloadURLs = new List<DownloadURL>();
        private static string relativePathToDownloadsFolder = "downloaded";

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        // Properties to bind to the DatePickers
        public DateTime StartDate
        {
            get { return StartDatePicker.SelectedDate ?? DateTime.MinValue; }
        }

        public DateTime EndDate
        {
            get { return EndDatePicker.SelectedDate ?? DateTime.MaxValue; }
        }

        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();

            string json = File.ReadAllText("giant_bomb_audio_metadata_all.json");
            List<Metadata> metadataList = JsonConvert.DeserializeObject<List<Metadata>>(json);            

            try {
                foreach(var md in metadataList) {
                    string filename = $"{md.Files.Where(f => 
                        f.Source == "original" &&
                        f.Format == "VBR MP3" &&
                        f.Name.EndsWith(".mp3")).First().Name}";

                    var dateString = $"{filename[..10]}";

                    var date = DateTime.Parse(dateString);
                    string url = $"https://{md.D1}{md.Dir}/{md.Files.Where(f => f.Source == "original" && f.Format == "VBR MP3" && f.Name.EndsWith(".mp3")).First().Name}";

                    downloadURLs.Add(new DownloadURL(date, url, filename));
                }
            } catch (Exception ignored) {}

            downloadURLs.Sort((a, b) => a.PublishDate.CompareTo(b.PublishDate));

             // Check if files have already been downloaded
            string absoluteFolderPath = Path.Combine(Directory.GetCurrentDirectory(), relativePathToDownloadsFolder);

            if (!Directory.Exists(absoluteFolderPath))
            {
                Console.WriteLine($"The folder '{relativePathToDownloadsFolder}' does not exist. Creating it...");
                Directory.CreateDirectory(absoluteFolderPath);
            }

            // Check for each file's existence
            foreach (var download in downloadURLs)
            {
                string filePath = Path.Combine(absoluteFolderPath, download.Filename);
                download.Downloaded = File.Exists(filePath);
            }

            // Bind the list to the DataGrid
            MetadataGrid.ItemsSource = downloadURLs;
        }

        private async Task StartDownloadProcess()
        {
            try
            {
                Console.WriteLine($"The start date is {StartDate} and end is {EndDate}");
                Console.WriteLine($"Going to filter");
                var filteredDownloads = downloadURLs.Where(d => d.PublishDate >= StartDate && d.PublishDate <= EndDate && !d.Downloaded).ToList();
                Console.WriteLine("Downloads have been filtered");

                // Folder to save the downloads
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), relativePathToDownloadsFolder);
                Console.WriteLine($"folderPath is {folderPath}");
                Directory.CreateDirectory(folderPath);

                using HttpClient httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(30) // internet archive can be slow
                };

                // Download files in series
                foreach (var download in filteredDownloads)
                {
                    Console.WriteLine($"Downloading {download.Filename}...");
                    string filePath = Path.Combine(folderPath, download.Filename);

                    // Check if the file already exists
                    if (File.Exists(filePath))
                    {
                        Console.WriteLine($"File '{download.Filename}' already exists. Skipping...");
                        continue;
                    }

                    // Download the file
                    Console.WriteLine($"Downloading: {download.URL}");
                    try
                    {
                        byte[] fileBytes = await httpClient.GetByteArrayAsync(download.URL);
                        await File.WriteAllBytesAsync(filePath, fileBytes);
                        Console.WriteLine($"Downloaded '{download.Filename}' to {filePath}");
                        download.Downloaded = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to download {download.URL}: {ex.Message}");
                    }
                }

                Console.WriteLine("All downloads complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            await StartDownloadProcess();
        }
    }

    public class MetadataFile
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("mtime")]
        public string Mtime { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("crc32")]
        public string Crc32 { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("track")]
        public string Track { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("rotation")]
        public string Rotation { get; set; }

        [JsonProperty("btih")]
        public string Btih { get; set; }

        [JsonProperty("summation")]
        public string Summation { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("d1")]
        public string D1 { get; set; }

        [JsonProperty("d2")]
        public string D2 { get; set; }

        [JsonProperty("dir")]
        public string Dir { get; set; }

        [JsonProperty("files")]
        public List<MetadataFile> Files { get; set; }
    }

    public class DownloadURL {
        public DateTime PublishDate { get; set; }
        public string URL { get; set; }
        public string Filename { get; set; }
        public bool Downloaded { get; set; }

        public DownloadURL(DateTime publishDate, string url, string filename) {
            PublishDate = publishDate;
            URL = url;
            Filename = filename;
            Downloaded = false; // Determined by checking for the file on disk
        }
    }
}
