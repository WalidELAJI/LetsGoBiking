using System;
using System.Linq;
using System.Threading.Tasks;
using MapProject.Models;
using MapProject.Services;

namespace MapProject.Services
{
    public class ItineraryService
    {
        public static async Task<string> GetItinerary(double originLat, double originLon, double destinationLat, double destinationLon, bool useBike)
        {
            try
            {
                // Step 1: Find JCDecaux contracts for origin and destination
                string originCity = await OpenStreetAPIService.ReverseGeocode(originLat, originLon);
                string destinationCity = await OpenStreetAPIService.ReverseGeocode(destinationLat, destinationLon);

                // Step 2: Retrieve all stations for the JCDecaux contracts, if available
                var originStations = JCDecauxService.GetBikeStations(originCity);
                var destinationStations = JCDecauxService.GetBikeStations(destinationCity);

                // Step 3: Find the closest station with available bikes
                var closestOriginStation = originStations?
                    .Where(station => station.AvailableBikes > 0)
                    .OrderBy(station => HaversineDistance(originLat, originLon, station.Latitude, station.Longitude))
                    .FirstOrDefault();

                // Step 4: Find the closest station with available spaces
                var closestDestinationStation = destinationStations?
                    .Where(station => station.BikeStands > station.AvailableBikes)
                    .OrderBy(station => HaversineDistance(destinationLat, destinationLon, station.Latitude, station.Longitude))
                    .FirstOrDefault();

                // Step 5: Determine whether to use stations or compute direct route
                if (closestOriginStation != null && closestDestinationStation != null)
                {
                    // Compute bike itinerary between the closest stations
                    string stationToStationItinerary = await OpenRouteAPIService.ComputeItinerary(
                        closestOriginStation.Latitude, closestOriginStation.Longitude,
                        closestDestinationStation.Latitude, closestDestinationStation.Longitude,
                        useBike: true // Always bike for station-to-station
                    );

                    // Include walking routes to/from stations based on the `useBike` parameter
                    string originToStationItinerary = await OpenRouteAPIService.ComputeItinerary(
                        originLat, originLon,
                        closestOriginStation.Latitude, closestOriginStation.Longitude,
                        useBike: !useBike // Walk if the main mode is bike, bike otherwise
                    );

                    string stationToDestinationItinerary = await OpenRouteAPIService.ComputeItinerary(
                        closestDestinationStation.Latitude, closestDestinationStation.Longitude,
                        destinationLat, destinationLon,
                        useBike: !useBike // Walk if the main mode is bike, bike otherwise
                    );

                    // Construct response including stations
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        UseBike = useBike,
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
                    // Compute direct itinerary without involving stations
                    string directItinerary = await OpenRouteAPIService.ComputeItinerary(
                        originLat, originLon,
                        destinationLat, destinationLon,
                        useBike: useBike // Use the selected mode
                    );

                    // Construct response without stations
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        UseBike = useBike,
                        ClosestOriginStation = (BikeStation)null,
                        ClosestDestinationStation = (BikeStation)null,
                        Itinerary = directItinerary
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                Console.WriteLine($"Error in GetItinerary: {ex.Message}");
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Error = "An unexpected error occurred while generating the itinerary.",
                    Details = ex.Message
                });
            }
        }


        // Haversine formula to compute distances
        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // Earth's radius in meters
            double phi1 = lat1 * Math.PI / 180;
            double phi2 = lat2 * Math.PI / 180;
            double deltaPhi = (lat2 - lat1) * Math.PI / 180;
            double deltaLambda = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in meters
        }
    }
}
