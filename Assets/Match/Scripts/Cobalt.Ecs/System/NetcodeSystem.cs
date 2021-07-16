using Cobalt.Net;

namespace Cobalt.Ecs
{
    public class NetcodeSystem : IMatchSystem
    {
        private NetcodeServer server;

        public NetcodeSystem(NetcodeServer server)
        {
            this.server = server;
        }

        public void Update(Match match, int dt)
        {
            server.Send(new NetcodeMessageState() { state = match.State });
        }
    }
}