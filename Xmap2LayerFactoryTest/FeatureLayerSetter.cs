using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ptv.XServer.Controls.Map.Layers.Xmap2;

namespace XMap2FactoryTest
{
    internal class FeatureLayerSetter
    {
        private readonly Form1 form;
        private readonly LayerFactory layerFactory;

        public FeatureLayerSetter(Form1 owner)
        {
            form = owner;
            layerFactory = form.formsMap.Xmap2LayerFactory;
            if (layerFactory == null)
                return; // Presumably xServer1 loaded

            // Connect GUI controls to the Feature Layer parameters.

            // Event of the themes list box when the visibility of a theme changes
            form.featureLayerCheckedListBox.ItemCheck += (sender, e) =>
            {
                string changedFeatureLayerTheme = form.featureLayerCheckedListBox.Items[e.Index].ToString();
                if (e.NewValue == CheckState.Checked)
                    layerFactory.LabelThemes.Add(changedFeatureLayerTheme);
                else
                    layerFactory.LabelThemes.Remove(changedFeatureLayerTheme);
            };

            // White list which contains only themes that can be rendered.
            var whiteList = new List<string> { "PTV_PreferredRoutes", "PTV_RestrictionZones", "PTV_TruckAttributes", "PTV_TrafficIncidents", "PTV_SpeedPatterns" };

            // Fill the checked list box with all relevant Feature Layer themes by means of the white list.
            layerFactory.FeatureLayers.AvailableThemes
                .Where(theme => whiteList.Contains(theme))
                .ToList().ForEach(theme => form.featureLayerCheckedListBox.Items.Add(theme, theme != "PTV_SpeedPatterns"));

            // For text boxes the Leave event is used instead of TextChanged because during editing inconsistent results may arise.
            form.noneTimeConsiderationRadioButton.CheckedChanged += SetTimeConsiderationScenario;
            form.optimisticRadioButton.CheckedChanged += SetTimeConsiderationScenario;
            form.snapshotRadioButton.CheckedChanged += SetTimeConsiderationScenario;
            form.timeSpanRadioButton.CheckedChanged += SetTimeConsiderationScenario;
            form.noneTimeConsiderationRadioButton.Checked = true;

            form.referenceTimeDatePicker.ValueChanged += SetReferenceTime;
            form.referenceTimeTimePicker.ValueChanged += SetReferenceTime;
            form.timeZoneTextBox.Leave += SetReferenceTimeWithTimeZoneCheck;
            SetReferenceTime(null, null);

            form.timeSpanTextBox.Leave += SetTimeSpan;
            SetTimeSpan(null, null);

            form.showOnlyRelevantCheckBox.CheckedChanged += SetShowOnlyRelevantByTime;
            SetShowOnlyRelevantByTime(null, null);

            form.contentSnapshotsComboBox.Items.Add("<No snapshot selected>");
            var contentSnapshotDescriptions = layerFactory.FeatureLayers.AvailableContentSnapshots.ToList();
            if (contentSnapshotDescriptions.Any())
            {
                foreach (var description in contentSnapshotDescriptions)
                    form.contentSnapshotsComboBox.Items.Add($"{description.label} ({description.id})");
            }
            else
            {
                form.contentSnapshotsComboBox.Enabled = false;
            }
            form.contentSnapshotsComboBox.SelectedIndex = 0;
            form.contentSnapshotsComboBox.SelectedIndexChanged += SetSnapshotID;
        }

        private void SetTimeConsiderationScenario(object sender, EventArgs e)
        {
            string timeConsiderationScenario;
            if      (form.optimisticRadioButton.Checked) timeConsiderationScenario = "OptimisticTimeConsideration";
            else if (form.snapshotRadioButton.Checked) timeConsiderationScenario = "SnapshotTimeConsideration";
            else if (form.timeSpanRadioButton.Checked) timeConsiderationScenario = "TimeSpanConsideration";
            else timeConsiderationScenario = string.Empty;
            layerFactory.FeatureLayers.TimeConsiderationScenario = timeConsiderationScenario;
        }

        // Valid example: "2016-10-21T04:00:00+02:00"
        private void SetReferenceTime(object sender, EventArgs e) => 
            layerFactory.FeatureLayers.ReferenceTime = $"{form.referenceTimeDatePicker.Value:yyyy-MM-dd}T{form.referenceTimeTimePicker.Value:HH:mm:ss}{form.timeZoneTextBox.Text}";

        private void SetReferenceTimeWithTimeZoneCheck(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(form.timeZoneTextBox.Text, @"^[-+]\d{2}:\d{2}$"))
            {
                System.Windows.MessageBox.Show(@"Wrong time zone format. Expected regular expression: [-+]\d{2}:\d{2}");
                return;
            }

            SetReferenceTime(sender, e);
        }

        private void SetTimeSpan(object sender, EventArgs e)
        {
            double.TryParse(form.timeSpanTextBox.Text, out double hours);
            layerFactory.FeatureLayers.TimeSpan = hours * 60 * 60;
        }

        private void SetShowOnlyRelevantByTime(object sender, EventArgs e) => layerFactory.FeatureLayers.ShowOnlyRelevantByTime = form.showOnlyRelevantCheckBox.Checked;

        private void SetSnapshotID(object sender, EventArgs e) => layerFactory.FeatureLayers.ContentSnapshotId = ExtractId(form.contentSnapshotsComboBox.Text);

        private static string ExtractId(string comboBoxEntry)
        {
            var match = Regex.Match(comboBoxEntry, @"\((.*)\)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
