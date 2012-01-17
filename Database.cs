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

        internal static IDbConnection GetConnection() {
            var ret = new System.Data.SQLite.SQLiteConnection("Data Source=" + DatabasePath);
            ret.Open();
            return ret;
        }

		internal static void InitDatabase() {
			using (var dbConn = (System.Data.SQLite.SQLiteConnection)GetConnection()) {
				var version = dbConn.Query<long>("pragma user_version;").First();

				if (version < 1) {
					DatabaseVersion1(dbConn);
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
				");

				dbConn.Execute("pragma user_version = 1");
				tx.Commit();
			}
		}
    }
}
