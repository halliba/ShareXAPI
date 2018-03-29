using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareXAPI.Options
{
    public class UploaderOptions
    {
        public string WebBasePath { get; set; }
        public string LocalBasePath { get; set; }
        public string[] FileExtensions { get; set; }
        public float MaxFileSize { get; set; } // in MB
        public string ApiKey { get; set; }
    }
}
