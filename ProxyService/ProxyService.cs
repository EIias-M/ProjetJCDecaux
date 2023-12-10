using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    internal class ProxyService : IProxyService
    {

        public string JCD_API_URL_SATION = "https://api.jcdecaux.com/vls/v3/stations/";
        public string JCD_API_URL_CONTRACT = "https://api.jcdecaux.com/vls/v3/contracts";
        public string API_KEY = "e5a0ace90b1cc2c0e18643770d95469a446dc8c8";
        public static readonly HttpClient client = new HttpClient();
        GenericProxyCache<JCDecauxItem> cache = new GenericProxyCache<JCDecauxItem>();

        public string getResponse(string url)
        {
            Task<string> response = communicate(url);
            return response.Result;
        }

        public static async Task<string> communicate(string request)
        {
            try
            {
                string responseBody = await client.GetStringAsync(request);
                return responseBody;
            }
            catch (Exception e)
            {

            }
            return null;
        }

        public string getStation(int number, string chosenContract)
        {
            string responseStationProcheBody = getResponse(JCD_API_URL_SATION + number + "?contract=" + chosenContract + "&apiKey=" + API_KEY);
            return responseStationProcheBody;
        }

        public string getContracts()
        {
            string responseAllContracts = cache.Get("allContracts", 500).getAllContracts();
            return responseAllContracts;
        }

        public string getAllStationsOfAContract(string chosenContract)
        {
            string stations = cache.Get(chosenContract,500).getStations();
            return stations;
        }
    }
}
