//-----------------------------------------------------------------------
// <copyright file="Database.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Dapper;

    /// <summary>
    /// Helper class for dealing with the database
    /// </summary>
    internal static class Database
    {
        /// <summary>
        /// Gets the connection string to open the database
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return "Data Source=" + DatabasePath;
            }
        }

        /// <summary>
        /// Gets the physical path to the SQLite database
        /// </summary>
        private static string DatabasePath
        {
            get
            {
                return Path.Combine(AppData.AppDataPath, "data.db");
            }
        }

        /// <summary>
        /// Returns a new connection to the database
        /// </summary>
        /// <returns>a new connection to the database</returns>
        internal static IDbConnection GetConnection()
        {
            var ret = new SQLiteConnection(ConnectionString);
            ret.Open();
            return ret;
        }

        /// <summary>
        /// Initializes the database (creating schema, etc)
        /// </summary>
        /// <param name="recurse">internal use only</param>
        internal static void InitDatabase(bool recurse = false)
        {
            bool created = false;
            var dbConn = new SQLiteConnection(ConnectionString);

            ////File.Delete(DatabasePath);

            try
            {
                dbConn.Open();
            }
            catch (SQLiteException ex1)
            {
                if (recurse)
                {
                    throw;
                }

                try
                {
                    if (File.Exists(DatabasePath + ".bak"))
                    {
                        File.Delete(DatabasePath + ".bak");
                    }

                    if (File.Exists(DatabasePath))
                    {
                        File.Move(DatabasePath, DatabasePath + ".bak");
                    }
                }
                catch (Exception ex)
                {
                    throw new AggregateException(ex1, ex);
                }

                dbConn.Open();
                created = true;
            }

            var version = dbConn.Query<long>("pragma user_version;").First();

            try
            {
                if (version < 1)
                {
                    DatabaseVersion1(dbConn);
                    version = 1;
                }

                dbConn.Close();
            }
            catch (Exception)
            {
                dbConn.Close();
                if (!created)
                {
                    if (File.Exists(DatabasePath + ".bak"))
                    {
                        File.Delete(DatabasePath + ".bak");
                    }

                    if (File.Exists(DatabasePath))
                    {
                        File.Move(DatabasePath, DatabasePath + ".bak");
                    }

                    InitDatabase(true);
                }
            }
        }

        /// <summary>
        /// Version 1 of the database schema
        /// </summary>
        /// <param name="dbConn">database connection</param>
        private static void DatabaseVersion1(SQLiteConnection dbConn)
        {
            using (var tx = dbConn.BeginTransaction())
            {
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
                        hash TEXT NULL,
                        filename TEXT NOT NULL,
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
