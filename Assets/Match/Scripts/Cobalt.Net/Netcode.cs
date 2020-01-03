using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Cobalt.Ecs;
using NetcodeIO.NET;
using ProtoBuf;

//
// This is adapter classes for all netcode libraries used (Netcode, Reliable, ProtoBuf)
//

namespace Cobalt.Net
{
    public class NetcodeServer
    {
        public event Action<int> OnClientAdded;
        public event Action<int> OnClientRemoved;
        public event Action<int, NetcodeMessage> OnClientMessage;

        public bool IsRunning { get; private set; }
        public int NumClients => clients.Count;

        private Server server;
        private List<RemoteClient> clients;

        public NetcodeServer(int numPlayer, int port, int version, byte[] key)
        {
            clients = new List<RemoteClient>();

            server = new Server(numPlayer, IPAddress.Any.ToString(), port, (ulong)version, key);
            server.OnClientConnected += OnClientConnected;
            server.OnClientDisconnected += OnClientDisconnected;
            server.OnClientMessageReceived += OnClientMessageReceived;
        }

        public void Start() { IsRunning = true; server.Start(false); }
        public void Stop() { IsRunning = false; server.Stop(); }
        public void Update(double totalTime) { server.Tick(totalTime); }

        public void Send(NetcodeMessage message, QoS qos = QoS.Reliable)
        {
            NetcodeMessage.Serialize(message, out byte[] data, out int dataLength);

            foreach (var client in clients)
                server.SendPayload(client, data, dataLength);
        }

        #region Netcode.IO
        
        private void OnClientConnected(RemoteClient client)
        {
            Log.Info(this, $"Connect #{client.ClientID} (now {clients.Count+1} clients)");
            
            clients.Add(client);

            if (OnClientAdded != null)
                OnClientAdded((int)client.ClientID);
        }
        
        private void OnClientDisconnected(RemoteClient client)
        {
            Log.Info(this, $"Disconnect #{client.ClientID} (now {clients.Count-1} clients)");

            clients.Remove(client);

            if (OnClientRemoved != null)
                OnClientRemoved((int)client.ClientID);
        }

        private void OnClientMessageReceived(RemoteClient sender, byte[] payload, int payloadSize)
        {
            var message = NetcodeMessage.Deserialize(payload, payloadSize);

            if (OnClientMessage != null)
                OnClientMessage((int)sender.ClientID, message);
        }

        #endregion
    }

    public class NetcodeClient
    {
        public event Action<NetcodeMessage> OnMessage;

        public bool IsConnected => client.State == ClientState.Connected;

        private Client client;
        private byte[] clientToken;

        public NetcodeClient(byte[] token)
        {
            clientToken = token;
            client = new Client();
            client.OnStateChanged += OnStateChanged;
            client.OnMessageReceived += OnMessageReceived;
        }

        public void Connect()
        {
            client.Connect(clientToken, false);
        }

        public void Send(NetcodeMessage message, QoS qos = QoS.Reliable)
        {
            NetcodeMessage.Serialize(message, out byte[] data, out int dataLength);

            client.Send(data, dataLength);   
        }

        public void Update(double totalTime)
        {
            client.Tick(totalTime);
        }

        #region Netcode.IO
        
        private void OnStateChanged(ClientState state)
        {
            Log.Info(this, $"State: '{state}'");
        }

        private void OnMessageReceived(byte[] payload, int payloadSize)
        {
            var message = NetcodeMessage.Deserialize(payload, payloadSize);

            if (OnMessage != null)
                OnMessage(message);
        }

        #endregion
    }

    public enum QoS
    {
		/// Message is guaranteed to arrive and in order
		Reliable = 0,
		/// Message is not guaranteed delivery nor order
		Unreliable = 1,
		/// Message is not guaranteed delivery, but will be in order
		UnreliableOrdered = 2
    }

    public abstract class NetcodeMessage
    {
        private static Dictionary<Type, byte> types = new Dictionary<Type, byte>();
        private static Dictionary<byte, Type> codes = new Dictionary<byte, Type>();

        public static void Register<T>() where T : NetcodeMessage
        {
            var type = typeof(T);
            var code = (byte)types.Count;
            types[type] = code;
            codes[code] = type;
        }

        public static void Serialize(NetcodeMessage message, out byte[] data, out int dataLength)
        {
            var type = message.GetType();
            var typeCode = types[type];

            var stream = new MemoryStream();
            stream.WriteByte(typeCode);
            Serializer.Serialize(stream, message);
            
            data = stream.GetBuffer();
            dataLength = (int)stream.Position;
        }

        public static NetcodeMessage Deserialize(byte[] data, int dataLength)
        {
            var stream = new MemoryStream(data, 0, dataLength);

            var typeCode = (byte)stream.ReadByte();
            var type = codes[typeCode];
            var message = (NetcodeMessage)Serializer.Deserialize(type, stream);

            return message;
        }

        static NetcodeMessage()
        {
            Register<NetcodeMessageState>();
            Register<NetcodeMessageInput>();
        }
    }

    [ProtoContract]
    public class NetcodeMessageState : NetcodeMessage
    {
        [ProtoMember(1)]
        public MatchState state;
    }

    [ProtoContract]
    public class NetcodeMessageInput : NetcodeMessage
    {
        [ProtoMember(1)]
        public UnitInput input;
    }
}