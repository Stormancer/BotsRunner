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
        private string command = String.Empty;

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

        [ObservableProperty]
        private string logs = String.Empty;

        [ObservableProperty]
        public object selectedBot;

        public void AddLog(DateTime date, string text)
        {
            _values.Add((date, text));
            _values.Sort((t1, t2) => (int)(t2.Item1.Ticks - t1.Item1.Ticks));
            Logs = String.Join('\n', _values.Select(tuple => $"{tuple.Item1:yyyy/MM/dd-HH:mm:ss:fff} : {tuple.Item2}"));
        }
        private List<(DateTime, string)> _values = new List<(DateTime, string)>();


        public ObservableCollection<BotViewModel> Bots { get; } = new ObservableCollection<BotViewModel>();

        [RelayCommand]
        private void CancelRunningCommand()
        {
            RunCommandCommand.Cancel();
        }

        [RelayCommand]
        private async Task RunCommandAsync(CancellationToken cancellationToken)
        {
            Bots.Clear();
            Logs = String.Empty;
            var dispatcher = Dispatcher.GetForCurrentThread();
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
                if (IsRunning)
                {
                    return;
                }

                IsRunning = true;
                Bots.Clear();

                await foreach (var entry in Api.RunCommandAsync(Command, agentsVm.SelectedAgents.Cast<AgentViewModel>().Select(vm => vm.SessionId), cancellationToken))
                {
                    if (dispatcher.IsDispatchRequired)
                    {
                        dispatcher.Dispatch(() => ProcessEntry(entry));
                    }
                    else
                    {
                        ProcessEntry(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                if (dispatcher.IsDispatchRequired)
                {
                    dispatcher.Dispatch(() => ProcessError(ex));
                }
                else
                {
                    ProcessError(ex);
                }
            }
            finally
            {
                IsRunning = false;
            }

        }

        private void ProcessError(Exception ex)
        {
            AddLog(DateTime.UtcNow, ex.ToString());
        }

        private void ProcessEntry(AgentCommandOutputEntry entry)
        {
            if (entry.Type.StartsWith("bot."))
            {
                var botId = entry.Result["botId"].ToObject<string>();

                BotViewModel? vm = null;
                foreach (var bot in Bots)
                {
                    if (bot.Name == botId)
                    {
                        vm = bot;
                    }
                }

                if (vm == null)
                {
                    vm = new BotViewModel { AgentName = entry.AgentName, Name = botId, Logs = "" };
                    Bots.Add(vm);
                }
                switch (entry.Type)
                {
                    case "bot.started":
                        vm.Running = true;
                        AddLog(entry.Result["createdOn"].ToObject<DateTime>(), $"started {botId}");
                        break;
                    case "bot.stopped":
                        vm.Running = false;
                        AddLog(entry.Result["createdOn"].ToObject<DateTime>(), $"stopped {botId}");
                        break;
                    case "bot.output":
                        vm.AddLog(entry.Result["createdOn"].ToObject<DateTime>(), $"Output | {entry.Result["data"].ToObject<string>()}");
                        break;
                    case "bot.error":
                        vm.AddLog(entry.Result["createdOn"].ToObject<DateTime>(), $"Error | {entry.Result["data"].ToObject<string>()}");
                        break;
                    default:

                        break;
                }



            }
            else
            {
                AddLog(entry.Result["createdOn"]?.ToObject<DateTime>() ?? DateTime.UtcNow, entry.Result.ContainsKey("msg") ? entry.Result["msg"].ToObject<string>() : entry.Result.ToString());
            }
        }
    }
}