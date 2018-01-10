using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcLib;
using System.Diagnostics;

namespace BlockChainConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("Init BlockChain");
            var chain = new BlockChain();
            Debug.WriteLine("Init WebServer");
            var server = new WebServer(chain);
            System.Console.Read();
        }
    }
}
