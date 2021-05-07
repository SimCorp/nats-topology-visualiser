using System.Collections.Generic;
using backend.models;

namespace backend.drawables
{
        public class LeafLink : Link
    {
        public List<LeafNode> connections { get; set; }
        public LeafLink(string source, string target) : base(source, target)
        {
            connections = new List<LeafNode>();
        }
    }
}