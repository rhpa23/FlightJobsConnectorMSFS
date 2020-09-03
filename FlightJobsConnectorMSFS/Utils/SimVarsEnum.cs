using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsConnectorMSFS.Utils
{
    public enum SimVarsEnum
    {
        [Description("TITLE")]
        TITLE,
        [Description("PLANE LATITUDE")]
        PLANE_LATITUDE,
        [Description("PLANE LONGITUDE")]
        PLANE_LONGITUDE,
        [Description("TOTAL WEIGHT")]
        TOTAL_WEIGHT,
        [Description("EMPTY WEIGHT")]
        EMPTY_WEIGHT,
        [Description("FUEL TOTAL QUANTITY WEIGHT")]
        FUEL_TOTAL_QUANTITY_WEIGHT,
        [Description("TOTAL PAYLOAD")]
        TOTAL_PAYLOAD,
    }
}
