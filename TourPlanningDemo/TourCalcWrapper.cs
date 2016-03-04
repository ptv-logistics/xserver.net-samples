using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
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
            if(Progress != null)
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
            xtour.ClientCredentials.UserName.UserName = "xtok";
            xtour.ClientCredentials.UserName.Password = App.Token;
            var orders = (from o in scenario.Orders
                          select new TransportDepot
                          {
                              id = orderMap.MapObject(o, o.Id),
                              transportPoint = new TransportPoint
                              {
                                  id = orderMap.MapObject(o, o.Id),                                  
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
                              id = depotMap.MapObject(d, d.Id),
                              location = new Point
                              {
                                  point = new PlainPoint
                                  {
                                      y = d.Latitude,
                                      x = d.Longitude
                                  }
                              }                          
                          }).ToArray();

            var yy = (from d in scenario.Depots select d.Fleet).SelectMany(x => x);

            var interval = new Interval();
            interval.from = 0;
            interval.till = scenario.OperatingPeriod * 200; // warum * 1000?

            var vehicles = (from v in yy
                            select new XTourServiceReference.Vehicle
                            {
                                id = vehicleMap.MapObject(v, v.Id),
                                depotIdStart = depotMap.bTos[v.Depot.Id],
                                depotIdEnd = depotMap.bTos[v.Depot.Id],
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
                                    Longitude = depotMap.sTob[tp.id].Longitude,
                                    Latitude = depotMap.sTob[tp.id].Latitude
                                });
                                break;
                            case TourPointType.TRANSPORT_POINT:
                                orderMap.sTob[tp.id].Tour = tour;
                                tps.Add(new TourPoint
                                {
                                    Longitude = orderMap.sTob[tp.id].Longitude,
                                    Latitude = orderMap.sTob[tp.id].Latitude
                                });
                                break;
                        }
                    }

                    tour.Vehicle = vehicleMap.sTob[c.vehicleId];
                    tour.TourPoints = tps;
                    scenario.Tours.Add(tour);
                }   
        }
    }

    /// <summary>
    /// A helper class which maps business objects (usually identified by a unique string) To xServer objects
    /// identified by an int
    /// </summary>
    /// <typeparam name="B"></typeparam>
    /// <typeparam name="K"></typeparam>
    public class BusinessToX<B, K>
    {
        int idx;
        public Dictionary<K, int> bTos = new Dictionary<K, int>();
        public Dictionary<int, B> sTob = new Dictionary<int, B>();

        public int MapObject(B obj, K key)
        {
            if (bTos.ContainsKey(key))
                return bTos[key];

            idx++;

            bTos[key] = idx;
            sTob[idx] = obj;

            return idx;
        }
    }
}
