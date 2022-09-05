using CommunityToolkit.Mvvm.ComponentModel;
using Stormancer.Plugins.RemoteControl;
using Stormancer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Diagnostics;

namespace Stormancer.Bots.Client
{

    [INotifyPropertyChanged]
    public partial class AppViewModel
    {
        private class DebugLogger : Stormancer.Diagnostics.ILogger
        {
            public void Log(LogLevel level, string category, string message, object data = null)
            {
                System.Diagnostics.Debug.WriteLine($"{level} : {category} | {message}");
            }
        }


        public AppViewModel()
        {
            ClientFactory.SetConfigFactory(() =>
            {
                var configuration = ClientConfiguration.Create("http://localhost", "remoteControl", "dev-client");

                configuration.Plugins.Add(new RemoteControlClientPlugin());
                configuration.Plugins.Add(new AuthenticationPlugin());
                configuration.Logger = new DebugLogger();
                return configuration;

            });
            Agents  = new ManageAgentsViewModel(this);
            Command = new ManageCommandViewModel(this);

        }


        public ManageAgentsViewModel Agents { get; }

        public ManageCommandViewModel Command { get; }

    }



}
