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
using System.Security.Policy;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics.Contracts;
using System.Collections;

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

                Console.WriteLine(formattedP1Lo + "/" + formattedP1La + ":" + formattedP2Lo + "/" + formattedP2La);

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

        public static async Task<Results> getGeometry(String param,List<Contract> contracts)
        {
            HttpResponseMessage responseOrigine = await client.GetAsync(($"https://api.opencagedata.com/geocode/v1/json?q=" + param + "&key=d699e83b5f0e4357a51f1c7f676243d5&pretty=1"));
            responseOrigine.EnsureSuccessStatusCode();
            var originJson = responseOrigine.Content.ReadAsStringAsync().Result;
            var root = JsonConvert.DeserializeObject<OrigineResult>(originJson);
                     
            foreach (var result in root.results)
            {
                string country=result.components.country_code.ToUpper();
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

    


      
        public static async Task<GeoJsonResponse> getGeojson (List<Station> stations, Position p1)
        {
            double distance = 100000000000000;
            double diswalking = 0;
            double diswalkingTmp = 100000000000000;
            double disRiding = 0;
            Station stat = null;
            Station secondStat = null;
            String mode = "cycling-road";
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
            var baseAddress = new Uri("https://api.openrouteservice.org/v2/directions/"+mode+"?api_key=5b3ce3597851110001cf6248fd179c7a7660432bac775e2788a5729a&start=" + formattedS1Lo + "," + formatteds1La + "&end=" + formatteds2Lo + "," + formatteds2La);
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

        public static async Task<Contract> getBestContract(Results result, List<Contract> contracts)
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
                        HttpResponseMessage responseOrigine = await client.GetAsync(($"https://api.opencagedata.com/geocode/v1/json?q=" + contract.name + "&key=d699e83b5f0e4357a51f1c7f676243d5&pretty=1"));
                        responseOrigine.EnsureSuccessStatusCode();
                        var originJson = responseOrigine.Content.ReadAsStringAsync().Result;
                        var root = JsonConvert.DeserializeObject<OrigineResult>(originJson);
                        Position p2 = new Position(root.results[0].geometry.lat, root.results[0].geometry.lng);
                        tempDistance = await getJsonOpenStreet(p1, p2, "cycling-regular");
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

        static async Task Main()
        {
            try
            {

                String reponse = await getJSON("https://api.jcdecaux.com/vls/v3/contracts?apiKey=468da863308d1676f7ad103e93c424c778269301");
                List<Contract> contracts = System.Text.Json.JsonSerializer.Deserialize<List<Contract>>(reponse);


                Console.Write("Entrez une adresse d'origine: ");
                string origine = Console.ReadLine();
                Results originGeo = await getGeometry(origine, contracts);
                while (originGeo == null)
                {
                    Console.Write("Entrez une nouvelle adresse d'origine: ");
                    origine = Console.ReadLine();
                    originGeo = await getGeometry(origine, contracts);
                }


                Console.Write("Entrez une adresse d'arrivé: ");
                string arrival = Console.ReadLine();
                Results arrivalGeo = await getGeometry(arrival, contracts);
                while (arrivalGeo == null)
                {
                    Console.Write("Entrez une nouvelle adresse d'arrivée: ");
                    origine = Console.ReadLine();
                    arrivalGeo = await getGeometry(origine, contracts);
                }
                Contract nearOrigin = await getBestContract(originGeo, contracts);
                Console.WriteLine(nearOrigin);
                Contract nearArrival = await getBestContract(arrivalGeo, contracts);
                Console.WriteLine(nearArrival);


                List<Station> stations = new List<Station>();
                String res = await getJSON("https://api.jcdecaux.com/vls/v3/stations?contract=" + nearOrigin.name + "&apiKey=468da863308d1676f7ad103e93c424c778269301");
                List<Station> tmp = System.Text.Json.JsonSerializer.Deserialize<List<Station>>(res);
                stations.AddRange(tmp);
                res = await getJSON("https://api.jcdecaux.com/vls/v3/stations?contract=" + nearOrigin.name + "&apiKey=468da863308d1676f7ad103e93c424c778269301");
                tmp = System.Text.Json.JsonSerializer.Deserialize<List<Station>>(res);
                stations.AddRange(tmp);


                Console.WriteLine(stations.Count);




                Console.WriteLine("Depart : " + originGeo.geometry);
                Console.WriteLine("Arrivé : " + arrivalGeo.geometry);
                Console.ReadKey();
                GeoJsonResponse OriginToS1 = await getGeojson(stations, new Position(originGeo.geometry.lat, originGeo.geometry.lng));
                GeoJsonResponse S2ToArrival = await getGeojson(stations, new Position(arrivalGeo.geometry.lat, arrivalGeo.geometry.lng));

                foreach (var f in OriginToS1.Features)
                {
                    foreach (var s in f.Properties.Segments)
                    {
                        foreach (var st in s.Steps)
                        {
                            Console.WriteLine(st.Instruction);
                        }
                    }

                    Console.WriteLine(S2ToArrival);

                    Console.ReadLine();





                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                Console.ReadLine();
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
        public int number { get; set; } = -1;
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

        public Position()
        {
        }

        public Position(double lat, double lng)
        {
            try
            {

                latitude = lat;
                longitude = lng;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Erreur de conversion en double : {ex.Message}");
            }

        }

        public override string ToString()
        {
            String result ="";
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

public class geometry
{
    public double lat { get; set; }
    public double lng { get; set; }

    public override string ToString()
    {
        String result = "";
        result += lat;
        result += ",";
        result += lng;
        return result;
    }

}

public class Results
{
    public geometry geometry { get; set; }

    public Components components { get; set; }


    public override string ToString()
    {
        string result = components.country_code + " "+ geometry.ToString();
        return result;
    }


}

public class Components
{
    public string country_code { get; set; }
    public string country { get; set; }
}



public class OrigineResult
{
    public List<Results> results { get; set; }
}

public class GeoJsonResponse
{
    public Metadata Metadata { get; set; }
    public List<Feature> Features { get; set; }


}

public class Metadata
{
    public Query Query { get; set; }

}
public class Query
{
    public List<List<double>> Coordinates { get; set; }
}


public class Feature
{
    public Properties Properties { get; set; }
}

public class Properties
{
    public List<Segment> Segments { get; set; }
}

public class Segment
{
    public double Distance { get; set; }

    public double Duration { get; set; }

    public List<Step> Steps { get; set; }
}

public class Step
{
    public double Distance { get; set; }

    public double Duration { get; set; }

    public string Instruction { get; set; }
    public List<int> WayPoints { get; set; }
}



