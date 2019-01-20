using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;
using Cobalt.Core;

namespace Cobalt.Core
{
    public class Shard
    {
        public ShardOptions options;

        private State state;

        private TokenFactory tokens;
        private Server server;
        private List<RemoteClient> clients;
        private Match match;

        private float time;

        public Shard(ShardOptions options)
        {
            this.options = options;
            
            state = State.Stop;
            tokens = new TokenFactory(options.version, ShardOptions.GetKey(options.key));
            clients = new List<RemoteClient>();
        }

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

            server = new Server(
                options.numPlayers,
                IPAddress.Any.ToString(),
                options.port,
                options.version,
                ShardOptions.GetKey(options.key)
            );

            server.OnClientConnected += OnClientConnected;
            server.OnClientDisconnected += OnClientDisconnected;
            server.OnClientMessageReceived += OnClientMessageReceived;
            server.Start(false);

            match = new Match();
        }

        public void Stop()
        {
            if (state == State.Stop)
                throw new Exception();

            state = State.Stop;

            clients.Clear();
            
            server.OnClientConnected -= OnClientConnected;
            server.OnClientDisconnected -= OnClientDisconnected;
            server.OnClientMessageReceived -= OnClientMessageReceived;
            server.Stop();
            server = null;

            match = null;
        }

        public void Tick(float sec)
        {
            if (state == State.Stop) return;

            time += sec;
            server.Tick(time);

            if (state != State.Play) return;

            var tickIndexBefore = (int)((time - sec) / options.spt);
            var tickIndexAfter = (int)(time / options.spt);
            if (tickIndexBefore != tickIndexAfter)
            {
                match.Tick(options.spt);
                clients.Send(match.State);
            }
        }

        #region Netcode Events

        private void OnClientConnected(RemoteClient client)
        {
            Utils.Log("[Shard] Client Connected #" + client.ClientID);

            if (state != State.Lobby)
                throw new Exception();

            clients.Add(client);

            if (clients.Count == options.numPlayers)
                state = State.Play;
        }

        private void OnClientDisconnected(RemoteClient client)
        {
            Utils.Log("[Shard] Client Disconnected #" + client.ClientID);

            if (state == State.Play)
                Stop();
            else
                clients.Remove(client);
        }

        private void OnClientMessageReceived(RemoteClient sender, byte[] payload, int payloadSize)
        {
            var stream = new MemoryStream(payload, 0, payloadSize);
            var input = Serializer.Deserialize<UnitInput>(stream);
            match.State.inputs[0] = input;
        }

        #endregion

        public bool IsRunning => state != State.Stop;

        private enum State
        {
            Stop,
            Lobby,
            Play
        }
    }

    public class ShardOptions
    {
        public int          numPlayers  = 1;
        public IPAddress[]  ips         = new [] { IPAddress.Loopback };
        public int          port        = 8889;
        public string       key         = "key";
        public ulong        version     = 0;

        public int          timeout     = int.MaxValue;
        public int          expiry      = int.MaxValue;

        public int          tps         = 20;
        public float        spt        => 1f / tps;

        internal static byte[] GetKey(string keyString)
        {
            var hash = SHA256.Create();
            var keyBytes = System.Text.Encoding.ASCII.GetBytes(keyString);
            var keyHash = hash.ComputeHash(keyBytes);
            hash.Dispose();

            return keyHash;
        }
    }
}