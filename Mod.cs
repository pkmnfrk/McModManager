using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using Dapper;
using System.Data;

namespace MCModManager {
    public class Mod {
        public ID Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        private IList<ModVersion> versions;
        public IEnumerable<ModVersion> Versions {
            get {
                if (versions == null) versions = ModVersion.GetVersions(Id).ToList();
                return versions;
            }
        }

        internal static readonly XNamespace ns = XNamespace.Get("http://mike-caron.com/McModManager/manifest");

        public Mod() {
            
        }

        public Mod(string uri) : this() {
            XDocument manifest = XDocument.Load(uri);

            if (manifest.Root.Name != ns.GetName("manifest")) {
                throw new Exception("That's not a McModManager manifest");
            }

            this.Name = manifest.Root.Element(ns.GetName("name")).Value;
            this.Id = manifest.Root.Element(ns.GetName("id"));

            if (this.Id.Version != null) throw new Exception("Manifest IDs cannot include versions");

            if(manifest.Root.Element(ns.GetName("url")) != null)
                this.Url = manifest.Root.Element(ns.GetName("url")).Value;

            foreach (var ver in manifest.Root.Element(ns.GetName("versions")).Elements(ns.GetName("version"))) {
                var v = ModVersion.LoadVersion(this, ver);
                v.Save();
            }
        }

        public static IEnumerable<Mod> LoadMods() {

            using (var dbConn = Database.GetConnection()) {
                
                return dbConn.Query("SELECT Id, Name, Url FROM mod")
                             .Select(m => new Mod {
                                 Id = m.id,
                                 Name = m.name,
                                 Url = m.url,
                             });
            }

        }

        public void Save() {
            using (var dbConn = Database.GetConnection())
            using(var tx = dbConn.BeginTransaction()) {
                
                dbConn.Execute("REPLACE INTO mod (id, name, url) VALUES (@Id, @Name, @Url)", new { Id = (string)this.Id, this.Name, this.Url }, tx);

                tx.Commit();
            }
        }

        public static Mod LoadFromUrl(string uri) {
            return new Mod(uri);
        }

        public override string ToString() {
            return string.Format("{0} - {1} ({2})", this.Id, this.Name, this.Url);
        }
        
    }
}
