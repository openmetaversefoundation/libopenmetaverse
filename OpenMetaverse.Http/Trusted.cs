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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Mono.Security;
using Mono.Security.X509;
using Mono.Security.X509.Extensions;
using Mono.Security.Authenticode;

namespace OpenMetaverse.Http
{
    public static class Trusted
    {
        public static void CreateServerRootCA(string issuer, out byte[] rootKey, out byte[] rootCert)
        {
            if (!issuer.StartsWith("CN="))
                issuer = "CN=" + issuer;

            // Create a temporary file
            string tempFile = Path.GetTempFileName();

            // Generate a new signing key
            RSA issuerKey = (RSA)RSA.Create();

            // Generate a private key
            PrivateKey key = new PrivateKey();
            key.RSA = issuerKey;

            // Save the private key and load it back into memory
            key.Save(tempFile);
            rootKey = File.ReadAllBytes(tempFile);
            File.Delete(tempFile);

            // Serial number MUST be positive
            byte[] sn = Guid.NewGuid().ToByteArray();
            if ((sn[0] & 0x80) == 0x80)
                sn[0] -= 0x80;

            // Generate a self-signed certificate
            X509CertificateBuilder cb = new X509CertificateBuilder(3);
            cb.SerialNumber = sn;
            cb.IssuerName = issuer;
            cb.NotBefore = DateTime.Now;
            cb.NotAfter = new DateTime(643445675990000000); // 12/31/2039 23:59:59Z
            cb.SubjectName = issuer;
            cb.SubjectPublicKey = issuerKey;
            cb.Hash = "SHA1";

            rootCert = cb.Sign(issuerKey);
        }

        public static byte[] CreateServerCert(string subjectName, byte[] rootKey, byte[] rootCert)
        {
            if (!subjectName.StartsWith("CN="))
                subjectName = "CN=" + subjectName;

            // Copy the root key since the PrivateKey constructor will blow away the data
            byte[] rootKeyCopy = new byte[rootKey.Length];
            Buffer.BlockCopy(rootKey, 0, rootKeyCopy, 0, rootKey.Length);

            // Load the server's private key and certificate
            PrivateKey pvk = new PrivateKey(rootKeyCopy, null);
            RSA issuerKey = pvk.RSA;
            X509Certificate issuerCert = new X509Certificate(rootCert);

            // Serial number MUST be positive
            byte[] sn = Guid.NewGuid().ToByteArray();
            if ((sn[0] & 0x80) == 0x80)
                sn[0] -= 0x80;

            ExtendedKeyUsageExtension eku = new ExtendedKeyUsageExtension();
            eku.KeyPurpose.Add("1.3.6.1.5.5.7.3.1"); // Indicates the cert is intended for server auth

            // Generate a server certificate signed by the server root CA
            X509CertificateBuilder cb = new X509CertificateBuilder(3);
            cb.SerialNumber = sn;
            cb.IssuerName = issuerCert.IssuerName;
            cb.NotBefore = DateTime.Now;
            cb.NotAfter = new DateTime(643445675990000000); // 12/31/2039 23:59:59Z
            cb.SubjectName = subjectName;
            cb.SubjectPublicKey = issuerKey;
            cb.Hash = "SHA1";
            cb.Extensions.Add(eku);
            byte[] serverCert = cb.Sign(issuerKey);

            // Generate a PKCS#12 file for the server containing the private key and certificate
            PKCS12 p12 = new PKCS12();
            p12.Password = null;

            ArrayList list = new ArrayList(4);
            // We use a fixed array to avoid endianess issues
            // (in case some tools requires the ID to be 1).
            list.Add(new byte[] { 1, 0, 0, 0 });
            Hashtable attributes = new Hashtable(1);
            attributes.Add(PKCS9.localKeyId, list);

            p12.AddCertificate(new X509Certificate(serverCert), attributes);
            p12.AddCertificate(issuerCert);
            p12.AddPkcs8ShroudedKeyBag(issuerKey, attributes);

            return p12.GetBytes();
        }

        public static byte[] CreateClientCert(string subjectName, byte[] rootKey, byte[] rootCert)
        {
            if (!subjectName.StartsWith("CN="))
                subjectName = "CN=" + subjectName;

            // Copy the root key since the PrivateKey constructor will blow away the data
            byte[] rootKeyCopy = new byte[rootKey.Length];
            Buffer.BlockCopy(rootKey, 0, rootKeyCopy, 0, rootKey.Length);

            // Load the server's private key and certificate
            PrivateKey pvk = new PrivateKey(rootKeyCopy, null);
            RSA issuerKey = pvk.RSA;
            X509Certificate issuerCert = new X509Certificate(rootCert);

            // Serial number MUST be positive
            byte[] sn = Guid.NewGuid().ToByteArray();
            if ((sn[0] & 0x80) == 0x80)
                sn[0] -= 0x80;

            ExtendedKeyUsageExtension eku = new ExtendedKeyUsageExtension();
            eku.KeyPurpose.Add("1.3.6.1.5.5.7.3.2"); // Indicates the cert is intended for client auth

            // Generate a client certificate signed by the server root CA
            X509CertificateBuilder cb = new X509CertificateBuilder(3);
            cb.SerialNumber = sn;
            cb.IssuerName = issuerCert.IssuerName;
            cb.NotBefore = DateTime.Now;
            cb.NotAfter = new DateTime(643445675990000000); // 12/31/2039 23:59:59Z
            cb.SubjectName = subjectName;
            cb.SubjectPublicKey = issuerKey;
            cb.Hash = "SHA1";
            cb.Extensions.Add(eku);
            byte[] clientCert = cb.Sign(issuerKey);

            // Generate a PKCS#12 file for the client containing the private key and certificate
            PKCS12 p12 = new PKCS12();
            p12.Password = null;

            ArrayList list = new ArrayList(4);
            // We use a fixed array to avoid endianess issues
            // (in case some tools requires the ID to be 1).
            list.Add(new byte[] { 1, 0, 0, 0 });
            Hashtable attributes = new Hashtable(1);
            attributes.Add(PKCS9.localKeyId, list);

            p12.AddCertificate(new X509Certificate(clientCert), attributes);
            p12.AddCertificate(issuerCert);
            p12.AddPkcs8ShroudedKeyBag(issuerKey, attributes);

            return p12.GetBytes();
        }
    }
}
