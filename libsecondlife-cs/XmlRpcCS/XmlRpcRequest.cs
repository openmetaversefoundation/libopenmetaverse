#pragma warning disable 0618

namespace Nwc.XmlRpc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Net;
    using System.Text;
    using System.Reflection;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    internal class AcceptAllCertificatePolicy : ICertificatePolicy
    {
        public AcceptAllCertificatePolicy()
        {
        }

        public bool CheckValidationResult(ServicePoint sPoint,
            System.Security.Cryptography.X509Certificates.X509Certificate cert,
            WebRequest wRequest, int certProb)
        {
            // Always accept
            return true;
        }
    }

    /// <summary>Class supporting the request side of an XML-RPC transaction.</summary>
    public class XmlRpcRequest
    {
        private String _methodName = null;
        private Encoding _encoding = new ASCIIEncoding();
        private XmlRpcRequestSerializer _serializer = new XmlRpcRequestSerializer();
        private XmlRpcResponseDeserializer _deserializer = new XmlRpcResponseDeserializer();

        /// <summary><c>ArrayList</c> containing the parameters.</summary>
        protected List<object> _params = null;

        /// <summary>Instantiate an <c>XmlRpcRequest</c></summary>
        public XmlRpcRequest()
        {
            _params = new List<object>();
        }

        /// <summary><c>ArrayList</c> containing the parameters for the request.</summary>
        public virtual List<object> Params
        {
            get { return _params; }
        }

        /// <summary><c>String</c> conntaining the method name, both object and method, that the request will be sent to.</summary>
        public virtual String MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }

        /// <summary>Send the request to the server.</summary>
        /// <param name="url"><c>String</c> The url of the XML-RPC server.</param>
        /// <returns><c>XmlRpcResponse</c> The response generated.</returns>
        public XmlRpcResponse Send(String url, int timeout)
        {
            // Override SSL authentication mechanisms
            ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
            //ServicePointManager.ServerCertificateValidationCallback += 
            //    delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            //    { return true; };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (request == null)
                throw new XmlRpcException(XmlRpcErrorCodes.TRANSPORT_ERROR,
                              XmlRpcErrorCodes.TRANSPORT_ERROR_MSG + ": Could not create request with " + url);
            request.Method = "POST";
            request.ContentType = "text/xml";
            request.AllowWriteStreamBuffering = true;
            request.KeepAlive = false;
            request.Timeout = timeout; // miliseconds adjust as you see fit

            Stream stream = request.GetRequestStream();
            XmlTextWriter xml = new XmlTextWriter(stream, _encoding);
            _serializer.Serialize(xml, this);
            xml.Flush();
            xml.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader input = new StreamReader(response.GetResponseStream());

            XmlRpcResponse resp = (XmlRpcResponse)_deserializer.Deserialize(input);
            input.Close();
            response.Close();
            return resp;
        }

        private bool CheckValidationResult(Object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
            System.Security.Cryptography.X509Certificates.X509Chain chain, 
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // Always accept
            return true;
        }

        /// <summary>Produce <c>String</c> representation of the object.</summary>
        /// <returns><c>String</c> representation of the object.</returns>
        override public String ToString()
        {
            return _serializer.Serialize(this);
        }
    }
}
