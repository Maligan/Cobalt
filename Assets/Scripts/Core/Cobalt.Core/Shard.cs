using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;

namespace Cobalt.Core
{
    public class Shard
    {
        public Options options;

        private TokenFactory tokens;
        public Server server;
        private List<RemoteClient> clients;
        private float time;
        private float timeDelta;

        private State state;
        public Match match;

        public Shard(Options options)
        {
            this.options = options;
            
            state = State.Stop;

            tokens = new TokenFactory(options.version, GetKey(options.key));
            server = new Server(
                options.numPlayers,
                IPAddress.Any.ToString(),
                options.port,
                options.version,
                GetKey(options.key)
            );

            server.OnClientConnected += OnClientConnected;
            server.OnClientDisconnected += OnClientDisconnected;
            server.OnClientMessageReceived += OnClientMessageReceived;

            server.LogLevel = NetcodeLogLevel.Debug;

            clients = new List<RemoteClient>();
            
            match = new Match();
        }

        public bool IsRunning => state != State.Stop;

        public byte[] GetToken()
        {
            return tokens.GenerateConnectToken(
                options.ips.Select(ip => new IPEndPoint(ip, options.port)).ToArray(),
                options.expiry,
                options.timeout,
                0,
                0,
                new byte[0]
            );
        }

        public void Start()
        {
            if (state != State.Stop)
                throw new Exception();

            time = 0;
            state = State.Lobby;
            server.Start(false);
        }

        public void Stop()
        {
            state = State.Stop;
            server.Stop();
            clients.Clear();
        }

        public void Update(float sec)
        {
            timeDelta -= sec;
            time += sec;
            server.Tick(time);
            
            if (timeDelta >= 0) return;

            timeDelta = 1/options.tps;

            if (state != State.Play) return;

            match.Tick(1/options.tps);

            try
            {
                var stream = new MemoryStream();
                Serializer.Serialize(stream, match.State);
                var bytes = stream.GetBuffer();

                foreach (var client in clients)
                    client.SendPayload(bytes, (int)stream.Position);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void OnClientConnected(RemoteClient client)
        {
            Utils.Log("[Shard] OnClientConnected");

            if (state != State.Lobby)
                throw new Exception();

            clients.Add(client);

            if (clients.Count == options.numPlayers)
                state = State.Play;
        }

        private void OnClientDisconnected(RemoteClient client)
        {
            if (state == State.Play)
                Stop();
            else
                clients.Remove(client);
        }

        private void OnClientMessageReceived(RemoteClient sender, byte[] payload, int payloadSize)
        {
        }

        public class Options
        {
            public int          numPlayers  = 1;
            public IPAddress[]  ips         = new [] { IPAddress.Loopback };
            public int          port        = 8889;
            public string       key         = "key";
            public ulong        version     = 0;

            public int          timeout     = 3;
            public int          expiry      = int.MaxValue;

            public int          tps         = 10;
        }

        private enum State
        {
            Stop,
            Lobby,
            Play
        }

        private static byte[] GetKey(string keyString)
        {
            var hash = SHA256.Create();
            var keyBytes = System.Text.Encoding.ASCII.GetBytes(keyString);
            var keyHash = hash.ComputeHash(keyBytes);
            hash.Dispose();

            return keyHash;
        }
    }


}