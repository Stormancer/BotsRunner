using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Stormancer.Plugins.RemoteControl;
using Stormancer.Server.Plugins.RemoteControl;
using System.Collections.ObjectModel;

namespace Stormancer.Bots.Client
{
    [INotifyPropertyChanged]
    public partial class ManageAgentsViewModel
    {
        private readonly AppViewModel parent;

        internal ManageAgentsViewModel(AppViewModel parent)
        {
            this.parent = parent;
        }

        [RelayCommand]
        private async Task LoadAgentsAsync()
        {
            Agents.Clear();
            var results = await Api.SearchAgentsAsync("{}", 100, 0, CancellationToken.None);


            foreach(var result in results.Hits)
            {
                Agents.Add(new AgentViewModel(result.Source));
            }
        }

        private RemoteControlClientApi Api
        {
            get
            {
                var client = ClientFactory.GetClient(0);

                return client.DependencyResolver.Resolve<RemoteControlClientApi>();
            }
        }


        public ObservableCollection<AgentViewModel> Agents { get; set; } = new ObservableCollection<AgentViewModel>();
        public ObservableCollection<object> SelectedAgents { get; set; } = new ObservableCollection<object>();
    }


    [INotifyPropertyChanged]
    public partial class AgentViewModel
    {
        private readonly Agent model;

        internal AgentViewModel(Agent model)
        {
            this.model = model;
            this.SessionId = model.SessionId;
            this.Name = model.Name.ToString();
        }
        [ObservableProperty]
        private string name;
      

        public SessionId SessionId { get; }
    }
}