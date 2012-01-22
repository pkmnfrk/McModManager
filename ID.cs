using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MCModManager
{
    public struct ID
    {
        public string Root;
        public string Value;
        public string Version;

        public static ID MakeID(string root, string value = null, string version = null)
        {

            if (root.Contains(':'))
            {
                ID tmp = Parse(root);
                root = tmp.Root;
                if (value != null && version == null)
                {
                    version = value;
                    value = tmp.Value;
                }
                if (version == null) version = tmp.Version;
            }

            return new ID { Root = root, Value = value, Version = version };
        }

        public static ID Parse(string str)
        {
            string root = string.Empty, value, version = null;

            if (str.Contains(":"))
            {
                string[] v = str.Split(':');
                root = v[0];
                if (v[1].Contains("#"))
                {
                    string[] v2 = v[1].Split('#');
                    value = v2[0];
                    version = v2[1];
                } else
                {
                    value = v[1];
                    version = null;
                }
            } else
            {
                value = str;
            }

            return new ID { Root = root, Value = value, Version = version };
        }

        public static ID Parse(XElement el)
        {
            if (el.Element("root") != null)
            {
                if (el.Element("version") != null)
                {
                    return new ID { Root = el.Element("root").Value, Value = el.Element("value").Value, Version = el.Element("version").Value };
                } else
                {
                    return new ID { Root = el.Element("root").Value, Value = el.Element("value").Value };
                }
            } else
            {
                return Parse(el.Value);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}{2}", Root, Value, Version != null ? "#" + Version : string.Empty);
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is ID)) return false;

            if (Root != ((ID)obj).Root || Value != ((ID)obj).Value || Version != ((ID)obj).Version)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int ret = 0;

            if (Root != null) ret ^= Root.GetHashCode();
            if (Value != null) ret ^= Value.GetHashCode();
            if (Version != null) ret ^= Version.GetHashCode();

            return ret;
        }

        public static implicit operator ID(string val)
        {
            return Parse(val);
        }

        public static implicit operator ID(XElement val)
        {
            return Parse(val);
        }

        public static implicit operator string(ID val)
        {
            return val.ToString();
        }
    }
}
