//-----------------------------------------------------------------------
// <copyright file="Archive.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System;
    using System.Collections.Generic;
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
    }
}
