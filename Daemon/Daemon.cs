using System;
using System.Threading.Tasks;
using Cobalt.Net;

namespace Cobalt
{
    public class Daemon
    {
        private LanServer _server;

        public static void Main(string[] args)
        {
            new Daemon().Start();
        }

        private void Start()
        {
            Log.Info(this, "Start...");
            _server = new LanServer();
            _server.Start(new ShardOptions());
            Tick();
        }

        private async void Tick()
        {
            var start = DateTime.Now;
            var step = 1000f/_server.Options.TPS;
            var frame = start;


            while (_server.IsRunning)
            {
                var now = DateTime.Now;
                
                var sinceFrame = (now - frame).TotalMilliseconds;
                if (sinceFrame > step)
                {
                    frame = now;
                    _server.Update((float)(now - start).TotalSeconds);
                }
            }

            _server.Stop();
        }
    }
}

/*
    GET /shards/
    GET /shards/{0}/auth
*/
