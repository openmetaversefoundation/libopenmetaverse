using System;
using System.Collections.Generic;
using System.Text;

namespace Simian
{
    class MainEntry
    {
        static void Main(string[] args)
        {
            Simian simulator = new Simian();
            if (simulator.Start())
            {
                Console.WriteLine("Simulator is running. Press ENTER to quit");
                Console.ReadLine();
                simulator.Stop();
            }
        }
    }
}
