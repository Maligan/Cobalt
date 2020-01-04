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

        public void Tick(Match match, float sec)
        {
            server.Send(new NetcodeMessageState() { state = match.State });
        }
    }
}