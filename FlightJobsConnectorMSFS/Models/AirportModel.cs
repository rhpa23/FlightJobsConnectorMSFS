using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightJobsConnectorMSFS.Models
{
    public class AirportModel
    {
        public string ICAO { get; set; }
        public string IATA { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int RunwaySize { get; set; }

        public int Elevation { get; set; }

        public int Trasition { get; set; }

    }
}