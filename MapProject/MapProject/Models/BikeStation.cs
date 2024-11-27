using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapProject.Models
{
    public class BikeStation
    {
        public string Name { get; set; }
        public int AvailableBikes { get; set; }
        public int BikeStands { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
