using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Dapper;
using System.Data;

namespace MCModManager {
    internal static class AppData {

        private static Dictionary<ID, Mod> mods = new Dictionary<ID, Mod>();
        public static Dictionary<ID, Mod> Mods {
            get { return mods; }
        }

        internal static string AppDataPath {
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

			Database.InitDatabase();

            mods = Mod.LoadMods().ToDictionary( k => k.Id );

            if (!mods.ContainsKey("base:minecraft")) {
                Mod mc = Mod.LoadFromUrl("base_manifests/minecraft.xml");
                mc.Save();
            }

            if (!mods.ContainsKey("base:testmod")) {
                Mod test = Mod.LoadFromUrl("test.xml");
                test.Save();
            }
            
        }
    }
}
