using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Dapper;


namespace MCModManager {
    static class Database {

        private static string DatabasePath {
            get {
                return Path.Combine(AppData.AppDataPath, "data.db");
            }
        }

        public static string ConnectionString {
            get {
                return "Data Source=" + DatabasePath;
            }
        }

        internal static IDbConnection GetConnection() {
            var ret = new SQLiteConnection(ConnectionString);
            ret.Open();
            return ret;
        }

        internal static void InitDatabase(bool recurse = false) {
            bool created = false;
            var dbConn = new SQLiteConnection(ConnectionString);

            //File.Delete(DatabasePath);

            try {
                dbConn.Open();
            } catch (SQLiteException ex1) {

                if (recurse) throw;
                try {
                    if (File.Exists(DatabasePath + ".bak")) File.Delete(DatabasePath + ".bak");
                    if (File.Exists(DatabasePath)) File.Move(DatabasePath, DatabasePath + ".bak");
                } catch (Exception ex) {
                    throw new AggregateException(ex1, ex);
                }
                dbConn.Open();
                created = true;
            }

            var version = dbConn.Query<long>("pragma user_version;").First();

            try {

                if (version < 1) {
                    DatabaseVersion1(dbConn);
                    version = 1;
                }
                dbConn.Close();
            } catch (Exception) {
                dbConn.Close();
                if (!created) {
                    if (File.Exists(DatabasePath + ".bak")) File.Delete(DatabasePath + ".bak");
                    if (File.Exists(DatabasePath)) File.Move(DatabasePath, DatabasePath + ".bak");
                    InitDatabase(true);
                }
            }

        }


        private static void DatabaseVersion1(SQLiteConnection dbConn) {
            using (var tx = dbConn.BeginTransaction()) {

                dbConn.Execute(@"
					CREATE TABLE mod (
						id TEXT NOT NULL PRIMARY KEY,
						name TEXT NOT NULL,
						url TEXT NULL
					);

					CREATE TABLE modversion (
						modid TEXT NOT NULL,
						version TEXT NOT NULL,
						url TEXT NOT NULL,
						packing TEXT NOT NULL,
						PRIMARY KEY (modid, version),
						FOREIGN KEY (modid) REFERENCES mod(id)
					);

                    CREATE TABLE moddependency (
                        modid TEXT NOT NULL,
                        version TEXT NOT NULL,
                        depmodid TEXT NOT NULL,
                        depversion TEXT NOT NULL,
                        PRIMARY KEY (modid, version, depmodid),
                        FOREIGN KEY (modid, version) REFERENCES modversion(modid, version),
                        FOREIGN KEY (depmodid, depversion) REFERENCES modversion(modid, version)
                    );
				");

                dbConn.Execute("pragma user_version = 1");
                tx.Commit();
            }
        }
    }
}
