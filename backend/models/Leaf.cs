using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace backend
{
    public class Leaf
    {

        public Leaf() 
        {
            leafs = new ConcurrentBag<LeafNode>();
        }

        public string server_id { get; set; }
        //public DateTime now { get; set; }
        public int leafnodes { get; set; }
        public ConcurrentBag<LeafNode> leafs { get; set; }
    }


    public class LeafNode 
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