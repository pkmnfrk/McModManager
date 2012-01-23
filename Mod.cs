//-----------------------------------------------------------------------
// <copyright file="Mod.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml.Linq;
    using Dapper;

    /// <summary>
    /// Represents a modification to Minecraft
    /// </summary>
    public class Mod
    {
        /// <summary>
        /// The namespace the Manifests must live under
        /// </summary>
        internal static readonly XNamespace Namespace = XNamespace.Get("http://mike-caron.com/mod-manager/manifest");

        /// <summary>
        /// Cached list of ModVersions
        /// </summary>
        private IList<ModVersion> versions;

        /// <summary>
        /// Initializes a new instance of the Mod class
        /// </summary>
        public Mod()
        {
        }

        /// <summary>
        /// Gets the unique ID of this particular Mod
        /// </summary>
        public ID Id { get; private set; }

        /// <summary>
        /// Gets the name of the Mod
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the URL where the Manifest lives
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets a list of all versions this Mod has
        /// </summary>
        public IEnumerable<ModVersion> Versions
        {
            get
            {
                if (this.versions == null)
                {
                    this.versions = ModVersion.GetVersions(this.Id).ToList();
                }

                return this.versions;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this Mod is actually Minecraft.Jar
        /// </summary>
        public bool IsMinecraftJar
        {
            get
            {
                return this.Id == "base:minecraft";
            }
        }

        /// <summary>
        /// Returns a list of all Mods in the database
        /// </summary>
        /// <returns>a list of all Mods in the database</returns>
        public static IEnumerable<Mod> LoadMods()
        {
            using (var dbConn = Database.GetConnection())
            {
                return dbConn.Query("SELECT Id, Name, Url FROM mod")
                             .Select(m => new Mod
                             {
                                 Id = m.id,
                                 Name = m.name,
                                 Url = m.url,
                             });
            }
        }

        /// <summary>
        /// Loads a Mod from a Manifest
        /// </summary>
        /// <param name="uri">the URI where the Manifest lives</param>
        /// <returns>a Mod</returns>
        public static Mod LoadFromUrl(string uri)
        {
            Mod ret = new Mod();

            XDocument manifest = XDocument.Load(uri);

            if (manifest.Root.Name != Namespace.GetName("manifest"))
            {
                throw new Exception("That's not a McModManager manifest");
            }

            ret.Name = manifest.Root.Element(Namespace.GetName("name")).Value;
            ret.Id = manifest.Root.Element(Namespace.GetName("id"));

            if (ret.Id.Version != null)
            {
                throw new Exception("Manifest IDs cannot include versions");
            }

            if (manifest.Root.Element(Namespace.GetName("url")) != null)
            {
                ret.Url = manifest.Root.Element(Namespace.GetName("url")).Value;
            }

            using (var dbConn = Database.GetConnection())
            using (var tx = dbConn.BeginTransaction())
            {
                ret.Save(dbConn, tx);
                foreach (var ver in manifest.Root.Element(Namespace.GetName("versions")).Elements(Namespace.GetName("version")))
                {
                    var v = ModVersion.LoadVersion(ret, ver, dbConn, tx);
                    v.Save(dbConn, tx);
                }

                tx.Commit();
            }

            return ret;
        }

        /// <summary>
        /// Saves this Mod to the database
        /// </summary>
        public void Save()
        {
            using (var dbConn = Database.GetConnection())
            using (var tx = dbConn.BeginTransaction())
            {
                this.Save(dbConn, tx);
                tx.Commit();
            }
        }

        /// <summary>
        /// Saves this Mod to the database
        /// </summary>
        /// <param name="dbConn">The database connection</param>
        /// <param name="tx">The transaction scope</param>
        public void Save(IDbConnection dbConn, IDbTransaction tx = null)
        {
            dbConn.Execute("REPLACE INTO mod (id, name, url) VALUES (@Id, @Name, @Url)", new { Id = (string)this.Id, this.Name, this.Url }, tx);
        }

        /// <summary>
        /// Returns a string representing this Mod
        /// </summary>
        /// <returns>a string representing this Mod</returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", this.Name, this.Versions.Last().Ver);
        }
    }
}
