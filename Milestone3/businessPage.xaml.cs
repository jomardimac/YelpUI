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
using System.ComponentModel;
using Npgsql;

namespace Milestone3 {
    /// <summary>
    /// Interaction logic for businessPage.xaml
    /// </summary>
    public partial class businessPage : Page {

        User currUser;
        SearchRes selectedBusiness;

        public businessPage(User newUser) {
            InitializeComponent();
            PopulateStates();
            PopulateCat();
            populateStars();
            SearchResultsCols();

            PopulateTimeFilters();
            currUser = newUser;
            FillSorting();
        }

        private void populateStars() {
            selectedRatingDropBox.Items.Add(1);
            selectedRatingDropBox.Items.Add(2);
            selectedRatingDropBox.Items.Add(3);
            selectedRatingDropBox.Items.Add(4);
            selectedRatingDropBox.Items.Add(5);
        }

        private string buildConnString() {
            //return "Host=localhost; Username=postgres; Password=Jaysio102609!; Database=Milestone3";                    //Devon Connection 
            //return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8181";        //Jomar Connection
            return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8282";        //Jomar Laptop Connection
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
        /***************************************************Open Time Filters:***************************************************/

        void PopulateTimeFilters() {
            //day of week:
            day_weekBox.Items.Add("Sunday");
            day_weekBox.Items.Add("Monday");
            day_weekBox.Items.Add("Tuesday");
            day_weekBox.Items.Add("Wednesday");
            day_weekBox.Items.Add("Thursday");
            day_weekBox.Items.Add("Friday");
            day_weekBox.Items.Add("Saturday");

            //open close time: 
            for (int i = 0; i <= 23; i++) {
                if (i < 9) {
                    string openquery = "0#time#:00";
                    openquery = openquery.Replace("#time#", i.ToString());
                    FromBox.Items.Add(openquery);
                    ToBox.Items.Add(openquery);
                }
                else {
                    string openquery = "#time#:00";
                    openquery = openquery.Replace("#time#", i.ToString());
                    FromBox.Items.Add(openquery);
                    ToBox.Items.Add(openquery);
                }
            }
        }

        /***************************************************Business Attributes Filters:***************************************************/
        string PriceFiltering() {
            string query = "";
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

        //MEALS:
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

        /***************************************************Sorting Methods:***************************************************/

        void FillSorting () {
            sortResDropBox.Items.Add("Business Name (default sort)");
            sortResDropBox.Items.Add("Highest Ratings (stars)");
            sortResDropBox.Items.Add("Most Reviewed");
            sortResDropBox.Items.Add("Best Review Rating (highest avg review rating)");
            sortResDropBox.Items.Add("Most Check-Ins");
            sortResDropBox.Items.Add("Nearest");
        }

        string CheckSorting() {
            var query = " ORDER BY bname ";
            if (sortResDropBox.SelectedItem == null) {
                return query;
            }
            else if (sortResDropBox.SelectedItem.ToString() == "Business Name (default sort)") {
                query = " ORDER BY bname ";
            }
            else if (sortResDropBox.SelectedItem.ToString() == "Highest Ratings (stars)") {
                query = " ORDER BY stars ";
            }
            else if (sortResDropBox.SelectedItem.ToString() == "Most Reviewed") {
                query = " ORDER BY reviewCount ";
            }
            else if (sortResDropBox.SelectedItem.ToString() == "Best Review Rating (highest avg review rating)") {
                query = " ORDER BY reviewRating ";
            }
            else if (sortResDropBox.SelectedItem.ToString() == "Most Check-Ins") {
                query = " ORDER BY numCheckins ";
            }

            else if (sortResDropBox.SelectedItem.ToString() == "Nearest") {
                //(searchResGrid.ItemsSource as DataGrid).Sorting
                /* MyDataGrid.ItemsSource = DataContext.RowItems.OrderBy(p => p.Score).ToList();*/
                //column #4:
                
                foreach (var items in searchResGrid.Columns) {
                    items.SortDirection = null;
                }

                string propName = searchResGrid.Columns[4].Header.ToString();
                searchResGrid.Items.SortDescriptions.Clear();
                searchResGrid.Items.SortDescriptions.Add(new SortDescription(searchResGrid.Columns[4].SortMemberPath, ListSortDirection.Ascending));
                foreach(var items in searchResGrid.Columns) {
                    items.SortDirection = null;
                }
                searchResGrid.Columns[4].SortDirection = ListSortDirection.Ascending;
                
            }
            
            return query;
        }
        private void applySortDirection(ListSortDirection listSortDirection) {
            
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
                            @"SELECT distinct b.bname, b.address, b.city, b.state, b.stars, b.reviewcount, b.reviewRating, b.numcheckins,b.bid, b.latitude, b.longitude FROM business as b ";

                        int n = 0;
                        foreach (var items in CatList()) {
                            query +=
                                @" JOIN (SELECT distinct business.bid FROM business, businesscat WHERE business.bid = businesscat.bid AND state = '#state#' AND city = '#city#' AND postalcode = '#zip#' AND catname = '#items#') t#n# ON t#n#.bid = b.bid";
                            query = query.Replace("#n#", n.ToString());
                            query = query.Replace("#items#", items);
                            n++;
                        }


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

                        

                        //open close time:
                        if(day_weekBox.SelectedValue == null && FromBox.SelectedValue == null && ToBox.SelectedValue == null) {

                        }
                        else if(day_weekBox.SelectedValue != null) {
                            if (ToBox.SelectedValue != null || FromBox.SelectedValue != null) {
                                query += @" JOIN (SELECT distinct b.bid FROM business b, bushours as bh WHERE b.bid = bh.bid AND bh.h_dayofweek = '#personalday#' AND (BH.open <= '#from#' AND BH.close >= '#to#' OR (BH.open = '00:00' AND BH.close = '00:00')))bidness on bidness.bid = b.bid ";
                                query = query.Replace("#personalday#", day_weekBox.SelectedValue.ToString());
                                query = query.Replace("#from#",FromBox.SelectedValue.ToString());
                                query = query.Replace("#to#",ToBox.SelectedValue.ToString());
                            }
                        }
                        else {
                            
                        }

                        //sorting:
                        query += @" WHERE b.state = '#state#' AND b.city = '#city#' AND b.postalcode = '#zip#' ";
                        query += CheckSorting();
                        query = query.Replace("#state#", stateBox.SelectedValue.ToString());
                        query = query.Replace("#city#", cityBox.SelectedValue.ToString());
                        query = query.Replace("#zip#", zipBox.SelectedValue.ToString());
                        cmd.CommandText = query;
                        using (var reader = cmd.ExecuteReader()) {
                            int j = 0;
                            while (reader.Read()) {
                                j++;
                                double bLat = reader.GetDouble(9);
                                double bLong = reader.GetDouble(10);
                                double distance = getDistance(currUser.lat, currUser.longi, bLat, bLong);
                                distance = Math.Round(distance, 2);
                                searchResGrid.Items.Add(new SearchRes() {
                                    busName = reader.GetString(0),
                                    Address = reader.GetString(1),
                                    City = reader.GetString(2),
                                    State = reader.GetString(3),
                                    Stars = reader.GetDouble(4),
                                    NumRev = reader.GetInt32(5),
                                    AvgRev = reader.GetDouble(6),
                                    TotalCheckin = reader.GetInt32(7),
                                    BID = reader.GetString(8),
                                    Distance = distance
                                });
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

            DataGridTextColumn BIDCol = new DataGridTextColumn {
                Header = "BID",
                Binding = new Binding("BID")
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
            searchResGrid.Columns.Add(BIDCol);
        }

        private void Search_Businesses_Click(object sender, RoutedEventArgs e) {
            PopulateSearchResults();
            searchResGrid.Items.Refresh();
        }
        

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {

        }

        private void businessSelected(object sender, SelectionChangedEventArgs e) {
            if (searchResGrid.SelectedItem != null) {
                selectedBusiness = (SearchRes)searchResGrid.SelectedItem;
                businessNameLabel.Text = selectedBusiness.busName;
            }
        }

        private void addCheckinClicked(object sender, RoutedEventArgs e) {
            if (selectedBusiness == null) {
                MessageBox.Show("No Business Selected.");
            } else {
                TimeSpan startMorning = new TimeSpan(6,0,0);
                TimeSpan endMorning = new TimeSpan(12,0,0);
                TimeSpan endAfteroon = new TimeSpan(17,0,0);
                TimeSpan endEvening = new TimeSpan(23, 0, 0);
                TimeSpan now = DateTime.Now.TimeOfDay;

                using (var conn = new NpgsqlConnection(buildConnString())) {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand()) {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT * FROM CHECKIN WHERE BID = '" + selectedBusiness.BID + "' AND CI_DAYOFWEEK = '" + DateTime.Now.DayOfWeek.ToString() + "'";
                        using (var reader = cmd.ExecuteReader()) {
                            if (!reader.HasRows) {
                                var conn2 = new NpgsqlConnection(buildConnString());
                                conn2.Open();
                                var cmd2 = new NpgsqlCommand();
                                cmd2.Connection = conn2;

                                //No tuple found for specified day. Need to insert a new tuple for it.
                                cmd2.CommandText = "INSERT INTO checkin (BID, CI_DayOfWeek, morning, afternoon, evening, night) VALUES ('" + selectedBusiness.BID + "','" + DateTime.Now.DayOfWeek + "',0,1,0,0)";                //Insert a new tuple for selected business with current day and 1 afternoon checkin.
                                cmd2.ExecuteReader();    
                            } else {
                                //Tuple already exists. Just update and increment whatever value is needed.
                                var conn2 = new NpgsqlConnection(buildConnString());
                                conn2.Open();
                                var cmd2 = new NpgsqlCommand();
                                cmd2.Connection = conn2;

                                cmd2.CommandText = "UPDATE CHECKIN SET AFTERNOON = AFTERNOON + 1 WHERE BID = '" + selectedBusiness.BID + "' AND CI_DAYOFWEEK = '" + DateTime.Now.DayOfWeek + "'";
                                cmd2.ExecuteReader();
                            }
                        }

                        PopulateSearchResults();
                    }
                }
            }
        }

        private static Random random = new Random();
        public static string RandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void addReviewClicked(object sender, RoutedEventArgs e) {
            bool validID = false;
            string possibleID;
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    
                    while (!validID) {
                        possibleID = RandomString(30);
                        var conn2 = new NpgsqlConnection(buildConnString());
                        conn2.Open();
                        var cmd2 = new NpgsqlCommand();
                        cmd2.Connection = conn2;
                        cmd2.CommandText = "SELECT * FROM REVIEW WHERE REVIEWID = '" + possibleID + "'";
                        using (var reader = cmd2.ExecuteReader()) {
                            if (!reader.HasRows) {
                                validID = true;
                            } 
                        }
                    }

                    string reviewID = RandomString(30);
                    cmd.CommandText = "INSERT INTO REVIEW(REVIEWID, UID, BID, STARS, REVIEWDATE, NOTES, USEFUL, FUNNY, COOL) VALUES('" + reviewID + "','" + currUser.uid + "','" + selectedBusiness.BID + "'," + selectedRatingDropBox.SelectedValue.ToString() + ",'" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + reviewNotesBox.Text + "',0,0,0)";
                    cmd.ExecuteReader();
                    PopulateSearchResults();
                }
            }
        }

        //TY StackOverflow
        public double getDistance(double lat1, double lon1, double lat2, double lon2) {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return dist;
        }

        private void showReviewsClicked(object sender, RoutedEventArgs e) {
            if (selectedBusiness == null) {
                MessageBox.Show("No Business Selected. Please Select One.");
            } else {
                Page3 p = new Page3(selectedBusiness);
                this.NavigationService.Navigate(p);
            }
        }
    }
}
