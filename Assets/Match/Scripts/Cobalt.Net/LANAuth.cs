using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cobalt.Net
{
    /// Simple HTTP server which response any valid /auth request with connection token for specified shard 
    public class LanAuth
    {
        private HttpListener _listener;
        private LanServer _server;
        private int _port;

        public string Prefix { get; private set; }

        public LanAuth(int port, LanServer server)
        {
            _port = port;
            _server = server;
            _listener = new HttpListener();
        }

        public void Stop()
        {
            Log.Info(this, "Stop");

            _listener.Stop();
        }

        public async void Start()
        {
            var ips = NetUtils.GetUnicasts();
            var prefixes = ips.Select(x => $"http://{x.Address}:{_port}/").ToList();

            Log.Info(this, $"Start (on {string.Join(", ", prefixes)})");

            _listener.Prefixes.Clear();
            foreach (var prefix in prefixes)
                _listener.Prefixes.Add(prefix);

            _listener.Start();

            while (_listener.IsListening)
            {
                try { await ProcessRequest(); }
                catch (Exception e) { Log.Error(this, e); }
            }
        }

        private async Task ProcessRequest()
        {
            HttpListenerContext context = null;
            
            try { context = await _listener.GetContextAsync(); }
            catch { return; /* listener is closed - it's ok */ }

            Log.Info(this, $"Request '{context.Request.Url.PathAndQuery}' (from {context.Request.RemoteEndPoint})");

            var responseData = GetResponse(context.Request);
            if (responseData != null)
            {                
                context.Response.ContentLength64 = responseData.Length;
                context.Response.OutputStream.Write(responseData, 0, responseData.Length);
            }
            else
            {
                context.Response.StatusCode = 403;
            }

            context.Response.OutputStream.Close();
        }

        private byte[] GetResponse(HttpListenerRequest request)
        {
            switch (request.Url.LocalPath)
            {
                case "/list":
                    var rows = new List<string>();

                    for (var i = 0; i < _server.Shards.Count; i++)
                    {
                        // XXX: Multithreading!
                        var shard = _server.Shards[i]; 
                        var values = new List<string>();
                        values.Add(request.LocalEndPoint.Address.ToString());
                        values.Add(shard.Port.ToString());
                        values.Add(shard.NumClients.ToString());
                        rows[i] = string.Join(", ", values);
                    }

                    var responseString = string.Join("\n", rows);
                    return Encoding.ASCII.GetBytes(responseString);

                case "/auth":
                    var id = request.QueryString["id"];
                    if (id == null)
                        id = _server.Add(new ShardOptions());

                    return _server.Auth(id);
            }

            return null;
        }
    }
}