using System;
using System.Threading;
using System.Threading.Tasks;
using Cobalt.Net;

namespace Cobalt
{
    public class Daemon
    {
        public static void Main(string[] args)
        {
            var server = new LanServer();
            server.Start();

            while (true)
            {
                server.Tick();
                Thread.Sleep(1);
            }
        }
    }
}

/*
    GET /list
    GET /auth&id=
    GET /auth&options=
*/