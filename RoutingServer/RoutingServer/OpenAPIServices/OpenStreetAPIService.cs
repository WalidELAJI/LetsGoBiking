using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RoutingServer.OpenAPIServices
{
    public class OpenStreetAPIService
    {
        // Static HttpClient instance for making requests to the OpenStreetMap API
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Performs forward geocoding to find locations based on a search query.
        /// </summary>
        /// <param name="query">The search query (e.g., an address or location name).</param>
        /// <returns>A list of dynamic objects containing geocode results.</returns>
        public async Task<List<dynamic>> GeocodeQuery(string query)
        {
            try
            {
                // Construct the API URL with the user query and parameters
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=5";

                // Send the GET request to the Geocode API
                HttpResponseMessage response = await httpClient.GetAsync(url);

                // Ensure the response indicates success (throws an exception for non-2xx status codes)
                response.EnsureSuccessStatusCode();

                // Read the response content as a JSON string
                string json = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON string into a list of dynamic objects
                return JsonConvert.DeserializeObject<List<dynamic>>(json);
            }
            catch (Exception exception)
            {
                // Log any errors that occur during the request or deserialization
                Console.WriteLine($"Error in Geocode: {exception.Message}");
                throw new Exception("Error occurred while fetching geocode data.");
            }
        }

        /// <summary>
        /// Performs reverse geocoding to find the city corresponding to a latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude of the location.</param>
        /// <param name="longitude">The longitude of the location.</param>
        /// <returns>The name of the city as a string.</returns>
        public async Task<string> ReverseGeocodeQuery(double latitude, double longitude)
        {
            try
            {
                // Format the latitude and longitude to ensure proper formatting for the API
                string latitudeFormatted = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string longitudeFormatted = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Construct the API URL for reverse geocoding
                string url = $"https://nominatim.openstreetmap.org/reverse?lat={latitudeFormatted}&lon={longitudeFormatted}&format=json";

                // Add a User-Agent header to comply with OpenStreetMap API usage policies
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WebMapProjet/1.0 (PNS@unice.fr)");

                // Send the GET request to the Reverse Geocode API
                HttpResponseMessage httpresponsemessage = await httpClient.GetAsync(url);

                // Ensure the response indicates success
                httpresponsemessage.EnsureSuccessStatusCode();

                // Read the response content as a JSON string
                string json = await httpresponsemessage.Content.ReadAsStringAsync();

                // Deserialize the JSON string into a dynamic object
                var responseInJson = JsonConvert.DeserializeObject<dynamic>(json);

                // Attempt to extract the city name from the response
                string city = responseInJson?.address?.city
                              ?? responseInJson?.address?.municipality
                              ?? responseInJson?.address?.town
                              ?? responseInJson?.address?.village;

                // Throw an exception if no city information is found in the response
                if (string.IsNullOrEmpty(city))
                {
                    throw new Exception("City not found.");
                }

                return city; // Return the extracted city name
            }
            catch (HttpRequestException httprequestexception)
            {
                // Log HTTP-specific errors (e.g., network issues, invalid URLs)
                Console.WriteLine($"Error in ReverseGeocode: {httprequestexception.Message}");
                throw new Exception("Error occurred while fetching reverse geocode data.");
            }
            catch (Exception exception)
            {
                // Log any unexpected errors
                Console.WriteLine($"Error in ReverseGeocode: {exception.Message}");
                throw new Exception("Error occurred while fetching reverse geocode data.");
            }
        }
    }
}
