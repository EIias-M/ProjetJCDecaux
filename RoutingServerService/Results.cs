using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServerService
{
    public class Results
    {
        public Geometry geometry { get; set; }

        public Components components { get; set; }


        public override string ToString()
        {
            string result = components.country_code + " " + geometry.ToString();
            return result;
        }


    }

    public class OrigineResult
    {
        public List<Results> results { get; set; }
    }

    public class Components
    {
        public string country_code { get; set; }
        public string country { get; set; }
    }

    public class Geometry
    {
        public double lat { get; set; }
        public double lng { get; set; }

        public List<List<double>> coordinates { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            String result = "";
            result += lat;
            result += ",";
            result += lng;
            return result;
        }

    }
}
