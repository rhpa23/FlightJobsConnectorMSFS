﻿using FlightJobsConnectorMSFS.Models;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace FlightJobsConnectorMSFS.Utils
{
    public class AirportDatabaseFile
    {
        public static AirportModel FindAirportInfo(string code)
        {
            //A,EDDS,STUTTGART,48.689878,9.221964,1276,5000,0,10900,0

            var airportFileDataBase = Properties.Resources.GlobalAirportDatabase;
            List<string> lines = airportFileDataBase.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //var lines = File.ReadLines(fileName);
            var airportInfo = lines.FirstOrDefault(line => Regex.IsMatch(line, "(^A,"+ code.ToUpper() +".*$)"));
            Console.WriteLine(airportInfo);

            if (airportInfo != null)
            {
                return BindModel(airportInfo);
            }
            else
            {
                return null;
            }
        }

        private static AirportModel BindModel(string line)
        {
            string[] lineArray = line.Split(',');

            double lat = 0;
            double log = 0;
            if (!isPositionEmpty(lineArray))
            {
                lat = double.Parse(lineArray[3].Replace(".", ","));
                log = double.Parse(lineArray[4].Replace(".", ","));
            }

            AirportModel airportBase = new AirportModel()
            {
                ICAO = lineArray[1],
                IATA = "",
                Name = lineArray[2],
                City = lineArray[2],
                Country = "",
                Latitude = lat,
                Longitude = log,
                Elevation = string.IsNullOrEmpty(lineArray[5]) ? 0 : int.Parse(lineArray[5]),
                Trasition = string.IsNullOrEmpty(lineArray[6]) ? 0 : int.Parse(lineArray[6]),
                RunwaySize = string.IsNullOrEmpty(lineArray[8]) ? 0 : int.Parse(lineArray[8]),
            };
            return airportBase;
        }

        public static List<AirportModel> GetAllAirportInfo()
        {
            var list = new List<AirportModel>();
            var airportFileDataBase = Properties.Resources.GlobalAirportDatabase;
            List<string> lines = airportFileDataBase.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var airportInfo in lines.Where(s => s.StartsWith("A,")))
            {
                try
                {
                    var model = BindModel(airportInfo);

                    list.Add(model);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return list;
        }

        private static bool isPositionEmpty(string[] infoArray)
        {
            return string.IsNullOrEmpty(infoArray[3]) || string.IsNullOrEmpty(infoArray[4]);
        }

        public static bool CheckClosestLocation(double actualLat, double actualLon, double verifyLat, double verifyLon)
        {
            var actualCoord = new GeoCoordinate(actualLat, actualLon);
            var verifyCoord = new GeoCoordinate(verifyLat, verifyLon);
            return verifyCoord.GetDistanceTo(actualCoord) < 10000;
        }


       /* public static List<AirportModel> FindClosestLocation(double latitude, double longitude)
        {
            var coord = new GeoCoordinate(latitude, longitude);
            var airportInfos = GetAllAirportInfo();

            int minDistance = 15000;

            var nearest = airportInfos.Select(x => new { nearest = x, co = new GeoCoordinate(x.Latitude, x.Longitude) })
                                   //.Where(x => x.co.GetDistanceTo(coord) < 1000)
                                   .OrderBy(x => x.co.GetDistanceTo(coord))
                                   .Take(5)
                                   .Where(x => x.co.GetDistanceTo(coord) < minDistance);

            return nearest.Select(x => x.nearest).ToList();
        }*/

    }
}
