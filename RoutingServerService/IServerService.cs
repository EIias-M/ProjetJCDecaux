using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServerService
{

    [ServiceContract]
    internal interface IServerService
    {

        [OperationContract]
        (string id, List<string> itineraire, List<List<List<double>>> points) findWay(String addressStart, String addressEnd);
    }
}
