using System;
using System.Net;
using System.Threading.Tasks;

namespace Cobalt.Net
{
    /// Simple HTTP server which response any valid /auth request with connection token for specified shard 
    public class LANAuth
    {
        private HttpListener listener;
        private LANServer shard;
        private int port;

        public string Prefix { get; private set; }

        public LANAuth(int port, LANServer shard)
        {
            this.port = port;
            this.shard = shard;
            listener = new HttpListener();
        }

        public void Stop()
        {
            listener.Stop();
        }

        public async void Start()
        {
            Log.Info("[LANAuth] Start");

            listener.Prefixes.Clear();
            
            foreach (var ip in NetUtils.GetSupportedIPs())
            {
                var prefix = string.Format("http://{0}:{1}/", ip.Address, port);
                Log.Info("[LANAuth] Prefix " + prefix);
                listener.Prefixes.Add(prefix);
            }

            listener.Start();

            while (listener.IsListening)
            {
                try { await ProcessRequest(); }
                catch (Exception e) { Log.Error(e); }
            }
        }

        private async Task ProcessRequest()
        {
            HttpListenerContext context = null;
            
            try { context = await listener.GetContextAsync(); }
            catch { return; /* Lister are closed - it's ok */ }

            var request = context.Request;
            var response = context.Response;

            var path = request.Url.LocalPath;
            if (path == "/auth")
            {
                var tokenBytes = shard.Options.GetToken(0);
                response.ContentLength64 = tokenBytes.Length;
                response.OutputStream.Write(tokenBytes, 0, tokenBytes.Length);
            }
            else
            {
                response.StatusCode = 403;
            }

            response.OutputStream.Close();
        }
    }
}