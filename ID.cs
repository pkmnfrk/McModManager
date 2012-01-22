//-----------------------------------------------------------------------
// <copyright file="ID.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a Tuple that uniquely identifies a Mod
    /// </summary>
    public struct ID
    {
        /// <summary>
        /// Gets the "Root" of the ID (aka the namespace)
        /// </summary>
        public string Root
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the "Value" of the ID (aka the Mod)
        /// </summary>
        public string Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Version of the ID
        /// </summary>
        public string Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Makes an ID from its base components
        /// </summary>
        /// <param name="root">a string of the form "root[:value[#version]]". The version will be ignored</param>
        /// <param name="value">a replacement value</param>
        /// <returns>an ID</returns>
        public static ID MakeID(string root, string value)
        {
            string version = null;

            if (root.Contains(':'))
            {
                ID tmp = Parse(root);
                root = tmp.Root;
            }

            return new ID { Root = root, Value = value, Version = version };
        }

        /// <summary>
        /// Makes an ID from its base components
        /// </summary>
        /// <param name="root">a string of the form "root[:value[#version]]". The version and value will be ignored</param>
        /// <param name="value">a replacement value</param>
        /// <param name="version">a replacement version</param>
        /// <returns>an ID</returns>
        public static ID MakeID(string root, string value, string version)
        {
            if (root.Contains(':'))
            {
                ID tmp = Parse(root);
                root = tmp.Root;
            }

            return new ID { Root = root, Value = value, Version = version };
        }

        /// <summary>
        /// Parses a string of the form "root[:value[#version]]" into an ID
        /// </summary>
        /// <param name="str">the input string</param>
        /// <returns>an ID</returns>
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
                }
                else
                {
                    value = v[1];
                    version = null;
                }
            }
            else
            {
                value = str;
            }

            return new ID { Root = root, Value = value, Version = version };
        }

        /// <summary>
        /// Parses an XML Element representing an ID into an actual ID.
        /// </summary>
        /// <param name="el">the input XML</param>
        /// <returns>an ID</returns>
        public static ID Parse(XElement el)
        {
            if (el.Element("root") != null)
            {
                if (el.Element("version") != null)
                {
                    return new ID { Root = el.Element("root").Value, Value = el.Element("value").Value, Version = el.Element("version").Value };
                }
                else
                {
                    return new ID { Root = el.Element("root").Value, Value = el.Element("value").Value };
                }
            }
            else
            {
                return Parse(el.Value);
            }
        }

        /// <summary>
        /// converts a string to ID
        /// </summary>
        /// <param name="val">the input string</param>
        /// <returns>an ID</returns>
        public static implicit operator ID(string val)
        {
            return Parse(val);
        }

        /// <summary>
        /// converts an XML node to ID
        /// </summary>
        /// <param name="val">the input XML node</param>
        /// <returns>an ID</returns>
        public static implicit operator ID(XElement val)
        {
            return Parse(val);
        }

        /// <summary>
        /// converts an ID to a string
        /// </summary>
        /// <param name="val">the input ID</param>
        /// <returns>a string of the form "root:value#version"</returns>
        public static implicit operator string(ID val)
        {
            return val.ToString();
        }

        /// <summary>
        /// Returns a string representation of this ID
        /// </summary>
        /// <returns>a string in the form "root:value#version"</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}{2}", this.Root, this.Value, this.Version != null ? "#" + this.Version : string.Empty);
        }

        /// <summary>
        /// Determines equivalence between two objects
        /// </summary>
        /// <param name="obj">the other object to compare against</param>
        /// <returns>true if the other object is an ID that represents an identical ID</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is ID))
            {
                return false;
            }

            if (this.Root != ((ID)obj).Root || this.Value != ((ID)obj).Value || this.Version != ((ID)obj).Version)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes a hashcode for this object
        /// </summary>
        /// <returns>a hashcode for this object</returns>
        public override int GetHashCode()
        {
            int ret = 0;

            if (this.Root != null)
            {
                ret ^= this.Root.GetHashCode();
            }

            if (this.Value != null)
            {
                ret ^= this.Value.GetHashCode();
            }

            if (this.Version != null)
            {
                ret ^= this.Version.GetHashCode();
            }

            return ret;
        }
    }
}
