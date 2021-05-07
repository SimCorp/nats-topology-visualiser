using System.Collections.Generic;
using System;

namespace backend.drawables
{
    public class Link 
    {
        public string source { get; }
        public string target { get; }
        public bool ntv_error { get; set; }


        public Link(string source, string target, bool ntv_error = false)
        {
            this.source = source;
            this.target = target;
            this.ntv_error = ntv_error;
        }
    }
}