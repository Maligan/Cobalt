using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;

namespace Cobalt.Core
{
    public class Shard
    {
        private Options options;

        private TokenFactory tokens;
        public Server server;
        private List<RemoteClient> clients;
        private float time;

        private State state;
        public Match match;

        public Shard(Options options)
        {
            this.options = options;
            
            state = State.Stop;

            tokens = new TokenFactory(options.version, GetKey(options.key));
            server = new Server(
                options.numPlayers,
                options.ip,
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
            match.tps = 60;
        }

        public bool IsRunning => state != State.Stop;

        public byte[] GetToken()
        {
            return tokens.GenerateConnectToken(
                new [] { new IPEndPoint(IPAddress.Parse(options.ip), options.port) },
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

        public void Tick(float sec)
        {
            time += sec;
            server.Tick(time);

            if (state != State.Play) return;

            var change = match.Tick(sec);
            if (change)
            {
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

                // match.tps = UnityEngine.Random.Range(10, 61);
            }
        }

        private void OnClientConnected(RemoteClient client)
        {
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
            public int      numPlayers  = 1;
            public string   ip          = IPAddress.Loopback.ToString();
            public int      port        = 8888;
            public string   key         = "key";
            public ulong    version     = 0;

            public int      timeout     = 3;
            public int      expiry      = int.MaxValue;
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