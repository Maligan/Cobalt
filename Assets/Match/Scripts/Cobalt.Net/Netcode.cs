using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Cobalt.Ecs;
using NetcodeIO.NET;
using ProtoBuf;
using ReliableNetcode;

//
// This is adapter classes for all netcode libraries used (Netcode, Reliable, ProtoBuf)
//

namespace Cobalt.Net
{
    public class NetcodeServer
    {
        public event Action<int> OnClientAdded;
        public event Action<int> OnClientRemoved;
        public event Action<int, object> OnClientMessage;

        public bool IsRunning { get; private set; }
        public int NumClients => clients.Count;

        private Server server;
        private Dictionary<RemoteClient, ReliableEndpoint> clients;
        private bool isUpdating;

        public NetcodeServer(int numPlayer, int port, int version, byte[] key)
        {
            clients = new Dictionary<RemoteClient, ReliableEndpoint>();

            server = new Server(numPlayer, IPAddress.Any.ToString(), port, (ulong)version, key);
            server.OnClientConnected += OnClientConnected;
            server.OnClientDisconnected += OnClientDisconnected;
            server.OnClientMessageReceived += OnClientMessageReceived;

            server.LogLevel = NetcodeLogLevel.None;
        }

        public void Start() { IsRunning = true; server.Start(false); }
        public void Stop() { IsRunning = false; if (!isUpdating) server.Stop(); }
        
        public void Update(double totalTime)
        {
            isUpdating = true;

            foreach (var endpoint in clients.Values)
                endpoint.Update(totalTime);

            server.Tick(totalTime);

            isUpdating = false;

            if (!IsRunning)
                server.Stop();

        }

        public void Send(object message, QoS qos = QoS.Reliable)
        {
            NetcodeSerializer.Serialize(message, out byte[] data, out int dataLength);

            foreach (var client in clients.Values)
                client.SendMessage(data, dataLength, (QosType)qos);
        }

        #region Netcode.IO / Reliable.IO
        
        private void OnClientConnected(RemoteClient client)
        {
            Log.Info(this, $"#{client.ClientID} connected ({client.RemoteEndpoint}) ({clients.Count+1} total)");

            var clientId = client.ClientID;
            var clientEndpoint = new ReliableEndpoint((uint)clientId);

            clientEndpoint.TransmitExtendedCallback += OnReliableTransmit;
            clientEndpoint.ReceiveExtendedCallback += OnReliableReceive;

            clients.Add(client, clientEndpoint);

            if (OnClientAdded != null)
                OnClientAdded((int)client.ClientID);
        }
        
        private void OnClientDisconnected(RemoteClient client)
        {
            Log.Info(this, $"#{client.ClientID} disconnected ({clients.Count-1} total)");

            clients.Remove(client);

            if (OnClientRemoved != null)
                OnClientRemoved((int)client.ClientID);
        }

        private void OnClientMessageReceived(RemoteClient sender, byte[] payload, int payloadSize)
        {
            clients[sender].ReceivePacket(payload, payloadSize);
        }

        private void OnReliableTransmit(uint clientId, byte[] payload, int payloadSize)
        {
            foreach (var client in clients.Keys)
            {
                if (client.ClientID == clientId)
                {
                    server.SendPayload(client, payload, payloadSize);
                    break;             
                }
            }
        }

        private void OnReliableReceive(uint clientId, byte[] payload, int payloadSize)
        {
            var message = NetcodeSerializer.Deserialize(payload, payloadSize);

            if (OnClientMessage != null)
                OnClientMessage((int)clientId, message);
        }

        #endregion
    }

    public class NetcodeClient
    {
        public event Action<object> OnMessage;

        public bool IsConnected => client.State == ClientState.Connected;

        private Client client;
        private byte[] clientToken;
        private ReliableEndpoint clientEndpoint;

        public NetcodeClient(byte[] token)
        {
            clientToken = token;
            
            client = new Client();
            client.OnStateChanged += OnStateChanged;
            client.OnMessageReceived += OnMessageReceived;

            clientEndpoint = new ReliableEndpoint();
            clientEndpoint.TransmitCallback += OnReliableTransmit;
            clientEndpoint.ReceiveCallback += OnReliableReceive;
        }

        public void Connect()
        {
            client.Connect(clientToken, false);
        }

        public void Send(object message, QoS qos = QoS.Reliable)
        {
            if (IsConnected == false)
                throw new InvalidOperationException();

            NetcodeSerializer.Serialize(message, out byte[] data, out int dataLength);
            clientEndpoint.SendMessage(data, dataLength, (QosType)qos);
        }

        public void Update(double totalTime)
        {
            if (IsConnected)
                clientEndpoint.Update(totalTime);
            
            client.Tick(totalTime);
        }

        #region Netcode.IO / Reliable.IO
        
        private void OnStateChanged(ClientState state)
        {
            Log.Info(this, $"State: '{state}'");
        }

        private void OnMessageReceived(byte[] payload, int payloadSize)
        {
            clientEndpoint.ReceivePacket(payload, payloadSize);
        }

        private void OnReliableTransmit(byte[] payload, int payloadSize)
        {
            client.Send(payload, payloadSize);
        }

        private void OnReliableReceive(byte[] payload, int payloadSize)
        {
            var message = NetcodeSerializer.Deserialize(payload, payloadSize);

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

    public static class NetcodeSerializer
    {
        private static Dictionary<Type, byte> types = new Dictionary<Type, byte>();
        private static Dictionary<byte, Type> codes = new Dictionary<byte, Type>();

        static NetcodeSerializer()
        {
            Register<NetcodeMessageState>();
            Register<NetcodeMessageInput>();
        }

        private static void Register<T>()
        {
            var type = typeof(T);
            var code = (byte)types.Count;
            types[type] = code;
            codes[code] = type;
        }

        public static void Serialize(object message, out byte[] data, out int dataLength)
        {
            var type = message.GetType();
            if (types.ContainsKey(type) == false) throw new ArgumentException($"Type '{type.Name}' not registered for transmit");
            var typeCode = types[type];

            var stream = new MemoryStream();
            stream.WriteByte(typeCode);
            Serializer.Serialize(stream, message);
            
            data = stream.GetBuffer();
            dataLength = (int)stream.Position;
        }

        public static object Deserialize(byte[] data, int dataLength)
        {
            var stream = new MemoryStream(data, 0, dataLength);

            var typeCode = (byte)stream.ReadByte();
            if (codes.ContainsKey(typeCode) == false) throw new ArgumentException($"TypeCode '{typeCode}' not registered for transmit");
            var type = codes[typeCode];
            var message = Serializer.Deserialize(type, stream);

            return message;
        }
    }

    [ProtoContract]
    public class NetcodeMessageState
    {
        [ProtoMember(1)]
        public MatchState state;
    }

    [ProtoContract]
    public class NetcodeMessageInput
    {
        [ProtoMember(1)]
        public UnitInput input;
    }
}