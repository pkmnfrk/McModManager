using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCModManager {
    internal static class AppData {
        private static Archive pureMinecraftJar;
        public static Archive PureMinecraftJar {
            get {
                return pureMinecraftJar;
            }
        }

        public static void InitAppData() {

        }
    }
}
