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
            ret.Id = ID.Parse(manifest.Root.Element(ns.GetName("id")).Value);

            foreach (var ver in manifest.Root.Element(ns.GetName("versions")).Elements(ns.GetName("version"))) {
                ret.Versions.Add(Version.LoadVersion(ver));
            }

            return ret;
        }

        public struct ID {
            public string Root;
            public string Value;

            public static ID Parse(string str) {
                if (str.Contains(":")) {
                    string[] v = str.Split(':');
                    return new ID { Root = v[0], Value = v[1] };
                } else {
                    return new ID { Root = string.Empty, Value = str };
                }
            }

            public override string ToString() {
                return string.Format("{0}:{1}", Root, Value);
            }
        }

        public class Version {
            public Uri URL { get; set; }
            public string Ver { get; set; }
            public PackingType Packing { get; set; }

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

                return ret;
            }

            public override string ToString() {
                return string.Format("{{{0}, {1}}}", Ver, URL);
            }
        }
    }
}
