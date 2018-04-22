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
                    cmd.CommandText = "SELECT distinct state FROM business";
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            stateBox.Items.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }

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

        //clicking on a state:
        private void stateBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            cityBox.Items.Clear();
            using (var conn = new NpgsqlConnection(buildConnString())) {
                conn.Open();
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    if (stateBox.SelectedValue != null) {
                        cmd.CommandText = "SELECT distinct city FROM business WHERE state = '" + stateBox.SelectedValue.ToString() + "'";
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                cityBox.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
        }



        private void Add_Click(object sender, RoutedEventArgs e) {

        }

        private void Remove_Click(object sender, RoutedEventArgs e) {

        }

        private void Search_Businesses_Click(object sender, RoutedEventArgs e) {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {

        }

        
    }
}
