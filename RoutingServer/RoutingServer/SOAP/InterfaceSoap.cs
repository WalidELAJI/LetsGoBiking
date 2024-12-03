using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServer.SOAP
{
    [ServiceContract]
    internal interface InterfaceSoap
    {
        [OperationContract]
        string getGeneratedItinerary(string originLat, string originLon, string destinationLat, string destinationLon, string mode);

    }
}
