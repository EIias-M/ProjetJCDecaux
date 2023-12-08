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
        private string API_KEY_OPEN_ROUTE = "5b3ce3597851110001cf624873d2c715e01e4c0fbfb738e58f9ddcf6";
        private string API_KEY_OPEN_CAGE = "d699e83b5f0e4357a51f1c7f676243d5";

        public async Task<String> getJSON(String url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task<double> getJsonOpenStreet(Position p1, Position p2, String type)
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

        public async Task<Results> getGeometry(String param, List<Contract> contracts)
        {
            HttpResponseMessage responseOrigine = await client.GetAsync(($"https://api.opencagedata.com/geocode/v1/json?q=" + param + "&key=" + API_KEY_OPEN_CAGE + "&pretty=1"));
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

        public async Task<GeoJsonResponse> geoJsonRequest(Position p1, Position p2, string mode)
        {
            string formatteds1Lo = Position.format(p1.longitude);
            string formatteds1La = Position.format(p1.latitude);
            string formatteds2Lo = Position.format(p2.longitude);
            string formatteds2La = Position.format(p2.latitude);
            Uri baseAddress;
            baseAddress = new Uri("https://api.openrouteservice.org/v2/directions/" + mode + "?api_key=" + API_KEY_OPEN_ROUTE + "&start=" + formatteds1Lo + "," + formatteds1La + "&end=" + formatteds2Lo + "," + formatteds2La);
            Console.WriteLine(baseAddress);
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json, application/geo+json, application/gpx+xml, img/png; charset=utf-8");
                using (var response = await httpClient.GetAsync(baseAddress))
                {

                    string responseData = await response.Content.ReadAsStringAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<GeoJsonResponse>(responseData);

                }
            }
        }

        public async Task<GeoJsonResponse> getGeojson(List<Station> stations, Position p1, Boolean originOrNot)
        {
            List<Station> nearStations = new List<Station>();


            foreach (var station in stations)
            {
                if (!p1.Equals(station.position) && station.nbBikes() != 0)
                {
                    if (nearStations.Count < 5)
                    {
                        nearStations.Add(station);
                    }
                    else
                    {
                        double disTMP = p1.getDistance(station.position);
                        nearStations.Sort((s1, s2) => p1.getDistance(s1.position).CompareTo(p1.getDistance(s2.position)));
                        if (disTMP < p1.getDistance(nearStations[4].position))
                        {
                            nearStations[4] = station;
                        }
                    }
                }
            }

            Station stat = null;
            double disWalking;
            double distance = double.MaxValue;

            foreach (var statTMP in nearStations)
            {
                disWalking = await getJsonOpenStreet(p1, statTMP.position, "foot-walking");
                if (disWalking < distance)
                {
                    distance = disWalking;
                    stat = statTMP;
                }
            }

            Console.WriteLine(stat);
            if (originOrNot)
            {
                return await geoJsonRequest(p1, stat.position, "foot-walking");
            }
            return await geoJsonRequest(stat.position, p1, "foot-walking");


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
                    String res = await getJSON("https://api.jcdecaux.com/vls/v3/stations?contract=" + contract.name + "&apiKey=468da863308d1676f7ad103e93c424c778269301");
                    List<Station> tmp = System.Text.Json.JsonSerializer.Deserialize<List<Station>>(res);

                    if (tmp.Count != 0)
                    {
                        HttpResponseMessage responseOrigine = await client.GetAsync(($"https://api.opencagedata.com/geocode/v1/json?q=" + contract.name + "&key=" + API_KEY_OPEN_CAGE + "&pretty=1"));
                        responseOrigine.EnsureSuccessStatusCode();
                        var originJson = responseOrigine.Content.ReadAsStringAsync().Result;
                        var root = JsonConvert.DeserializeObject<OrigineResult>(originJson);
                        Position p2 = new Position(root.results[0].geometry.lat, root.results[0].geometry.lng);
                        tempDistance = await getJsonOpenStreet(p1, p2, "foot-walking");
                        if (distance > tempDistance)
                        {
                            distance = tempDistance;
                            contratResult = contract;
                        }
                    }
                }
            }
            return contratResult;
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

        public string getItinairare(GeoJsonResponse geoJson)
        {
           
            var itineraireFinal = "";

            foreach (var f in geoJson.features)
            {
                foreach (var s in f.properties.segments)
                {
                    foreach (var st in s.steps)
                    {
                        itineraireFinal += st.instruction + ", Durée : " + st.duration + ", Distance : " + st.distance + "\n";

                    }
                }

            }

            return itineraireFinal + "\n";

        }

        public List<String> findWay(string addressStart, string addressEnd)
        {
            List<Contract> contracts =  getAllContractAsync().Result;
            Results originGeo =  getGeometry(addressStart, contracts).Result;
            Results arrivalGeo =  getGeometry(addressEnd, contracts).Result;

            Console.WriteLine("Depart : " + originGeo.geometry);
            Console.WriteLine("Arrivé : " + arrivalGeo.geometry);

            Contract nearOrigin =  getBestContract(originGeo, contracts).Result;
            Contract nearArrival = getBestContract(arrivalGeo, contracts).Result;

            List<Station> stationsOrigin = getStationFromAContractAsync(nearOrigin.name).Result;
            List<Station> stationsArrival = getStationFromAContractAsync(nearArrival.name).Result;

            GeoJsonResponse OriginToS1 = getGeojson(stationsOrigin, new Position(originGeo.geometry.lat, originGeo.geometry.lng), true).Result;
            GeoJsonResponse S2ToArrival = getGeojson(stationsArrival, new Position(arrivalGeo.geometry.lat, arrivalGeo.geometry.lng), false).Result;

            Position station1 = new Position(OriginToS1.bbox[1], OriginToS1.bbox[2]);
            Position station2 = new Position(S2ToArrival.bbox[3], S2ToArrival.bbox[0]);

            GeoJsonResponse S1toS2 = geoJsonRequest(station1, station2, "cycling-road").Result;

            double durationTotal = OriginToS1.GetDuration() + S1toS2.GetDuration() + S2ToArrival.GetDuration();

            Position pOriginGeo = new Position(originGeo.geometry.lat, originGeo.geometry.lng);
            Position pArrivalGeo = new Position(arrivalGeo.geometry.lat, arrivalGeo.geometry.lng);

            GeoJsonResponse walkingOrignToArrival = geoJsonRequest(pOriginGeo, pArrivalGeo, "foot-walking").Result;

            List<string> itineraire = new List<string>();


            if (durationTotal > walkingOrignToArrival.GetDistance())
            {
                itineraire.Add("Le trajet est plus simple à pieds \n Intructions : \n");
                itineraire.Add(getItinairare(walkingOrignToArrival));
            }
            else
            {
                itineraire.Add("Le trajet est plus simple en vélo \n\n Intructions : \n\n");
                itineraire.Add("- Marcher jusqu'a la station pour récuperer le vélo : \n\n");
                itineraire.Add(getItinairare(OriginToS1));
                itineraire.Add("- Pedaler jusqu'a la station pour déposer le vélo : \n\n");
                itineraire.Add(getItinairare(S1toS2));
                itineraire.Add("- Marcher jusqu'à la destination : \n\n");
                itineraire.Add(getItinairare(S2ToArrival));
            }

            return itineraire;
        }
    }
}
