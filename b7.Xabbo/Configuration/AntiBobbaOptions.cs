using System;
using System.Collections.Generic;

namespace b7.Xabbo.Configuration
{
    public class AntiBobbaOptions
    {
        public const string Path = "AntiBobba";

        public string Inject { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public bool Localized { get; set; } = true;
        public bool Auto { get; set; } = true;
        public List<string> AutoList { get; set; } = new();
    }
}
