using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServerService
{
    public class GeoJsonResponse
    {
        public Metadata metadata { get; set; }
        public List<Feature> features { get; set; }
        public List<double> bbox { get; set; }

        public double GetDistance()
        {
            return features[0].properties.segments[0].distance;
        }

        public double GetDuration()
        {
            return features[0].properties.segments[0].duration;
        }

    }

    public class Metadata
    {
        public Query query { get; set; }

    }
    public class Query
    {
        public List<List<double>> coordinates { get; set; }
    }


    public class Feature
    {
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public List<Segment> segments { get; set; }
    }

    public class Segment
    {
        public double distance { get; set; }

        public double duration { get; set; }

        public List<Step> steps { get; set; }


    }

    public class Step
    {
        public double distance { get; set; }

        public double duration { get; set; }

        public string instruction { get; set; }
        public List<int> wayPoints { get; set; }
    }
}
