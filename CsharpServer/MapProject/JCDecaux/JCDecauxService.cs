using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace CsharpServer.JCDecaux
{
    public class JCDecauxService
    {
        // API key for accessing JCDecaux's bike station services
        private const string JCDApiKey = "c8cb5a7b30b3bac4849ab1a43f40174505597837";

        /// <summary>
        /// Retrieves a list of bike stations for the specified city using the JCDecaux API.
        /// </summary>
        /// <param name="city">The name of the city for which bike station data is requested.</param>
        /// <returns>A list of BikeStation objects representing the bike stations in the city.</returns>
        public static List<BikeStation> GetBikeStations(string city)
        {
            // Encode the city name to ensure it is safe for use in a URL
            string encodedCity = Uri.EscapeDataString(city);

            // Construct the API URL with the city and API key
            string url = $"https://api.jcdecaux.com/vls/v1/stations?contract={encodedCity}&apiKey={JCDApiKey}";

            // Initialize a WebClient to send the request
            using (var webclient = new WebClient())
            {
                try
                {
                    // Fetch the raw JSON data from the API
                    string json = webclient.DownloadString(url);

                    // Check if the API response is empty or invalid
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new List<BikeStation>(); // Return an empty list if no data is received
                    }

                    // Deserialize the JSON data into a dynamic list of stations
                    var stations = JsonConvert.DeserializeObject<List<dynamic>>(json);

                    // Verify if any stations were returned by the API
                    if (stations == null || stations.Count == 0)
                    {
                        return new List<BikeStation>(); // Return an empty list if no stations are found
                    }

                    // Map the dynamic objects into strongly-typed BikeStation instances
                    var bikeStations = new List<BikeStation>();
                    foreach (var station in stations)
                    {
                        bikeStations.Add(new BikeStation
                        {
                            Name = station.name, // Station name
                            BikesAvailable = station.available_bikes, // Number of bikes available
                            BikeStands = station.bike_stands, // Total number of bike stands
                            Latitude = station.position.lat, // Latitude coordinate of the station
                            Longitude = station.position.lng // Longitude coordinate of the station
                        });
                    }

                    // Return the list of BikeStation objects
                    return bikeStations;
                }
                catch (WebException exception)
                {
                    // Handle API errors, such as network issues or server unavailability
                    Console.WriteLine($"Error finding bike stations: {exception.Message}");
                    return new List<BikeStation>(); // Return an empty list if an error occurs
                }
                catch (Exception exception)
                {
                    // Handle unexpected errors (e.g., deserialization issues)
                    Console.WriteLine($"error: {exception.Message}");
                    return new List<BikeStation>(); // Return an empty list on unexpected exceptions
                }
            }
        }
    }
}
