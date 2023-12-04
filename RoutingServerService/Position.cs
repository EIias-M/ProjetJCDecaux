using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;

namespace RoutingServerService
{
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
