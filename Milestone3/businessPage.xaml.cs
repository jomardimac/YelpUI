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
using System.Security.Policy;
using Npgsql;

namespace Milestone3 {
    /// <summary>
    /// Interaction logic for businessPage.xaml
    /// </summary>
    public partial class businessPage : Page {

        User currUser;

        public businessPage(User newUser) {
            InitializeComponent();
            PopulateStates();
            PopulateCat();
            SearchResultsCols();

            currUser = newUser;
            
        }

        private string buildConnString() {
            return "Host=localhost; Username=postgres; Password=Jaysio102609!; Database=Milestone3";                    //Devon Connection 
            //return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8181";        //Jomar Connection
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
                        //filter by attributes:
                        query += CreditFilter();
                        query += ReservationFilter();
                        query += WheelChairFilter();
                        query += OutdoorFilter();
                        query += GKidsFilter();
                        query += GGroupsFilter();
                        query += DeliveryFilter();
                        query += ResTakeOutFilter();
                        query += WiFiFilter();
                        query += BikeFilter();

                        //filter by meals:
                        query += BreakfastFilter();
                        query += BrunchFilter();
                        query += LunchFilter();
                        query += DinnerFilter();
                        query += DessertFilter();
                        query += LatenightFilter();


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

        string CreditFilter() {
            string query = "";
            if (AccCredCard.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'BusinessAcceptsCreditCards' AND bval = 'True') r1 ON r1.bid = b.bid";
            }
            return query;
        }
        string ReservationFilter() {
            string query = "";
            if (TakesRes.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'BusinessAcceptsCreditCards' AND bval = 'True') res ON res.bid = b.bid";
            }
            return query;
        }
        string WheelChairFilter() {
            string query = "";
            if (WheelAcc.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'WheelchairAccessible' AND bval = 'True') wheel ON wheel.bid = b.bid";
            }
            return query;
        }
        string OutdoorFilter() {
            string query = "";
            if (OutdoorSeat.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'OutdoorSeating' AND bval = 'True') outdoor ON outdoor.bid = b.bid";
            }
            return query;
        }
        string GKidsFilter() {
            string query = "";
            if (GoodKid.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'GoodForKids' AND bval = 'True') gfk ON gfk.bid = b.bid";
            }
            return query;
        }
        string GGroupsFilter() {
            string query = "";
            if (GoodGroups.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'RestaurantsGoodForGroups' AND bval = 'True') gfg ON gfg.bid = b.bid";
            }
            return query;
        }
        string DeliveryFilter() {
            string query = "";
            if (Deliv.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'RestaurantsDelivery' AND bval = 'True') resDel ON resDel.bid = b.bid";
            }
            return query;
        }
        string ResTakeOutFilter() {
            string query = "";
            if (TakeOut.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'RestaurantsTakeOut' AND bval = 'True') resTakeOut ON resTakeOut.bid = b.bid";
            }
            return query;
        }
        string WiFiFilter() {
            string query = "";
            if (FWifi.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'WiFi' AND bval = 'free') wfi ON wfi.bid = b.bid";
            }
            return query;
        }
        string BikeFilter() {
            string query = "";
            if (BikePark.IsChecked.GetValueOrDefault()) {
                query += @" JOIN ( SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'BikeParking' AND bval = 'True') bike ON bike.bid = b.bid";
            }
            return query;
        }

        
        //MEALS TO DO LATER
        string BreakfastFilter() {
            string query = "";
            if (Breakfast.IsChecked.GetValueOrDefault()) {
                query += @" JOIN(SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'breakfast' AND bval = 'True') breakfast ON breakfast.bid = b.bid";
            }
            return query;
        }

        string BrunchFilter() {
            string query = "";
            if (Brunch.IsChecked.GetValueOrDefault()) {
                query += @" JOIN(SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'brunch' AND bval = 'True') brunch ON brunch.bid = b.bid";
            }
            return query;
        }

        string LunchFilter() {
            string query = "";
            if (Lunch.IsChecked.GetValueOrDefault()) {
                query += @" JOIN(SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'lunch' AND bval = 'True') lunch ON lunch.bid = b.bid";
            }
            return query;
        }

        string DinnerFilter() {
            string query = "";
            if (Dinner.IsChecked.GetValueOrDefault()) {
                query += @" JOIN(SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'dinner' AND bval = 'True') dinner ON dinner.bid = b.bid";
            }
            return query;
        }

        string DessertFilter() {
            string query = "";
            if (Dessert.IsChecked.GetValueOrDefault()) {
                query += @" JOIN(SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'dessert' AND bval = 'True') dessert ON dessert.bid = b.bid";
            }
            return query;
        }

        string LatenightFilter() {
            string query = "";
            if (LateNight.IsChecked.GetValueOrDefault()) {
                query += @" JOIN(SELECT b.bid FROM BUSINESSATT AS BA INNER JOIN BUSINESS AS B ON B.BID = BA.BID WHERE ATTNAME = 'latenight' AND bval = 'True') latenight ON latenight.bid = b.bid";
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
