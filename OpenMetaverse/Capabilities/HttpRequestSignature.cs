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

        /// <summary>HTTP method</summary>
        public string Method
        {
            get { return method; }
            set
            {
                if (!String.IsNullOrEmpty(value)) method = value.ToLower();
                else method = String.Empty;
            }
        }
        /// <summary>HTTP Content-Type</summary>
        public string ContentType
        {
            get { return contentType; }
            set
            {
                if (!String.IsNullOrEmpty(value)) contentType = value.ToLower();
                else contentType = String.Empty;
            }
        }
        /// <summary>Relative URL path</summary>
        public string Path
        {
            get { return path; }
            set
            {
                if (!String.IsNullOrEmpty(value)) path = value;
                else path = String.Empty;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">HTTP request to build a signature for</param>
        public HttpRequestSignature(HttpListenerContext context)
        {
            method = contentType = path = String.Empty;

            Method = context.Request.HttpMethod;
            ContentType = context.Request.ContentType;
            Path = context.Request.RawUrl;
        }

        /// <summary>
        /// Test if two HTTP request signatures contain exactly the same data
        /// </summary>
        /// <param name="signature">Signature to test against</param>
        /// <returns>True if the contents of both signatures are identical, 
        /// otherwise false</returns>
        public bool ExactlyEquals(HttpRequestSignature signature)
        {
            return (method.Equals(signature.Method) && contentType.Equals(signature.ContentType) && path.Equals(signature.Path));
        }

        /// <summary>
        /// Does pattern matching to determine if an incoming HTTP request
        /// matches a given pattern. Equals can only be called on an incoming
        /// request; the pattern to match against is the parameter
        /// </summary>
        /// <param name="obj">The pattern to test against this request</param>
        /// <returns>True if the request matches the given pattern, otherwise
        /// false</returns>
        public override bool Equals(object obj)
        {
            return (obj is HttpRequestSignature) ? this == (HttpRequestSignature)obj : false;
        }

        /// <summary>
        /// Does pattern matching to determine if an incoming HTTP request
        /// matches a given pattern. Equals can only be called on an incoming
        /// request; the pattern to match against is the parameter
        /// </summary>
        /// <param name="pattern">The pattern to test against this request</param>
        /// <returns>True if the request matches the given pattern, otherwise
        /// false</returns>
        public bool Equals(HttpRequestSignature pattern)
        {
            return (this == pattern);
        }

        public override int GetHashCode()
        {
            int hash = (method != null) ? method.GetHashCode() : 0;
            hash ^= (contentType != null) ? contentType.GetHashCode() : 0;
            hash ^= (path != null) ? path.GetHashCode() : 0;
            return hash;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} Content-Type: {2}", method, path, contentType);
        }

        /// <summary>
        /// Does pattern matching to determine if an incoming HTTP request
        /// matches a given pattern. The incoming request must be on the
        /// left-hand side, and the pattern to match against must be on the
        /// right-hand side
        /// </summary>
        /// <param name="request">The incoming HTTP request signature</param>
        /// <param name="pattern">The pattern to test against the incoming request</param>
        /// <returns>True if the request matches the given pattern, otherwise
        /// false</returns>
        public static bool operator ==(HttpRequestSignature request, HttpRequestSignature pattern)
        {
            bool methodMatch = (String.IsNullOrEmpty(pattern.Method) || request.Method.Equals(pattern.Method));
            bool contentTypeMatch = (String.IsNullOrEmpty(pattern.ContentType) || request.ContentType.Equals(pattern.ContentType));
            bool pathMatch = true;

            if (methodMatch && contentTypeMatch && !String.IsNullOrEmpty(pattern.Path))
                pathMatch = Regex.IsMatch(request.Path, pattern.Path, RegexOptions.IgnoreCase);

            return (methodMatch && contentTypeMatch && pathMatch);
        }

        /// <summary>
        /// Does pattern matching to determine if an incoming HTTP request
        /// matches a given pattern. The incoming request must be on the
        /// left-hand side, and the pattern to match against must be on the
        /// right-hand side
        /// </summary>
        /// <param name="request">The incoming HTTP request signature</param>
        /// <param name="pattern">The pattern to test against the incoming request</param>
        /// <returns>True if the request does not match the given pattern, otherwise
        /// false</returns>
        public static bool operator !=(HttpRequestSignature request, HttpRequestSignature pattern)
        {
            return !(request == pattern);
        }
    }
}
