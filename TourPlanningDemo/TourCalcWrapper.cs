using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TourPlanningDemo.XTourServiceReference;

namespace TourPlanningDemo
{
    public class TourCalcWrapper
    {
        // these dictionaries are used to map the business entities to xTour entities.
        // This is needed, because the xTour entities can only be identified by integer IDs.
        public BusinessToX<Order, string> orderMap;
        public BusinessToX<Depot, string> depotMap;
        public BusinessToX<Vehicle, string> vehicleMap;

        public Action Progress;
        public Action Finished;

        public string ProgressMessage;
        public int ProgressPercent;

        public Scenario scenario;

        private BackgroundWorker backgroundWorker;

        public void Cancel()
        {
            backgroundWorker.CancelAsync();
        }

        public void StartPlanScenario(Scenario usedScenario)
        {
            orderMap = new BusinessToX<Order, string>();
            depotMap = new BusinessToX<Depot, string>();
            vehicleMap = new BusinessToX<Vehicle, string>();

            scenario = usedScenario;

            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork += BackgroundWorkerDoWork;
            backgroundWorker.RunWorkerCompleted += bw_RunWorkerCompleted;
            backgroundWorker.ProgressChanged += bw_ProgressChanged;
            backgroundWorker.RunWorkerAsync();
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (Progress == null) return;

            var job = e.UserState as Job;
            if (job.progress == null)
                ProgressMessage = job.status.ToString();
            else if (job.progress is PlanProgress)
            {
                var planProgress = (PlanProgress) job.progress;
                switch (planProgress.action)
                {
                    case "DistanceMatrix.Calculation":
                    {
                        var dimaProgress = planProgress.distanceMatrixCalculationProgress.currentDistanceMatrixProgress;
                        var currentRowIndex = dimaProgress.currentRowIndex;
                        var lastRowIndex = dimaProgress.lastRowIndex;
                        ProgressPercent = 50 * currentRowIndex / lastRowIndex;
                        ProgressMessage = string.Format("Calculating Distance Matrix: {0}/{1}", currentRowIndex, lastRowIndex);
                        break;
                    }
                    case "Optimization.Improvement":
                    {
                        var improvementProgress = planProgress.improvementProgress;
                        var availableMachineTime = improvementProgress.availableMachineTime;
                        var usedMachineTime = improvementProgress.usedMachineTime;
                        var iterationIndex = improvementProgress.iterationIndex;
                        ProgressPercent = 50 + 50 * usedMachineTime / availableMachineTime;
                        ProgressMessage = string.Format("Improving plan, iteration index: {0}, machine time: {1}/{2}", iterationIndex, usedMachineTime, availableMachineTime);
                        break;
                    }
                    default:
                        ProgressMessage = planProgress.action;
                        break;
                }
            }
            else
                ProgressMessage = job.progress.ToString();

            Progress();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Progress != null)
            {
                if (!e.Cancelled)
                {
                    ProgressMessage = "Finished";
                    ProgressPercent = 100;
                }
                else
                {
                    ProgressMessage = "Cancelled";
                    ProgressPercent = 0;
                }
                Progress();
            }

            if (Finished != null)
                Finished();
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            // reset old plan
            scenario.Tours = new List<Tour>();
            foreach (var o in scenario.Orders)
                o.Tour = null;

            var xTour = new XTourWSClient();
            if (xTour.ClientCredentials != null)
            {
                xTour.ClientCredentials.UserName.UserName = "xtok";
                xTour.ClientCredentials.UserName.Password = App.Token;
            }

            TransportOrder[] orders = (from o in scenario.Orders
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
                                  openingIntervalConstraint = OpeningIntervalConstraint.START_OF_SERVICE
                              },
                              deliveryQuantities = new Quantities { wrappedQuantities = new[] { o.Quantity } }
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

            var interval = new Interval
            {
                from = 0,
                till = Convert.ToInt32(scenario.OperatingPeriod.TotalSeconds)
            };

            var vehicles = (from v in allVehicles
                            select new XTourServiceReference.Vehicle
                            {
                                id = vehicleMap.B2X(v, v.Id),
                                depotIdStart = depotMap.B2X(v.Depot, v.Depot.Id),
                                depotIdEnd = depotMap.B2X(v.Depot, v.Depot.Id),
                                isPreloaded = false,
                                capacities = new Capacities
                                {
                                    wrappedCapacities = new[] { new Quantities { wrappedQuantities =
                                    new[] { v.Capacity } } }
                                },
                                wrappedOperatingIntervals = new[] { interval },
                                dimaId = 1,
                                dimaIdSpecified = true
                            }).ToArray();

            var fleet = new Fleet { wrappedVehicles = vehicles };

            var planningParams = new StandardParams
            {
                wrappedDistanceMatrixCalculation = new DistanceMatrixCalculation[] 
                {
                    new DistanceMatrixByRoad
                    {
                        dimaId = 1,
                        deleteBeforeUsage = true,
                        deleteAfterUsage = true,
                        profileName = "dimaTruck"
                    }
                },
                availableMachineTime = 15,
                availableMachineTimeSpecified = true
            };

            var xTourJob = xTour.startPlanBasicTours(orders, depots, fleet, planningParams, null,
                new CallerContext
                {
                    wrappedProperties = new[] {
                    new CallerContextProperty { key = "CoordFormat", value = "OG_GEODECIMAL" },
                    new CallerContextProperty { key = "TenantId", value = Guid.NewGuid().ToString() }}
                });

            backgroundWorker.ReportProgress(-1, xTourJob);
            var status = xTourJob.status;
            while (status == JobStatus.QUEUING || status == JobStatus.RUNNING)
            {
                if (backgroundWorker.CancellationPending)
                {
                    xTour.stopJob(xTourJob.id, null);
                    e.Cancel = true;
                    return;
                }

                xTourJob = xTour.watchJob(xTourJob.id, new WatchOptions
                {
                    maximumPollingPeriod = 250,
                    maximumPollingPeriodSpecified = true,
                    progressUpdatePeriod = 250,
                    progressUpdatePeriodSpecified = true
                }, null);
                status = xTourJob.status;

                backgroundWorker.ReportProgress(-1, xTourJob);

                // wait a bit on the client-side to reduce network+server load
                System.Threading.Thread.Sleep(250);
            }

            var result = xTour.fetchPlan(xTourJob.id, null);

            scenario.Tours = new List<Tour>();
            foreach (var c in result.wrappedChains)
                foreach (var wt in c.wrappedTours)
                {
                    var tour = new Tour();

                    var tourPoints = new List<TourPoint>();
                    foreach (var tourPoint in wt.wrappedTourPoints)
                    {
                        switch (tourPoint.type)
                        {
                            case TourPointType.DEPOT:
                                tourPoints.Add(new TourPoint
                                {
                                    Longitude = depotMap.X2B(tourPoint.id).Longitude,
                                    Latitude = depotMap.X2B(tourPoint.id).Latitude
                                });
                                break;
                            case TourPointType.TRANSPORT_POINT:
                                orderMap.X2B(tourPoint.id).Tour = tour;
                                tourPoints.Add(new TourPoint
                                {
                                    Longitude = orderMap.X2B(tourPoint.id).Longitude,
                                    Latitude = orderMap.X2B(tourPoint.id).Latitude
                                });
                                break;
                        }
                    }

                    tour.Vehicle = vehicleMap.X2B(c.vehicleId);
                    tour.TourPoints = tourPoints;
                    scenario.Tours.Add(tour);
                }
        }
    }

    /// <summary>
    /// A helper class which maps business objects (usually identified by a unique string) To xServer objects
    /// identified by an int.
    /// Note: The mapping is not unique! If you create it repeatedly, you get different IDs.
    /// Remind this if you need to work with persistent xTour objects, for example if you re-use distance matrices.
    /// </summary>
    /// <typeparam name="B">The type of the business object</typeparam>
    /// <typeparam name="K">The type of the key (id)</typeparam>
    public class BusinessToX<B, K>
    {
        private int idx;
        private readonly Dictionary<K, int> bTox = new Dictionary<K, int>();
        private readonly Dictionary<int, B> xTob = new Dictionary<int, B>();

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
