using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MCModManager {
    internal static class AppData {

        private static Dictionary<ID, Mod> mods = new Dictionary<ID, Mod>();
        public static Dictionary<ID, Mod> Mods {
            get { return mods; }
        }

        private static string AppDataPath {
            get {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MCModManager");
            }
        }

        private static string ArchivesPath {
            get {
                return Path.Combine(AppDataPath, "archives");
            }
        }

        private static string ModArchivePath(string mod) {
            return Path.Combine(ArchivesPath, mod);
        }

        public static Mod LookupMod(ID id) {
            if (Mods.ContainsKey(id)) {
                return Mods[id];
            }

            return null;
        }

        public static void InitAppData() {

            if (!Directory.Exists(AppDataPath)) {
                Directory.CreateDirectory(AppDataPath);
            }

            if (!Directory.Exists(ArchivesPath)) {
                Directory.CreateDirectory(ArchivesPath);
            }

            //minecraft.jar is special, so lets load it now
            Mods.Add("base:minecraft".ID(), new Mod("base_manifests/minecraft.xml"));
            

        }
    }
}
