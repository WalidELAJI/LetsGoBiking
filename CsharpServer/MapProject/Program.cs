using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CsharpServer.Itinerary;
using CsharpServer.JCDecaux;
using CsharpServer.OpenAPIServices;
using CsharpServer.Suggestions;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;


namespace CsharpServer
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
                else if (path == "/suggestions")
                {
                    string query = context.Request.QueryString["query"];

                    if (string.IsNullOrWhiteSpace(query))
                    {
                        response = "{\"error\": \"Missing required query parameter: query.\"}";
                        context.Response.StatusCode = 400; // Bad Request
                    }
                    else
                    {
                        try
                        {
                            // Call the suggestion service
                            var suggestions = await SuggestionService.GetSuggestions(query);

                            // Serialize the result as JSON
                            response = Newtonsoft.Json.JsonConvert.SerializeObject(suggestions);
                            context.Response.StatusCode = 200; // OK
                        }
                        catch (Exception ex)
                        {
                            response = "{\"error\": \"An unexpected error occurred while fetching suggestions.\"}";
                            context.Response.StatusCode = 500; // Internal Server Error
                        }
                    }

                    // Write the response
                    context.Response.ContentType = "application/json";
                    using (var streamwriter = new StreamWriter(context.Response.OutputStream))
                    {
                        streamwriter.Write(response);
                        streamwriter.Flush();
                    }
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

                            //VIDER LA LISTE : après chaque requête vider la queue (si contient déjà instructions de l'ancien appel)
                            instructionsQueue.Clear();

                            // Désérialiser la réponse JSON en JObject
                            JObject responseObject = JObject.Parse(response);

                            // Vérifier si l'élément "Itinerary" existe dans la réponse (2 format de réponses possibles en fct du mode)
                            if (responseObject["Itinerary"] != null)
                            {
                                var itinerary = responseObject["Itinerary"];

                                // Cas 1 : pour mode "cycling" 
                                if (itinerary.Type == JTokenType.Object &&
                                    itinerary.Children<JProperty>().Any(p => p.Name == "OriginToStation" || p.Name == "StationToStation" || p.Name == "StationToDestination"))
                                {
                                    
                                    foreach (var property in itinerary.Children<JProperty>()) 
                                    {
                                        // Chaque "property" contient une clé et une valeur
                                        string propertyName = property.Name;
                                        string itineraryJson = property.Value.ToString();

                                        // Vérifier que la chaîne JSON est valide avant de l'envoyer à ExtractInstructions
                                        if (!string.IsNullOrEmpty(itineraryJson))
                                        {
                                            // Extrait que les instructions pour chaque segment
                                            ExtractInstructions(itineraryJson);
                                        }
                                    }
                                }
                                // Cas 2 : pour mode "walking"
                                else if (itinerary.Type == JTokenType.String)
                                {
                                    string itineraryJson = itinerary.ToString();

                                    if (!string.IsNullOrEmpty(itineraryJson))
                                    {
                                        ExtractInstructions(itineraryJson);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Format inattendu pour l'objet Itinerary.");
                                }

                                // Publier les instructions sur la queue
                                PublishInstructions(instructionsQueue);
                            }
                            else
                            {
                                Console.WriteLine("L'élément 'Itinerary' est absent de la réponse.");
                            }




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
