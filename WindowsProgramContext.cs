using System;

namespace NoZ.Platform.Windows {
    public class WindowsProgramContext {
        public string[] Args { get; set; }
        public ResourceArchive[] Archives { get; set; }
        public string GameResourceName { get; set; }
        public string Name { get; set; }
        public bool AllowWindowResize { get; set; } 
        public Vector2Int WindowSize { get; set; }
    }
}
