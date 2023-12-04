using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServerService
{
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
            result += $"Capacité Max : " + totalStands.capacity + "\n";
            result += $"Nombre de vélo disponible : " + totalStands.availabilities.bikes + "\n";
            result += $"Places libres :" + nbBikes() + "\n";
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

            return totalStands.capacity - totalStands.availabilities.bikes;
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
}
