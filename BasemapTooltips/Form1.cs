using System.Net;
using System;
using System.Windows.Forms;

namespace BasemapTooltips
{
    public partial class Form1 : Form
    {
        // Temporary demo token for xServer-internet
        // Use your own test-token or your on-premise xServer when migrating the code
        private const string token = "BB2A4CCB-65D9-4783-BCA6-529AD7A6F4C4";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            // initialize base map (for xServer internet)
            formsMap1.XMapUrl = "https://api-test.cloud.ptvgroup.com/xmap/ws/XMap";
            formsMap1.XMapCredentials = "xtok:" + token;

            // initialize our tool tip handler
            var tt = new BaseMapToolTips(formsMap1.WrappedMap) { ReverseLocatingFunc = ReverseLocate };
        }

        public static LocationTooltipInfo ReverseLocate(double x, double y)
        {
            try
            {
                var xlocate = new XLocateServiceReference.XLocateWSClient();
                xlocate.ClientCredentials.UserName.UserName = "xtok";
                xlocate.ClientCredentials.UserName.Password = token;

                var result = xlocate.findLocation(
                    new XLocateServiceReference.Location
                    {
                        coordinate =
                            new XLocateServiceReference.Point
                            {
                                point = new XLocateServiceReference.PlainPoint {x = x, y = y}
                            }
                    },
                    null, null, new[] { XLocateServiceReference.ResultField.XYN },
                    new XLocateServiceReference.CallerContext
                    {
                        wrappedProperties = new[]
                        {
                            new XLocateServiceReference.CallerContextProperty
                            {
                                key = "CoordFormat",
                                value = "OG_GEODECIMAL"
                            }
                        }
                    }
                    );

                if (result.wrappedResultList == null || result.wrappedResultList.Length < 1)
                    return null;

                var address = result.wrappedResultList[0];
                return new LocationTooltipInfo
                {
                    Country = (address.country + " " + address.state).Trim(),
                    City = (address.postCode + " " + address.city).Trim(),
                    Street = (address.street + " " + address.houseNumber).Trim()
                };
            }
            catch (WebException)
            {
                return null;
            }
        }
    }
}
