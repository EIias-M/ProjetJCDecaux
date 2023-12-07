using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
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
using Apache.NMS;
using Apache.NMS.ActiveMQ;


namespace RoutingServerService
{
    internal class ServerService : IServerService
    {
        static readonly HttpClient client = new HttpClient();
        ProxyServiceClient proxy = new ProxyServiceClient();
        private string API_KEY_OPEN_ROUTE = "5b3ce3597851110001cf6248ef7bd53f4e384bb1931823475d47e2ca";

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
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY_OPEN_ROUTE);
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
            var baseAddress = new Uri("https://api.openrouteservice.org/v2/directions/" + mode + "?api_key="+ API_KEY_OPEN_ROUTE + "&start=" + formattedS1Lo + "," + formatteds1La + "&end=" + formatteds2Lo + "," + formatteds2La);
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

        private (string id, List<string> itineraire, List<List<GeoCoordinate>> points) activeMQ(List<string> itineraire, List<List<GeoCoordinate>> points)
        {
            try
            {

                Uri connecturi = new Uri("activemq:tcp://localhost:61616");
                ConnectionFactory connectionFactory = new ConnectionFactory(connecturi);

                // Create a single Connection from the Connection Factory.
                IConnection connection = connectionFactory.CreateConnection();
                connection.Start();

                // Create a session from the Connection.
                Apache.NMS.ISession session = connection.CreateSession();

                //creer un nom de queue aleatoire
                Random random = new Random();


                int randomNumber = random.Next();
                IDestination destination = session.GetQueue("" + randomNumber);

                // Create a Producer targetting the selected queue.
                IMessageProducer producer = session.CreateProducer(destination);

                // You may configure everything to your needs, for instance:
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                // Finally, to send messages:
                itineraire.Add("Fin du trajet.");

                foreach (string s in itineraire)
                {
                    ITextMessage message = session.CreateTextMessage(s);
                    producer.Send(message);
                }


                Console.WriteLine("Message sent, check ActiveMQ web interface to confirm at queue : " + randomNumber);

                // Don't forget to close your session and connection when finished.
                session.Close();
                connection.Close();
                return ("" + randomNumber, itineraire, points);
            }
            catch { return ("", itineraire, points); }
            //retourne le nom de la queue au client
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

            var S2ToArrival = getGeojson(stations, new Position(arrivalGeo.geometry.lat, arrivalGeo.geometry.lng));

            GeoJsonResponse itineraire = JsonConvert.DeserializeObject<GeoJsonResponse>(OriginToS1.Result.ToString());

            var itineraireFinal = "Instructions : \n";

            foreach (var f in itineraire.Features)
            {
                foreach (var s in f.Properties.Segments)
                {
                    foreach (var st in s.Steps)
                    {
                        itineraireFinal += st.Instruction + "\n";
                    }
                }

            }

            return itineraireFinal;
        }
    }
}
