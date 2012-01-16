using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCModManager {
    public class Mod {
        public Manifest Manifest { get; protected set; }

        public ID Id {
            get {
                return Manifest.Id;
            }
        }

        public Mod(string uri) {
            Manifest = Manifest.LoadFromUrl(uri);
        }
    }
}
