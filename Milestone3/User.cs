using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milestone3 {
    public class User {
        public string name;
        public string uid;
        public double lat;
        public double longi;

        public User(string newName, string newUID, double newLat, double newLongi) {
            name = newName;
            uid = newUID;
            lat = newLat;
            longi = newLongi;
        }
    }
}
