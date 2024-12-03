using System;
using System.Linq;
using System.Threading.Tasks;
using RoutingServer.JCDecaux;
using RoutingServer.OpenAPIServices;
using MapProject.Proxy;
using RoutingServer.Proxy;

namespace RoutingServer.Itinerary
{
    public class ItineraryService
    {

        // Calculate the approximate distance between two geographical points using the Heaviside approximation
        // This method simplifies the distance calculation by assuming a spherical Earth and using Cartesian geometry.
        // Suitable for small distances with reduced computational complexity compared to the Haversine formula.

        private static double HeavisideDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // Rayon de la Terre en mètres
            double deltaLat = (lat2 - lat1) * Math.PI / 180; // Différence de latitude en radians
            double deltaLon = (lon2 - lon1) * Math.PI / 180; // Différence de longitude en radians

            // Approximation par Heaviside : distance simplifiée
            double x = deltaLon * Math.Cos(lat1 * Math.PI / 180); // Ajuste la convergence des méridiens
            double y = deltaLat;

            // Retourne la distance Euclidienne approximée
            return Math.Sqrt(x * x + y * y) * R;
        }

        public async Task<string> GenerateItinerary(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude, bool Bike)
        {
            ProxyService proxyService = new ProxyService();
            try
            {
                // Retrieve the city names for the origin and destination using reverse geocoding
                // Reverse geocoding converts latitude and longitude into a human-readable location (city name).
                string originCity = await proxyService.getAPIReverseGeocode(originLatitude, originLongitude);
                string destinationCity = await proxyService.getAPIReverseGeocode(destinationLatitude, destinationLongitude);

                // Fetch bike stations associated with the JCDecaux contracts for the identified cities
                // JCDecauxService retrieves a list of bike stations available in the specified city.
                var originStations = await proxyService.GetStationsJcdecaux(originCity);
                var destinationStations = await proxyService.GetStationsJcdecaux(destinationCity);

                // Identify the nearest station with available bikes near the origin
                // Closest station is determined based on distance and bike availability.
                var closestOriginStation = originStations?
                    .Where(station => station.BikesAvailable > 0) // Station must have bikes available
                    .OrderBy(station => HeavisideDistance(originLatitude, originLongitude, station.Latitude, station.Longitude))
                    .FirstOrDefault(); // Select the closest station

                // Identify the nearest station with available spaces near the destination
                // Closest station is determined based on distance and space availability for bike drop-off.
                var closestDestinationStation = destinationStations?
                    .Where(station => station.BikeStands > station.BikesAvailable) // Must have free stands
                    .OrderBy(station => HeavisideDistance(destinationLatitude, destinationLongitude, station.Latitude, station.Longitude))
                    .FirstOrDefault(); // Select the closest station

                // Determine the itinerary approach based on station availability and user preference for bike usage
                if (closestOriginStation != null && closestDestinationStation != null && Bike == true)
                {
                    // If bike stations are available, calculate the bike route between the stations

                    // Calculate the route between the two closest stations (station-to-station route)
                    string stationToStationItinerary = await proxyService.getCalculatedItinerary(
                        closestOriginStation.Latitude, closestOriginStation.Longitude,
                        closestDestinationStation.Latitude, closestDestinationStation.Longitude,
                        Bike: true // Ensure biking is used for station-to-station travel
                    );

                    // Calculate the walking/biking route from the origin to the nearest origin station
                    string originToStationItinerary = await proxyService.getCalculatedItinerary(
                        originLatitude, originLongitude,
                        closestOriginStation.Latitude, closestOriginStation.Longitude,
                        Bike: !Bike // Walk if the main mode is bike, otherwise bike
                    );

                    // Calculate the walking/biking route from the destination station to the final destination
                    string stationToDestinationItinerary = await proxyService.getCalculatedItinerary(
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
                    string directItinerary = await proxyService.getCalculatedItinerary(
                        originLatitude, originLongitude,
                        destinationLatitude, destinationLongitude,
                        Bike: Bike // Use the selected travel mode (bike or walk)
                    );

                    // Construct a simpler response without station details
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        UseBike = Bike,
                        ClosestOriginStation = (Station)null, // No stations involved
                        ClosestDestinationStation = (Station)null, // No stations involved
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
