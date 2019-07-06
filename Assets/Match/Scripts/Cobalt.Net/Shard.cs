using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;

namespace Cobalt.Net
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
        private float timeCursor;

        public Shard(ShardOptions options)
        {
            this.options = options;
            
            state = State.Stop;
            tokens = new TokenFactory(options.Version, options.Hash);
            clients = new List<RemoteClient>();
        }

        public byte[] GetToken()
        {
            return tokens.GenerateConnectToken(
                options.IPs.Select(ip => new IPEndPoint(ip, options.Port)).ToArray(),
                options.TokenExpiry,
                options.TokenTimeout,
                0,
                0,
                new byte[0]
            );
        }

        public void Start()
        {
            if (state != State.Stop)
                throw new InvalidOperationException();

            Log.Info("[Shard] Start on {0} port", options.Port);

            time = 0;
            state = State.Lobby;

            server = new Server(
                options.NumPlayers,
                //IPAddress.Any.ToString(),
                options.IPs.First().ToString(),
                options.Port,
                options.Version,
                options.Hash
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

            Log.Info("[Shard] Stop");

            state = State.Stop;

            clients.Clear();
            
            server.OnClientConnected -= OnClientConnected;
            server.OnClientDisconnected -= OnClientDisconnected;
            server.OnClientMessageReceived -= OnClientMessageReceived;
            server.Stop();
            server = null;

            match = null;
        }

        public void Tick(float time)
        {
            if (state == State.Stop) return;

            this.time = time;
            server.Tick(time);

            if (state != State.Play) return;

            if(timeCursor == 0) timeCursor = time;

            var dt = options.SPT;
            while (timeCursor + dt < time)
            {
                timeCursor += dt;

                Log.Info("Tick #" + (int)(timeCursor*options.TPS));
                match.Tick(options.SPT);
                clients.Send(match.State);
            }
        }

        #region Netcode Events

        private void OnClientConnected(RemoteClient client)
        {
            if (state != State.Lobby)
                throw new InvalidOperationException();

            Log.Info("[Shard] Client Connected ({0}/{1})", clients.Count+1, options.NumPlayers);

            clients.Add(client);

            if (clients.Count == options.NumPlayers)
                state = State.Play;
        }

        private void OnClientDisconnected(RemoteClient client)
        {
            Log.Info("[Shard] Client Disconnected #" + client.ClientID);

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

        private enum State { Stop, Lobby, Play }
    }

    public class ShardOptions
    {
        /// Default IP port
        public static int PORT = 4123;

        public int          NumPlayers   = 1;
        public IPAddress[]  IPs          = new [] { IPAddress.Loopback };
        public int          Port         = PORT;
        public string       Key          = "anonymous";
        public ulong        Version      = 0;

        public int          TokenTimeout = int.MaxValue;
        public int          TokenExpiry  = int.MaxValue;

        public int          TPS          = 60;
        public float        SPT          => 1f / TPS;

        public byte[] Hash
        {
            get { return GetKey(Key); }
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