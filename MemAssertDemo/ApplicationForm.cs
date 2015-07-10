using Ptvag.Dawn.Tools;
using System;
using System.Windows.Forms;

namespace MemoryDemo
{
    public partial class ApplicationForm : Form
    {
        public ApplicationForm()
        {
            InitializeComponent();
        }

        // our child-form
        private MapForm mapForm;

        // this flag tests our memore-leak detection by provoking memory-leak
        private bool hasMemoryLeak;

        private void openMapWindow1_Click(object sender, EventArgs e)
        {
            // set our "memory-leak flag" to false
            hasMemoryLeak = true;

            // open our map-window
            ShowMapWindow();
        }

        private void openMapWindow2_Click(object sender, EventArgs e)
        {
            // set our "memory-leak flag" to false
            hasMemoryLeak = false;

            // open our map-window
            ShowMapWindow();
        }

        private void ShowMapWindow()
        {
            // change button states
            openMapWindow1.Enabled = false;
            openMapWindow2.Enabled = false;
            AddTruckButton.Enabled = true;

            // create the modal dialog we want to test for leaks.
            mapForm = new MapForm(this);

            // show the modal dialog
            mapForm.Show();

            // attach to closed-event
            mapForm.Closed += mapForm_Closed;
        }

        void mapForm_Closed(object sender, EventArgs e)
        {
            // change button states
            openMapWindow1.Enabled = true;
            openMapWindow2.Enabled = true;
            AddTruckButton.Enabled = false;

            // disposing won't help here
            mapForm.Dispose();

            if (!hasMemoryLeak) 
                // this line fixes our memory leak inside the mapForm.
                mapForm.FixMemoryLeak();

            // At this point dlg should be the last reference to the modal dialog
            // If the dialog causes a memory leak, a box will appear.
            // It is possible that the box appears because the map control still
            // loads tiles from xMapServer. In this case the box should disapper after
            // pressing retry.
            DawnMemAssert<MapForm>.SetNullAndAssertDead(ref mapForm);
        }
    }
}
