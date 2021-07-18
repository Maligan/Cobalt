using System;
using System.Collections.Generic;

namespace Cobalt.Net
{
    public class LanServer : IDisposable
    {
        public static int DEFAULT_AUTH_PORT = 8500;
        public static int DEFAULT_SPOT_PORT = 8501;

        public ShardOptions Options { get; private set; }

        private LanAuth _auth;
        private LanSpot _authSpot;
        private List<Shard> _shards;

        public LanServer()
        {
            _auth = new LanAuth(DEFAULT_AUTH_PORT, this);
            _authSpot = new LanSpot(DEFAULT_SPOT_PORT, 1, DEFAULT_AUTH_PORT);
        }

        public void Start()
        {
            _auth.Start();
            _authSpot.Start();
            _shards = new List<Shard>();

            // Add(new ShardOptions());
        }

        public string Add(ShardOptions options)
        {
            if (_shards == null)
                throw new InvalidOperationException();

            var shard = new Shard(options);
            shard.Start();

            lock (_shards)
                _shards.Add(shard);

            return (_shards.Count-1).ToString();
        }

        public IReadOnlyList<Shard> Shards => _shards;

        public byte[] Auth(string id)
        {
            if (_shards == null)
                throw new InvalidOperationException();

            var index = int.Parse(id);
            var shard = _shards[index];
            return shard.GetToken();
        }

        public void Tick()
        {
            if (_shards == null)
                throw new InvalidOperationException();

            lock (_shards)
            {
                for (var i = 0; i < _shards.Count; i++)
                {
                    if (_shards[i].IsRunning)
                    {
                        _shards[i].Tick();
                    }
                    else
                    {
                        Log.Info(this, "Drop #" + i);
                        var lastIndex = _shards.Count-1;
                        _shards[i] = _shards[lastIndex];
                        _shards.RemoveAt(lastIndex);
                        i--;
                    }
                }
            }
        }

        public void Dispose()
        {
            _auth.Stop();
            _authSpot.Stop();

            if (_shards != null)
                foreach (var shard in _shards)
                    shard.Stop();
        }
    }
}