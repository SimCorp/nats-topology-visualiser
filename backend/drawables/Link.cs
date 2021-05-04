using System.Collections.Generic;
using System;

namespace backend.drawables
{
    public class Link 
    {
        public string source { get; }
        public string target { get; }
        public bool ntv_error { get; set; }
        public List<string> errors { get; set; }
        public string errorsAsString { get; set; }

        public Dictionary<string, bool> accountToBidirectional { get; set; }

        public Link(string source, string target, bool ntv_error = false)
        {
            this.source = source;
            this.target = target;
            this.ntv_error = ntv_error;
            errors  = new List<string>();
            errorsAsString = source + " to/from " + target;
            accountToBidirectional = new Dictionary<string, bool>();
        }
    }
}