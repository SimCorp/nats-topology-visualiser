using System.Collections.Generic;
using backend.models;

namespace backend.drawables
{
    public class GatewayLink : Link
    {
        public List<string> errors { get; set; }
        public string errorsAsString { get; set; }

        public GatewayLink(string source, string target, bool ntv_error = false) : base(source, target, ntv_error)
        {
            errors  = new List<string>();
            errorsAsString = source + " to/from " + target;
        }
    }
}