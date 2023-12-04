using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
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
}
