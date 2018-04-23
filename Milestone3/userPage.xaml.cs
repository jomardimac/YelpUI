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
    /// Interaction logic for userPage.xaml
    /// </summary>
    public partial class userPage : Page {

        User currUser;

        public userPage() {
            InitializeComponent();
            addFriendsListCols();
            addFriendsReviewsCols();
        }

        private string buildConnString() {
            return "Host=localhost; Username=postgres; Password=Jaysio102609!; Database=Milestone3";                    //Devon Connection 
            //return "Host=localhost; Username=postgres; Password=db2018; Database=yelpdb; port=8181";        //Jomar Connection
        }


        private void nameEnterPressed(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                possibleUIDS.Items.Clear();
                using (var conn = new NpgsqlConnection(buildConnString())) {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand()) {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT dbuser.uid FROM dbuser WHERE dbuser.name = '" + nameBox.Text + "'";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                possibleUIDS.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
        }

        private void uidChosen(object sender, SelectionChangedEventArgs e) {
            friendsList.Items.Clear();
            friendsReviews.Items.Clear();

            populateUserInfo();
            populateFriendsList();
            populateFriendsReviews();
        }

        void populateUserInfo() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (possibleUIDS.SelectedValue != null) {
                        cmd.CommandText = "SELECT dbuser.name, dbuser.avgstars, dbuser.fans, dbuser.yelping_since, dbuser.funny, dbuser.cool, dbuser.useful, dbuser.uid FROM DBUSER WHERE DBUSER.UID = '" + possibleUIDS.SelectedValue.ToString() + "'";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                usersNameBox.Text = reader.GetString(0);
                                userStarsBox.Text = reader.GetDouble(1).ToString();
                                userFansBox.Text = reader.GetString(2).ToString();
                                userSinceBox.Text = reader.GetDate(3).ToString();
                                userFunny.Text = reader.GetString(4).ToString();
                                userCool.Text = reader.GetString(5).ToString();
                                userUseful.Text = reader.GetString(6).ToString();

                                currUser = new User(reader.GetString(0), reader.GetString(7), 0.0, 0.0);
                            }
                        }
                    }
                }
            }
        }

        //Populate the friendsLIst datagrid with information about the friends of the current selected user.
        void populateFriendsList() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (possibleUIDS.SelectedValue != null) {
                        cmd.CommandText = "SELECT DBUSER.NAME, DBUSER.AVGSTARS, DBUSER.YELPING_SINCE, DBUSER.UID FROM DBUSER WHERE DBUSER.UID IN (SELECT FRIENDS.FID FROM FRIENDS WHERE UID = '" + possibleUIDS.SelectedValue.ToString() + "')";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                friendsList.Items.Add(new Friend() { name = reader.GetString(0), avgStars = reader.GetDouble(1).ToString(), yelpingSince = reader.GetDate(2).ToString(), fid = reader.GetString(3) });
                            }
                        }
                    }
                }
            }
        }

        //Populate the friendsReviews datagrid with reviews written by the selected users friends.
        void populateFriendsReviews() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (possibleUIDS.SelectedValue != null) {
                        cmd.CommandText = "SELECT U.NAME, B.BNAME, B.CITY, R.NOTES, R.STARS, R.FUNNY, R.COOL, R.USEFUL FROM REVIEW AS R INNER JOIN BUSINESS AS B ON B.BID = R.BID INNER JOIN DBUSER AS U on U.UID = R.UID WHERE U.UID IN(SELECT FRIENDS.FID FROM FRIENDS WHERE FRIENDS.UID = '" + possibleUIDS.SelectedValue.ToString() + "')";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                friendsReviews.Items.Add(new Review() { friendName = reader.GetString(0), businessName = reader.GetString(1), businessCity = reader.GetString(2), reviewText = reader.GetString(3), reviewStars = reader.GetInt32(4), reviewFunny = reader.GetInt32(5), reviewCool = reader.GetInt32(6), reviewUseful = reader.GetInt32(7) });
                            }
                        }
                    }
                }
            }
        }

        //Add the needed columsn in the friends' reviews datagrid view.
        void addFriendsReviewsCols() {
            DataGridTextColumn nameCol = new DataGridTextColumn();
            nameCol.Header = "Name";
            nameCol.Binding = new Binding("friendName");
            friendsReviews.Columns.Add(nameCol);

            DataGridTextColumn bnameCol = new DataGridTextColumn();
            bnameCol.Header = "Business Name";
            bnameCol.Binding = new Binding("businessName");
            friendsReviews.Columns.Add(bnameCol);

            DataGridTextColumn cityCol = new DataGridTextColumn();
            cityCol.Header = "City";
            cityCol.Binding = new Binding("businessCity");
            friendsReviews.Columns.Add(cityCol);

            DataGridTextColumn notesCol = new DataGridTextColumn();
            notesCol.Header = "Notes";
            notesCol.Binding = new Binding("reviewText");
            friendsReviews.Columns.Add(notesCol);

            DataGridTextColumn starsCol = new DataGridTextColumn();
            starsCol.Header = "Stars";
            starsCol.Binding = new Binding("reviewStars");
            friendsReviews.Columns.Add(starsCol);

            DataGridTextColumn funnyCol = new DataGridTextColumn();
            funnyCol.Header = "Funny";
            funnyCol.Binding = new Binding("reviewFunny");
            friendsReviews.Columns.Add(funnyCol);

            DataGridTextColumn coolCol = new DataGridTextColumn();
            coolCol.Header = "Cool";
            coolCol.Binding = new Binding("reviewCool");
            friendsReviews.Columns.Add(coolCol);

            DataGridTextColumn usefulCol = new DataGridTextColumn();
            usefulCol.Header = "Useful";
            usefulCol.Binding = new Binding("reviewUseful");
            friendsReviews.Columns.Add(usefulCol);

        }

        private void removeFriendClicked(object sender, RoutedEventArgs e) {
            if (friendsList.SelectedItem == null) {
                MessageBox.Show("No friend selected.");
            } else {
                Friend friend2Remove = (Friend)friendsList.SelectedItem;
                using (var conn = new NpgsqlConnection(buildConnString())) {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand()) {
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE FROM FRIENDS WHERE UID = '" + possibleUIDS.SelectedValue.ToString() + "' AND FID = '" + friend2Remove.fid + "'";
                        friendsList.Items.Clear();
                        friendsReviews.Items.Clear();
                        cmd.ExecuteReader();
                        populateFriendsList();
                        populateFriendsReviews();
                    }
                }
            }
        }

        private void toBusiness_Click(object sender, RoutedEventArgs e) {
            if (currUser != null) {
                businessPage newBusiness = new businessPage(currUser);
                this.NavigationService.Navigate(newBusiness);
            } else {
                MessageBox.Show("No User Selected.");
            }
        }

        //Add the columns needed in the friendsList datagrid view.
        void addFriendsListCols() {
            DataGridTextColumn nameCol = new DataGridTextColumn();
            nameCol.Header = "Name";
            nameCol.Binding = new Binding("name");
            nameCol.Width = 75;
            friendsList.Columns.Add(nameCol);

            DataGridTextColumn avgStarsCol = new DataGridTextColumn();
            avgStarsCol.Header = "Avg. Stars";
            avgStarsCol.Binding = new Binding("avgStars");
            avgStarsCol.Width = 75;
            friendsList.Columns.Add(avgStarsCol);

            DataGridTextColumn sinceCol = new DataGridTextColumn();
            sinceCol.Header = "Yelping Since";
            sinceCol.Binding = new Binding("yelpingSince");
            sinceCol.Width = 150;
            friendsList.Columns.Add(sinceCol);

            DataGridTextColumn fidCol = new DataGridTextColumn();
            fidCol.Header = "FID";
            fidCol.Binding = new Binding("fid");
            friendsList.Columns.Add(fidCol);
        }
    }
}
