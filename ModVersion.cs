using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Data;
using Dapper;

namespace MCModManager
{
    public class ModVersion
    {
        public ID ParentId { get; set; }
        public string Url { get; set; }
        public string Ver { get; set; }
        public PackingType Packing { get; set; }

        private IList<ID> deps;

        public IEnumerable<ID> Dependencies
        {
            get
            {
                if (deps == null)
                {
                    using (var dbConn = Database.GetConnection())
                    {
                        deps = dbConn.Query("SELECT depmodid, depversion FROM moddependency WHERE modid = @id AND version = @ver", new { id = (string)ParentId, ver = Ver }).Select<dynamic, ID>(i => ID.MakeID(i.depmodid, i.depversion)).ToList();
                    }
                }
                return deps;
            }
        }

        public ModVersion()
        {

        }

        public enum PackingType
        {
            unknown,
            modloader,
            raw,
            @base
        }

        internal static ModVersion LoadVersion(Mod parent, XElement ver)
        {
            var ret = new ModVersion();

            var ns = Mod.ns;

            ret.ParentId = parent.Id;
            ret.Url = ver.Element(ns.GetName("url")).Value;
            ret.Ver = ver.Element(ns.GetName("ver")).Value;

            var packing = ver.Element(ns.GetName("packing")).Value.ToLower();

            ret.Packing = (PackingType)Enum.Parse(typeof(PackingType), packing);

            using (var dbConn = Database.GetConnection())
            {
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

        public override string ToString()
        {
            return string.Format("{{{0}, {1}}}", Ver, Url);
        }

        internal void Save(IDbConnection dbConn, IDbTransaction tx = null)
        {

            dbConn.Execute("REPLACE INTO modversion (modid, version, url, packing) VALUES (@Id, @Ver, @Url, @Packing)", new { Id = (string)ParentId, this.Ver, this.Url, Packing = this.Packing.ToString() }, tx);

        }

        internal static IEnumerable<ModVersion> GetVersions(ID Id)
        {
            using (var dbConn = Database.GetConnection())
            {
                return dbConn.Query("SELECT version,url,packing FROM ModVersion WHERE ModId = @Id", new { Id = (string)Id })
                    .Select(v => new ModVersion
                    {
                        ParentId = Id,
                        Ver = v.version,
                        Url = v.url,
                        Packing = Enum.Parse(typeof(PackingType), v.packing),
                    });
            }
        }

        internal void Save()
        {
            using (var dbConn = Database.GetConnection())
            {
                this.Save(dbConn);
            }
        }
    }
}
