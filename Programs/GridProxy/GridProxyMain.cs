using System;
using System.Reflection;
using GridProxy;

class ProxyMain
{
    public static void Main(string[] args)
    {
        ProxyFrame p = new ProxyFrame(args);
	    ProxyPlugin analyst = new Analyst(p);
        analyst.Init();
        ProxyPlugin capAnalyst = new CapAnalyst(p);
        capAnalyst.Init();
	    p.proxy.Start();
    }
}