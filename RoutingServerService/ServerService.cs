using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RoutingServerService.ServiceReference1;


namespace RoutingServerService
{
    internal class ServerService : IServerService
    {
        static readonly HttpClient client = new HttpClient();
        ProxyServiceClient proxy = new ProxyServiceClient();

        public async Task<string> getJSON(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task<double> getJsonOpenStreet(Position p1, Position p2, string type)
        {
            var baseAddress = new Uri("https://api.openrouteservice.org");
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "5b3ce3597851110001cf6248fd179c7a7660432bac775e2788a5729a");
                string formattedP1Lo = Position.format(p1.longitude);
                string formattedP1La = Position.format(p1.latitude);
                string formattedP2Lo = Position.format(p2.longitude);
                string formattedP2La = Position.format(p2.latitude);

                using (var content = new StringContent("{\"locations\":[[" + formattedP1Lo + "," + formattedP1La + "],[" + formattedP2Lo + "," + formattedP2La + "]],\"metrics\":[\"distance\"]}", Encoding.UTF8, "application/json"))
                {
                    using (var response = await httpClient.PostAsync("/v2/matrix/" + type, content))
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        double result = System.Text.Json.JsonSerializer.Deserialize<OpenRouteServiceResponse>(responseData).distances[0][1];
                        return result;
                    }
                }
            }
        }

        public async Task<Results> getGeometry(string param, List<Contract> contracts)
        {
            HttpResponseMessage responseOrigine = await client.GetAsync(($"https://api.opencagedata.com/geocode/v1/json?q=" + param + "&key=d699e83b5f0e4357a51f1c7f676243d5&pretty=1"));
            responseOrigine.EnsureSuccessStatusCode();
            var originJson = responseOrigine.Content.ReadAsStringAsync().Result;
            var root = JsonConvert.DeserializeObject<OrigineResult>(originJson);

            foreach (var result in root.results)
            {
                string country = result.components.country_code.ToUpper();
                foreach (var contract in contracts)
                {

                    if (country.Equals(contract.country_code))
                    {
                        return result;
                    }
                }
            }
            return null;

        }

        public async Task<Contract> getBestContract(Results result, List<Contract> contracts)
        {
            double distance = 100000000000000;
            double tempDistance = 0;
            Contract contratResult = null;
            string country = result.components.country_code.ToUpper();
            Position p1 = new Position(result.geometry.lat, result.geometry.lng);

            foreach (var contract in contracts)
            {
                if (contract.country_code == country)
                {
                    HttpResponseMessage responseOrigine = await client.GetAsync(($"https://api.opencagedata.com/geocode/v1/json?q=" + contract.name + "&key=d699e83b5f0e4357a51f1c7f676243d5&pretty=1"));
                    responseOrigine.EnsureSuccessStatusCode();
                    var originJson = responseOrigine.Content.ReadAsStringAsync().Result;
                    var root = JsonConvert.DeserializeObject<OrigineResult>(originJson);
                    Position p2 = new Position(root.results[0].geometry.lat, root.results[0].geometry.lng);

                    tempDistance = await getJsonOpenStreet(p1, p2, "cycling-regular");

                    if (distance > tempDistance)
                    {
                        Console.WriteLine(contract.name + ":" + tempDistance);
                        distance = tempDistance;
                        contratResult = contract;
                    }


                }

            }
            return contratResult;

        }

        public async Task<object> getGeojson(List<Station> stations, Position p1)
        {
            double distance = 100000000000000;
            double diswalking = 0;
            double diswalkingTmp = 100000000000000;
            double disRiding = 0;
            Station stat = null;
            Station secondStat = null;
            string mode = "cycling-road";


            foreach (var station in stations)
            {
                double test = p1.getDistance(station.position);
                if (!p1.Equals(station.position) && test < distance && station.nbBikes() != 0)
                {
                    diswalking = await getJsonOpenStreet(p1, station.position, "foot-walking");
                    disRiding = await getJsonOpenStreet(p1, station.position, "cycling-regular");
                    if (diswalking < disRiding)
                    {
                        distance = test;
                        stat = station;
                    }
                    if (diswalking < diswalkingTmp)
                    {
                        diswalkingTmp = diswalking;
                        secondStat = station;
                    }
                }
            }
            if (stat == null)
            {
                stat = secondStat;
                mode = "foot-walking";
                Console.WriteLine("Le trajet est plus rapide a pieds voici l'itinéraire à pieds");
            }


            string formattedS1Lo = Position.format(p1.longitude);
            string formatteds1La = Position.format(p1.latitude);
            string formatteds2Lo = Position.format(stat.position.longitude);
            string formatteds2La = Position.format(stat.position.latitude);
            var baseAddress = new Uri("https://api.openrouteservice.org/v2/directions/" + mode + "?api_key=5b3ce3597851110001cf6248fd179c7a7660432bac775e2788a5729a&start=" + formattedS1Lo + "," + formatteds1La + "&end=" + formatteds2Lo + "," + formatteds2La);
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json, application/geo+json, application/gpx+xml, img/png; charset=utf-8");
                using (var response = await httpClient.GetAsync(baseAddress))
                {

                    string responseData = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject(responseData);
                    return data;
                }
            }
        }

        public async Task<List<Contract>> getAllContractAsync()
        {
            string reponse = await proxy.getContractsAsync();
            List<Contract> contracts = System.Text.Json.JsonSerializer.Deserialize<List<Contract>>(reponse);
            return contracts;
        }

        public async Task<List<Station>> getStationFromAContractAsync(string  contract)
        {
            string reponse = await proxy.getAllStationsOfAContractAsync(contract);
            List<Station> stations = System.Text.Json.JsonSerializer.Deserialize<List<Station>>(reponse);
            return stations;
        }

        public string findWay(string addressStart, string addressEnd)
        {
            List<Contract> contracts =  getAllContractAsync().Result;
            Results originGeo =  getGeometry(addressStart, contracts).Result;
            Results arrivalGeo =  getGeometry(addressEnd, contracts).Result;

            Console.WriteLine("Depart : " + originGeo.geometry);
            Console.WriteLine("Arrivé : " + arrivalGeo.geometry);

            Contract nearOrigin = getBestContract(originGeo, contracts).Result;

            Console.WriteLine(nearOrigin);

            List<Station> stations = getStationFromAContractAsync(nearOrigin.name).Result;

            var OriginToS1 = getGeojson(stations, new Position(originGeo.geometry.lat, originGeo.geometry.lng));

            Console.WriteLine(OriginToS1.Result);

            return "" + OriginToS1.Result;
        }
    }
}
