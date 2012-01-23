//-----------------------------------------------------------------------
// <copyright file="AppData.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using Dapper;

    /// <summary>
    /// Everything relating our local cache of data, including a list of mods
    /// </summary>
    internal static class AppData
    {
        /// <summary>
        /// list of all mods we know about
        /// </summary>
        private static Dictionary<ID, Mod> mods = new Dictionary<ID, Mod>();

        /// <summary>
        /// Gets a list of all mods we know about
        /// </summary>
        public static Dictionary<ID, Mod> Mods
        {
            get { return mods; }
        }

        /// <summary>
        /// Gets the path to our App Data folder (usually something like
        /// c:\Users\Mike\AppData\Roaming\McModManager)
        /// </summary>
        internal static string AppDataPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MCModManager");
            }
        }

        /// <summary>
        /// Gets the path to our archives folder (Something like %AppDataPath%\archives
        /// </summary>
        internal static string ArchivesPath
        {
            get
            {
                return Path.Combine(AppDataPath, "archives");
            }
        }

        /// <summary>
        /// Gets the path to our temp folder (%AppDataPath%\temp)
        /// </summary>
        internal static string TempPath
        {
            get
            {
                return Path.Combine(AppDataPath, "temp");
            }
        }

        /// <summary>
        /// Ensures our Appdata folder is set up, loads the databse, etc.
        /// </summary>
        public static void InitAppData()
        {
            try
            {
                if (!Directory.Exists(AppDataPath))
                {
                    Directory.CreateDirectory(AppDataPath);
                }

                if (!Directory.Exists(ArchivesPath))
                {
                    Directory.CreateDirectory(ArchivesPath);
                }

                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create the application data folder. Please ensure you have permission to write to '" + AppDataPath + "'.\n\nThe original error was:\n\n" + ex.ToString());
                Environment.Exit(1);
            }

            try
            {
                Database.InitDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could initialize the application database. Please ensure you have permission to write to '" + AppDataPath + "'.\n\nThe original error was:\n\n" + ex.ToString());
                Environment.Exit(1);
            }

            Mod.LoadFromUrl("base_manifests/minecraft.xml");
            Mod.LoadFromUrl("singlePlayerCommands.xml");
            Mod.LoadFromUrl("test.xml");            

            try
            {
                mods = Mod.LoadMods().ToDictionary(k => k.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load the list of mods from the database. Please ensure you have permission to write to '" + AppDataPath + "'.\n\nThe original error was:\n\n" + ex.ToString());
                Environment.Exit(1);
            }

            /*foreach (Mod m in Mods.Values)
            {
                foreach (ModVersion v in m.Versions)
                {
                    if (!v.IsDownloaded)
                    {
                        try
                        {
                            v.Download();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (v.IsDownloaded)
                    {
                        v.AnalyzeMod();
                    }
                }
            }*/
        }

        /// <summary>
        /// Gets the path to a specific Mod's folder
        /// </summary>
        /// <param name="mod">the Id of the mod</param>
        /// <returns>The path to a specific Mod's folder</returns>
        internal static string ModArchivePath(ID mod)
        {
            string root = Regex.Replace(mod.Root, @"[^\w]", "_");
            string value = Regex.Replace(mod.Value, @"[^\w]", "_");
            string ver = Regex.Replace(mod.Version, @"[^\w]", "_");

            return Path.Combine(ArchivesPath, root, value, ver);
        }
    }
}
