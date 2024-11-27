using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using MapProject.Models;

namespace MapProject.Services
{
    public class JCDecauxService
    {
        private const string ApiKey = "c8cb5a7b30b3bac4849ab1a43f40174505597837";

        public static List<BikeStation> GetBikeStations(string city)
        {
            string encodedCity = Uri.EscapeDataString(city);
            string url = $"https://api.jcdecaux.com/vls/v1/stations?contract={encodedCity}&apiKey={ApiKey}";
            Console.WriteLine(url);
            using (var client = new WebClient())
            {
                try
                {
                    string json = client.DownloadString(url);

                    // Check if the JSON is empty or null
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Console.WriteLine("No data received from the API.");
                        return new List<BikeStation>();
                    }

                    // Deserialize the raw JSON into a dynamic object
                    var stations = JsonConvert.DeserializeObject<List<dynamic>>(json);

                    // Check if the stations list is null or empty
                    if (stations == null || stations.Count == 0)
                    {
                        Console.WriteLine("No bike stations found.");
                        return new List<BikeStation>();
                    }

                    // Map the dynamic data into strongly-typed BikeStation objects
                    var bikeStations = new List<BikeStation>();
                    foreach (var station in stations)
                    {
                        bikeStations.Add(new BikeStation
                        {
                            Name = station.name,
                            AvailableBikes = station.available_bikes,
                            BikeStands = station.bike_stands,
                            Latitude = station.position.lat,
                            Longitude = station.position.lng
                        });
                    }

                    return bikeStations;
                }
                catch (WebException ex)
                {
                    Console.WriteLine($"Error fetching bike stations: {ex.Message}");
                    return new List<BikeStation>(); // Return empty list on error
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return new List<BikeStation>();
                }
            }
        }
    }
}
