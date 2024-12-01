using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsharpServer.Itinerary;
using CsharpServer.JCDecaux;
using CsharpServer.OpenAPIServices;

namespace MapProject.Proxy
{

    public class ProxyService : InterfaceProxy
    {

        /*------proxy+cache pour Service JCDecaux------*/
        private readonly JCDecauxService monJCDservice = new JCDecauxService();
        private readonly CacheService<List<BikeStation>> cacheJCDecaux = new CacheService<List<BikeStation>>(TimeSpan.FromMinutes(10));
        public async Task<List<BikeStation>> GetStationsJcdecaux(string city)
        {
            string cacheKey = $"JCDecaux_BikeStations_{city}_Cache";
            try
            {
                return await cacheJCDecaux.GetOrAddAsync(cacheKey, async () =>
                {
                    return await monJCDservice.GetBikeStations(city);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur GetStationsJcdecaux: " + e);
                throw;
            }
        }

        /*------proxy+cache pour Service Itinerary------*/
        private readonly ItineraryService monItineraryservice = new ItineraryService();
        private readonly CacheService<string> cacheItinerary = new CacheService<string>(TimeSpan.FromMinutes(10));

        public async Task<string> getGeneratedItinerary(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude, bool Bike)
        {
            string cacheKey = $"Itinerary_{originLatitude}_{originLongitude}_{destinationLatitude}_{destinationLongitude}_{Bike}_Cache";
            try
            {
                return await cacheItinerary.GetOrAddAsync(cacheKey, async () =>
                {
                    return await monItineraryservice.GenerateItinerary(originLatitude, originLongitude, destinationLatitude, destinationLongitude, Bike);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur getGeneratedItinerary: " + e);
                throw;
            }
        }








        /*------proxy+cache pour Service OpenStreetApi------*/
        private readonly OpenStreetAPIService monOpentStreetApi = new OpenStreetAPIService();
        private readonly CacheService<List<dynamic>> cacheGeocode = new CacheService<List<dynamic>>(TimeSpan.FromMinutes(10));
        private readonly CacheService<string> cacheReverseGeocode = new CacheService<string>(TimeSpan.FromMinutes(10));

        public async Task<List<dynamic>> getAPIGeocode(string query)
        {
            string cacheKey = $"OpenStreetAPI_Geocode_{query}_Cache";
            try
            {
                return await cacheGeocode.GetOrAddAsync(cacheKey, async () =>
                {
                    return await monOpentStreetApi.GeocodeQuery(query);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur getAPIGeocode: " + e);
                throw;
            }
        }
        public async Task<string> getAPIReverseGeocode(double latitude, double longitude)
        {
            string cacheKey = $"OpenStreetAPI_ReverseGeocode_{latitude}_{longitude}_Cache";
            try
            {
                return await cacheReverseGeocode.GetOrAddAsync(cacheKey, async () =>
                {
                    return await monOpentStreetApi.ReverseGeocodeQuery(latitude, longitude);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur getAPIReverseGeocode: " + e);
                throw;
            }
        }

        /*------proxy+cache pour Service OpenRouteApi------*/
        private readonly OpenRouteAPIService monOpenRouteApi = new OpenRouteAPIService();
        private readonly CacheService<string> cacheOpenRoute = new CacheService<string>(TimeSpan.FromMinutes(10));

        public async Task<string> getCalculatedItinerary(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude, bool Bike)
        {
            string cacheKey = $"OpenRouteAPI_Itinerary_{originLatitude}_{originLongitude}_{destinationLatitude}_{destinationLongitude}_{Bike}_Cache";
            try
            {
                return await cacheOpenRoute.GetOrAddAsync(cacheKey, async () =>
                {
                    return await monOpenRouteApi.CalculateItinerary(originLatitude, originLongitude, destinationLatitude, destinationLongitude, Bike);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur getCalculatedItinerary: " + e);
                throw;
            }
        }

       

    }
}
