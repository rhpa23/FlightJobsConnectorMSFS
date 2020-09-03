using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorClientAPI
{
    public class DataModel
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double PayloadPounds { get; set; }
        public double FuelWeightPounds { get; set; }
        public double PayloadKilograms { get; set; }
        public double FuelWeightKilograms { get; set; }

    }
}
