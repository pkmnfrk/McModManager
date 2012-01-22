namespace MCModManager
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ICSharpCode.SharpZipLib;
    using ICSharpCode.SharpZipLib.Zip;

    public class Archive
    {
        private ZipFile zip;

        public Archive(string file)
        {
            zip = new ZipFile(file);
        }
    }
}
