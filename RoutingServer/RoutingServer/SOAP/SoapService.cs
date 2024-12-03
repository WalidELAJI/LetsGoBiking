using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServer.SOAP
{
    internal class SoapService : InterfaceSoap
    {
        private readonly string routingServerBaseUrl = "http://localhost:5000/";

        public string getGeneratedItinerary(string originLat, string originLon, string destinationLat, string destinationLon, string mode)
        {
            try
            {
                string endpoint = $"{routingServerBaseUrl}/itinerary";
                string queryString = $"originLat={originLat}&originLon={originLon}&destinationLat={destinationLat}&destinationLon={destinationLon}&mode={mode}";
                string requestUrl = $"{endpoint}?{queryString}";

                // Make an HTTP GET request to the RoutingServer
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd(); // Return the JSON response as a string
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching itinerary from RoutingServer: {ex.Message}");
                return "error";
            }
        }


    }
}
