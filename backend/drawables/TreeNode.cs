using System.Collections.Concurrent;
using System.Collections.Generic;

namespace backend.drawables
{
    public class TreeNode
    {
        public string name { get; set; }
        public int id { get; set; }
        public string server_id { get; set; }
        public int pid { get; set; }
        public bool dragDisabled { get; set; }
        public bool addTreeNodeDisabled { get; set; }
        public bool addLeafNodeDisabled { get; set; }
        public bool editNodeDisabled { get; set; }
        public bool delNodeDisabled { get; set; }
        public bool isLeaf { get; set; }
        public List<TreeNode> children { get; set; }
    }
}