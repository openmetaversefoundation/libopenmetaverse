namespace Nwc.XmlRpc
{
  using System;

  /// <summary>Standard XML-RPC error codes</summary>
  public class XmlRpcErrorCodes
  {
    // -32400 ---> system error
    
    /// <summary>Transport error</summary>
    public const int TRANSPORT_ERROR = -32300;
    /// <summary>Transport error message</summary>
    public const String TRANSPORT_ERROR_MSG = "Transport Layer Error";
  }
}
