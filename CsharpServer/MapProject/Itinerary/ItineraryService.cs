using System;
using System.Linq;
using System.Threading.Tasks;
using CsharpServer.JCDecaux;
using CsharpServer.OpenAPIServices;

namespace CsharpServer.Itinerary
{
    public class ItineraryService
    {

        // Calculate the approximate distance between two geographical points using Euclidean distance
        // This approximation is suitable for small distances and provides similar results to the Haversine formula.
        private static double EuclideanDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // Earth's radius in meters
            double deltaLat = (lat2 - lat1) * Math.PI / 180; // Latitude difference in radians
            double deltaLon = (lon2 - lon1) * Math.PI / 180; // Longitude difference in radians
            double phi1 = lat1 * Math.PI / 180; // Convert latitude to radians
            double phi2 = lat2 * Math.PI / 180; // Convert latitude to radians

            // Approximation of Cartesian distances
            double x = deltaLon * Math.Cos((phi1 + phi2) / 2); // Adjust for convergence of meridians
            double y = deltaLat;

            // Return the Euclidean distance scaled to Earth's radius
            return Math.Sqrt(x * x + y * y) * R;
        }

        public static async Task<string> GenerateItinerary(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude, bool Bike)
        {
            try
            {
                // Retrieve the city names for the origin and destination using reverse geocoding
                // Reverse geocoding converts latitude and longitude into a human-readable location (city name).
                string originCity = await OpenStreetAPIService.ReverseGeocodeQuery(originLatitude, originLongitude);
                string destinationCity = await OpenStreetAPIService.ReverseGeocodeQuery(destinationLatitude, destinationLongitude);

                // Fetch bike stations associated with the JCDecaux contracts for the identified cities
                // JCDecauxService retrieves a list of bike stations available in the specified city.
                var originStations = JCDecauxService.GetBikeStations(originCity);
                var destinationStations = JCDecauxService.GetBikeStations(destinationCity);

                // Identify the nearest station with available bikes near the origin
                // Closest station is determined based on distance and bike availability.
                var closestOriginStation = originStations?
                    .Where(station => station.BikesAvailable > 0) // Station must have bikes available
                    .OrderBy(station => EuclideanDistance(originLatitude, originLongitude, station.Latitude, station.Longitude))
                    .FirstOrDefault(); // Select the closest station

                // Identify the nearest station with available spaces near the destination
                // Closest station is determined based on distance and space availability for bike drop-off.
                var closestDestinationStation = destinationStations?
                    .Where(station => station.BikeStands > station.BikesAvailable) // Must have free stands
                    .OrderBy(station => EuclideanDistance(destinationLatitude, destinationLongitude, station.Latitude, station.Longitude))
                    .FirstOrDefault(); // Select the closest station

                // Determine the itinerary approach based on station availability and user preference for bike usage
                if (closestOriginStation != null && closestDestinationStation != null && Bike == true)
                {
                    // If bike stations are available, calculate the bike route between the stations

                    // Calculate the route between the two closest stations (station-to-station route)
                    string stationToStationItinerary = await OpenRouteAPIService.CalculateItinerary(
                        closestOriginStation.Latitude, closestOriginStation.Longitude,
                        closestDestinationStation.Latitude, closestDestinationStation.Longitude,
                        Bike: true // Ensure biking is used for station-to-station travel
                    );

                    // Calculate the walking/biking route from the origin to the nearest origin station
                    string originToStationItinerary = await OpenRouteAPIService.CalculateItinerary(
                        originLatitude, originLongitude,
                        closestOriginStation.Latitude, closestOriginStation.Longitude,
                        Bike: !Bike // Walk if the main mode is bike, otherwise bike
                    );

                    // Calculate the walking/biking route from the destination station to the final destination
                    string stationToDestinationItinerary = await OpenRouteAPIService.CalculateItinerary(
                        closestDestinationStation.Latitude, closestDestinationStation.Longitude,
                        destinationLatitude, destinationLongitude,
                        Bike: !Bike // Walk if the main mode is bike, otherwise bike
                    );

                    // Construct a detailed response containing the station details and calculated itineraries
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        UseBike = Bike,
                        ClosestOriginStation = closestOriginStation,
                        ClosestDestinationStation = closestDestinationStation,
                        Itinerary = new
                        {
                            OriginToStation = originToStationItinerary,
                            StationToStation = stationToStationItinerary,
                            StationToDestination = stationToDestinationItinerary
                        }
                    });
                }
                else
                {
                    // If bike stations are unavailable or biking is not preferred, calculate a direct route

                    // Calculate the direct route from origin to destination without involving bike stations
                    string directItinerary = await OpenRouteAPIService.CalculateItinerary(
                        originLatitude, originLongitude,
                        destinationLatitude, destinationLongitude,
                        Bike: Bike // Use the selected travel mode (bike or walk)
                    );

                    // Construct a simpler response without station details
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        UseBike = Bike,
                        ClosestOriginStation = (BikeStation)null, // No stations involved
                        ClosestDestinationStation = (BikeStation)null, // No stations involved
                        Itinerary = directItinerary
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors and return a user-friendly error message
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Error = "An unexpected error occurred while generating the itinerary.",
                    Details = ex.Message // Include technical details for developers
                });
            }
        }

    }
}
