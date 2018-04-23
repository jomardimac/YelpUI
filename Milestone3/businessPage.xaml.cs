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
            PopulateStates();
            PopulateCat();
            SearchResultsCols();
        }

        private string buildConnString() {
            //return "Host=localhost; Username=postgres; Password=Jaysio102609!; Database=Milestone3";                    //Devon Connection 
            return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8181";        //Jomar Connection
        }

        //Populate states:
        private void PopulateStates() {
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
        private void PopulateCat() {
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

        private List<String> CatList() {
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
            searchResGrid.Items.Clear();
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;

                    //Normal location filtering:
                    if (stateBox.SelectedValue != null) {
                        String query =
                            @"SELECT distinct b.bname, b.address, b.city, b.state, b.stars, b.reviewcount, b.reviewRating, b.numcheckins FROM business as b JOIN businesscat bc on b.bid = bc.bid AND b.state = '#state#' AND b.city = '#city#' AND b.postalcode = '#zip#'";

                        int n = 0;
                        foreach (var items in CatList()) {
                            query +=
                                @" JOIN (SELECT distinct business.bid FROM business, businesscat WHERE business.bid = businesscat.bid AND state = '#state#' AND city = '#city#' AND postalcode = '#zip#' AND catname = '#items#') t#n# ON t#n#.bid = b.bid";
                            query = query.Replace("#n#", n.ToString());
                            query = query.Replace("#items#", items);
                            n++;
                        }

                        
                        query = query.Replace("#state#", stateBox.SelectedValue.ToString());
                        query = query.Replace("#city#", cityBox.SelectedValue.ToString());
                        query = query.Replace("#zip#", zipBox.SelectedValue.ToString());
                        


                        //Filtering by price:
                        query += PriceFiltering();

                        query += " ORDER BY bname";
                        cmd.CommandText = query;
                        using (var reader = cmd.ExecuteReader()) {
                            int j = 0;
                            while (reader.Read()) {
                                j++;
                                searchResGrid.Items.Add(new SearchRes() {
                                    busName = reader.GetString(0),
                                    Address = reader.GetString(1),
                                    City = reader.GetString(2),
                                    State = reader.GetString(3),
                                    Stars = reader.GetDouble(4),
                                    NumRev = reader.GetInt32(5),
                                    AvgRev = reader.GetDouble(6),
                                    TotalCheckin = reader.GetInt32(7)
                                });
                            }
                        }

                    }


                    
                }
            }
        }

        string PriceFiltering() {
            string query = "";
            bool orstatement = false;
            var p1 = price1.IsChecked.GetValueOrDefault() == true;
            var p2 = price2.IsChecked.GetValueOrDefault() == true;
            var p3 = price3.IsChecked.GetValueOrDefault() == true;
            var p4 = price4.IsChecked.GetValueOrDefault() == true;

            //check if more than 2 are checked:
            if (p1 || p2 || p3 || p4) {
                query += @" JOIN (SELECT a.bid from BUSINESSATT a INNER JOIN BUSINESS b ON b.bid = a.bid WHERE attname = 'RestaurantsPriceRange2' AND (";
                if (p1) {
                    query += "CAST(BVAL AS INTEGER) = 1 ";
                }
                
                if (p2) {
                    if (p1) {
                        query += " OR ";
                    }
                    query += " CAST(BVAL AS INTEGER) = 2 ";
                }

                
                if (p3) {
                    if (p1 || p2) {
                        query += " OR ";
                    }
                    query += " CAST(BVAL AS INTEGER) = 3 ";
                }

                
                if (p4) {
                    if (p1 || p2 || p3) {
                        query += " OR ";
                    }
                    query += " CAST(BVAL AS INTEGER) = 4 ";
                }

                query += ") ) r on r.bid = b.bid";
            }
            
            return query;
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
