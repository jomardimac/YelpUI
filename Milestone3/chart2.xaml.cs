using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;

namespace Milestone3 {
    /// <summary>
    /// Interaction logic for chart2.xaml
    /// </summary>
    public partial class chart2 : Page {
        SearchRes currBus;

        public chart2(SearchRes selectedBusiness) {
            InitializeComponent();
            currBus = selectedBusiness;
            populateChart();
        }

        private string buildConnString() {
            //return "Host=localhost; Username=postgres; Password=Jaysio102609!; Database=Milestone3";                    //Devon Connection 
            //return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8181";        //Jomar Connection
            return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8282";        //Jomar Laptop Connection
        }

        private void populateChart() {
            List<KeyValuePair<string, int>> myChartData = new List<KeyValuePair<string, int>>();

            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT COUNT(BID), postalcode FROM(SELECT DISTINCT BUSINESS.POSTALCODE, bid FROM BUSINESS WHERE BUSINESS.STATE = '" + currBus.State + "' AND BUSINESS.CITY = '" + currBus.City + "') zips group by postalcode";
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            myChartData.Add(new KeyValuePair<string, int>(reader.GetString(1), reader.GetInt32(0)));
                        }
                    }
                }
            }
            numBusChart.DataContext = myChartData;
        }
    }
}
