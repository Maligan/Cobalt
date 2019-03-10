using System;
using System.Net;
using System.Threading.Tasks;
using Cobalt.Core;

namespace Cobalt.Net
{
    public class TokenService
    {
        private HttpListener listener;
        private Shard shard;
        private int port;

        public string Prefix { get; private set; }

        public TokenService(int port, Shard shard)
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
            listener.Prefixes.Clear();
            
            foreach (var ip in NetUtils.GetSupportedIPs())
            {
                var prefix = string.Format("http://{0}:{1}/", ip, port);
                listener.Prefixes.Add(prefix);
            }

            listener.Start();

            while (listener.IsListening)
            {
                try { await Process(); }
                catch (Exception e) { Log.Error(e); }
            }
        }

        private async Task Process()
        {
            HttpListenerContext context = null;
            
            try { context = await listener.GetContextAsync(); }
            catch { return; /* Lister are closed - it's ok */ }

            var request = context.Request;
            var response = context.Response;

            var path = request.Url.LocalPath;
            if (path == "/join")
            {
                var tokenBytes = shard.GetToken();
                var token = Convert.ToBase64String(tokenBytes);

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