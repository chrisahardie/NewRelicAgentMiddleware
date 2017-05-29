using System;
using System.Collections.Generic;
using System.Text;

namespace NewRelicAgentMiddleware.Configuration
{
    public class NewRelicOptions
    {
        public bool Enabled { get; set; }
        public string LicenseKey { get; set; }
        public string AppName { get; set; }
        public string Language { get; set; }
        public string LanguageVersion { get; set; }
        public string LibraryPath { get; set; }
    }
}
