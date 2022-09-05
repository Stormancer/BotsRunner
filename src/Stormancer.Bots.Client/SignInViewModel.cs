using CommunityToolkit.Mvvm.ComponentModel;
using Stormancer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Bots.Client
{
    [INotifyPropertyChanged]
    public partial class SignInViewModel
    {

        [ObservableProperty]
        private bool isConnecting;

        [ObservableProperty]
        private bool isConnected;

        private UserApi Users
        {
            get
            {
                var client = ClientFactory.GetClient(0);

                return client.DependencyResolver.Resolve<UserApi>();
            }
        }
        public SignInViewModel()
        {
            if (Users != null)
            {
                Users.OnGameConnectionStateChanged = (GameConnectionStateCtx ctx) =>
                {
                    switch (ctx.State)
                    {
                        case GameConnectionState.Reconnecting:
                        case GameConnectionState.Authenticating:
                        case GameConnectionState.Connecting:
                            IsConnecting = true;
                            IsConnected = false;
                            break;

                        case GameConnectionState.Authenticated:
                            IsConnecting = false;
                            IsConnected = true;
                            break;
                        case GameConnectionState.Disconnected:
                        case GameConnectionState.Disconnecting:
                            IsConnecting = false;
                            IsConnected = false;
                            break;
                        default:
                            break;
                    }
                };

                Users.OnGetAuthParameters = () => Task.FromResult(new AuthParameters { Type = "ephemeral" });
            }

        }
        public async Task SignIn()
        {


            if (Users.State == GameConnectionState.Disconnected)
            {
                await Users.Login();
            }

            //Login succeed, move to main page.
            await Shell.Current.GoToAsync(new ShellNavigationState("//Main"));


        }
    }
}
