//-----------------------------------------------------------------------
// <copyright file="UrlHelpers.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A number of helpers to take care of special URLs that we can't use directly
    /// </summary>
    internal static class UrlHelpers
    {
        private static readonly Dictionary<string, Func<string, string>> actions = new Dictionary<string, Func<string, string>>
        {
            { @"http://(adf\.ly)/.*", AdFly },
            { @"http://(bit.ly)/.*", BitLy },
        };

        /// <summary>
        /// Transforms a URL into its real version (eg, bypass ad.fly, etc)
        /// </summary>
        /// <param name="url">the URL</param>
        /// <returns>a URL</returns>
        public static string GetRealUrl(string url)
        {
        tryAgain:
            foreach (var m in actions)
            {
                if (Regex.IsMatch(url, m.Key))
                {
                    var newUrl = m.Value(url);
                    if (newUrl != url) 
                    {
                        url = newUrl;
                        goto tryAgain;
                    }
                }
            }

            return url;
        }

        private static string AdFly(string url)
        {
            string body;

            using (var client = new WebClient())
            {
                body = client.DownloadString(url);
            }

            // now, search for the real URL
            var realUrl = Regex.Match(body, "var url = '(http.*)';");

            if (realUrl.Success)
            {
                return realUrl.Groups[1].Value;
            }

            return string.Empty;
        }

        private static string BitLy(string url)
        {
            var client = (HttpWebRequest)WebRequest.Create(url);

            client.AllowAutoRedirect = false;
            client.Method = "HEAD"; // I just want to see the redirects
            
            var response = client.GetResponse();

            if (response.Headers["location"] != null)
            {
                return response.Headers["location"];
            }

            return url;
        }
    }
}
