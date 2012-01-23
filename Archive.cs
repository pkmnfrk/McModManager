//-----------------------------------------------------------------------
// <copyright file="Archive.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using ICSharpCode.SharpZipLib;
    using ICSharpCode.SharpZipLib.Zip;

    /// <summary>
    /// A helper class for dealing with Zip/Jar files
    /// </summary>
    public class Archive
    {
        /// <summary>
        /// The zip file
        /// </summary>
        private ZipFile zip;

        /// <summary>
        /// Initializes a new instance of the Archive class
        /// </summary>
        /// <param name="file">the file to open</param>
        public Archive(string file)
        {
            this.zip = new ZipFile(file);
        }

        /// <summary>
        /// Gets all entries in the archive
        /// </summary>
        public IEnumerable<Entry> AllEntries
        {
            get
            {
                return this.zip.Cast<ZipEntry>().Select(ze => new Entry(this, ze));
            }
        }

        /// <summary>
        /// Gets all .class entries in the archive
        /// </summary>
        public IEnumerable<Entry> ClassEntries
        {
            get
            {
                return this.zip.Cast<ZipEntry>().Where(ze => Path.GetExtension(ze.Name) == ".class").Select(ze => new Entry(this, ze));
            }
        }

        /// <summary>
        /// Represents a file or folder in an archive
        /// </summary>
        public class Entry
        {
            private ZipEntry zipEntry;

            private Archive parent;

            /// <summary>
            /// Initializes a new instance of the Entry class
            /// </summary>
            /// <param name="parent">The owner Archive</param>
            /// <param name="ze">the ZipEntry to wrap</param>
            internal Entry(Archive parent, ZipEntry ze)
            {
                this.zipEntry = ze;
                this.parent = parent;
            }

            /// <summary>
            /// Gets the name of the ZipEntry
            /// </summary>
            public string Name
            {
                get
                {
                    return this.zipEntry.Name;
                }
            }

            /// <summary>
            /// Returns a stream to read the archive entry
            /// </summary>
            /// <returns>a stream to read the archive entry</returns>
            public Stream OpenToRead()
            {
                return this.parent.zip.GetInputStream(this.zipEntry);
            }
        }
    }
}
