//-----------------------------------------------------------------------
// <copyright file="MD5.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// Helper methods for computing hashes
    /// </summary>
    internal static class MD5
    {
        /// <summary>
        /// Computes the MD5 hash of a file
        /// </summary>
        /// <param name="file">A file path</param>
        /// <returns>A string representing the hash in string form</returns>
        public static string Hash(string file)
        {
            using (var fs = File.OpenRead(file))
            {
                return Hash(fs);
            }
        }

        /// <summary>
        /// Computes the MD5 hash of a stream
        /// </summary>
        /// <param name="input">A finite stream</param>
        /// <returns>A string representing the hash in string form</returns>
        public static string Hash(Stream input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var bytes = md5.ComputeHash(input);

                var hash = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    hash.AppendFormat("{0:x}", bytes[i]);
                }

                return hash.ToString();
            }
        }
    }
}
