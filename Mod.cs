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
        internal static readonly XNamespace Namespace = XNamespace.Get("http://mike-caron.com/McModManager/manifest");

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
        /// Initializes a new instance of the Mod class from a Manifest file
        /// </summary>
        /// <param name="uri">The URI of the manifest to load</param>
        public Mod(string uri)
            : this()
        {
            XDocument manifest = XDocument.Load(uri);

            if (manifest.Root.Name != Namespace.GetName("manifest"))
            {
                throw new Exception("That's not a McModManager manifest");
            }

            this.Name = manifest.Root.Element(Namespace.GetName("name")).Value;
            this.Id = manifest.Root.Element(Namespace.GetName("id"));

            if (this.Id.Version != null)
            {
                throw new Exception("Manifest IDs cannot include versions");
            }

            if (manifest.Root.Element(Namespace.GetName("url")) != null)
            {
                this.Url = manifest.Root.Element(Namespace.GetName("url")).Value;
            }

            foreach (var ver in manifest.Root.Element(Namespace.GetName("versions")).Elements(Namespace.GetName("version")))
            {
                var v = ModVersion.LoadVersion(this, ver);
                v.Save();
            }
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
            return new Mod(uri);
        }

        /// <summary>
        /// Saves this Mod to the database
        /// </summary>
        public void Save()
        {
            using (var dbConn = Database.GetConnection())
            using (var tx = dbConn.BeginTransaction())
            {
                dbConn.Execute("REPLACE INTO mod (id, name, url) VALUES (@Id, @Name, @Url)", new { Id = (string)this.Id, this.Name, this.Url }, tx);

                tx.Commit();
            }
        }

        /// <summary>
        /// Returns a string representing this Mod
        /// </summary>
        /// <returns>a string representing this Mod</returns>
        public override string ToString()
        {
            return string.Format("{0} - {1} ({2})", this.Id, this.Name, this.Url);
        }
    }
}
