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
    /// Interaction logic for Page3.xaml
    /// </summary>
    public partial class Page3 : Page {

        SearchRes currentBusiness;

        public Page3(SearchRes selectedBusiness) {
            InitializeComponent();
            populateCols();
            currentBusiness = selectedBusiness;
            populateTable();
        }

        private string buildConnString() {
            //return "Host=localhost; Username=postgres; Password=Jaysio102609!; Database=Milestone3";                    //Devon Connection 
            //return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8181";        //Jomar Connection
            return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8282";        //Jomar Laptop Connection
        }

        public void populateCols() {
            DataGridTextColumn nameCol = new DataGridTextColumn();
            nameCol.Header = "Name";
            nameCol.Binding = new Binding("name");
            businessReviews.Columns.Add(nameCol);

            DataGridTextColumn dateCol = new DataGridTextColumn();
            dateCol.Header = "Date";
            dateCol.Binding = new Binding("date");
            businessReviews.Columns.Add(dateCol);

            DataGridTextColumn notesCol = new DataGridTextColumn();
            notesCol.Header = "Notes";
            notesCol.Binding = new Binding("text");
            businessReviews.Columns.Add(notesCol);

            DataGridTextColumn starsCol = new DataGridTextColumn();
            starsCol.Header = "Stars";
            starsCol.Binding = new Binding("stars");
            businessReviews.Columns.Add(starsCol);
        }

        public void populateTable() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT U.NAME, R.REVIEWDATE, R.STARS, R.NOTES FROM BUSINESS AS B INNER JOIN REVIEW AS R ON R.BID = B.BID INNER JOIN DBUSER AS U ON U.UID = R.UID WHERE B.BID = '" + currentBusiness.BID + "'";
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            businessReviews.Items.Add(new busReview() {name = reader.GetString(0), date = reader.GetDate(1).ToString(), stars = reader.GetInt32(2), text = reader.GetString(3)});
                        }
                    }
                }
            }
        }
    }
}
