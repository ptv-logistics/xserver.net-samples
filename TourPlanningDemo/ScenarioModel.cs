using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace TourPlanningDemo
{
    /// <summary>
    /// This class represents our scenario which models our tour planning objects
    /// Unlike the objects used in xTour, which are modelled to build-up our xtour requests, 
    /// these objects are modelled to build-up our application domain. 
    /// They are usually mapped to some kind of database
    /// </summary>
    public class Scenario
    {
        public List<Order> Orders;
        public List<Depot> Depots;
        public List<Tour> Tours;
        public TimeSpan OperatingPeriod { get; set; }
    }

    public class Vehicle
    {
        public string Id { get; set; }

        public int Capacity { get; set; }

        public Depot Depot { get; set; }
    }

    public class Depot
    {
        public string Id { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public List<Vehicle> Fleet { get; set; }

        public Color Color { get; set; }
    }

    public class Order
    {
        public string Id { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int Quantity { get; set; }

        public Tour Tour { get; set; }
    }

    public class Tour
    {
        public Vehicle Vehicle { get; set; }

        public List<TourPoint> TourPoints { get; set; }
    }

    public class TourPoint
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
