using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    [ServiceContract]
    internal interface IProxyService
    {
        [OperationContract]
        string getStation(int number, string chosenContract);

        [OperationContract]
        string getContracts();

        [OperationContract]
        string getAllStationsOfAContract(string chosenContract);
    }
}
