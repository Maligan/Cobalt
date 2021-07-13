using System;
using System.Threading.Tasks;
using Cobalt.Net;

namespace Cobalt
{
    public class Daemon
    {
        public static void Main(string[] args)
        {
            var server = new LanServer();

            while (true)
            {
                server.Start(new ShardOptions());

                var start = DateTime.Now;
                var step = 1000f/server.Options.TPS;
                var frame = start;

                while (server.IsRunning)
                {
                    var now = DateTime.Now;
                    
                    var sinceFrame = (now - frame).TotalMilliseconds;
                    if (sinceFrame > step)
                    {
                        frame = now;
                        server.Update((float)(now - start).TotalSeconds);
                    }
                }
            }
        }
    }
}

/*
    GET /shards/
    GET /shards/{0}/auth
*/
