using Stormancer.Plugins.RemoteControl;

namespace Stormancer.Bots.Agent
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("arguments required : {endpoint} {account} {app} {name}");
            }



            ClientFactory.SetConfigFactory(() =>
            {
                var configuration = ClientConfiguration.Create(args[0], args[1], args[2]);

                configuration.Plugins.Add(new RemoteControlAgentPlugin());
                return configuration;

            });

            var client = ClientFactory.GetClient(0);

            var rcConfig = client.DependencyResolver.Resolve<RemoteControlConfiguration>();
            rcConfig.Id = args[3];

           
            var api = client.DependencyResolver.Resolve<RemoteControlAgentApi>();
            api.AddCommandHandler("shutdown", ctx =>
            {
                client.Disconnect();
                return Task.CompletedTask;
            });


            await api.Run();
            
           
        }
    }
}