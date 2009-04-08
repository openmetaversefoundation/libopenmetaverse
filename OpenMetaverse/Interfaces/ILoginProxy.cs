using System;
using System.Collections.Generic;
using System.Text;
using CookComputing.XmlRpc;
namespace OpenMetaverse.Interfaces
{
    public interface ILoginProxy : IXmlRpcProxy
    {
        [XmlRpcMethod("login_to_simulator")]
        LoginResponseData LoginToSimulator(LoginParams loginParams);

        [XmlRpcBegin("login_to_simulator")]
        IAsyncResult BeginLoginToSimulator(LoginParams loginParams);

        [XmlRpcBegin("login_to_simulator")]
        IAsyncResult BeginLoginToSimulator(LoginParams loginParams, AsyncCallback callback);

        [XmlRpcBegin("login_to_simulator")]
        IAsyncResult BeginLoginToSimulator(LoginParams loginParams, AsyncCallback callback, object asyncState);

        [XmlRpcEnd("login_to_simulator")]
        LoginResponseData EndLoginToSimulator(IAsyncResult result);
    }
}
