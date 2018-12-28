using System;
using System.Collections.Generic;
using System.Net;
// using System.Net.Http;
using System.Threading;
using Cobalt.Core;

namespace Cobalt
{
    public class Daemon
    {
        private static ShardCollection shards;

        public static void Main(string[] args)
        {
            shards = new ShardCollection();
            // shards.Create(new Shard.Options());
            // shards.Create(new Shard.Options() { port = 8890 });
            
            HttpProc(null);

            Console.ReadLine();
        }

        private static void HttpProc(object stateInfo)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/auth/");

            Console.WriteLine("Listening...");
            listener.Start();

            while (true)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                var shard = shards.Peak();
                if (shard == null)
                    shard = shards.Create(new Shard.Options());

                var tokenBytes = shard.GetToken();
                var token = Convert.ToBase64String(tokenBytes);

                // Construct a response.
                string responseString = token;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }

            listener.Stop();
        }

        
    }
}


public class ShardCollection
{
    private List<Shard> shards = new List<Shard>();

    public Shard Create(Shard.Options options)
    {
        Clean();
        var shard = new Shard(options);
        shards.Add(shard);
        Utils.Log("[Shard #{0}] Start on {1}:{2}", shards.Count, shard.options.ip, shard.options.port);
        ThreadPool.QueueUserWorkItem(ShardProc, shard);
        return shard;
    }

    public Shard Peak()
    {
        Clean();

        return shards.Count > 0
             ? shards[0]
             : null;
    }

    private void Clean()
    {
        for (var i = 0; i < shards.Count; i++)
        {
            var shard = shards[i];
            if (shard.IsRunning == false)
            {
                shards[i] = shards[shards.Count-1];
                shards.RemoveAt(shards.Count-1);
            }
        }
    }

    private void ShardProc(object stateInfo)
    {
        var shard = (Shard)stateInfo;
        var deltaSec = 1f / shard.options.tps;
        var deltaMs = (int)(deltaSec * 1000);

        shard.Start();

        while (shard.IsRunning)
        {
            shard.Tick(deltaSec);
            Thread.Sleep(deltaMs);
        }
    }
}



/*
    GET /shards/
    GET /shards/{0}/auth
*/