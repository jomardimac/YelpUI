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
        public userPage() {
            InitializeComponent();
            addFriendsListCols();
        }

        private string buildConnString() {
            return "Host=localhost; Username=postgres; Password=Jaysio102609!; Database=Milestone3";
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
            populateUserInfo();
            populateFriendsList();
        }

        void populateUserInfo() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (possibleUIDS.SelectedValue != null) {
                        cmd.CommandText = "SELECT dbuser.name, dbuser.avgstars, dbuser.fans, dbuser.yelping_since, dbuser.funny, dbuser.cool, dbuser.useful FROM DBUSER WHERE DBUSER.UID = '" + possibleUIDS.SelectedValue.ToString() + "'";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                usersNameBox.Text = reader.GetString(0);
                                userStarsBox.Text = reader.GetDouble(1).ToString();
                                userFansBox.Text = reader.GetString(2).ToString();
                                userSinceBox.Text = reader.GetDate(3).ToString();
                                userFunny.Text = reader.GetString(4).ToString();
                                userCool.Text = reader.GetString(5).ToString();
                                userUseful.Text = reader.GetString(6).ToString();
                            }
                        }
                    }
                }
            }
        }

        void populateFriendsList() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (possibleUIDS.SelectedValue != null) {
                        cmd.CommandText = "SELECT DBUSER.NAME, DBUSER.AVGSTARS, DBUSER.YELPING_SINCE FROM DBUSER WHERE DBUSER.UID IN (SELECT FRIENDS.FID FROM FRIENDS WHERE UID = '" + possibleUIDS.SelectedValue.ToString() + "')";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                friendsList.Items.Add(new Friend() { name = reader.GetString(0), avgStars = reader.GetDouble(1).ToString(), yelpingSince = reader.GetDate(2).ToString() });
                            }
                        }
                    }
                }
            }

            //SELECT DBUSER.NAME, DBUSER.AVGSTARS, DBUSER.YELPING_SINCE FROM DBUSER WHERE DBUSER.UID IN(SELECT FRIENDS.FID FROM FRIENDS WHERE UID = 'a-PyYzTVrisNpDQ0bwbyvA');
        }

        void populateFriendsReviews() {
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (possibleUIDS.SelectedValue != null) {
                        cmd.CommandText = "SELECT U.NAME, B.BNAME, R.NOTES, R.STARS, R.FUNNY, R.COOL, R.USEFUL FROM REVIEW AS R INNER JOIN BUSINESS AS B ON B.BID = R.BID INNER JOIN DBUSER AS U on U.UID = R.UID WHERE U.UID IN(SELECT FRIENDS.FID FROM FRIENDS WHERE FRIENDS.UID = '" + possibleUIDS.SelectedValue.ToString() + "')";


                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                
                            }
                        }
                    }
                }
            }
        }

        void addFriendsReviewsCols() {
            DataGridTextColumn nameCol = new DataGridTextColumn();
            nameCol.Header = "Name";
            nameCol.Binding = new Binding("friendName");
            friendsReviews.Columns.Add(nameCol);

            DataGridTextColumn bnameCol = new DataGridTextColumn();
            bnameCol.Header = "Business Name";
            bnameCol.Binding = new Binding("businessName");
            friendsReviews.Columns.Add(bnameCol);


        }

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
        }
    }
}
