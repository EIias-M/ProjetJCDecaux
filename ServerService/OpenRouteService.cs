using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerService
{
    public class OpenRouteServiceResponse
    {
        public List<List<double>> distances { get; set; }
        public List<Dictionary<string, object>> destinations { get; set; }
        public List<Dictionary<string, object>> sources { get; set; }
    }
}
