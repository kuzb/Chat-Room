using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace myApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter a numeric argument.");
                System.Console.WriteLine("Usage: App <Port>");
            }            
            Server chatServer = new Server();
            Thread thrServerChat = new Thread(() => chatServer.startListen(args[0]));
            thrServerChat.Start();
        }
    }
}
