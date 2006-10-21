namespace Nwc.XmlRpc
{
  using System;
  using System.Collections;
  using System.IO;
  using System.Xml;
  using System.Net;
  using System.Text;
  using System.Reflection;

  internal class AcceptAllCertificatePolicy : ICertificatePolicy
  {
    public AcceptAllCertificatePolicy()
    {
    }

    public bool CheckValidationResult(ServicePoint sPoint, 
      System.Security.Cryptography.X509Certificates.X509Certificate cert, 
      WebRequest wRequest,int certProb)
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
    protected IList _params = null;

    /// <summary>Instantiate an <c>XmlRpcRequest</c></summary>
    public XmlRpcRequest()
      {
	_params = new ArrayList();
      }

    /// <summary><c>ArrayList</c> conntaining the parameters for the request.</summary>
    public virtual IList Params
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
    public XmlRpcResponse Send(String url)
      {
		// Override SSL authentication mechanisms
		ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();

	HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
	if (request == null)
	  throw new XmlRpcException(XmlRpcErrorCodes.TRANSPORT_ERROR,
				    XmlRpcErrorCodes.TRANSPORT_ERROR_MSG +": Could not create request with " + url);
	request.Method = "POST";
	request.ContentType = "text/xml";
	request.AllowWriteStreamBuffering = true;
	request.KeepAlive = false;
	request.Timeout = 15000; // miliseconds adjust as you see fit

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

    /// <summary>Produce <c>String</c> representation of the object.</summary>
    /// <returns><c>String</c> representation of the object.</returns>
    override public String ToString()
      {
	return _serializer.Serialize(this);
      }
  }
}
