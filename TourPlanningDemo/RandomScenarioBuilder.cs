using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using TourPlanningDemo.XLocateServiceReference;

namespace TourPlanningDemo
{
    public enum ScenarioSize
    {
        Tiny,
        Small,
        Medium,
        Large
    }

    public class ScenarioBuilder
    {
        public static Scenario CreateRandomScenario(ScenarioSize size, System.Windows.Point center, double radius)
        {
            int TRUCK_MIN_CAPACITY = 4;
            int TRUCK_MAX_CAPACITY = 10;
            int ORDER_MIN_AMOUNT = 1;
            int ORDER_MAX_AMOUNT = 4;

            int numDepots, numVehiclesPerDepot, numOrdersPerVehicle, operatingPeriod;

            switch (size)
            {
                case ScenarioSize.Tiny:
                default:
                    numDepots = 1; numVehiclesPerDepot = 3; numOrdersPerVehicle = 3; operatingPeriod = 2 * 60 * 60;
                    break;
                case ScenarioSize.Small:
                    numDepots = 2; numVehiclesPerDepot = 3; numOrdersPerVehicle = 6; operatingPeriod = 3 * 60 * 60;
                    break;
                case ScenarioSize.Medium:
                    numDepots = 2; numVehiclesPerDepot = 4; numOrdersPerVehicle = 9; operatingPeriod = 4 * 60 * 60;
                    break;
                case ScenarioSize.Large:
                    numDepots = 3; numVehiclesPerDepot = 5; numOrdersPerVehicle = 17; operatingPeriod = 6 * 60 * 60;
                    break;
            }

            var rand = new Random();
            Func<System.Windows.Point, double, System.Windows.Point> randomCoordinate = (c, r) =>
            {
                var angle = rand.NextDouble() * 2 * Math.PI;
                var distance = r * Math.Sqrt(rand.NextDouble());

                return new System.Windows.Point
                {
                    X = c.X + distance * Math.Cos(angle),
                    Y = c.Y + distance * Math.Sin(angle)
                };
            };

            List<Location> locations = new List<Location>();
            for (int i = 0; i < numOrdersPerVehicle * numVehiclesPerDepot * numDepots + numDepots; i++)
            {
                var p = randomCoordinate(center, radius);
                locations.Add(new Location
                {
                    coordinate = new Point
                    {
                        point = new PlainPoint
                        {
                            x = p.X,
                            y = p.Y
                        }
                    }
                });
            }

            var xlocate = new XLocateWSClient();
            xlocate.ClientCredentials.UserName.UserName = "xtok";
            xlocate.ClientCredentials.UserName.Password = App.Token;
            var result = xlocate.findLocations(locations.ToArray(), new[] { 
                new ReverseSearchOption { param = ReverseSearchParameter.ENGINE_TARGETSIZE, value = "1" },
                new ReverseSearchOption { param = ReverseSearchParameter.ENGINE_FILTERMODE, value = "1" }},
                null, null,
               new CallerContext { wrappedProperties = new[] { new CallerContextProperty { key = "CoordFormat", value = "OG_GEODECIMAL" } } }
                );

            var orders = (from p in result.Take(numOrdersPerVehicle * numVehiclesPerDepot * numDepots)
                          select new Order
                          {
                              Id = Guid.NewGuid().ToString(),
                              Quantity = System.Convert.ToInt32(ORDER_MIN_AMOUNT + Math.Floor(rand.NextDouble() * (1 + ORDER_MAX_AMOUNT - ORDER_MIN_AMOUNT))),
                              Longitude = p.wrappedResultList[0].coordinates.point.x,
                              Latitude = p.wrappedResultList[0].coordinates.point.y
                          }).ToList();

            var palette = new Color[] { Colors.Blue, Colors.Green, Colors.Brown };
            int ci = 0;
            Func<Color> GetColor = () => palette[(ci++) % palette.Length];

            var depots = (from p in result.Skip(numOrdersPerVehicle * numVehiclesPerDepot * numDepots)
                          select new Depot
                          {
                              Id = Guid.NewGuid().ToString(),
                              Longitude = p.wrappedResultList[0].coordinates.point.x,
                              Latitude = p.wrappedResultList[0].coordinates.point.y,
                              Color = GetColor(),
                              Fleet = (from a in Enumerable.Range(0, numVehiclesPerDepot)
                                       select new Vehicle
                                       {
                                           Id = Guid.NewGuid().ToString(),
                                           Capacity = System.Convert.ToInt32(TRUCK_MIN_CAPACITY + Math.Floor(rand.NextDouble() * (1 + TRUCK_MAX_CAPACITY - TRUCK_MIN_CAPACITY)))
                                       }).ToList(),
                          }).ToList();

            // wire-up back-reference vehicle->depot
            foreach (var d in depots)
                foreach (var v in d.Fleet)
                    v.Depot = d;

            return new Scenario
            {
                NumDepots = numDepots,
                NumOrdersPerVehicle = numOrdersPerVehicle,
                NumVehiclesPerDepot = numOrdersPerVehicle,
                OperatingPeriod = operatingPeriod,
                Orders = orders,
                Depots = depots,
            };
        }
    }
}
