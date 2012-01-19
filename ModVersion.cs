using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Data;
using Dapper;

namespace MCModManager {
	public class ModVersion {
		internal Mod parent;
		public string Url { get; set; }
		public string Ver { get; set; }
		public PackingType Packing { get; set; }
		public IList<ID> Dependencies { get; protected set; }

		public ModVersion() {
			Dependencies = new List<ID>();
		}

		internal ModVersion(IEnumerable<ID> Deps) {
			Dependencies = Deps.ToList();
		}

		public enum PackingType {
			Unknown,
			ModLoader,
			Raw,
			Base
		}

		internal static ModVersion LoadVersion(XElement ver) {
			var ret = new ModVersion();

			var ns = Mod.ns;

			ret.Url = ver.Element(ns.GetName("url")).Value;
			ret.Ver = ver.Element(ns.GetName("ver")).Value;

			var packing = ver.Element(ns.GetName("packing")).Value;

			if (packing == "modloader") {
				ret.Packing = PackingType.ModLoader;
			} else if (packing == "raw") {
				ret.Packing = PackingType.Raw;
			} else if (packing == "base") {
				ret.Packing = PackingType.Base; //only minecraft.jar gets this...
			} else {
				throw new Exception("Unknown packing type " + packing);
			}

			if (ver.Element(ns.GetName("depends")) != null) {
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

			foreach (var dep in Dependencies) {
				dbConn.Execute("REPLACE INTO moddependency (modid, version, depmodid, depversion) VALUES (@Id, @Ver, @DepId, @DepVer)", new { Id = (string)parent.Id, this.Ver, DepId = dep.Root + ":" + dep.Value, DepVer = dep.Version });
			}
		}
	}
}
