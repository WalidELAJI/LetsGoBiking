using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CsharpServer.OpenAPIServices
{
    public class OpenRouteAPIService
    {
        // API key for accessing OpenRouteService
        private const string ORSApiKey = "5b3ce3597851110001cf6248bbc1b0a571f842458d7d6321cf494140";

        /// <summary>
        /// Calculates the itinerary between two geographical points using OpenRouteService.
        /// </summary>
        /// <param name="originLatitude">Latitude of the origin point.</param>
        /// <param name="originLongitude">Longitude of the origin point.</param>
        /// <param name="destinationLatitude">Latitude of the destination point.</param>
        /// <param name="destinationLongitude">Longitude of the destination point.</param>
        /// <param name="Bike">Specifies whether the route is for biking (true) or walking (false).</param>
        /// <returns>The JSON response from OpenRouteService as a string.</returns>
        public static async Task<string> CalculateItinerary(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude, bool Bike)
        {
            // Select the transportation profile based on the useBike parameter
            string profile = Bike ? "cycling-regular" : "foot-walking"; // Biking or walking profile

            // Construct the OpenRouteService API URL with language parameter set to French
            string url = $"https://api.openrouteservice.org/v2/directions/{profile}?language=fr";

            // Define the payload with coordinates in the expected [longitude, latitude] format
            var payload = new
            {
                coordinates = new[]
                {
                    new[] { originLongitude, originLatitude }, // Origin coordinates
                    new[] { destinationLongitude, destinationLatitude } // Destination coordinates
                }
            };

            try
            {
                using (var client = new WebClient())
                {
                    // Set the required headers for the API request
                    client.Headers.Add("Content-Type", "application/json"); // Specify JSON content type
                    client.Headers.Add("Authorization", ORSApiKey); // API key for authentication

                    // Serialize the payload into a JSON string
                    string payloadInJson = JsonConvert.SerializeObject(payload);

                    // Send the POST request with the payload and get the response
                    string response = await client.UploadStringTaskAsync(url, "POST", payloadInJson);

                    // Parse the response JSON and return it as a formatted string
                    JObject responseInJson = JObject.Parse(response);
                    return responseInJson.ToString();
                }
            }
            catch (WebException webException)
            {
                // If the server returned an error response, log its details
                if (webException.Response != null)
                {
                    using (var reader = new StreamReader(webException.Response.GetResponseStream()))
                    {
                        string errorResponse = await reader.ReadToEndAsync();
                        Console.WriteLine($"Error response from OpenRouteService: {errorResponse}");
                    }
                }

                // Throw a general exception for the caller to handle
                throw new Exception("Error calling OpenRouteService.");
            }
            catch (Exception exception)
            {
                // Handle unexpected errors
                Console.WriteLine($"error: {exception.Message}");
                throw new Exception("An unexpected error occurred while computing the itinerary.");
            }
        }
    }
}
