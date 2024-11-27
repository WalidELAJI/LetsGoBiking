using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapProject.Services
{
    public class OpenStreetAPIService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<List<dynamic>> Geocode(string query)
        {
            try
            {
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=5";
                Console.WriteLine($"Geocode API Request: {url}");

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throws if the status code is not 2xx

                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<dynamic>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Geocode: {ex.Message}");
                throw new Exception("Error occurred while fetching geocode data.");
            }
        }

        public static async Task<string> ReverseGeocode(double latitude, double longitude)
        {
            try
            {
                string latFormatted = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string lonFormatted = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

                string url = $"https://nominatim.openstreetmap.org/reverse?lat={latFormatted}&lon={lonFormatted}&format=json";

                // Add User-Agent header
                client.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName/1.0 (your_email@example.com)");
                Console.WriteLine($"Reverse Geocode API Request: {url}");

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                // Deserialize and extract the city
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(json);
                string city = jsonResponse?.address?.city ?? jsonResponse?.address?.municipality ?? jsonResponse?.address?.town ?? jsonResponse?.address?.village;

                if (string.IsNullOrEmpty(city))
                {
                    throw new Exception("City not found in Reverse Geocode response.");
                }

                return city;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Error in ReverseGeocode: {ex.Message}");
                throw new Exception("Error occurred while fetching reverse geocode data.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error in ReverseGeocode: {ex.Message}");
                throw new Exception("Error occurred while fetching reverse geocode data.");
            }
        }





    }
}
