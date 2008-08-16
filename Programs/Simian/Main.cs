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
            simulator.Start(9000, false);

            Console.WriteLine("Simulator is running. Press ENTER to quit");

            Console.ReadLine();

            simulator.Stop();
        }
    }
}
