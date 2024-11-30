using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CsharpServer.Itinerary;
using CsharpServer.JCDecaux;
using CsharpServer.OpenAPIServices;


namespace CsharpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {
            HttpListener httplistener = new HttpListener();
            httplistener.Prefixes.Add("http://localhost:5000/"); // Base URL
            httplistener.Start();
            Console.WriteLine("Server listening at http://localhost:5000/");

            while (true)
            {
                var context = httplistener.GetContext(); // Wait for incoming requests
                _ = HandleRequest(context); // Fire and forget to handle requests asynchronously
            }
        }

        public static async Task HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            // Add CORS headers
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            string response = "";

            try
            {
                if (path == "/jcdecaux/stations")
                {
                    string city = context.Request.QueryString["city"];
                    var stations = JCDecauxService.GetBikeStations(city);
                    response = Newtonsoft.Json.JsonConvert.SerializeObject(stations);
                    context.Response.StatusCode = 200; // OK
                }
                else if (path == "/openstreetmap/geocode")
                {
                    string query = context.Request.QueryString["query"];
                    var geocodeResponse = await OpenStreetAPIService.GeocodeQuery(query);
                    response = Newtonsoft.Json.JsonConvert.SerializeObject(geocodeResponse);
                    context.Response.StatusCode = 200; // OK
                }
                else if (path == "/openstreetmap/reverse")
                {
                    string latitude = context.Request.QueryString["lat"];
                    string longitude = context.Request.QueryString["lon"];
                    var responseReverted = await OpenStreetAPIService.ReverseGeocodeQuery(
                        double.Parse(latitude, System.Globalization.CultureInfo.InvariantCulture),
                        double.Parse(longitude, System.Globalization.CultureInfo.InvariantCulture)
                    );
                    response = Newtonsoft.Json.JsonConvert.SerializeObject(responseReverted);
                    context.Response.StatusCode = 200; // OK
                }

                else if (path == "/itinerary")
                {
                    string originLatitude = context.Request.QueryString["originLat"];
                    string originLongitude = context.Request.QueryString["originLon"];
                    string destinationLatitude = context.Request.QueryString["destinationLat"];
                    string destinationLongitude = context.Request.QueryString["destinationLon"];
                    string mode = context.Request.QueryString["mode"]; // New parameter

                    Console.WriteLine($" Parameters received : originLatitude={originLatitude}, originLongitude={originLongitude}, destinationLatitude={destinationLatitude}, destinationLongitude={destinationLongitude}, ItineraryMode={mode}");

                    if (string.IsNullOrWhiteSpace(originLatitude) || string.IsNullOrWhiteSpace(originLongitude) ||
                        string.IsNullOrWhiteSpace(destinationLatitude) || string.IsNullOrWhiteSpace(destinationLongitude) || string.IsNullOrWhiteSpace(mode))
                    {
                        response = "{\"error\": \"Missing required query parameters.\"}";
                        context.Response.StatusCode = 400; // Bad Request
                    }
                    else
                    {
                        try
                        {
                            // Parse query parameters
                            double parsedoriginLatitude = double.Parse(originLatitude, System.Globalization.CultureInfo.InvariantCulture);
                            double parsedOriginLongitude = double.Parse(originLongitude, System.Globalization.CultureInfo.InvariantCulture);
                            double parsedDestinationLatitude = double.Parse(destinationLatitude, System.Globalization.CultureInfo.InvariantCulture);
                            double parsedDestinationLongitude = double.Parse(destinationLongitude, System.Globalization.CultureInfo.InvariantCulture);
                            bool Bike = mode == "cycling"; // Determine the mode

                            // Call the ItineraryService
                            response = await ItineraryService.GenerateItinerary(
                                parsedoriginLatitude, parsedOriginLongitude, parsedDestinationLatitude, parsedDestinationLongitude, Bike);

                            context.Response.StatusCode = 200; // OK
                        }
                        catch (FormatException ex)
                        {
                            response = "{\"error\": \"Query parameters must be valid numbers.\"}";
                            context.Response.StatusCode = 400; // Bad Request
                        }
                        catch (Exception ex)
                        {
                            response = "{\"error\": \"An unexpected error occurred while processing the request.\"}";
                            context.Response.StatusCode = 500; // Internal Server Error
                        }
                    }
                }

                else
                {
                    response = "{\"error\": \" Endpoint Invalid.\"}";
                    context.Response.StatusCode = 404; // Not Found
                }

                // Write response
                context.Response.ContentType = "application/json";
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(response);
                    writer.Flush(); // Ensure all data is sent before closing
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in HandleRequest: {exception.Message}");
                try
                {
                    context.Response.StatusCode = 500; // Internal Server Error
                    context.Response.ContentType = "application/json";
                    using (var streamwriter = new StreamWriter(context.Response.OutputStream))
                    {
                        streamwriter.Write("{\"error\": \"Unexpected error occurred.\"}");
                    }
                }
                catch (Exception exception_)
                {
                    Console.WriteLine($"Failed to send error response: {exception_.Message}");
                }
            }
            finally
            {
                // Ensure the response is always closed
                try
                {
                    context.Response.Close();
                }
                catch (Exception _exception_)
                {
                    Console.WriteLine($"Error while closing response: {_exception_.Message}");
                }
            }
        }
    }
}
