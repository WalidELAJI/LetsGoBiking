using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace CsharpServer.Suggestions
{
    public class SuggestionService
    {
        // Base URL for the OpenStreetMap Nominatim API
        private const string OSMNominatimApiUrl = "https://nominatim.openstreetmap.org/search";

        /// <summary>
        /// Fetches location suggestions based on a user query.
        /// </summary>
        /// <param name="query">The search query string entered by the user.</param>
        /// <returns>A list of suggestions filtered to include locations in France.</returns>
        public static async Task<List<SuggestionDetails>> GetSuggestions(string query)
        {
            // Construct the API URL with the query parameter
            string url = $"{OSMNominatimApiUrl}?q={WebUtility.UrlEncode(query)}&format=json&limit=5";

            try
            {
                using (var webclient = new WebClient())
                {
                    // Step 1: Configure the WebClient with UTF-8 encoding and appropriate headers
                    webclient.Encoding = Encoding.UTF8; // Ensure correct character encoding
                    webclient.Headers.Add("User-Agent", "WebMapProjet/1.0"); // Add User-Agent for compliance

                    // Step 2: Send the GET request and fetch the response as a string
                    string response = await webclient.DownloadStringTaskAsync(url);

                    // Step 3: Deserialize the response into a list of suggestion details
                    var CompleteSuggestions = JsonConvert.DeserializeObject<List<SuggestionDetails>>(response);

                    // Log all deserialized suggestions for debugging purposes
                    Console.WriteLine("Deserialized Suggestions:");
                    CompleteSuggestions.ForEach(s =>
                        Console.WriteLine($"DisplayName: {s.Name}, Lat: {s.Latitude}, Lon: {s.Longitude}")
                    );

                    // Step 4: Filter the suggestions to include only locations in France
                    var SuggestionsFiltered = CompleteSuggestions
                        .Where(s => s.Name != null && s.Name.Contains(", France")) // Check if location is in France
                        .ToList();

                    // Log the filtered suggestions
                    Console.WriteLine("Filtered Suggestions:");
                    SuggestionsFiltered.ForEach(s =>
                        Console.WriteLine($"DisplayName: {s.Name}, Lat: {s.Latitude}, Lon: {s.Longitude}")
                    );

                    // Return the filtered list of suggestions
                    return SuggestionsFiltered;
                }
            }
            catch (WebException webexceptions)
            {
                // Handle WebException (e.g., network issues or API errors)
                Console.WriteLine($"Error in Nominatim API: {webexceptions.Message}");
                throw new Exception("Error in suggestions.");
            }
        }
    }
}
