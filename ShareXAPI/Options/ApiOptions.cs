using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareXAPI.Options
{
    public class ApiOptions
    {
        public string WebImageBasePath { get; set; }
        public string LocalImageBasePath { get; set; }
        public string[] FileExtensions { get; set; }
        public float MaxImageSize { get; set; } // in MB
        
    }
}
