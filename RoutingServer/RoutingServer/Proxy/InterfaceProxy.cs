using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using RoutingServer.JCDecaux;

namespace RoutingServer.Proxy
{
    [ServiceContract]
    internal interface InterfaceProxy
    {
        [OperationContract]
        Task<string> getCalculatedItinerary(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude, bool Bike);

        [OperationContract]
        Task<List<Station>> GetStationsJcdecaux(string city);

        [OperationContract]
        Task<List<dynamic>> getAPIGeocode(string query);
        [OperationContract]
        Task<string> getAPIReverseGeocode(double latitude, double longitude);

        [OperationContract]
        Task<string> getGeneratedItinerary(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude, bool Bike);

    }
}
