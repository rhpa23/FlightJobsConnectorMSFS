using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorClientAPI
{
    public class StartJobResponseModel
    {
        public string ArrivalICAO { get; set; }
        public double ArrivalLAT { get; set; }
        public double ArrivalLON { get; set; }

        public string ResultMessage { get; set; }
    }
}
