using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Dapper;
using System.Data;
using System.Windows.Forms;

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
            try {
                if (!Directory.Exists(AppDataPath)) {
                    Directory.CreateDirectory(AppDataPath);
                }

                if (!Directory.Exists(ArchivesPath)) {
                    Directory.CreateDirectory(ArchivesPath);
                }
            } catch (Exception ex) {
                MessageBox.Show("Could not create the application data folder. Please ensure you have permission to write to '" + AppDataPath + "'.\n\nThe original error was:\n\n" + ex.ToString());
                Environment.Exit(1);
            }
            try {
                Database.InitDatabase();
            } catch (Exception ex) {
                MessageBox.Show("Could initialize the application database. Please ensure you have permission to write to '" + AppDataPath + "'.\n\nThe original error was:\n\n" + ex.ToString());
                Environment.Exit(1);
            }

            try {
                mods = Mod.LoadMods().ToDictionary(k => k.Id);
            } catch (Exception ex) {
                MessageBox.Show("Could not load the list of mods from the database. Please ensure you have permission to write to '" + AppDataPath + "'.\n\nThe original error was:\n\n" + ex.ToString());
                Environment.Exit(1);
            }

            if (!mods.ContainsKey("base:minecraft")) {
                Mod mc = Mod.LoadFromUrl("base_manifests/minecraft.xml");
                mc.Save();
                mods[mc.Id] = mc;
            }

            if (!mods.ContainsKey("base:testmod")) {
                Mod test = Mod.LoadFromUrl("test.xml");
                test.Save();
                mods[test.Id] = test;
            }
            
        }
    }
}
