using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;

namespace MCModManager {
    public class Manifest {
        public string Name { get; set; }
        public IList<Version> Versions { get; protected set; }

        private const XNamespace ns = XNamespace.Get("http://mike-caron.com/McModManager/manifest");

        public static Manifest LoadFromUrl(string uri) {
            XDocument manifest = XDocument.Load(uri);

            if (manifest.Root.Name != ns.GetName("manifest")) {
                throw new Exception("That's not a McModManager manifest");
            }

            Manifest ret = new Manifest();

            ret.Name = manifest.Root.Element(ns.GetName("name")).Value;

            return ret;
        }

        public class Version {
            public string URL { get; set; }
            public string Version { get; set; }
            public PackingType Packing { get; set; }

            public enum PackingType {
                Unknown,
                ModLoader,
                Raw
            }
        }
    }
}
