// -----------------------------------------------------------------------
// <copyright file="DawnMemAssert.cs" company="PTV AG"></copyright>
//
// DawnMemAssert is a helper-class to check for memory-leaks in 
// UI-applications. You can call DawnMemAssert.SetNullAndAssertDead()
// for a reference to an object which should be removable from memory.
// If the assert-box pops up, this indicates a memory-leak, i.e.
// There are still references to this object. 
//
// The typical scenario are "lapsed event-listeners", references to a
// long-living object by attaching events to it. In this case the
// receiver of the event will not be freed, as long as the sender lives.
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ptvag.Dawn.Tools
{
    /// <summary>
    /// The mode for the Assert box
    /// </summary>
    public enum AssertMode
    {
        /// <summary>
        /// Always assert
        /// </summary>
        Always,
        /// <summary>
        /// Never assert
        /// </summary>
        Never,
        /// <summary>
        /// Assert only if the debugger is attached
        /// </summary>
        IfDebuggerIsAttached
    }

    public class DawnMemAssert<T> where T : class
    {
        public static bool SetNullAndAssertDead(ref T o)
        {
            // default is if debugger is attached
            var mode = AssertMode.IfDebuggerIsAttached;

            // try to read the assertion value from the app.config
            var configStr = System.Configuration.ConfigurationManager.AppSettings["DawnMemAssert"];
            if (string.Equals(configStr, "Always", StringComparison.InvariantCultureIgnoreCase))
                mode = AssertMode.Always;
            else if (string.Equals(configStr, "Never", StringComparison.InvariantCultureIgnoreCase))
                mode = AssertMode.Never;

            // check if assertion should be executed
            bool doAssert =
                (mode == AssertMode.Always) ||
                (mode == AssertMode.IfDebuggerIsAttached && Debugger.IsAttached);

            if (doAssert)
                new DawnMemAssert<T>(new WeakReference(o));

            // set the object reference to null
            o = null;

            // returns true if the assertion will be executed
            return doAssert;
        }

        /// <summary>
        /// Holds the weak reference to the object
        /// </summary>
        WeakReference weakReference;

        /// <summary>
        /// Creates an instance of the assert object
        /// </summary>
        /// <param name="wr">The weak reference to the object</param>
        private DawnMemAssert(WeakReference wr)
        {
            // set the weak reference
            this.weakReference = wr;

            // attach to the idle-event, so we'll leave the call-stack
            Application.Idle += new EventHandler(Application_Idle);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            // Invoke the garbage collector
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // At this point the weak reference shouldn't be alive anymore
            if (weakReference.IsAlive)
            {
                var result = MessageBox.Show(weakReference.Target + 
                    " is still alive! Maybe a thread still has a reference to it. Press Retry to test again.", 
                    "DawnMemAssert", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Retry)
                    return;
            }

            // detach from idle-event
            Application.Idle -= new EventHandler(Application_Idle);
        }
    }
}
