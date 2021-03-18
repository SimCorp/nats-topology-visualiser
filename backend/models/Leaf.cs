using System;
using System.Collections.Generic;

namespace backend
{
    public class Leaf
    {

        public Leaf() 
        {
            leafs = new HashSet<leafnode>();
        }

        public string server_id { get; set; }
        //public DateTime now { get; set; }
        public int leafnodes { get; set; }
        public ICollection<leafnode> leafs { get; set; }
    }


    public class leafnode 
    {
        public string account { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public long in_msgs { get; set; }
        public long out_msgs { get; set; }
        public long in_bytes { get; set; }
        public long out_bytes { get; set; }
        public int subscriptions { get; set; }
    }




}