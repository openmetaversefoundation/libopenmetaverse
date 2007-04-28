using System;
using System.Reflection;
using SLProxy;

class ProxyMain
{
    public static void Main(string[] args)
    {
        ProxyFrame p = new ProxyFrame(args);
        p.proxy.Start();
    }
}