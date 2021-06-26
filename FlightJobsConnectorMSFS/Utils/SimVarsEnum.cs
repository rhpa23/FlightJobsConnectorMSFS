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
        [Description("SEA LEVEL PRESSURE")]
        SEA_LEVEL_PRESSURE,
        [Description("AMBIENT WIND DIRECTION")]
        AMBIENT_WIND_DIRECTION,
        [Description("AMBIENT WIND VELOCITY")]
        AMBIENT_WIND_VELOCITY,
        [Description("AMBIENT TEMPERATURE")]
        AMBIENT_TEMPERATURE,
        [Description("AMBIENT VISIBILITY")]
        AMBIENT_VISIBILITY,
        [Description("BRAKE INDICATOR")]
        BRAKE_INDICATOR,
        [Description("ENG COMBUSTION:1")]
        ENG_COMBUSTION,


        TOTAL_PAYLOAD,
        WEATHER_INFO,
    }
}
