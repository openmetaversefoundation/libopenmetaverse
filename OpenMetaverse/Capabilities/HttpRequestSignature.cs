/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Net;
using System.Text.RegularExpressions;

namespace OpenMetaverse.Capabilities
{
    /// <summary>
    /// Used to match incoming HTTP requests against registered handlers.
    /// Matches based on any combination of HTTP Method, Content-Type header,
    /// and URL path. URL path matching supports the * wildcard character
    /// </summary>
    public struct HttpRequestSignature : IEquatable<HttpRequestSignature>
    {
        private string method;
        private string contentType;
        private string path;
        private bool pathIsWildcard;

        public string Method
        {
            get { return method; }
            set
            {
                if (!String.IsNullOrEmpty(value)) method = value.ToLower();
                else method = String.Empty;
            }
        }
        public string ContentType
        {
            get { return contentType; }
            set
            {
                if (!String.IsNullOrEmpty(value)) contentType = value.ToLower();
                else contentType = String.Empty;
            }
        }
        public string Path
        {
            get { return path; }
            set
            {
                pathIsWildcard = false;

                if (!String.IsNullOrEmpty(value))
                {
                    // Regex to tear apart URLs, used to extract just a URL
                    // path from any data we're given
                    string regexPattern =
                        @"^(?<s1>(?<s0>[^:/\?#]+):)?(?<a1>"
                      + @"//(?<a0>[^/\?#]*))?(?<p0>[^\?#]*)"
                      + @"(?<q1>\?(?<q0>[^#]*))?"
                      + @"(?<f1>#(?<f0>.*))?";
                    Regex re = new Regex(regexPattern, RegexOptions.ExplicitCapture);
                    Match m = re.Match(value);
                    string newPath = m.Groups["p0"].Value.ToLower();

                    // Remove any trailing forward-slashes
                    if (newPath.EndsWith("/"))
                        newPath = newPath.Substring(0, newPath.Length - 1);

                    // Check if this path contains a wildcard. If so, convert it to a regular expression
                    if (newPath.Contains("*"))
                    {
                        pathIsWildcard = true;
                        newPath = String.Format("^{0}$", newPath.Replace("\\*", ".*"));
                    }

                    path = newPath;
                }
                else
                {
                    path = String.Empty;
                }
            }
        }
        public bool PathIsWildcard
        {
            get { return pathIsWildcard; }
        }

        public HttpRequestSignature(HttpListenerContext context)
        {
            method = contentType = path = String.Empty;
            pathIsWildcard = false;

            Method = context.Request.HttpMethod;
            ContentType = context.Request.ContentType;
            Path = context.Request.RawUrl;
        }

        public bool ExactlyEquals(HttpRequestSignature signature)
        {
            return (method.Equals(signature.Method) && contentType.Equals(signature.ContentType) && path.Equals(signature.Path));
        }

        public override bool Equals(object obj)
        {
            return (obj is HttpRequestSignature) ? this == (HttpRequestSignature)obj : false;
        }

        public bool Equals(HttpRequestSignature signature)
        {
            return (this == signature);
        }

        public override int GetHashCode()
        {
            int hash = (method != null) ? method.GetHashCode() : 0;
            hash ^= (contentType != null) ? contentType.GetHashCode() : 0;
            hash ^= (path != null) ? path.GetHashCode() : 0;
            return hash;
        }

        public static bool operator ==(HttpRequestSignature lhs, HttpRequestSignature rhs)
        {
            bool methodMatch = (String.IsNullOrEmpty(lhs.Method) || String.IsNullOrEmpty(rhs.Method) || lhs.Method.Equals(rhs.Method));
            bool contentTypeMatch = (String.IsNullOrEmpty(lhs.ContentType) || String.IsNullOrEmpty(rhs.ContentType) || lhs.ContentType.Equals(rhs.ContentType));
            bool pathMatch = false;

            if (methodMatch && contentTypeMatch)
            {
                if (!String.IsNullOrEmpty(lhs.Path) && !String.IsNullOrEmpty(rhs.Path))
                {
                    // Do wildcard matching if there is any to be done
                    if (lhs.PathIsWildcard)
                        pathMatch = Regex.IsMatch(rhs.Path, lhs.Path);
                    else if (rhs.PathIsWildcard)
                        pathMatch = Regex.IsMatch(lhs.Path, rhs.Path);
                    else
                        pathMatch = lhs.Path.Equals(rhs.Path);
                }
                else
                {
                    pathMatch = true;
                }
            }

            return (methodMatch && contentTypeMatch && pathMatch);
        }

        public static bool operator !=(HttpRequestSignature lhs, HttpRequestSignature rhs)
        {
            return !(lhs == rhs);
        }
    }
}
