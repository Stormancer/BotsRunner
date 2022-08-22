using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Stormancer.Plugins.RemoteControl;
using System.Collections.ObjectModel;

namespace Stormancer.Bots.Client
{
    [INotifyPropertyChanged]
    public partial class ManageCommandViewModel
    {
        private readonly AppViewModel parent;

        public ManageCommandViewModel(AppViewModel parent)
        {
            this.parent = parent;
        }

        [ObservableProperty]
        private string command;

        [ObservableProperty]
        private bool isRunning;
        private RemoteControlClientApi Api
        {
            get
            {
                var client = ClientFactory.GetClient(0);

                return client.DependencyResolver.Resolve<RemoteControlClientApi>();
            }
        }

        public ObservableCollection<AgentCommandOutputEntry> CommandOutput { get; }= new ObservableCollection<AgentCommandOutputEntry>();

        [RelayCommand]
        private async Task RunCommandAsync(CancellationToken cancellationToken)
        {
            
            try
            {
                
                if (string.IsNullOrWhiteSpace(Command))
                {
                    return;
                }

                var agentsVm = parent.Agents;

                if (agentsVm.SelectedAgents.Count == 0)
                {
                    return;
                }
                if(IsRunning)
                {
                    return;
                }

                IsRunning = true;
                CommandOutput.Clear();
               
                await foreach (var entry in Api.RunCommandAsync(Command, agentsVm.SelectedAgents.Cast<AgentViewModel>().Select(vm => vm.SessionId), cancellationToken))
                {
                    CommandOutput.Add(entry);
                }
            }
            catch(Exception ex)
            {
                CommandOutput.Add(new AgentCommandOutputEntry { AgentName = "-", Type="exception" });
            }
            finally
            {
                IsRunning = false;
            }
            
        }
    }
}