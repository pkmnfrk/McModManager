//-----------------------------------------------------------------------
// <copyright file="ModVersion.cs" company="Mike Caron">
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
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml.Linq;
    
    using Dapper;

    /// <summary>
    /// Represents a single version of a Mod
    /// </summary>
    public class ModVersion
    {
        /// <summary>
        /// Cached list of dependencies
        /// </summary>
        private IList<ID> deps;

        /// <summary>
        /// Determines how a mod is packaged
        /// </summary>
        public enum PackingType
        {
            /// <summary>
            /// Not known (invalid)
            /// </summary>
            unknown,

            /// <summary>
            /// The mod is ModLoader compatible, and goes in the mods folder
            /// </summary>
            modloader,

            /// <summary>
            /// The mod is simply a collection of class files that go into the jar file
            /// </summary>
            raw,

            /// <summary>
            /// The mod is actually Minecraft itself, and doesn't go anywhere
            /// </summary>
            @base
        }

        /// <summary>
        /// Gets the ID of the Mod this version belongs to
        /// </summary>
        public ID ParentId { get; private set; }

        /// <summary>
        /// Gets the URL where the Mod can be downloaded
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the version string for this version
        /// </summary>
        public string Ver { get; private set; }

        /// <summary>
        /// Gets the packing value for this version, which determines how it is meant to be installed
        /// </summary>
        public PackingType Packing { get; private set; }

        /// <summary>
        /// Gets the stored MD5 hash value for this file
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// Gets the actual MD5 hash value for the downloaded file
        /// </summary>
        public string FileHash
        {
            get
            {
                if (!this.IsDownloaded)
                {
                    throw new InvalidOperationException("The file must be downloaded to compute its hash.");
                }

                using (MD5 md5 = MD5.Create())
                using (var fs = File.OpenRead(Path.Combine(this.CachePath, this.FileName)))
                {
                    var bytes = md5.ComputeHash(fs);

                    StringBuilder hash = new StringBuilder();

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        hash.AppendFormat("{0:x}", bytes[i]);
                    }

                    return hash.ToString();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mod has been downloaded or not
        /// </summary>
        public bool IsDownloaded
        {
            get
            {
                if (!Directory.Exists(this.CachePath))
                {
                    return false;
                }

                if (!File.Exists(Path.Combine(this.CachePath, this.FileName)))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the physical path to the modversion's cache
        /// </summary>
        public string CachePath
        {
            get
            {
                return AppData.ModArchivePath(ID.MakeID(this.ParentId, this.Ver));
            }
        }

        /// <summary>
        /// Gets name of the mod file
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the list of mods on which this mod is dependent to work correctly
        /// </summary>
        public IEnumerable<ID> Dependencies
        {
            get
            {
                if (this.deps == null)
                {
                    using (var dbConn = Database.GetConnection())
                    {
                        this.deps = dbConn.Query("SELECT depmodid, depversion FROM moddependency WHERE modid = @id AND version = @ver", new { id = (string)this.ParentId, ver = this.Ver }).Select<dynamic, ID>(i => ID.MakeID(i.depmodid, i.depversion)).ToList();
                    }
                }

                return this.deps;
            }
        }

        /// <summary>
        /// Returns a string version of this ModVersion
        /// </summary>
        /// <returns>a string version of this ModVersion</returns>
        public override string ToString()
        {
            return string.Format("{{{0}, {1}}}", this.Ver, this.Url);
        }

        /// <summary>
        /// Creates a ModVersion from an XML manifest node
        /// </summary>
        /// <param name="parent">the Mod that owns this version</param>
        /// <param name="ver">the XML from which to create the node</param>
        /// <returns>a ModVersion</returns>
        internal static ModVersion LoadVersion(Mod parent, XElement ver)
        {
            var ret = new ModVersion();

            var ns = Mod.Namespace;

            ret.ParentId = parent.Id;
            ret.Url = ver.Element(ns.GetName("url")).Value;
            ret.Ver = ver.Element(ns.GetName("ver")).Value;
            if (ver.Element(ns.GetName("hash")) != null)
            {
                ret.Hash = ver.Element(ns.GetName("hash")).Value.ToLower();
            }

            var packing = ver.Element(ns.GetName("packing")).Value.ToLower();

            ret.Packing = (PackingType)Enum.Parse(typeof(PackingType), packing);

            ret.Url = UrlHelpers.GetRealUrl(ret.Url);

            Uri temp = new Uri(ret.Url);
            ret.FileName = Path.GetFileName(temp.AbsolutePath);

            if (string.IsNullOrEmpty(ret.FileName))
            {
                ret.FileName = "mod.zip";
            }

            using (var dbConn = Database.GetConnection())
            {
                ret.Save(dbConn);

                if (ver.Element(ns.GetName("depends")) != null)
                {
                    foreach (var dep in ver.Element(ns.GetName("depends")).Elements(ns.GetName("depend")).Select(d => ID.Parse(d)))
                    {
                        dbConn.Execute("REPLACE INTO moddependency (modid, version, depmodid, depversion) VALUES (@Id, @Ver, @DepId, @DepVer)", new { Id = (string)ret.ParentId, ret.Ver, DepId = dep.Root + ":" + dep.Value, DepVer = dep.Version });
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Loads a list of ModVersions from the database for a given Mod
        /// </summary>
        /// <param name="id">the ID of the Mod for which to load versions</param>
        /// <returns>a list of ModVersions</returns>
        internal static IEnumerable<ModVersion> GetVersions(ID id)
        {
            using (var dbConn = Database.GetConnection())
            {
                return dbConn.Query("SELECT version, url, packing, hash, filename FROM ModVersion WHERE ModId = @Id", new { Id = (string)id })
                    .Select(v => new ModVersion
                    {
                        ParentId = id,
                        Ver = v.version,
                        Url = v.url,
                        Packing = Enum.Parse(typeof(PackingType), v.packing),
                        Hash = v.hash,
                        FileName = v.filename
                    });
            }
        }

        /// <summary>
        /// Saves this ModVersion to the database
        /// </summary>
        /// <param name="dbConn">the Database connection</param>
        /// <param name="tx">the Transaction scope</param>
        internal void Save(IDbConnection dbConn, IDbTransaction tx = null)
        {
            dbConn.Execute("REPLACE INTO modversion (modid, version, url, packing, hash, filename) VALUES (@Id, @Ver, @Url, @Packing, @Hash, @FileName)", new { Id = (string)this.ParentId, this.Ver, this.Url, Packing = this.Packing.ToString(), this.Hash, this.FileName }, tx);
        }

        /// <summary>
        /// Saves this ModVersion to the database.
        /// </summary>
        internal void Save()
        {
            using (var dbConn = Database.GetConnection())
            {
                this.Save(dbConn);
            }
        }

        /// <summary>
        /// Downloads the file if it already hasn't been
        /// </summary>
        internal void Download()
        {
            if (this.IsDownloaded)
            {
                if (this.Hash != this.FileHash)
                {
                    File.Delete(Path.Combine(this.CachePath, this.FileName));
                }
                else
                {
                    return;
                }
            }

            // if the url isn't http, this will fail (which is good)
            var client = new WebClient();

            if (!Directory.Exists(this.CachePath))
            {
                Directory.CreateDirectory(this.CachePath);
            }

            client.DownloadFile(this.Url, Path.Combine(this.CachePath, this.FileName));

            if (string.IsNullOrEmpty(this.Hash))
            {
                this.Hash = this.FileHash;
            }
        }
    }
}
