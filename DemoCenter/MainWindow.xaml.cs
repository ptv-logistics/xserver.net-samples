// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Demo.MapMarket;
using Ptv.XServer.Demo.Routing;
using Ptv.XServer.Demo.UseCases.FeatureLayer;
using Ptv.XServer.Demo.UseCases.Shapes;
using Ptv.XServer.Demo.Tools;
using Ptv.XServer.Demo.UseCases.Here;
using Ptv.XServer.Demo.UseCases.ShapeFile;
using Ptv.XServer.Demo.UseCases.MapProfile;
using Ptv.XServer.Demo.UseCases.GeoRss;
using Ptv.XServer.Demo.UseCases.Clustering;
using Ptv.XServer.Demo.UseCases.WMS;
using Ptv.XServer.Demo.UseCases.RoutingDragAndDrop;
using Ptv.XServer.Demo.UseCases.TourPlanning;
using Ptv.XServer.Demo.UseCases.Selection;
using System.Globalization;
using Ptv.XServer.Demo.UseCases;
using System.Collections.Generic;
using Ptv.XServer.Demo.Resources.MessageBox;

namespace Ptv.XServer.Demo
{
    /// <summary> Interaction logic for MainWindow.xaml. </summary>
    public partial class MainWindow
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once MemberInitializerValueIgnored
        private bool wait = true;
        
#if(!DEBUG)
        /// <summary> Splash window which is shown at startup. </summary>
        private static readonly SplashWindow splashWindow = new SplashWindow("PTV xServer .NET Demo Center;component/Resources/Splash_PTV Group_640x478_e_white.jpg");
#endif

        #region constructor
        /// <summary> Initializes a new instance of the <see cref="MainWindow"/> class. Shows a splash window if not in debug mode. </summary>
        public MainWindow()
        {
            // Setting a dedicated UI culture results in a modification of the used language in the map control's sub elements, but not in the
            // DemoCenter application. The constructor of the main window is used to get an early call of the method, before the sub elements
            // are drawn.
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

#if(!DEBUG)
            // Showing the splash window if not in debug mode.
            splashWindow.Show();
            // And close it after 3 seconds
            splashWindow.Close(this, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(0.6));
#endif

            InitializeComponent();

            // Attaching to mouse events for handling window changes.
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseDoubleClick += MainWindow_MouseDoubleClick;

            wait = false;
        }

        /// <summary> Event handler for having loaded the main window. </summary>
        /// <param name="sender"> The sender of the Loaded event. </param>
        /// <param name="e"> The event parameters. </param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool result = ConfigureWPFMap(wpfMap);
            if (!result)
            {
                var authenticationOK = UseCase.ManagedAuthentication.Set("api-eu-test", DefaultXServerInternetToken.Value);

                if (authenticationOK)
                {
                    ConfigureWPFMap(wpfMap);
                }
                else
                {
                    var msgBox = new MsgBox("Connection failed", UseCase.ManagedAuthentication.ErrorMessage);
                    msgBox.ShowDialog();
                }
            }

            // use case 'XServerConnection'
            XURLComboBox.DefaultText("Enter URL here", Properties.Settings.Default.XUrl);

            bool useDefaultToken = DefaultXServerInternetToken.Value.Equals(Properties.Settings.Default.XToken)
                || string.IsNullOrEmpty(Properties.Settings.Default.XToken);

            XTokenToggleButton.IsChecked = XTokenTextBox.IsReadOnly = useDefaultToken;
            XTokenTextBox.DefaultText("Azure: Enter xToken here", useDefaultToken
                ? DefaultXServerInternetToken.Value + " (Trial-Key)"
                : Properties.Settings.Default.XToken);

            // Use case 'HERE'
            HereAppIdTextBox.DefaultText("Enter AppId here", Properties.Settings.Default.HereAppId);
            HereAppCodeTextBox.DefaultText("Enter AppCode here", Properties.Settings.Default.HereAppCode);

            // Use case 'Clustering'
            clusteringUseCase.CheckBoxToDisable = ClusteringCheckBox;

            // Use case 'Additional WPF map'
            // For a dynamic extension of the column's count, a default column definition has to be inserted.
            MapGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 50 });

            // Use case 'Switch UI theme'
            UIThemeComboBox.SelectedIndex = 2; // This item corresponds to default theme

            // Use case 'Switch map style'
            ProfileComboBox.Text = "Silkysand";
            ProfileOKButton_OnClick(null, null);

            // Use case 'xLocate'
            DeactivatedGeocodingRadiobutton.IsChecked = true;
            SingleFieldInput.Visibility = Visibility.Collapsed;
            MultiFieldInput.Visibility = Visibility.Collapsed;

            // Use case 'Tour planning'
            TourScenarioComboBox.SelectedIndex = 0; // First item corresponds to 'deactivated'
            TourPlanningStart.IsEnabled = false;

            #region doc:TourPlanningAsynchronousEvents
            tourPlanningUseCase.Initialized = () =>
            {
                TourPlanningStart.IsEnabled = true;
            };

            tourPlanningUseCase.Progress = (progressMessage, percent) =>
            {
                TourPlanningStatus.Content = progressMessage;
                TourPlanningProgressBar.Value = percent;
            };

            tourPlanningUseCase.Finished = () =>
            {
                TourPlanningStart.Content = "Start planning";
                TourScenarioComboBox.IsEnabled = true;
                TourPlanningStart.IsEnabled = true;
            };
            #endregion //doc:TourPlanningAsynchronousEvents

            #region Intro initializing
            if (Properties.Settings.Default.IntroSkipped) return;

            var demoCenterIntro = new DemoCenterInstructions(IntroGrid);
            demoCenterIntro.StartIntro();
            demoCenterIntro.HandlerCallback = (id, start) =>
            {
                switch (id)
                {
                    case "Connection": if (start) Menu.IsExpanded = xServerExpander.IsExpanded = true;
                        ConnectionIntroEllpise.Visibility = (start) ? Visibility.Visible : Visibility.Hidden;
                        break;
                    case "xServerinternet": break;
                    case "_SkippedIntro": Properties.Settings.Default.IntroSkipped = start;
                        Properties.Settings.Default.Save(); break;
                    case "ExploreUsecases": if (start) Menu.IsExpanded = RoutingExpander.IsExpanded = true;
                        PossibilitiesIntroEllpise.Visibility = (start) ? Visibility.Visible : Visibility.Hidden; break;
                }
            };

            #endregion
        }
        #endregion

        private bool ConfigureWPFMap(Map map)
        {
            #region doc:Static Map Control
            // Initialize map control with xMap layers, especially the URL of the xServer. 
            // For layers provided on Azure xServers, an xToken has to be taken into consideration. This information is 
            // managed in the Usecase base class, which is globally available for the application and all other use cases.

            if (!UseCase.ManagedAuthentication.IsOk)
                return false;

            // Safe the layer order for realigning.
            var layerOrder = map.Layers.Select(layer => layer.Name).ToList();

            // Remove only layers which are potentially inserted by this method.
            map.Layers.RemoveXMapBaseLayers();
            map.Layers.Remove(map.Layers["Poi"]);
            if (map.Layers["Feature Layer routes"] != null) map.Layers.Remove(map.Layers["Feature Layer routes"]);
            map.Layers.InsertXMapBaseLayers(UseCase.ManagedAuthentication.XMapMetaInfo);

            // Feature Layer settings 
            featureLayerUseCase = new FeatureLayerUseCase(map)
            {
                ScenarioChanged = (theme, isBegin) => 
                {
                    Cursor = isBegin ? Cursors.AppStarting : Cursors.Arrow;
                    ComboBoxBy(theme).IsEnabled = CheckBoxBy(theme).IsEnabled = !isBegin;
                }
            };

            InitFeatureLayer(featureLayerUseCase.AvailableTrafficIncidents, TrafficIncidentsComboBox, TrafficIncidentsCheckBox);
            InitFeatureLayer(featureLayerUseCase.AvailableTruckAttributes, TruckAttributesComboBox, TruckAttributesCheckBox);
            InitFeatureLayer(featureLayerUseCase.AvailableRestrictionZones, RestrictionZonesComboBox, RestrictionZonesCheckBox);
            InitFeatureLayer(featureLayerUseCase.AvailablePreferredRoutes, PreferredRoutesComboBox, PreferredRoutesCheckBox);
            InitFeatureLayer(featureLayerUseCase.AvailableSpeedPatterns, SpeedPatternsComboBox, SpeedPatternsCheckBox);

            // Triggers the MapProfile use case to change the maps profile.
            ProfileOKButton_OnClick(null, null);

            // add POI layer (if available)
            var url = UseCase.ManagedAuthentication.XMapMetaInfo.Url;
            if (!(XServerUrl.IsDecartaBackend(url) 
                || XServerUrl.IsXServerInternet(url) && url.Contains("china")))
            {
                #region doc:AdditionalLayerCreation
                map.InsertPoiLayer(UseCase.ManagedAuthentication.XMapMetaInfo, "Poi", "default.points-of-interest", "Points of interest");
                #endregion
            }

            // recreate the old layer order if the set of layers has not changed
            if (map.Layers.All(layer => layerOrder.Contains(layer.Name)))
            {
                var layers = layerOrder
                    .Where(name => map.Layers[name] != null)
                    .Select((name, index) => new { newIndex = index, oldIndex = map.Layers.IndexOf(map.Layers[name]) });

                foreach (var item in layers)
                    map.Layers.Move(item.oldIndex, item.newIndex);
            }

            if (UseCase.ManagedAuthentication.XMapMetaInfo.GetRegion() == Region.eu)
                map.SetMapLocation(new Point(8.4, 49), 10); // Center in Karlsruhe
            else if (UseCase.ManagedAuthentication.XMapMetaInfo.GetRegion() == Region.na)
                map.SetMapLocation(new Point(-74.11, 40.93), 10); // Center in New York
            else if (UseCase.ManagedAuthentication.XMapMetaInfo.GetRegion() == Region.au)
                map.SetMapLocation(new Point(149.16, -35.25), 10); // Center in Canberra
            else
                map.SetMapLocation(new Point(0, 33), 1.8); // Center on equator, meridian

            #endregion
            return true;
        }

        ///////////////////////////////////////////////////////////////////////
        #region XServerConnection use case
        ///////////////////////////////////////////////////////////////////////

        // For xServer connections no dedicated use case is implemented due to the complete support
        // by the UseCase base class.

        /// <summary> Event handler for a click on the ok button of the XServer expander.  </summary>
        /// <param name="sender"> Sender of the Click event. </param>
        /// <param name="e"> Event parameters. </param>
        private void XServerOKButton_Click(object sender, RoutedEventArgs e)
        {
            if (wait) return;

            Cursor = Cursors.Wait;
            var authenticationOK = UseCase.ManagedAuthentication.Set(getUrlFromComboBox(), getXTokenFromTextBox());
            Cursor = null;

            if (!authenticationOK)
            {
                var msgBox = new MsgBox("Connection failed", UseCase.ManagedAuthentication.ErrorMessage);
                msgBox.ShowDialog();
                return;
            }

            ConfigureWPFMap(wpfMap);
        }

        private string getUrlFromComboBox()
        {
            return (!(XURLComboBox.SelectedItem is ComboBoxItem urlItem) || string.IsNullOrEmpty(urlItem.Tag as string)) 
                ? XURLComboBox.Text.Trim() 
                : (string) urlItem.Tag;
        }

        private string getXTokenFromTextBox()
        {
            string result = XTokenTextBox.Text.Trim();

            return (XTokenToggleButton.IsChecked ?? false)
                ? result.Replace(" (Trial-Key)", "")
                : result;
        }

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region HERE use case

        private readonly HereUseCase hereUseCase = new HereUseCase();

        private void EnableHereUseCase()
        {
            hereUseCase.Activate(false, wpfMap);

            if ((String.IsNullOrEmpty(Properties.Settings.Default.HereAppCode)) 
                || Properties.Settings.Default.HereAppCode.Equals("Enter AppCode here")
                || (String.IsNullOrEmpty(Properties.Settings.Default.HereAppId))
                || Properties.Settings.Default.HereAppId.Equals("Enter AppId here")) return;

            try { hereUseCase.Activate(true, wpfMap); }
            catch (Exception ex)
            {
                HereExpander.IsExpanded = true;
                MessageBox.Show(ex.Message);
                HereAppIdTextBox.Focus();
            }
        }

        private void HereAppIdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            HereOKButton_Click(this, null);
        }

        private void HereOKButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HereAppId = HereAppIdTextBox.Text;
            Properties.Settings.Default.HereAppCode = HereAppCodeTextBox.Text;
            Properties.Settings.Default.Save();
            EnableHereUseCase();
        }

        private void HereAppCodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            HereOKButton_Click(this, null);
        }
        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region 'Additional WPF map' use case

        private void InsertWpfMap()
        {
            #region doc:Dynamic Map Control
        
            var dynamicWpfMap = new WpfMap();
            if (!ConfigureWPFMap(dynamicWpfMap))
                return;

            // First a grid splitter is created and inserted into MapGrid
            var splitter = new GridSplitter
            {
                Background = new SolidColorBrush(Color.FromArgb(0xff, 0x5B, 0x5B, 0x5B)),
                Width = 4,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            MapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });

            // Define the gridsplitters's position in the grid...
            Grid.SetRow(splitter, 0);
            Grid.SetColumn(splitter, 1);
            MapGrid.Children.Add(splitter);

            // Insertion into the grid:
            // Define an additional column for the second map control
            MapGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 50 });

            // Define the map control's position in the grid...
            Grid.SetRow(dynamicWpfMap, 0);
            Grid.SetColumn(dynamicWpfMap, 2);

            // ... and add it finally
            MapGrid.Children.Add(dynamicWpfMap);

            #endregion doc:Dynamic Map Control
        }

        private void RemoveWpfMap()
        {
            var gridSplitterList = MapGrid.Children.OfType<GridSplitter>().ToList();
            var mapList = MapGrid.Children.OfType<WpfMap>().ToList();

            // Check if second map control already created
            if (mapList.Count < 2)
                return;

            //Remove the slider
            MapGrid.Children.Remove(gridSplitterList[0]);
            MapGrid.ColumnDefinitions.RemoveAt(1);

            // Removal from the grid
            MapGrid.Children.Remove(mapList[1]);
            MapGrid.ColumnDefinitions.RemoveAt(1);
            MapGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        }

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region 'Different shape' use case

        private readonly DifferentShapesUseCase differentShapesUseCase = new DifferentShapesUseCase();

        private void ShapesUseLabelsCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            differentShapesUseCase.UseLabels = ShapesUseLabelsCheckBox.IsChecked.GetValueOrDefault(false);
        }

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region 'Switch UI theme' use case

        #region doc:switch theme

        /// <summary> Event handler for changing the UI theme according the new combo box entry. </summary>
        /// <param name="sender"> Sender of the Changed event. </param>
        /// <param name="e"> Event parameters. </param>
        private void UIThemeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theme = "Ptv.XServer.Demo.Resources.Themes.PTV" + UIThemeComboBox.SelectedValue + ".xaml";
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(MainWindow));
            using (var themeFile = assembly.GetManifestResourceStream(theme))
                wpfMap?.SetThemeFromXaml(themeFile);
        }
        #endregion doc:switch theme

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region 'Switch profile' use case

        private readonly MapProfileUseCase mapProfileUseCase = new MapProfileUseCase();

        /// <summary> Event handler triggered by the switch map profile OK button. </summary>
        /// <param name="sender"> Sender of the event. </param>
        /// <param name="e"> Event parameters. </param>
        private void ProfileOKButton_OnClick(object sender, RoutedEventArgs e)
        {
            mapProfileUseCase.ChangeMapProfile(wpfMap, ProfileComboBox.Text.ToLower());
        }
        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region 'xLocate' use case

        /// <summary> Event handler for having checked the single field locating radio button. </summary>
        /// <param name="sender"> Sender of the Checked event. </param>
        /// <param name="e"> Event parameters. </param>
        private void SingleFieldLocatingRB_Checked(object sender, RoutedEventArgs e)
        {
            SingleFieldInput.Visibility = (SingleFieldRadioButton.IsChecked ?? false) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary> Event handler for having checked multi field locating radio button. </summary>
        /// <param name="sender"> Sender of the Checked event. </param>
        /// <param name="e"> Event parameters. </param>
        private void MultiFieldLocatingRB_Checked(object sender, RoutedEventArgs e)
        {
            MultiFieldInput.Visibility = (MultiFieldRadioButton.IsChecked ?? false) ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Feature Layer use case

        private FeatureLayerUseCase featureLayerUseCase;

        private void InitFeatureLayer(bool available, ComboBox comboBox, CheckBox checkBox)
        {
            checkBox.IsChecked = false;
            comboBox.Visibility = checkBox.Visibility = available ? Visibility.Visible : Visibility.Collapsed;
            comboBox.Items.Clear(); // Prevents from showing example routes by multiple feature layers
            foreach (var description in featureLayerUseCase.GetScenarioDescriptionsBy(ThemeFrom(comboBox)))
                comboBox.Items.Add(new ComboBoxItem { Content = description });
        }

        private void FeatureLayerCheckBox_Changed(object uiElement, bool state)
        {
            var comboBox = ComboBoxBy(ThemeFrom(uiElement));
            comboBox.IsEnabled = state && (comboBox.Items.Count > 0);
            if (!state)
            {
                comboBox.SelectedIndex = -1;
                comboBox.Text = string.Empty;
            }
        }

        private void FeatureLayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox comboBox)) return;
            featureLayerUseCase.SetScenario(ThemeFrom(comboBox), comboBox.SelectedIndex);
        }

        private Theme ThemeFrom(object uiElement)
        {
            if (uiElement.Equals(TrafficIncidentsCheckBox) || uiElement.Equals(TrafficIncidentsComboBox)) return Theme.TrafficIncidents;
            if (uiElement.Equals(TruckAttributesCheckBox) || uiElement.Equals(TruckAttributesComboBox)) return Theme.TruckAttributes;
            if (uiElement.Equals(RestrictionZonesCheckBox) || uiElement.Equals(RestrictionZonesComboBox)) return Theme.RestrictionZones;
            if (uiElement.Equals(PreferredRoutesCheckBox) || uiElement.Equals(PreferredRoutesComboBox)) return Theme.PreferredRoutes;
            if (uiElement.Equals(SpeedPatternsCheckBox) || uiElement.Equals(SpeedPatternsComboBox)) return Theme.SpeedPatterns;
            throw new ArgumentException("UIElement is not part of the Feature Layer use case.");
        }

        private ComboBox ComboBoxBy(Theme theme)
        {
            switch (theme)
            {
                case Theme.TrafficIncidents: return TrafficIncidentsComboBox;
                case Theme.TruckAttributes: return TruckAttributesComboBox;
                case Theme.RestrictionZones: return RestrictionZonesComboBox;
                case Theme.PreferredRoutes: return PreferredRoutesComboBox;
                case Theme.SpeedPatterns: return SpeedPatternsComboBox;
                default: throw new ArgumentException("No combo box available for this theme.");
            }
        }

        private CheckBox CheckBoxBy(Theme theme)
        {
            switch (theme)
            {
                case Theme.TrafficIncidents: return TrafficIncidentsCheckBox;
                case Theme.TruckAttributes: return TruckAttributesCheckBox;
                case Theme.RestrictionZones: return RestrictionZonesCheckBox;
                case Theme.PreferredRoutes: return PreferredRoutesCheckBox;
                case Theme.SpeedPatterns: return SpeedPatternsCheckBox;
                default: throw new ArgumentException("No Check box available for this theme.");
            }
        }

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Tour planning use case

        private readonly TourPlanningUseCase tourPlanningUseCase = new TourPlanningUseCase();

        private void TourScenarioComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool tourScenarioSelected = (TourScenarioComboBox.SelectedIndex != 0); 
            TourPlanningStart.IsEnabled = false;

            tourPlanningUseCase.Activate(tourScenarioSelected, wpfMap);
            if (tourScenarioSelected)
                tourPlanningUseCase.SetScenario((ScenarioSize) Enum.Parse(typeof (ScenarioSize), ((ListBoxItem) e.AddedItems[0]).Content.ToString()));
        }

        private void TourPlanningStartButton_Click(object sender, RoutedEventArgs e)
        {
            if ((TourPlanningStart.Content as string) == "Cancel")
                tourPlanningUseCase.StopPlanning();
            else
            {
                tourPlanningUseCase.StartPlanning();
                TourPlanningStart.Content = "Cancel";
                TourScenarioComboBox.IsEnabled = false;
            }
        }

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Other use cases

        private readonly GeoRssUseCase geoRssUseCase = new GeoRssUseCase();
        private readonly ClusteringUseCase clusteringUseCase = new ClusteringUseCase();
        private readonly MapMarketController mapMarketController = new MapMarketController();
        private readonly DataManagerUseCase dataManagerUseCase = new DataManagerUseCase();
        private readonly ShapeFileUseCase shapeFileUseCase = new ShapeFileUseCase();
        private readonly WMSUseCase wmsUseCase = new WMSUseCase();
        private RoutingUseCase routingUseCase;
        private readonly RoutingDragAndDropUseCase routingDragAndDropUseCase = new RoutingDragAndDropUseCase();
        private readonly SelectionUseCase selectionUseCase = new SelectionUseCase();

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Global handling of check boxes enabling the different use cases

        /// <summary> Event handler for changing the state of all check boxes, which enable/disable the individual use cases. </summary>
        /// <param name="sender"> The sender of the CheckedChanged event. </param>
        /// <param name="e"> The event parameters. </param>
        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var state = ((CheckBox)sender).IsChecked ?? false;
            
            oldCursor = Cursor;
            Cursor = Cursors.Wait;

            if (sender.Equals(HereCheckBox))
            {
                HereOkButton.IsEnabled = HereAppIdTextBox.IsEnabled = HereAppCodeTextBox.IsEnabled = state;
                if (state)
                    EnableHereUseCase();
                else
                    hereUseCase.Activate(false, wpfMap);
            }
            else if (sender.Equals(ClusteringCheckBox))
                clusteringUseCase.Activate(state, wpfMap);
            else if (sender.Equals(DifferentShapesCheckBox))
                differentShapesUseCase.Activate(ShapesUseLabelsCheckBox.IsEnabled = state, wpfMap);
            else if (sender.Equals(AddWpfMapCheckBox))
            {
                if (state)
                    InsertWpfMap();
                else
                    RemoveWpfMap();
            }
            else if (sender.Equals(GeoRSSCheckBox))
                geoRssUseCase.Activate(state, wpfMap);
            else if (sender.Equals(MapMarketCheckBox))
                mapMarketController.Activate(state, wpfMap);
            else if (sender.Equals(DataManagerCheckBox))
                dataManagerUseCase.Activate(state, wpfMap);
            else if (sender.Equals(ShapeFileCheckBox))
                shapeFileUseCase.Activate(state, wpfMap);
            else if (sender.Equals(ManySymbolsCheckBox))
                selectionUseCase.Activate(state, wpfMap);
            else if (sender.Equals(WmsEnableCheckBox))
            {
                wmsUseCase.Activate(state, wpfMap);
                WmsUseTiledLayerCheckBox.IsEnabled = state;
            }
            else if (sender.Equals(WmsUseTiledLayerCheckBox))
                wmsUseCase.UseTiledVersionOfGeobaseData = state;
            // Feature Layer rendering check boxes
            else if (sender.Equals(TrafficIncidentsCheckBox))
            {
                featureLayerUseCase.UseTrafficIncidents = state;
                FeatureLayerCheckBox_Changed(sender, state);
            }
            else if (sender.Equals(TruckAttributesCheckBox))
            {
                featureLayerUseCase.UseTruckAttributes = state;
                FeatureLayerCheckBox_Changed(sender, state);
            }
            else if (sender.Equals(RestrictionZonesCheckBox))
            {
                featureLayerUseCase.UseRestrictionZones = state;
                FeatureLayerCheckBox_Changed(sender, state);
            }
            else if (sender.Equals(PreferredRoutesCheckBox))
            {
                featureLayerUseCase.UsePreferredRoutes = state;
                FeatureLayerCheckBox_Changed(sender, state);
            }
            else if (sender.Equals(SpeedPatternsCheckBox))
            {
                featureLayerUseCase.UseSpeedPatterns = state;
                FeatureLayerCheckBox_Changed(sender, state);
            }
            else if (sender.Equals(ElementaryRoutingCheckBox))
            {
                if (state) routingUseCase = new RoutingUseCase(wpfMap); // Adding the xRoute elements.
                else routingUseCase.Remove();
            }
            else if (sender.Equals(XRouteDragAndDropCheckBox))
            {
                routingDragAndDropUseCase.Activate(state, wpfMap);

                if (dragDropInstruction == null)
                    dragDropInstruction = new DemoCenterInstructions(IntroGrid);

                if (state && !Properties.Settings.Default.DragDropIntroSkipped)
                    dragDropInstruction.StartDragDropIntro();
                dragDropInstruction.HandlerCallback = (id, start) =>
                {
                    switch (id)
                    {
                        case "_SkippedIntro":
                            Properties.Settings.Default.DragDropIntroSkipped = start;
                            Properties.Settings.Default.Save();
                            dragDropInstruction.isActive = false;
                            break;
                    }
                };
            }

            Cursor = oldCursor;
        }
        #endregion // use cases


        ///////////////////////////////////////////////////////////////////////
        #region Use case reset
        private void UseCaseReset()
        {
            var shrinkExpanders = new List<Expander>();
            shrinkExpanders.AddRange(new[] { HereExpander, ClusteringExpander, DifferentShapesExpander, AdditionalWPFMapExpander,
                GeoRSSExpander, MapMarketExpander, DataManagerExpander, ShapeFileExpander, WmsExpander, ManySymbolsExpander, UIThemeExpander, ProfileExpander,
                GeocodingExpander, RoutingExpander, XRouteDragAndDropExpander, FeatureLayerExpander, TourPlanningExpander });
            shrinkExpanders.ForEach(expander => expander.IsExpanded = false);

            var deactivateCheckBoxes = new List<CheckBox>();
            deactivateCheckBoxes.AddRange(new[] { HereCheckBox, ClusteringCheckBox, DifferentShapesCheckBox, ShapesUseLabelsCheckBox, 
                AddWpfMapCheckBox, GeoRSSCheckBox, MapMarketCheckBox, DataManagerCheckBox, ShapeFileCheckBox, WmsEnableCheckBox, WmsUseTiledLayerCheckBox,
                ManySymbolsCheckBox, ElementaryRoutingCheckBox, XRouteDragAndDropCheckBox, TrafficIncidentsCheckBox, TruckAttributesCheckBox, 
                PreferredRoutesCheckBox, RestrictionZonesCheckBox, SpeedPatternsCheckBox });
            deactivateCheckBoxes.ForEach(checkBox => checkBox.IsChecked = false);

            UIThemeComboBox.Text = "Default";
            ProfileComboBox.Text = "Silkysand";
            ProfileOKButton_OnClick(null, null);
            DeactivatedGeocodingRadiobutton.IsChecked = true;

            if (TourPlanningStart.Content.Equals("Cancel")) tourPlanningUseCase.StopPlanning();
            tourPlanningUseCase.Activate(false, wpfMap);
        }
        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Intros
        DemoCenterInstructions dragDropInstruction;
        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Own handling of 'system menu'
        /// <summary> Event handler for a double click on the main window. Double clicking adapts the size of the window. </summary>
        /// <param name="sender"> Sender of the MouseDoubleClick event. </param>
        /// <param name="e"> Event parameters. </param>
        private void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Only active is the background image or headline is hit, not if the map is hit.
            if (e.MouseDevice.DirectlyOver == null ||
                (!e.MouseDevice.DirectlyOver.Equals(BackgroundImage) && !e.MouseDevice.DirectlyOver.Equals(Headline)))
                return;

            // Change the size to normal size if the window is currently maximized.
            switch (WindowState)
            {
                case WindowState.Maximized: WindowState = WindowState.Normal; break;
                case WindowState.Normal: WindowState = WindowState.Maximized; break;
            }
        }

        /// <summary> Event handler for a left mouse click on the main window. If on the right position, a Drag&amp;Drop of the window is started. </summary>
        /// <param name="sender"> Sender of the MouseLeftButtonDown event. </param>
        /// <param name="e"> Event parameters. </param>
        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // A drag&drop of the main window is started if the mouse is positioned over the background image or the headline.
            if (e.MouseDevice.DirectlyOver != null && (e.MouseDevice.DirectlyOver.Equals(BackgroundImage)
                || e.MouseDevice.DirectlyOver.Equals(Headline)))
                DragMove();
        }

        /// <summary> Event handler for a click on the close button. Closes the whole application. </summary>
        /// <param name="sender"> Sender of the Click event. </param>
        /// <param name="e"> Event parameters. </param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary> Event handler for a click on the maximize button. Maximizes the application so that it is shown
        /// all over the screen. </summary>
        /// <param name="sender"> Sender of the Click event. </param>
        /// <param name="e"> Event parameters. </param>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                var gg = new GeometryGroup();
                gg.Children.Add(new RectangleGeometry(new Rect(new Point(0, 4), new Size(8, 8))));
                gg.Children.Add(new RectangleGeometry(new Rect(new Point(2, 6), new Size(4, 4))));
                var pf = new PathFigure {StartPoint = new Point(4, 0)};
                pf.Segments.Add(new LineSegment(new Point(12, 0), false));
                pf.Segments.Add(new LineSegment(new Point(12, 8), false));
                pf.Segments.Add(new LineSegment(new Point(8, 8), false));
                pf.Segments.Add(new LineSegment(new Point(8, 6), false));
                pf.Segments.Add(new LineSegment(new Point(10, 6), false));
                pf.Segments.Add(new LineSegment(new Point(10, 2), false));
                pf.Segments.Add(new LineSegment(new Point(6, 2), false));
                pf.Segments.Add(new LineSegment(new Point(6, 4), false));
                pf.Segments.Add(new LineSegment(new Point(4, 4), false));
                pf.Segments.Add(new LineSegment(new Point(4, 0), false));
                var pg = new PathGeometry();
                pg.Figures.Add(pf);
                gg.Children.Add(pg);
                MaximizeButton.Content = new Path { Fill = CloseButton.Foreground, Data = gg };
            }
            else
            {
                WindowState = WindowState.Normal;
                var gg = new GeometryGroup();
                gg.Children.Add(new RectangleGeometry(new Rect(new Point(0, 0), new Size(12, 12))));
                gg.Children.Add(new RectangleGeometry(new Rect(new Point(2, 2), new Size(8, 8))));
                MaximizeButton.Content = new Path { Fill = CloseButton.Foreground, Data = gg };
            }
        }

        /// <summary> Event handler for a click on the minimize button. Minimizes the application so that only an icon
        /// on the task bar is shown. </summary>
        /// <param name="sender"> Sender of the Click event. </param>
        /// <param name="e"> Event parameters. </param>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        #endregion

        /// <summary> Stores the old cursor before entering the logo with the map in order to restore it after leaving. </summary>
        private Cursor oldCursor;

        /// <summary> Event handler for entering the logo with the mouse. A hand cursor is shown to make known that
        /// there is a link below the logo which guides to the PTV homepage. </summary>
        /// <param name="sender"> Sender of the MouseEnter event. </param>
        /// <param name="e"> Event parameters. </param>
        private void Logo_MouseEnter(object sender, MouseEventArgs e)
        {
            oldCursor = Cursor;
            Cursor = Cursors.Hand;
        }

        /// <summary> Event handler for leaving the logo with the mouse. The old cursor is restored. </summary>
        /// <param name="sender"> Sender of the MouseLeave event. </param>
        /// <param name="e"> Event parameters. </param>
        private void Logo_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = oldCursor;
        }

        /// <summary> Event handler for a release of the left mouse button after a click on the logo image. The web page of PTV Group is opened. </summary>
        /// <param name="sender"> Sender of the MouseLeftButtonUp event. </param>
        /// <param name="e"> Event parameters. </param>
        private void Logo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ci = CultureInfo.InstalledUICulture;
            string url;
            switch (ci.ThreeLetterWindowsLanguageName)
            {
                case "DEU": url = "http://www.ptvgroup.com/de/"; break; // German
                case "NLD": url = "http://www.ptvgroup.com/nl/"; break; // Dutch
                case "FRA": url = "http://www.ptvgroup.com/fr/"; break; // French
                case "ITA": url = "http://www.ptvgroup.com/it/"; break; // Italian
                case "ESP": url = "http://www.ptvgroup.com/es/"; break; // Spanish
                case "POL": url = "http://www.ptvgroup.com/pl/"; break; // Polish
                case "CHN": url = "http://www.ptvgroup.com/cn/"; break; // Chinese
                case "CZE": url = "http://www.ptvgroup.com/cz/"; break; // Czech
                default:    url = "http://www.ptvgroup.com/en/"; break; // English as international alternative
            }

            var process = new System.Diagnostics.Process { StartInfo = { FileName = url, Verb = "Open", WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal } };
            process.Start();
        }

        private void FurtherSamples_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var process = new System.Diagnostics.Process { StartInfo = { FileName = "https://github.com/ptv-logistics/xservernet-bin", Verb = "Open", WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal } };
            process.Start();
        }

        private void XServerInternet_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var process = new System.Diagnostics.Process { StartInfo = { FileName = "http://xserver.ptvgroup.com/en-uk/cookbook/home/", Verb = "Open", WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal } };
            process.Start();
        }

        private void Forum_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var process = new System.Diagnostics.Process { StartInfo = { FileName = "http://xserver.ptvgroup.com/forum/viewforum.php?f=14", Verb = "Open", WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal } };
            process.Start();
        }

        /// <summary> Event handler preventing a bubbling up of the event. The event is marked to be handled and thus
        /// will be stopped here. So the upper elements do not get them. </summary>
        /// <param name="sender"> Sender of the event. </param>
        /// <param name="e"> Event parameters. </param>
        private void PreventBubblingUpHandler(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private string ownToken = "";

        private void XTokenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!XTokenToggleButton.IsChecked ?? false)
                ownToken = XTokenTextBox.Text;
        }

        private void XTokenToggleButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (XTokenToggleButton.IsChecked ?? false)
            {
                XTokenTextBox.Text = DefaultXServerInternetToken.Value + " (Trial-Key)";
                XTokenTextBox.IsReadOnly = true;
                XServerOKButton_Click(null, null);
            }
            else
            {
                XTokenTextBox.IsReadOnly = false;
                XTokenTextBox.Text = ownToken;
                XTokenTextBox.Focus();
            }
        }

        private void XTokenTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (XTokenToggleButton.IsChecked ?? false)
                XServerOKButton.Focus();
        }

        private void ResetDemos_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UseCaseReset();
        }
    }

    #region Instructions 
    internal class DemoCenterInstructions
    {
        public Action<string, bool> HandlerCallback;
        public bool isActive;
        private readonly Grid location;

        public DemoCenterInstructions(Grid location)
        {
            this.location = location;
        }

        public void StartIntro()
        {
            if (isActive)
                return;

            isActive = true;
            var introManager = new IntroManager(new[] {
                new Intro(Intro.PageType.First, "Start", "PTV xServer .NET\nDemo Center", 
                    "PTV xServer .NET is a SDK which adds xServer functionality to .NET client applications. The PTV xServer .NET Demo Center provides "
                    + "a set of code samples to build interactive map applications with the PTV xServers. The primary component is the WpfMap "
                    + "control which is part of the PTV xServer .NET SDK. This control allows building interactive map applications with PTV "
                    + "xServers. While the technology of it is based on Windows Presentation Foundation, the SDK also provides a FormsMap control "
                    + "for easy WinForms integration.\n\nClick Next to continue the short instruction."),
                new Intro(Intro.PageType.Normal, "Connection", "Connect to your\ndesired xServers", "In the xServer tab you have the opportunity to set the connection data for the usage of "
                    + "your own xServers. If you do not have any xServers available you can use a trial-key for PTV xServer internet by checking the key "
                    + "button. The trial-key can access all test clusters like 'eu-n-test' but expires periodically. So you have to download a fresh copy "
                    + "of the PTV xServer .NET Demo Center.\n\nUsers of PTV xServer internet are able to easily connect to a specific cluster by setting "
                    + "the URL to the needed cluster like 'eu-n'. Simply uncheck the key button to use your own PTV xServer internet token."),
                new Intro(Intro.PageType.Last, "ExploreUsecases", "Explore all your possibilities", "As the PTV xServer .NET Demo Center provides "
                    + "code samples for building interactive map applications with our PTV xServers, you can explore those samples as use cases "
                    + "located on the left side of Demo Center. Just expand one tab to read the description and start it.\n\n"
                    + "For example just activate the checkbox in the 'Elementary routing' tab to show a basic routing.\n\nClick Start to explore "
                    + "the PTV xServer .NET Demo Center on your own.")}, location);
            introManager.StartIntro();

            introManager.HandlerCallback = (id, start) => HandlerCallback(id, start);
        }

        public void StartDragDropIntro()
        {
            if (isActive)
                return;

            isActive = true;
            var introManager = new IntroManager(new[] {
                new Intro(Intro.PageType.Single, "Routing", "Drag & Drop Routing Demo", "To calculate a route you have to set at minimum two waypoints "
                    + "for the calculation. Simply right click in the map on the desired location and select 'Route from here' to set your start and "
                    + "click on 'Route to here' to set your destination. To extend the route you are able to add more waypoints by selecting "
                    + "'Add destination'.\nIf the minimum of two waypoints are set and the route shows up you have the oppurtunity to "
                    + "drag and drop the painted route as you like. After there are any changes to route segments they will be re-calculated.\n\n"
                    + "Click Start to explore the Drag and Drop routing use case.")}, location);
            introManager.StartIntro();

            introManager.HandlerCallback = (id, start) => HandlerCallback(id, start);
        }
    }
    #endregion
}
