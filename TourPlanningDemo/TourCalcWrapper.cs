using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TourPlanningDemo.XTourServiceReference;

namespace TourPlanningDemo
{
    public class TourCalcWrapper
    {
        // these dictionaries are used to map the business entities to xtour entities.
        // This is needed, because the xtour entities can only be identified by integer IDs.
        public BusinessToX<Order, string> orderMap;
        public BusinessToX<Depot, string> depotMap;
        public BusinessToX<Vehicle, string> vehicleMap;

        public Action Progress;
        public Action Finished;

        public string ProgressMessage;
        public int ProgressPercent;

        BackgroundWorker bw;
        public Scenario scenario;

        public void Cancel()
        {
            bw.CancelAsync();
        }

        public void StartPlanScenario(Scenario scenario)
        {
            orderMap = new BusinessToX<Order, string>();
            depotMap = new BusinessToX<Depot, string>();
            vehicleMap = new BusinessToX<Vehicle, string>();

            this.scenario = scenario;

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerAsync();
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (Progress != null)
            {
                var job = e.UserState as Job;
                if (job.progress == null)
                    this.ProgressMessage = job.status.ToString();
                else if (job.progress is PlanProgress)
                {
                    var pp = job.progress as PlanProgress;
                    if (pp.action == "DistanceMatrix.Calculation")
                    {
                        var dimaProgress = pp.distanceMatrixCalculationProgress.currentDistanceMatrixProgress;
                        var currentRowIndex = dimaProgress.currentRowIndex;
                        var lastRowIndex = dimaProgress.lastRowIndex;
                        this.ProgressPercent = 50 * currentRowIndex / lastRowIndex;
                        this.ProgressMessage = string.Format("Calculating Distance Matrix: {0}/{1}", currentRowIndex, lastRowIndex);
                    }
                    else if (pp.action == "Optimization.Improvement")
                    {
                        var improvementProgress = pp.improvementProgress;
                        var availableMachineTime = improvementProgress.availableMachineTime;
                        var usedMachineTime = improvementProgress.usedMachineTime;
                        var iterationIndex = improvementProgress.iterationIndex;
                        this.ProgressPercent = 50 + 50 * usedMachineTime / availableMachineTime;
                        this.ProgressMessage = string.Format("Improving plan, iteration index: {0}, machine time: {1}/{2}", iterationIndex, usedMachineTime, availableMachineTime);
                    }
                    else
                    {
                        this.ProgressMessage = pp.action;
                    }
                }
                else
                    this.ProgressMessage = job.progress.ToString();

                Progress();
            }
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Progress != null)
            {
                if (!e.Cancelled)
                {
                    this.ProgressMessage = "Finished";
                    this.ProgressPercent = 100;
                }
                else
                {
                    this.ProgressMessage = "Cancelled";
                    this.ProgressPercent = 0;
                }
                Progress();
            }

            if (Finished != null)
                Finished();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            // reset old plan
            scenario.Tours = new List<Tour>();
            foreach (var o in scenario.Orders)
                o.Tour = null;

            var xtour = new XTourWSClient();
            xtour.ClientCredentials.UserName.UserName = "EBB3ABF6-C1FD-4B01-9D69-349332944AD9";
            xtour.ClientCredentials.UserName.Password = App.Token;
            var orders = (from o in scenario.Orders
                          select new TransportDepot
                          {
                              id = orderMap.B2X(o, o.Id),
                              transportPoint = new TransportPoint
                              {
                                  id = orderMap.B2X(o, o.Id),
                                  servicePeriod = 0,  // 0sec; unrealistic but okay for this sample                                  
                                  location = new Point
                                  {
                                      point = new PlainPoint
                                      {
                                          x = o.Longitude,
                                          y = o.Latitude
                                      }
                                  },
                                  openingIntervalConstraint = OpeningIntervalConstraint.START_OF_SERVICE,
                              },
                              deliveryQuantities = new Quantities { wrappedQuantities = new int[] { o.Quantity } }
                          }).ToArray();

            var depots = (from d in scenario.Depots
                          select new XTourServiceReference.Depot
                          {
                              id = depotMap.B2X(d, d.Id),
                              location = new Point
                              {
                                  point = new PlainPoint
                                  {
                                      y = d.Latitude,
                                      x = d.Longitude
                                  }
                              }
                          }).ToArray();

            var allVehicles = (from d in scenario.Depots select d.Fleet).SelectMany(x => x);

            var interval = new Interval();
            interval.from = 0;
            interval.till = Convert.ToInt32(scenario.OperatingPeriod.TotalSeconds);

            var vehicles = (from v in allVehicles
                            select new XTourServiceReference.Vehicle
                            {
                                id = vehicleMap.B2X(v, v.Id),
                                depotIdStart = depotMap.B2X(v.Depot, v.Depot.Id),
                                depotIdEnd = depotMap.B2X(v.Depot, v.Depot.Id),
                                isPreloaded = false,
                                capacities = new Capacities
                                {
                                    wrappedCapacities = new Quantities[] { new Quantities { wrappedQuantities =
                                    new int[] { v.Capacity } } }
                                },
                                wrappedOperatingIntervals = new Interval[] { interval },
                                dimaId = 1,
                                dimaIdSpecified = true,
                            }).ToArray();

            var fleet = new Fleet { wrappedVehicles = vehicles };

            var planningParams = new StandardParams
            {
                wrappedDistanceMatrixCalculation = new[] {new DistanceMatrixByRoad
                {
                    dimaId = 1,
                    deleteBeforeUsage = true,
                    deleteAfterUsage = true,
                    profileName = "dimaTruck",
                }},
                availableMachineTime = 15,
                availableMachineTimeSpecified = true
            };

            var xtourJob = xtour.startPlanBasicTours(orders, depots, fleet, planningParams, null,
                new CallerContext
                {
                    wrappedProperties = new[] {
                    new CallerContextProperty { key = "CoordFormat", value = "OG_GEODECIMAL" },
                    new CallerContextProperty { key = "TenantId", value = Guid.NewGuid().ToString() }}
                });

            bw.ReportProgress(-1, xtourJob);
            var status = xtourJob.status;
            while (status == JobStatus.QUEUING || status == JobStatus.RUNNING)
            {
                if (bw.CancellationPending)
                {
                    xtour.stopJob(xtourJob.id, null);
                    e.Cancel = true;
                    return;
                }

                xtourJob = xtour.watchJob(xtourJob.id, new WatchOptions
                {
                    maximumPollingPeriod = 250,
                    maximumPollingPeriodSpecified = true,
                    progressUpdatePeriod = 250,
                    progressUpdatePeriodSpecified = true
                }, null);
                status = xtourJob.status;

                bw.ReportProgress(-1, xtourJob);

                // wait a bit on the client-side to reduce network+server load
                System.Threading.Thread.Sleep(250);
            }

            var result = xtour.fetchPlan(xtourJob.id, null);

            scenario.Tours = new List<Tour>();
            foreach (var c in result.wrappedChains)
                foreach (var wt in c.wrappedTours)
                {
                    var tour = new Tour();

                    List<TourPoint> tps = new List<TourPoint>();
                    foreach (var tp in wt.wrappedTourPoints)
                    {
                        switch (tp.type)
                        {
                            case TourPointType.DEPOT:
                                tps.Add(new TourPoint
                                {
                                    Longitude = depotMap.X2B(tp.id).Longitude,
                                    Latitude = depotMap.X2B(tp.id).Latitude
                                });
                                break;
                            case TourPointType.TRANSPORT_POINT:
                                orderMap.X2B(tp.id).Tour = tour;
                                tps.Add(new TourPoint
                                {
                                    Longitude = orderMap.X2B(tp.id).Longitude,
                                    Latitude = orderMap.X2B(tp.id).Latitude
                                });
                                break;
                        }
                    }

                    tour.Vehicle = vehicleMap.X2B(c.vehicleId);
                    tour.TourPoints = tps;
                    scenario.Tours.Add(tour);
                }
        }
    }

    /// <summary>
    /// A helper class which maps business objects (usually identified by a unique string) To xServer objects
    /// identified by an int.
    /// Note: The mapping is not unique! If you create it repeatedly, you get different IDs.
    /// Remind this if you need to work with persistant xTour objects, for example if you re-use distance matrices.
    /// </summary>
    /// <typeparam name="B">The type of the business object</typeparam>
    /// <typeparam name="K">The type of the key (id)</typeparam>
    public class BusinessToX<B, K>
    {
        private int idx;
        private Dictionary<K, int> bTox = new Dictionary<K, int>();
        private Dictionary<int, B> xTob = new Dictionary<int, B>();

        public int B2X(B obj, K key)
        {
            if (bTox.ContainsKey(key))
                return bTox[key];

            idx++;

            bTox[key] = idx;
            xTob[idx] = obj;

            return idx;
        }

        public B X2B(int key)
        {
            return xTob[key];
        }
    }
}
