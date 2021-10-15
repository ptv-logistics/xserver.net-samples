﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using TourPlanningDemo.XLocateServiceReference;
using xPoint = TourPlanningDemo.XLocateServiceReference.Point;
using Point = System.Windows.Point;

namespace TourPlanningDemo
{
    public enum ScenarioSize
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge
    }

    public class RandomScenarioBuilder
    {
        public static Scenario CreateScenario(ScenarioSize size, Point center)
        {
            double radius = 5 * (Convert.ToInt32(size) + 1); // radius in km

            const int TRUCK_MIN_CAPACITY = 20;
            const int TRUCK_MAX_CAPACITY = 30;
            const int ORDER_MIN_AMOUNT = 1;
            const int ORDER_MAX_AMOUNT = 4;

            int numDepots, numVehiclesPerDepot, numOrdersPerVehicle;
            TimeSpan operatingPeriod;

            switch (size)
            {
                default:
                    numDepots = 1; numVehiclesPerDepot = 3; numOrdersPerVehicle = 3; operatingPeriod = TimeSpan.FromHours(2);
                    break;
                case ScenarioSize.Small:
                    numDepots = 2; numVehiclesPerDepot = 3; numOrdersPerVehicle = 6; operatingPeriod = TimeSpan.FromHours(3);
                    break;
                case ScenarioSize.Medium:
                    numDepots = 2; numVehiclesPerDepot = 4; numOrdersPerVehicle = 9; operatingPeriod = TimeSpan.FromHours(4);
                    break;
                case ScenarioSize.Large:
                    numDepots = 3; numVehiclesPerDepot = 5; numOrdersPerVehicle = 17; operatingPeriod = TimeSpan.FromHours(6);
                    break;
                case ScenarioSize.Huge:
                    numDepots = 5; numVehiclesPerDepot = 6; numOrdersPerVehicle = 21; operatingPeriod = TimeSpan.FromHours(8);
                    break;
            }

            // calculate random coordinates around the center
            var coordinates = GetRandomCoordinates(center, radius, numOrdersPerVehicle * numVehiclesPerDepot * numDepots + numDepots);

            // and match them to the road by reverse-locating them
            var result = GetMatchedCoordinates(coordinates);

            var rand = new Random();

            // build orders
            var enumerable = result.ToList();
            var orders = (from p in enumerable.Take(numOrdersPerVehicle * numVehiclesPerDepot * numDepots)
                          select new Order
                          {
                              Id = Guid.NewGuid().ToString(),
                              Quantity = Convert.ToInt32(ORDER_MIN_AMOUNT + Math.Floor(rand.NextDouble() * (1 + ORDER_MAX_AMOUNT - ORDER_MIN_AMOUNT))),
                              Longitude = p.Item1.X,
                              Latitude = p.Item1.Y,
                              Description = p.Item2
                          }).ToList();

            // build depots
            var palette = new[] { 
                (Color)ColorConverter.ConvertFromString("#66c2a5"),
                (Color)ColorConverter.ConvertFromString("#fc8d62"),
                (Color)ColorConverter.ConvertFromString("#8da0cb"),
                (Color)ColorConverter.ConvertFromString("#e78ac3"),
                (Color)ColorConverter.ConvertFromString("#a6d854") }; 

            int ci = 0;
            Func<Color> GetColor = () => palette[ci++ % palette.Length];

            var depots = (from p in enumerable.Skip(numOrdersPerVehicle * numVehiclesPerDepot * numDepots)
                select new Depot
                {
                    Id = Guid.NewGuid().ToString(),
                    Longitude = p.Item1.X,
                    Latitude = p.Item1.Y,
                    Description = p.Item2,
                    Color = GetColor(),
                    Fleet = (from a in Enumerable.Range(0, numVehiclesPerDepot)
                            select new Vehicle
                            {
                                Id = Guid.NewGuid().ToString(),
                                Capacity = Convert.ToInt32(TRUCK_MIN_CAPACITY + Math.Floor(rand.NextDouble() * (1 + TRUCK_MAX_CAPACITY - TRUCK_MIN_CAPACITY)))
                            }).ToList()
                }).ToList();

            // wire-up back-reference vehicle->depot
            foreach (var d in depots)
                foreach (var v in d.Fleet)
                    v.Depot = d;

            return new Scenario
            {
                OperatingPeriod = operatingPeriod,
                Orders = orders,
                Depots = depots
            };
        }

        // get coordinates matched on the road by some totally random coordinates
        private static IEnumerable<Tuple<Point, string>> GetMatchedCoordinates(IEnumerable<Point> inputCoords)
        {
            var locations = from c in inputCoords
                            select new Location
                            {
                                coordinate = new xPoint
                                {
                                    point = new PlainPoint
                                    {
                                        x = c.X,
                                        y = c.Y
                                    }
                                }
                            };

            var xLocate = new XLocateWSClient();
            if (xLocate.ClientCredentials != null)
            {
                xLocate.ClientCredentials.UserName.UserName = "xtok";
                xLocate.ClientCredentials.UserName.Password = App.Token;
            }

            var result = xLocate.findLocations(locations.ToArray(), new SearchOptionBase[] {
                new ReverseSearchOption { param = ReverseSearchParameter.ENGINE_TARGETSIZE, value = "1" },
                new ReverseSearchOption { param = ReverseSearchParameter.ENGINE_FILTERMODE, value = "1" }},
                null, null,
                new CallerContext { wrappedProperties = new[] { new CallerContextProperty { key = "CoordFormat", value = "OG_GEODECIMAL" } } }
            );

            foreach (var address in result)
            {
                var location = address.wrappedResultList[0];
                yield return new Tuple<Point, string>(
                    new Point(location.coordinates.point.x, location.coordinates.point.y),
                    $"{location.city}\n{location.street}"
                );
            }
        }

        // calculate the random coordinates around a center c and a radius r
        // by using the (conformal) mercator projection
        private static IEnumerable<Point> GetRandomCoordinates(Point c, double r, int count)
        {
            var rand = new Random();
            var mercC = Wgs_2_SphereMercator(c);
            var mercR = r * 1000.0 / Math.Cos(c.Y); // 1/cos(latitude) is the ground scale

            for (int i = 0; i < count; i++)
            {
                var angle = rand.NextDouble() * 2 * Math.PI;
                var distance = mercR * rand.NextDouble();

                var mercP = new Point
                {
                    X = mercC.X + distance * Math.Cos(angle),
                    Y = mercC.Y + distance * Math.Sin(angle)
                };

                yield return SphereMercator_2_Wgs(mercP); // back to WGS
            }
        }

        // lat/lng to mercator
        private static Point Wgs_2_SphereMercator(Point point)
        {
            const double earthRadius = 6371000;

            double x = earthRadius * point.X * Math.PI / 180.0;
            double y = earthRadius * Math.Log(Math.Tan(Math.PI / 4.0 + point.Y * Math.PI / 360.0));

            return new Point(x, y);
        }

        // mercator to lat/lng
        private static Point SphereMercator_2_Wgs(Point point)
        {
            const double earthRadius = 6371000;

            double x = 180.0 / Math.PI * (point.X / earthRadius);
            double y = 360 / Math.PI * (Math.Atan(Math.Exp(point.Y / earthRadius)) - Math.PI / 4);

            return new Point(x, y);
        }
    }
}
