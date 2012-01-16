using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;

namespace MCModManager {
    public class Manifest {
        public ID Id { get; set; }
        public string Name { get; set; }
        public IList<Version> Versions { get; protected set; }

        private static readonly XNamespace ns = XNamespace.Get("http://mike-caron.com/McModManager/manifest");

        public Manifest() {
            Versions = new List<Version>();
        }

        public static Manifest LoadFromUrl(string uri) {
            XDocument manifest = XDocument.Load(uri);

            if (manifest.Root.Name != ns.GetName("manifest")) {
                throw new Exception("That's not a McModManager manifest");
            }

            Manifest ret = new Manifest();

            ret.Name = manifest.Root.Element(ns.GetName("name")).Value;
            ret.Id = manifest.Root.Element(ns.GetName("id")).ID();

            if (ret.Id.Version != null) throw new Exception("Manifest IDs cannot include versions");

            foreach (var ver in manifest.Root.Element(ns.GetName("versions")).Elements(ns.GetName("version"))) {
                ret.Versions.Add(Version.LoadVersion(ver));
            }

            return ret;
        }

        public class Version {
            public Uri URL { get; set; }
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

                ret.URL = new Uri(ver.Element(ns.GetName("url")).Value);
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
                        ret.Dependencies.Add(dep.ID());
                    }
                }

                return ret;
            }

            public override string ToString() {
                return string.Format("{{{0}, {1}}}", Ver, URL);
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

            return new ID { Root = root, Value = str, Version = version };
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
    }

    public static class IDExtensions {
        public static ID ID(this string str) {
            return MCModManager.ID.Parse(str);
        }

        public static ID ID(this XElement str) {
            return MCModManager.ID.Parse(str);
        }

        
    }
}
