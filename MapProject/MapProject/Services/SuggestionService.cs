using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MapProject.Models;
using System.Text;

namespace MapProject.Services
{
    public class SuggestionService
    {
        private const string NominatimApiUrl = "https://nominatim.openstreetmap.org/search";


        public static async Task<List<Suggestion>> GetSuggestions(string query)
        {
            string url = $"{NominatimApiUrl}?q={WebUtility.UrlEncode(query)}&format=json&limit=5";

            try
            {
                using (var client = new WebClient())
                {
                    // Ensure UTF-8 encoding
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add("User-Agent", "MapProject/1.0");

                    // Fetch response
                    string response = await client.DownloadStringTaskAsync(url);
                    Console.WriteLine("Raw Nominatim Response:");
                    Console.WriteLine(response); // Log raw response

                    // Deserialize response into a list of suggestions
                    var allSuggestions = JsonConvert.DeserializeObject<List<Suggestion>>(response);

                    // Log deserialized suggestions
                    Console.WriteLine("Deserialized Suggestions:");
                    allSuggestions.ForEach(s => Console.WriteLine($"DisplayName: {s.DisplayName}, Lat: {s.Latitude}, Lon: {s.Longitude}"));

                    // Filter suggestions for France
                    var filteredSuggestions = allSuggestions
                        .Where(s => s.DisplayName != null && s.DisplayName.Contains(", France"))
                        .ToList();

                    Console.WriteLine("Filtered Suggestions:");
                    filteredSuggestions.ForEach(s => Console.WriteLine($"DisplayName: {s.DisplayName}, Lat: {s.Latitude}, Lon: {s.Longitude}"));

                    return filteredSuggestions;
                }
            }
            catch (WebException webEx)
            {
                Console.WriteLine($"Error calling Nominatim API: {webEx.Message}");
                throw new Exception("Error fetching suggestions. Check the logs for details.");
            }
        }


    }
}
