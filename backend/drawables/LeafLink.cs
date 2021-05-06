using System.Collections.Generic;
using backend.models;

namespace backend.drawables
{
        public class LeafLink : Link
    {
        public List<LeafNode> connections { get; set; }
        public Dictionary<string, bool> accountToBidirectional { get; set; } // TODO maybe remove this if we can't determine accounts properly
        public LeafLink(string source, string target) : base(source, target)
        {
            connections = new List<LeafNode>();
            accountToBidirectional = new Dictionary<string, bool>();
        }
    }
}