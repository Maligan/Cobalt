using System;
using System.Net;
using System.Threading.Tasks;

namespace Cobalt.Core.Net
{
    public class HttpService
    {
        private HttpListener listener;
        private ShardService shards;
        private int port;

        public string Prefix { get; private set; }

        public HttpService(int port, ShardService shards)
        {
            this.port = port;
            this.shards = shards;
            listener = new HttpListener();
        }

        public void Stop()
        {
            listener.Prefixes.Clear();
            listener.Stop();
        }

        public async void Start()
        {
            listener.Prefixes.Clear();
            foreach (var ip in SpotUtils.GetSupportedIPs())
            {
                var prefix = string.Format("http://{0}:{1}/", ip, port);
                listener.Prefixes.Add(prefix);
            }

            listener.Start();

            while (listener.IsListening)
            {
                try { await Process(); }
                catch (Exception e) { Utils.LogError(e); }
            }
        }

        private async Task Process()
        {
            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            var path = request.Url.LocalPath;
            if (path == "/auth")
            {
                var shard = shards.Peak();
                if (shard == null)
                    shard = shards.Create(new Shard.Options());

                var tokenBytes = shard.GetToken();
                var token = Convert.ToBase64String(tokenBytes);

                // Construct a response.
                string responseString = token;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = 403;
            }

            response.OutputStream.Close();
        }
    }
}