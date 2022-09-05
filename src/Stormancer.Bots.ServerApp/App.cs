using Stormancer.Plugins;
using Stormancer.Server;

namespace Stormancer.Bots.ServerApp
{
    public class App
    {
        public void Run(IAppBuilder builder)
        {
            builder.AddPlugin(new BotsControlServerPlugin());
        }
    }

    internal class BotsControlServerPlugin : IHostPlugin
    {
        public void Build(HostPluginBuildContext ctx)
        {
            ctx.HostStarting += (IHost host) =>
            {
                host.ConfigureUsers(u => u.ConfigureEphemeral(e => e.Enabled()));
            };
        }
    }
}