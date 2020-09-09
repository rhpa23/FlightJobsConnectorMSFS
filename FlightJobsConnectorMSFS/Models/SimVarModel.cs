using FlightJobsConnectorMSFS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FlightJobsConnectorMSFS.Data.SimVarsData;

namespace FlightJobsConnectorMSFS.Models
{
    public class SimVarModel
    {
        public string DataName { get; set; }
        public string UnitysName { get; set; }

        public SimVarsStruct Value { get; set; }
    }
}
