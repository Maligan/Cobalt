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
        public ShardOptions Options { get; private set; }

        private enum State { Stop, Lobby, Play }
        private State state;

        private TokenFactory tokens;
        private Server server;
        private List<RemoteClient> clients;
        private Match match;

        private float time;
        private float timeCursor;

        public Shard(ShardOptions options)
        {
            Options = options;
            
            state = State.Stop;
            tokens = new TokenFactory(options.Version, options.KeyHash);
            clients = new List<RemoteClient>();
        }

        public void Start()
        {
            if (state != State.Stop)
                throw new InvalidOperationException();

            Log.Info("[Shard] Bind to {0}", Options.Port);

            time = 0;
            state = State.Lobby;

            server = new Server(
                Options.NumPlayers,
                IPAddress.Any.ToString(),
                Options.Port,
                Options.Version,
                Options.KeyHash
            );

            server.LogLevel = NetcodeLogLevel.Info;

            server.OnClientConnected += OnClientConnected;
            server.OnClientDisconnected += OnClientDisconnected;
            server.OnClientMessageReceived += OnClientMessageReceived;
            server.Start(false);

            match = new Match();
        }

        public void Stop()
        {
            if (state == State.Stop) throw new InvalidOperationException();

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

            if (timeCursor == 0) timeCursor = time;

            var dt = Options.SPT;
            while (timeCursor + dt < time)
            {
                timeCursor += dt;

                Log.Info("Tick #" + (int)(timeCursor*Options.TPS));
                match.Tick(Options.SPT);
                clients.Send(match.State);
            }
        }

        #region Netcode Events

        private void OnClientConnected(RemoteClient client)
        {
            if (state != State.Lobby) throw new InvalidOperationException();

            Log.Info("[Shard] Client Connected #{0} ({1}/{2})", client.ClientID, clients.Count+1, Options.NumPlayers);

            clients.Add(client);

            if (clients.Count == Options.NumPlayers)
                state = State.Play;
        }

        private void OnClientDisconnected(RemoteClient client)
        {
            Log.Info("[Shard] Client Disconnected #{0} ({1}/{2})", client.ClientID, clients.Count-1, Options.NumPlayers);

            if (state == State.Play) Stop();
            else clients.Remove(client);
        }

        private void OnClientMessageReceived(RemoteClient sender, byte[] payload, int payloadSize)
        {
            var stream = new MemoryStream(payload, 0, payloadSize);
            var input = Serializer.Deserialize<UnitInput>(stream);
            match.State.inputs[0] = input;
        }

        #endregion

        public bool IsRunning => state != State.Stop;
    }

    public class ShardOptions
    {
        public static int DEFAULT_PORT = 4123;
        public static int DEFAULT_TPS  = 10;

        public int          NumPlayers   = 1;
        public IPAddress[]  IPs          = null;
        public int          Port         = DEFAULT_PORT;
        public string       Key          = null;
        public byte[]       KeyHash      => GetKey(Key);
        public ulong        Version      = 0;

        // Таймаут соединения  между клиентом и сервером;
        public int          TokenTimeout = 3; 
        public int          TokenExpiry  = int.MaxValue;

        public int          TPS          = DEFAULT_TPS;
        public float        SPT          => 1f / TPS;

        private static byte[] GetKey(string keyString)
        {
            if (keyString == null)
                keyString = string.Empty;

            var hash = SHA256.Create();
            var keyBytes = System.Text.Encoding.ASCII.GetBytes(keyString);
            var keyHash = hash.ComputeHash(keyBytes);
            hash.Dispose();

            return keyHash;
        }

        public byte[] GetToken(ulong userId, byte[] userData = null)
        {
            var addresses = IPs == null ? NetUtils.GetSupportedIPs().Select(x => x.Address) : IPs;
            var addressList = addresses.Select(addr => new IPEndPoint(addr, Port)).ToArray();

            var tokenFactory = new TokenFactory(Version, KeyHash);
            var tokenBytes = tokenFactory.GenerateConnectToken(
                addressList,
                TokenExpiry,
                TokenTimeout,
                0,
                userId,
                userData ?? new byte[0]
            );

            return tokenBytes;
        }
    }
}