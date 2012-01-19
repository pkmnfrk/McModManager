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
        public IList<ModVersion> Versions { get; set; }

        internal static readonly XNamespace ns = XNamespace.Get("http://mike-caron.com/McModManager/manifest");

        public Mod() {
            Versions = new List<ModVersion>();
        }

        private Mod(IEnumerable<ModVersion> versions) :this() {
            foreach (var v in versions) {
                v.parent = this;
                Versions.Add(v);
            }
        }

        public Mod(string uri) : this() {
            XDocument manifest = XDocument.Load(uri);

            if (manifest.Root.Name != ns.GetName("manifest")) {
                throw new Exception("That's not a McModManager manifest");
            }

            this.Name = manifest.Root.Element(ns.GetName("name")).Value;
            this.Id = manifest.Root.Element(ns.GetName("id"));

            if (this.Id.Version != null) throw new Exception("Manifest IDs cannot include versions");

            foreach (var ver in manifest.Root.Element(ns.GetName("versions")).Elements(ns.GetName("version"))) {
                var v = ModVersion.LoadVersion(ver);
                v.parent = this;
                this.Versions.Add(v);
            }
        }

        public static IEnumerable<Mod> LoadMods() {

            using (var dbConn = Database.GetConnection()) {
                var deps = dbConn.Query("SELECT modid, version, depmodid, depversion FROM moddependency");
                var versions = dbConn.Query("SELECT modid, version, url, packing FROM modversion");
                
                return dbConn.Query("SELECT Id, Name, Url FROM mod")
                             .Select(m => new Mod(versions.Where(v => v.modid == m.id).Select(v => new ModVersion(deps.Where(d => d.modid == m.id && d.version == v.version).Select<dynamic, ID>(d => ID.MakeID(d.depmodid, d.depversion))) {
                                 Ver = v.version,
                                 Url = v.url,
                                 Packing = (ModVersion.PackingType)Enum.Parse(typeof(ModVersion.PackingType), v.packing)
                             })) {
                                 Id = m.id,
                                 Name = m.name,
                                 Url = m.url,
                             }).ToList();
            }

        }

        public void Save() {
            using (var dbConn = Database.GetConnection())
            using(var tx = dbConn.BeginTransaction()) {
                
                dbConn.Execute("REPLACE INTO mod (id, name, url) VALUES (@Id, @Name, @Url)", new { Id = (string)this.Id, this.Name, this.Url }, tx);

                foreach (var version in Versions) {
                    version.Save(dbConn, tx);
                }

                tx.Commit();
            }
        }

        public static Mod LoadFromUrl(string uri) {
            return new Mod(uri);
        }

        
    }
}
