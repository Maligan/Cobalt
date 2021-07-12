using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Cobalt.Net
{
    /// Simple HTTP server which response any valid /auth request with connection token for specified shard 
    public class LanAuth
    {
        private HttpListener listener;
        private LanServer shard;
        private int port;

        public string Prefix { get; private set; }

        public LanAuth(int port, LanServer shard)
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
            var ips = NetUtils.GetSupportedIPs();
            var prefixes = ips.Select(x => $"http://{x.Address}:{port}/").ToList();

            Log.Info(this, $"Start on ({string.Join(", ", prefixes)})");

            listener.Prefixes.Clear();
            foreach (var prefix in prefixes)
                listener.Prefixes.Add(prefix);

            listener.Start();

            while (listener.IsListening)
            {
                try { await ProcessRequest(); }
                catch (Exception e) { Log.Error(this, e); }
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
            Log.Info(this, "Request " + path);
            
            if (path == "/auth")
            {                
                var tokenBytes = shard.GetToken();
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