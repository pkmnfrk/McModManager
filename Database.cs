using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.IO;


namespace MCModManager {
    static class Database {

        private static string DatabasePath {
            get {
                return Path.Combine(AppData.AppDataPath, "data.db");
            }
        }

        internal static IDbConnection GetConnection() {
            var ret = new System.Data.SQLite.SQLiteConnection("Data Source=" + DatabasePath);
            ret.Open();
            return ret;
        }
    }
}
