using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace IdGamesRandomDownloader
{
    internal class Program
    {
        // Use the program's directory for downloads.
        private static readonly string downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads");
        private static readonly int[] contentsIds = ContentId.Ids;

        static async Task Main()
        {
            Console.WriteLine("Retrieving a random WAD file...");
            do
            {
                int id = contentsIds[RandomNumberGenerator.GetInt32(contentsIds.Length)];
                string apiUrl = $"https://www.doomworld.com/idgames/api/api.php?action=getcontents&id={id}";

                string response = await SendRequest(apiUrl);

                Console.WriteLine("File Information:\r\n----------------\r\n");
                bool downloaded = DisplayFileAsCard(response);

                if (downloaded)
                    Thread.Sleep(3000);
                Console.Clear();
            }
            while (true);
        }

        static async Task<string> SendRequest(string apiUrl)
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : $"Error in request: {response.StatusCode}";
        }

        static bool DisplayFileAsCard(string xml)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(xml);
                var randomFile = xDoc.Descendants("file").ToArray()[RandomNumberGenerator.GetInt32(xDoc.Descendants("file").Count())];

                if (Debugger.IsAttached) //Debug only
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(randomFile);
                    Console.ResetColor();
                    Console.WriteLine("\r\n----------------\r\n");
                }

                if (randomFile != null)
                {
                    string? title = randomFile.Element("title")?.Value;
                    string? author = randomFile.Element("author")?.Value;
                    string? date = randomFile.Element("date")?.Value;

                    string? description = randomFile.Element("description")?.Value;
                    if (!string.IsNullOrEmpty(description))
                    {
                        description = description.Replace("<br>", "\n");
                    }

                    float.TryParse(randomFile.Element("rating")?.Value, CultureInfo.InvariantCulture, out float rating);
                    string? votes = randomFile.Element("votes")?.Value;
                    string? url = randomFile.Element("url")?.Value;
                    string? idgamesprotocol = randomFile.Element("idgamesurl")?.Value;

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Title: {title}");
                    Console.WriteLine($"Author: {author}");
                    Console.WriteLine($"Date : {date}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Description: {description}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Average Rating : {rating:0.0}/5");
                    Console.WriteLine($"Votes: {votes}");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"URL: {url}");
                    Console.WriteLine($"IdGames Protocol: {idgamesprotocol}");
                    Console.ResetColor();
                    Console.WriteLine("\r\n----------------\r\n");

                    Console.Write("Would you like to download this file? (yes/no): ");
                    string? userResponse = Console.ReadLine();

                    if ((userResponse?.ToLower() == "yes" || userResponse == string.Empty) && idgamesprotocol is not null)
                    {
                        DownloadFile(idgamesprotocol);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("No 'file' element found in XML response.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing XML: {ex.Message}");
            }
            return false;
        }

        static void DownloadFile(string fileUrl)
        {
            try
            {
                string mirrorUrl = GetMirrorUrl(fileUrl);
                using HttpClient client = new();
                byte[] fileData = client.GetByteArrayAsync(mirrorUrl).Result;
                string cleanedFileName = CleanFileName(fileUrl);
                string filePath = Path.Combine(downloadPath, $"{cleanedFileName}");

                if (!Directory.Exists(downloadPath))
                {
                    Directory.CreateDirectory(downloadPath);
                }

                File.WriteAllBytes(filePath, fileData);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"File successfully downloaded to: {filePath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error downloading file: {ex.Message}");
                Console.ResetColor();
            }
        }

        static string CleanFileName(string fileName)
        {
            return fileName.Split('/').Last().Split('?').FirstOrDefault() ?? "download";
        }

        static string GetMirrorUrl(string idgamesUrl)
        {
            return idgamesUrl.Replace("idgames://", "https://www.gamers.org/pub/idgames/");
        }
    }
}
