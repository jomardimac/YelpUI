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
using System.Collections.ObjectModel;
using Npgsql;

namespace Milestone3 {
    /// <summary>
    /// Interaction logic for businessPage.xaml
    /// </summary>
    public partial class businessPage : Page {
        public businessPage() {
            InitializeComponent();
            populateStates();
            populateCat();
            SearchResultsCols();
        }

        private string buildConnString() {
            return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8181";
        }

        //Populate states:
        void populateStates() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT distinct state FROM business ORDER BY state;";
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            stateBox.Items.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }


        /***************************************************STATE CITY ZIP METHODS***************************************************/

        //clicking on a state adds city:
        private void stateBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            cityBox.Items.Clear();
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (stateBox.SelectedValue != null) {
                        cmd.CommandText = "SELECT distinct city FROM business WHERE state = '" + stateBox.SelectedValue.ToString() + "' ORDER BY city";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                cityBox.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
        }
        //Clicking on the city adds zip
        private void cityBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            zipBox.Items.Clear();
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (cityBox.SelectedValue != null) {
                        cmd.CommandText = "SELECT distinct postalcode FROM business WHERE city = '" + cityBox.SelectedValue.ToString() + "' ORDER BY postalcode;";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                zipBox.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
        }

        /***************************************************BUSINESS CAT METHODS***************************************************/

        //populate categories:
        void populateCat() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT distinct catname FROM businesscat ORDER BY catname;";
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            buscatBox.Items.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }
        //Add Categories on the textbox aka listOfBusCat
        private void Add_Click(object sender, RoutedEventArgs e) {
            if (buscatBox.SelectedValue != null) {
                listOfBusCat.Items.Add(buscatBox.SelectedValue.ToString());
            }
        }

        private List<String> catList() {
            List<String> str = new List<string>();
            foreach (var item in listOfBusCat.Items) {
               str.Add(item.ToString());
            }
            return str;
        }

        //Remove Categories from the textbox:
        private void Remove_Click(object sender, RoutedEventArgs e) {
            if (listOfBusCat.SelectedValue != null) {
                listOfBusCat.Items.Remove(listOfBusCat.SelectedValue);
            }
        }

        /***************************************************SEARCH RESULT METHODS***************************************************/
        //Populate the populate teh whole search.
        void PopulateSearchResults() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (cityBox.SelectedValue != null) {
                        String query =
                            @"SELECT distinct b.bname, b.address, b.city, b.state, b.stars FROM business as b, businesscat as bc    WHERE b.bid = bc.bid AND b.state = '#state#' AND b.city = '#city#' AND b.postalcode = '#zip#'";
                        foreach (var items in catList()) {
                            query += " AND bc.catname = '" + items.ToString() + "' ";
                        }

                        query += "ORDER BY bname";
                        query = query.Replace("#state#", stateBox.SelectedValue.ToString());
                        query = query.Replace("#city#", cityBox.SelectedValue.ToString());
                        query = query.Replace("#zip#", zipBox.SelectedValue.ToString());
                        cmd.CommandText = query;
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                searchResGrid.Items.Add(new SearchRes(){Address =  reader.GetString(1), busName = reader.GetString(0), City = reader.GetString(2), State = reader.GetString(3)});
                            }
                        }
                    }
                }
            }
        }

        //Add the needed columsn in the friends' reviews datagrid view.
        void SearchResultsCols() {
            DataGridTextColumn busNameCol = new DataGridTextColumn {
                Header = "BusinessName",
                Binding = new Binding("busName")
            };
            
            DataGridTextColumn addrNameCol = new DataGridTextColumn {
                Header = "Address",
                Binding = new Binding("Address")
            };

            DataGridTextColumn cityCol = new DataGridTextColumn {
                Header = "City",
                Binding = new Binding("City")
            };
            DataGridTextColumn stateCol = new DataGridTextColumn {
                Header = "State",
                Binding = new Binding("State")
            };
            DataGridTextColumn distanceCol = new DataGridTextColumn {
                Header = "Distance\n(miles)",
                Binding = new Binding("Distance")
            };
            DataGridTextColumn starsCol = new DataGridTextColumn {
                Header = "Stars",
                Binding = new Binding("Stars")
            };

            DataGridTextColumn numRevCol = new DataGridTextColumn {
                Header = "# of\nRev",
                Binding = new Binding("NumRev")
            };

            DataGridTextColumn avgRevCol = new DataGridTextColumn {
                Header = "Avg\nReview\nRating",
                Binding = new Binding("AvgRev")
            };

            DataGridTextColumn totalCheckinCol = new DataGridTextColumn {
                Header = "Total\nCheckins",
                Binding = new Binding("TotalCheckin")
            };

            searchResGrid.Columns.Add(busNameCol);
            searchResGrid.Columns.Add(addrNameCol);
            searchResGrid.Columns.Add(cityCol);
            searchResGrid.Columns.Add(stateCol);
            searchResGrid.Columns.Add(distanceCol);
            searchResGrid.Columns.Add(starsCol);
            searchResGrid.Columns.Add(numRevCol);
            searchResGrid.Columns.Add(avgRevCol);
            searchResGrid.Columns.Add(totalCheckinCol);
        }

        private void Search_Businesses_Click(object sender, RoutedEventArgs e) {
            PopulateSearchResults();
        }
        

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {

        }

        
    }
}
