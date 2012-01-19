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
        public IList<Version> Versions { get; set; }

        private static readonly XNamespace ns = XNamespace.Get("http://mike-caron.com/McModManager/manifest");

        public Mod() {
            Versions = new List<Version>();
        }

        private Mod(IEnumerable<Version> versions) :this() {
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
                var v = Version.LoadVersion(ver);
                v.parent = this;
                this.Versions.Add(v);
            }
        }

        public static IEnumerable<Mod> LoadMods() {

            using (var dbConn = Database.GetConnection()) {
                var versions = dbConn.Query("SELECT modid, version, url, packing FROM modversion");
                
                return dbConn.Query("SELECT Id, Name, Url FROM mod")
                             .Select(m => new Mod(versions.Where(v => v.modid == m.id).Select(v => new Mod.Version {
                                 Ver = v.version,
                                 Url = v.url,
                                 Packing = (Mod.Version.PackingType)Enum.Parse(typeof(Mod.Version.PackingType), v.packing)
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

        public class Version {
            internal Mod parent;
            public string Url { get; set; }
            public string Ver { get; set; }
            public PackingType Packing { get; set; }
            public IList<ID> Dependencies { get; protected set; }

            public Version() {
                Dependencies = new List<ID>();
            }

            public enum PackingType {
                Unknown,
                ModLoader,
                Raw,
                Base
            }

            internal static Version LoadVersion(XElement ver) {
                Version ret = new Version();

                ret.Url = ver.Element(ns.GetName("url")).Value;
                ret.Ver = ver.Element(ns.GetName("ver")).Value;

                var packing = ver.Element(ns.GetName("packing")).Value;

                if (packing == "modloader") {
                    ret.Packing = PackingType.ModLoader;
                } else if (packing == "raw") {
                    ret.Packing = PackingType.Raw;
                } else if(packing == "base") {
                    ret.Packing = PackingType.Base; //only minecraft.jar gets this...
                } else {
                    throw new Exception("Unknown packing type " + packing);
                }

                if(ver.Element(ns.GetName("depends")) != null) {
                    foreach (var dep in ver.Element(ns.GetName("depends")).Elements(ns.GetName("depend"))) {
                        ret.Dependencies.Add(dep);
                    }
                }

                return ret;
            }

            public override string ToString() {
                return string.Format("{{{0}, {1}}}", Ver, Url);
            }

            internal void Save(IDbConnection dbConn, IDbTransaction tx) {
                dbConn.Execute("REPLACE INTO modversion (modid, version, url, packing) VALUES (@Id, @Ver, @Url, @Packing)", new { Id = (string)parent.Id, this.Ver, this.Url, this.Packing }, tx);
            }
        }
    }

    public struct ID {
        public string Root;
        public string Value;
        public string Version;

        public static ID Parse(string str) {
            string root = string.Empty, value, version = null;

            if (str.Contains(":")) {
                string[] v = str.Split(':');
                root = v[0];
                if (v[1].Contains("#")) {
                    string[] v2 = v[1].Split('#');
                    value = v2[0];
                    version = v2[1];
                } else {
                    value = v[1];
                    version = null;
                }
            } else {
                value = str;
            }

            return new ID { Root = root, Value = value, Version = version };
        }

        public static ID Parse(XElement el) {
            if (el.Element("root") != null) {
                if (el.Element("version") != null) {
                    return new ID { Root = el.Element("root").Value, Value = el.Element("value").Value, Version = el.Element("version").Value };
                } else {
                    return new ID { Root = el.Element("root").Value, Value = el.Element("value").Value };
                }
            } else {
                return Parse(el.Value);
            }
        }

        public override string ToString() {
            return string.Format("{0}:{1}", Root, Value);
        }
        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (!(obj is ID)) return false;

            if (Root != ((ID)obj).Root || Value != ((ID)obj).Value || Version != ((ID)obj).Version) {
                return false;
            }

            return true;
        }

        public override int GetHashCode() {
            int ret = 0;

            if (Root != null) ret ^= Root.GetHashCode();
            if (Value != null) ret ^= Value.GetHashCode();
            if (Version != null) ret ^= Version.GetHashCode();

            return ret;
        }

        public static implicit operator ID(string val) {
            return Parse(val);
        }

        public static implicit operator ID(XElement val) {
            return Parse(val);
        }

        public static implicit operator string(ID val) {
            return val.ToString();
        }
    }

}
