using System;
using System.Device.Location;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ProjetLab
{
    internal class Program
    {
        static readonly HttpClient client = new HttpClient();

        public static async Task<String> getJSON(String url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }


        public static async Task<double> getJsonOpenStreet(Position p1, Position p2,String type)
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
        static async Task Main()
        {
            try
            {
                String reponse = await getJSON("https://api.jcdecaux.com/vls/v3/contracts?apiKey=468da863308d1676f7ad103e93c424c778269301");
                List<Contract> contracts = System.Text.Json.JsonSerializer.Deserialize<List<Contract>>(reponse);
                foreach (var contract in contracts)
                {
                    Console.WriteLine(contract);
                }


                Console.WriteLine("Choose one contract");
                String result = Console.ReadLine();
                reponse = await getJSON("https://api.jcdecaux.com/vls/v3/stations?contract=" + result + "&apiKey=468da863308d1676f7ad103e93c424c778269301");
                List<Station> stations = System.Text.Json.JsonSerializer.Deserialize<List<Station>>(reponse);
                foreach (var station in stations)
                {
                    Console.WriteLine(station);
                }

                Console.WriteLine("Choose one station");
                String nbstat = Console.ReadLine();
                reponse = await getJSON("https://api.jcdecaux.com/vls/v3/stations/" + nbstat + "?contract=" + result + "&apiKey=468da863308d1676f7ad103e93c424c778269301");
                Station s1 = System.Text.Json.JsonSerializer.Deserialize<Station>(reponse);
                double distance = s1.position.getDistance(stations[0].position);
                Station stat = null;
                double diswalking, disRiding=0;

                foreach (var station in stations)
                {
                    double test = s1.position.getDistance(station.position);
                    if (!s1.Equals(station)&& test < distance && station.nbBikes()!=0)
                    { 
                         diswalking= await getJsonOpenStreet(s1.position, station.position, "foot-walking");
                         disRiding = await getJsonOpenStreet(s1.position, station.position, "cycling-regular");
                   
                        if (diswalking < disRiding)
                        {
                            distance = test;
                            stat = station;
                        }

                      
                    }

                }
                Console.WriteLine(distance);
                Console.WriteLine(stat);
                Console.WriteLine(disRiding);
                string formattedS1Lo = Position.format(s1.position.longitude);
                string formatteds1La = Position.format(s1.position.latitude);
                string formatteds2Lo = Position.format(stat.position.longitude);
                string formatteds2La = Position.format(stat.position.latitude);
                var baseAddress = new Uri("https://api.openrouteservice.org/v2/directions/driving-car?api_key=5b3ce3597851110001cf6248fd179c7a7660432bac775e2788a5729a&start=" + formattedS1Lo+","+formatteds1La+"&end="+formatteds2Lo+","+formatteds2La);
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {

                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json, application/geo+json, application/gpx+xml, img/png; charset=utf-8");
                    using ( var response = await httpClient.GetAsync(baseAddress))
                    {
                        
                        string responseData = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject(responseData);
                        Console.WriteLine(data);
                        Console.ReadLine();

                    }
                }


            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
    }

    public class OpenRouteServiceResponse
    {
        public List<List<double>> distances { get; set; }
        public List<Dictionary<string, object>> destinations { get; set; }
        public List<Dictionary<string, object>> sources { get; set; }
    }

    public class Contract
    {
        public string name { get; set; }
        public string commercial_name { get; set; }
        public string country_code { get; set; }
        public List<string> cities { get; set; }


        public override string ToString()
        {
            String result = "";
            result += $"Name:" + name + "\n";
            result += $"Commercial Name: " + commercial_name + "\n";
            result += $"Country Code: " + country_code + "\n";
            result += "Cities:";
            if (cities == null)
            {
                result += "[]";
                result += "\n";
            }
            else
            {
                foreach (var city in cities)
                {
                    result += city;
                    result += "\n";
                }
            }
            return result;
        }
    }

    public class Availabilities
    {
        public int bikes { get; set; }
        public int stands { get; set; }
        public int mechanicalBikes { get; set; }
        public int electricalBikes { get; set; }
    }


    public class TotalStands
    {
        public Availabilities availabilities { get; set; }

        public int capacity { get; set; }

    }

    public class Station
    {
        public int number { get; set; }
        public string contractName { get; set; }

        public string name { get; set; }
        public Position position { get; set; }

        public TotalStands totalStands { get; set; }


        public override string ToString()
        {
            String result = "";
            result += $"number:" + number + "\n";
            result += $"contractName: " + contractName + "\n";
            result += $"name: " + name + "\n";
            result += $"position: " + position + "\n";
            result += $"Capacité Max : " + totalStands.capacity+"\n";
            result += $"Nombre de vélo disponible : " + totalStands.availabilities.bikes + "\n";
            result += $"Places libres :"+nbBikes() + "\n";
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            Station station = (Station)obj;
            return station.number == number;
        }

        public int nbBikes()
        {
            
            return totalStands.capacity-totalStands.availabilities.bikes;
        }
    }

    public class Position
    {
        public double latitude { get; set; }
        public double longitude { get; set; }

        public override string ToString()
        {
            String result = "";
            result += $"latitude:" + latitude + "\n";
            result += $"longitude: " + longitude + "\n";
            return result;
        }


        public double getDistance(Position other)
        {
            GeoCoordinate g1 = new GeoCoordinate(latitude, longitude);
            GeoCoordinate g2 = new GeoCoordinate(other.latitude, other.longitude);
            return g1.GetDistanceTo(g2);
        }

        public static string format(double pos)
        {
            return pos.ToString().Replace(',', '.');
        }
    }
}

