using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServerService
{
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
}
