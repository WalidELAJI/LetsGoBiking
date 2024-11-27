using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapProject.Services
{
    public class OpenRouteAPIService
    {
        private const string ApiKey = "5b3ce3597851110001cf6248bbc1b0a571f842458d7d6321cf494140";

        public static async Task<string> ComputeItinerary(double originLat, double originLon, double destinationLat, double destinationLon, bool useBike)
        {
            string profile = useBike ? "cycling-regular" : "foot-walking"; // Use bike or pedestrian profile
            string url = $"https://api.openrouteservice.org/v2/directions/{profile}?language=fr"; // Add French language parameter

            var payload = new
            {
                coordinates = new[]
                {
                    new[] { originLon, originLat }, // OpenRouteService expects [longitude, latitude]
                    new[] { destinationLon, destinationLat }
                }
            };

            try
            {
                using (var client = new WebClient())
                {
                    // Add headers
                    client.Headers.Add("Authorization", ApiKey);
                    client.Headers.Add("Content-Type", "application/json");

                    // Serialize the payload
                    string payloadJson = JsonConvert.SerializeObject(payload);

                    // Log request details (optional for debugging)
                    Console.WriteLine($"Sending request to OpenRouteService: {url}");
                    Console.WriteLine($"Payload: {payloadJson}");

                    // Send the POST request
                    string response = await client.UploadStringTaskAsync(url, "POST", payloadJson);

                    // Parse and return the JSON response
                    JObject jsonResponse = JObject.Parse(response);
                    return jsonResponse.ToString();
                }
            }
            catch (WebException webEx)
            {
                // Log details for WebException
                Console.WriteLine($"Error in WebClient: {webEx.Message}");
                if (webEx.Response != null)
                {
                    using (var reader = new StreamReader(webEx.Response.GetResponseStream()))
                    {
                        string errorResponse = await reader.ReadToEndAsync();
                        Console.WriteLine($"Error response from OpenRouteService: {errorResponse}");
                    }
                }
                throw new Exception("Error calling OpenRouteService. Check logs for details.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw new Exception("An unexpected error occurred while computing the itinerary.");
            }
        }
    }
}
