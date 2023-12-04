using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    class JCDecauxItem
    {
        public static readonly HttpClient client = new HttpClient();
        public string JCD_API_URL = "https://api.jcdecaux.com/vls/v3/";
        public string API_KEY = "e5a0ace90b1cc2c0e18643770d95469a446dc8c8";
        public static string allStationsOfAContract;
        public string contractSelected;

        public JCDecauxItem(string contractSelected)
        {
            this.contractSelected = contractSelected;
            allStationsOfAContract = communicate(JCD_API_URL + "stations?contract=" + contractSelected + "&apiKey=" + API_KEY).Result;

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

        public string getStations()
        {
            return allStationsOfAContract;
        }

        public string getContract()
        {
            return contractSelected;
        }
    }
}
