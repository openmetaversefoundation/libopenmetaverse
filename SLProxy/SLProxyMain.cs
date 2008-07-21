using System;
using System.Reflection;
using SLProxy;

class ProxyMain
{
    public static void Main(string[] args)
    {
        ProxyFrame p = new ProxyFrame(args);
	ProxyPlugin analyst = new Analyst(p);
        analyst.Init(); 
	p.proxy.Start();
    }
}