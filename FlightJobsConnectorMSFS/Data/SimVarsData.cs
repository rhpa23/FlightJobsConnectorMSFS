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
            public double sea_level_pressure { get; set; }
            public double ambient_wind_direction { get; set; }
            public double ambient_wind_velocity { get; set; }
            public double ambient_temperature { get; set; }
            public double ambient_visibility { get; set; }
            public double brake_parking_position { get; set; }
            public double eng_combustion { get; set; }
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
                DataName = SimVarsEnum.TITLE.ToDescriptionString(),
                UnitysName = null // Set NULL for Strings
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.PLANE_LATITUDE.ToDescriptionString(),
                UnitysName = "degree"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.PLANE_LONGITUDE.ToDescriptionString(),
                UnitysName = "degree"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.TOTAL_WEIGHT.ToDescriptionString(),
                UnitysName = weightUnit
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.EMPTY_WEIGHT.ToDescriptionString(),
                UnitysName = weightUnit
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.FUEL_TOTAL_QUANTITY_WEIGHT.ToDescriptionString(),
                UnitysName = weightUnit
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.SEA_LEVEL_PRESSURE.ToDescriptionString(),
                UnitysName = "millibars"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.AMBIENT_WIND_DIRECTION.ToDescriptionString(),
                UnitysName = "degrees"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.AMBIENT_WIND_VELOCITY.ToDescriptionString(),
                UnitysName = "knots"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.AMBIENT_TEMPERATURE.ToDescriptionString(),
                UnitysName = "celsius"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.AMBIENT_VISIBILITY.ToDescriptionString(),
                UnitysName = "meters"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.BRAKE_PARKING_POSITION.ToDescriptionString(),
                UnitysName = "number"
            });

            simVarModelList.Add(new SimVarModel()
            {
                DataName = SimVarsEnum.ENG_COMBUSTION.ToDescriptionString(),
                UnitysName = "number"
            });

            return simVarModelList;
        }
    }
}
