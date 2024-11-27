using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Device.Location;
using System.Text.Json.Serialization;

namespace RoutingServer
{
    class Program
    {
        // Clés API (assurez-vous de les remplacer par vos propres clés)
        private static readonly string jcdecauxApiKey = "VOTRE_CLE_API_JCDECAUX";
        private static readonly string openRouteServiceApiKey = "VOTRE_CLE_API_OPENROUTESERVICE";

        static async Task Main(string[] args)
        {
            // URL du broker ActiveMQ
            string brokerUri = "activemq:tcp://localhost:61616"; // Assurez-vous que le broker est accessible

            // Créer une factory de connexion
            IConnectionFactory factory = new ConnectionFactory(brokerUri);

            // Créer une connexion
            using (IConnection connection = factory.CreateConnection())
            {
                connection.Start();

                // Créer une session
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    // File pour recevoir les demandes du client
                    IDestination requestQueue = session.GetQueue("itinerary.requests");

                    // File pour envoyer les réponses au client
                    IDestination responseQueue = session.GetQueue("itinerary.responses");

                    // Création du consommateur pour les demandes
                    using (IMessageConsumer consumer = session.CreateConsumer(requestQueue))
                    {
                        // Création du producteur pour les réponses
                        using (IMessageProducer producer = session.CreateProducer(responseQueue))
                        {
                            Console.WriteLine("Serveur prêt à recevoir des demandes d'itinéraire.");

                            while (true)
                            {
                                // Recevoir un message de demande
                                ITextMessage requestMessage = consumer.Receive() as ITextMessage;

                                if (requestMessage != null)
                                {
                                    Console.WriteLine("Demande reçue du client.");

                                    // Traiter la demande
                                    string requestText = requestMessage.Text;

                                    // La demande doit contenir l'origine et la destination
                                    dynamic requestData = JsonConvert.DeserializeObject(requestText);
                                    string origin = requestData.origin;
                                    string destination = requestData.destination;

                                    // Appeler la fonction pour calculer l'itinéraire
                                    string itineraryResponse = await ProcessItineraryRequest(origin, destination);

                                    // Envoyer la réponse au client
                                    ITextMessage responseMessage = session.CreateTextMessage(itineraryResponse);
                                    producer.Send(responseMessage);

                                    Console.WriteLine("Réponse envoyée au client.");
                                }
                            }
                        }
                    }
                }
            }
        }

        // Implémentation de la méthode ProcessItineraryRequest (à compléter)
        static async Task<string> ProcessItineraryRequest(string origin, string destination)
        {
            // Nous allons implémenter cette méthode à l'étape suivante
            return "";
        }
    }
}
