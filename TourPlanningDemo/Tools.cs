using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TourPlanningDemo
{
    public static class Tools
    {
        // A helper method to invoke any method non-blocking
        public static void AsyncUIHelper<T>(Func<T> method, Action<T> success, Action<Exception> error)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (o, e) =>
            {
                try { e.Result = method(); }
                catch (Exception ex) { e.Result = ex; }
            };
            worker.RunWorkerCompleted += (o, e) =>
            {
                try
                {
                    if (e.Result is Exception)
                        error(e.Result as Exception);
                    else
                        success((T)e.Result);
                }
                finally
                {
                    worker.Dispose();
                }
            };
            worker.RunWorkerAsync();
        }
    }
}
