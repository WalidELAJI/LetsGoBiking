using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MapProject.Services;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MapProject
{
    class Program
    {
        static List<string> instructionsQueue = new List<string>(); // Queue pour stocker les instructions de la requête en cours
        static IConnection connection;
        static ISession session;
        static IDestination destination;


        static void Main(string[] args)
        {
            // Initialiser la connexion à ActiveMQ et créer une queue
            InitializeActiveMQ("InstructionQueue");
            StartServer();

        }

        public static void StartServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/"); // Base URL
            listener.Start();
            Console.WriteLine("Server started at http://localhost:8080/");

            while (true)
            {
                var context = listener.GetContext(); // Wait for incoming requests
                _ = HandleRequest(context); // Fire and forget to handle requests asynchronously
            }
        }
        /*---------- méthodes pour gérer la connexion ActiveMQ ----------*/
        public static void InitializeActiveMQ(string queueName)
        {
            try
            {
                Uri connectUri = new Uri("activemq:tcp://localhost:61616");
                IConnectionFactory connectionFactory = new ConnectionFactory(connectUri);

                // Créer une connexion unique pour l'application
                connection = connectionFactory.CreateConnection();
                connection.Start();
                // Créer une session unique
                session = connection.CreateSession();
                // Créer ou cibler une queue
                destination = session.GetQueue(queueName);

                Console.WriteLine($"ActiveMQ initialisé avec la queue '{queueName}' en activemq:tcp://localhost:61616");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'initialisation de ActiveMQ : {ex.Message}");
            }
        }

        public static async Task HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            // Add CORS headers
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            string responseString = "";

            try
            {
                if (path == "/jcdecaux/stations")
                {
                    string city = context.Request.QueryString["city"];
                    var stations = JCDecauxService.GetBikeStations(city);
                    responseString = Newtonsoft.Json.JsonConvert.SerializeObject(stations);
                    context.Response.StatusCode = 200; // OK
                }
                else if (path == "/openstreetmap/geocode")
                {
                    string query = context.Request.QueryString["query"];
                    var geocodeResult = await OpenStreetAPIService.Geocode(query);
                    responseString = Newtonsoft.Json.JsonConvert.SerializeObject(geocodeResult);
                    context.Response.StatusCode = 200; // OK
                }
                else if (path == "/openstreetmap/reverse")
                {
                    string lat = context.Request.QueryString["lat"];
                    string lon = context.Request.QueryString["lon"];
                    var reverseResult = await OpenStreetAPIService.ReverseGeocode(
                        double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture),
                        double.Parse(lon, System.Globalization.CultureInfo.InvariantCulture)
                    );
                    responseString = Newtonsoft.Json.JsonConvert.SerializeObject(reverseResult);
                    context.Response.StatusCode = 200; // OK
                }
                else if (path == "/suggestions")
                {
                    string query = context.Request.QueryString["query"];

                    if (string.IsNullOrWhiteSpace(query))
                    {
                        responseString = "{\"error\": \"Missing required query parameter: query.\"}";
                        context.Response.StatusCode = 400; // Bad Request
                    }
                    else
                    {
                        try
                        {
                            // Call the suggestion service
                            var suggestions = await SuggestionService.GetSuggestions(query);

                            // Serialize the result as JSON
                            responseString = Newtonsoft.Json.JsonConvert.SerializeObject(suggestions);
                            context.Response.StatusCode = 200; // OK
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in /suggestions endpoint: {ex.Message}");
                            responseString = "{\"error\": \"An unexpected error occurred while fetching suggestions.\"}";
                            context.Response.StatusCode = 500; // Internal Server Error
                        }
                    }

                    // Write the response
                    context.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.Write(responseString);
                        writer.Flush();
                    }
                }

                else if (path == "/itinerary")
                {
                    string originLat = context.Request.QueryString["originLat"];
                    string originLon = context.Request.QueryString["originLon"];
                    string destinationLat = context.Request.QueryString["destinationLat"];
                    string destinationLon = context.Request.QueryString["destinationLon"];
                    string mode = context.Request.QueryString["mode"]; // New parameter

                    Console.WriteLine($"Received parameters: originLat={originLat}, originLon={originLon}, destinationLat={destinationLat}, destinationLon={destinationLon}, mode={mode}");

                    if (string.IsNullOrWhiteSpace(originLat) || string.IsNullOrWhiteSpace(originLon) ||
                        string.IsNullOrWhiteSpace(destinationLat) || string.IsNullOrWhiteSpace(destinationLon) || string.IsNullOrWhiteSpace(mode))
                    {
                        responseString = "{\"error\": \"Missing required query parameters.\"}";
                        context.Response.StatusCode = 400; // Bad Request
                    }
                    else
                    {
                        try
                        {
                            // Parse query parameters
                            double parsedOriginLat = double.Parse(originLat, System.Globalization.CultureInfo.InvariantCulture);
                            double parsedOriginLon = double.Parse(originLon, System.Globalization.CultureInfo.InvariantCulture);
                            double parsedDestinationLat = double.Parse(destinationLat, System.Globalization.CultureInfo.InvariantCulture);
                            double parsedDestinationLon = double.Parse(destinationLon, System.Globalization.CultureInfo.InvariantCulture);
                            bool useBike = mode == "cycling"; // Determine the mode

                            // Call the ItineraryService
                            responseString = await ItineraryService.GetItinerary(
                                parsedOriginLat, parsedOriginLon, parsedDestinationLat, parsedDestinationLon, useBike);

                            //VIDER LA LISTE : après chaque requête vider la queue (si contient déjà instructions de l'ancien appel)
                            instructionsQueue.Clear();

                            /*--- EXTRACTION des instructions SEUL ---*/

                            // Désérialiser la réponse JSON en JObject
                            JObject responseObject = JObject.Parse(responseString);

                            // Vérifier si l'élément "Itinerary" existe dans la réponse
                            if (responseObject["Itinerary"] != null)
                            {
                                // Accéder à l'élément "Itinerary"
                                var itineraryValues = responseObject["Itinerary"];

                                // Utilisation de .Children() pour itérer sur les propriétés de l'objet "Itinerary"
                                foreach (var property in itineraryValues.Children<JProperty>())
                                {
                                    // Chaque "property" contient une clé et une valeur
                                    string propertyName = property.Name;
                                    string itineraryJson = property.Value.ToString();

                                    // Vérifier que la chaîne JSON est valide avant de l'envoyer à ExtractInstructions
                                    if (!string.IsNullOrEmpty(itineraryJson))
                                    {
                                        // Appeler la méthode ExtractInstructions pour traiter cette chaîne JSON + ajout dans la liste instructionsQueue
                                        ExtractInstructions(itineraryJson);
                                    }
                                }

                                // Publier les instructions sur la queue
                                PublishInstructions(instructionsQueue);

                            }
                            else
                            {
                                Console.WriteLine("Itinerary information is missing in the response.");
                            }

                            context.Response.StatusCode = 200; // OK
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine($"Parsing error: {ex.Message}");
                            responseString = "{\"error\": \"Query parameters must be valid numbers.\"}";
                            context.Response.StatusCode = 400; // Bad Request
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error in itinerary generation: {ex.Message}");
                            responseString = "{\"error\": \"An unexpected error occurred while processing the request.\"}";
                            context.Response.StatusCode = 500; // Internal Server Error
                        }
                    }
                }

                else
                {
                    responseString = "{\"error\": \"Invalid endpoint.\"}";
                    context.Response.StatusCode = 404; // Not Found
                }

                // Write response
                context.Response.ContentType = "application/json";
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(responseString);
                    writer.Flush(); // Ensure all data is sent before closing
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error in HandleRequest: {ex.Message}");
                try
                {
                    context.Response.StatusCode = 500; // Internal Server Error
                    context.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.Write("{\"error\": \"An unexpected error occurred.\"}");
                    }
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Failed to send error response: {innerEx.Message}");
                }
            }
            finally
            {
                // Ensure the response is always closed
                try
                {
                    context.Response.Close();
                }
                catch (Exception closeEx)
                {
                    Console.WriteLine($"Error while closing response: {closeEx.Message}");
                }
            }
        }

        public static void ExtractInstructions(string itineraryJson)
        {
            // Désérialiser la chaîne JSON en un objet JObject
            JObject itineraryObject = JObject.Parse(itineraryJson);

            // Parcourir les étapes (steps) dans les segments
            var steps = itineraryObject["routes"][0]["segments"][0]["steps"];

            foreach (var step in steps)
            {
                string instruction = step["instruction"].ToString();
                instructionsQueue.Add(instruction);

            }
        }
        public static void PublishInstructions(List<string> instructions)
        {
            try
            {
                // Créer un producteur
                using (IMessageProducer producer = session.CreateProducer(destination))
                {
                    producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                    // Supprimer tous les messages existants dans la queue
                    ClearQueue();

                    // Publier chaque instruction comme un message séparé
                    foreach (var instruction in instructions)
                    {
                        ITextMessage message = session.CreateTextMessage(instruction);
                        producer.Send(message);
                        Console.WriteLine($"Instruction publiée : {instruction}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la publication des instructions : {ex.Message}");
            }
        }

        private static void ClearQueue()
        {
            try
            {
                using (IMessageConsumer consumer = session.CreateConsumer(destination))
                {
                    Console.WriteLine("Suppression des messages existants dans la queue...");
                    IMessage message;
                    // Lire et consommer tous les messages existants dans la queue
                    while ((message = consumer.Receive(TimeSpan.FromMilliseconds(500))) != null)
                    {
                        Console.WriteLine("Message supprimé : " + (message as ITextMessage)?.Text);
                    }
                    Console.WriteLine("Tous les messages existants ont été supprimés.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression des messages dans la queue : {ex.Message}");
            }
        }

    }
}
