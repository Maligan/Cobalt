using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Cobalt.Ecs;
using NetcodeIO.NET;

namespace Cobalt.Net
{
    //
    // Stadalone dedicated server
    //
    public class Shard
    {
        public ShardOptions Options { get; private set; }

        public bool IsRunning => state != State.Stop;
        public int Port => server.Port;
        public int NumClients => server.NumClients;

        private string logTag => nameof(Shard) + "#" + server.Port;

        private enum State { Stop, Pause, Play }
        private State state;

        private TokenFactory tokens;
        private int tokensCount;
        private NetcodeServer server;
        private Match match;

        private DateTime start;

        public Shard(ShardOptions options)
        {
            Options = options;
            
            state = State.Stop;
            tokens = new TokenFactory((ulong)options.Version, options.KeyHash);
        }

        public void Start()
        {
            if (state != State.Stop)
                throw new InvalidOperationException();

            state = State.Pause;

            server = new NetcodeServer(
                Options.NumPlayers,
                Options.Port,
                Options.Version,
                Options.KeyHash
            );

            server.OnClientAdded += OnClientConnected;
            server.OnClientRemoved += OnClientDisconnected;
            server.OnClientMessage += OnClientMessage;
            server.Start();

            match = new Match();
            match.Add(new NetcodeSystem(server));

            Log.Info(logTag, "Started");
        }

        public void Stop()
        {
            if (state == State.Stop) return;

            Log.Info(logTag, "Stop");

            state = State.Stop;

            server.OnClientAdded -= OnClientConnected;
            server.OnClientRemoved -= OnClientDisconnected;
            server.OnClientMessage -= OnClientMessage;
            server.Stop();
            server = null;

            match = null;
        }

        public void Tick()
        {
            if (state == State.Stop)
                return;

            server.Tick();

            if (state == State.Play)
            {
                var total = (DateTime.Now - start).TotalMilliseconds;
                if (total >= match.State.time + Options.MSPT)
                    match.Update(Options.MSPT);
            }
        }

        public byte[] GetToken()
        {
            var addresses = Options.IPs ?? NetUtils.GetUnicasts().Select(x => x.Address);
            var addressList = addresses.Select(addr => new IPEndPoint(addr, server.Port)).ToArray();

            var tokenBytes = tokens.GenerateConnectToken(
                addressList,
                Options.TokenExpiry,
                Options.Timeout,
                (ulong)tokensCount++,
                (ulong)server.NumClients,
                new byte[0]
            );

            return tokenBytes;
        }

        #region Netcode Events

        private void OnClientConnected(int clientId)
        {
            if (state != State.Pause)
                throw new InvalidOperationException();

            if (server.NumClients == Options.NumPlayers)
            {
                start = DateTime.Now;
                state = State.Play;
            }
        }

        private void OnClientDisconnected(int clientId)
        {
            if (state == State.Play)
                Stop();
        }

        private void OnClientMessage(int clientId, object message)
        {
            var input = (NetcodeMessageInput)message;
            match.State.inputs[clientId] = input.input;
        }

        #endregion
    }

    public class ShardOptions
    {
        public static int DEFAULT_TPS  = 10;

        public IPAddress[]  IPs          = null;
        public int          Port         = 0;
        public string       Key          = null;
        public byte[]       KeyHash      => GetKeyHash(Key);
        public int          Version      = 0;

        public int          NumPlayers   = 1;

        // Таймаут соединения  между клиентом и сервером;
        public int          Timeout = 10;
        public int          TokenExpiry  = 30;

        public int          TPS          = DEFAULT_TPS; // Ticks per Seconds
        public int          MSPT         => 1000 / TPS; // Milliseconds per Tick

        private static byte[] GetKeyHash(string keyString)
        {
            if (keyString == null)
                keyString = string.Empty;

            var hash = SHA256.Create();
            var keyBytes = System.Text.Encoding.ASCII.GetBytes(keyString);
            var keyHash = hash.ComputeHash(keyBytes);
            hash.Dispose();

            return keyHash;
        }
    }
}