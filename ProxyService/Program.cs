using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Uri httpUrl = new Uri("http://localhost:8733/Projet_Biking/Proxy/");

            ServiceHost host = new ServiceHost(typeof(ProxyService), httpUrl);

            
            host.AddServiceEndpoint(typeof(IProxyService), new BasicHttpBinding(), "");

            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            host.Description.Behaviors.Add(smb);

            host.Open();

            Console.WriteLine("Service is host at " + DateTime.Now.ToString());
            Console.WriteLine("Service url at " + httpUrl);
            Console.WriteLine("Host is running... Press <Enter> key to stop");
            Console.ReadLine();

        }
    }
}
