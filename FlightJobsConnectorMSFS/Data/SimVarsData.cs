using FlightJobsConnectorMSFS.Extensions;
using FlightJobsConnectorMSFS.Models;
using FlightJobsConnectorMSFS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsConnectorMSFS.Data
{
    public class SimVarsData
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct SimVarsStruct
        {
            // this is how you declare a fixed size string
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public String title;
            public double latitude;
            public double longitude;
            public double total_weight;
            public double empty_weight;
            public double fuel_total_quantity_weight;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct SimVarsPayloadStruct
        {
            public double payload_station_weight_1;
            public double payload_station_weight_2;
            public double payload_station_weight_3;
            public double payload_station_weight_4;
            public double payload_station_weight_5;
            public double payload_station_weight_6;
            public double payload_station_weight_7;
            public double payload_station_weight_8;
            public double payload_station_weight_9;
            public double payload_station_weight_10;
            public double payload_station_weight_11;
            public double payload_station_weight_12;
            public double payload_station_weight_13;
            public double payload_station_weight_14;
            public double payload_station_weight_15;
        };

        public static IList<SimVarModel> CreateSimVarList(string weightUnit = "pounds")
        {
            IList<SimVarModel> simVarModelList = new List<SimVarModel>();
            // IMPORTANT: Add in the same struct definition order

            simVarModelList.Add(new SimVarModel()
            {
                DefineId = SimVarsEnum.TITLE,
                RequestId = SimVarsEnum.TITLE,
                DataName = SimVarsEnum.TITLE.ToDescriptionString(),
                UnitysName = null // Set NULL for Strings
            });

            simVarModelList.Add(new SimVarModel()
            {
                DefineId = SimVarsEnum.PLANE_LATITUDE,
                RequestId = SimVarsEnum.PLANE_LATITUDE,
                DataName = SimVarsEnum.PLANE_LATITUDE.ToDescriptionString(),
                UnitysName = "degree"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DefineId = SimVarsEnum.PLANE_LONGITUDE,
                RequestId = SimVarsEnum.PLANE_LONGITUDE,
                DataName = SimVarsEnum.PLANE_LONGITUDE.ToDescriptionString(),
                UnitysName = "degree"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DefineId = SimVarsEnum.TOTAL_WEIGHT,
                RequestId = SimVarsEnum.TOTAL_WEIGHT,
                DataName = SimVarsEnum.TOTAL_WEIGHT.ToDescriptionString(),
                UnitysName = weightUnit
            });

            simVarModelList.Add(new SimVarModel()
            {
                DefineId = SimVarsEnum.EMPTY_WEIGHT,
                RequestId = SimVarsEnum.EMPTY_WEIGHT,
                DataName = SimVarsEnum.EMPTY_WEIGHT.ToDescriptionString(),
                UnitysName = weightUnit
            });

            simVarModelList.Add(new SimVarModel()
            {
                DefineId = SimVarsEnum.FUEL_TOTAL_QUANTITY_WEIGHT,
                RequestId = SimVarsEnum.FUEL_TOTAL_QUANTITY_WEIGHT,
                DataName = SimVarsEnum.FUEL_TOTAL_QUANTITY_WEIGHT.ToDescriptionString(),
                UnitysName = weightUnit
            });

            return simVarModelList;
        }
    }
}
