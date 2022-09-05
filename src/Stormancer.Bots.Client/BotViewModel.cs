using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Bots.Client
{
 
    [ObservableObject]
    public partial class BotViewModel
    {
        public string Name { get; set; }
    
        public string AgentName { get; set; }

        [ObservableProperty]
        private bool running;

        [ObservableProperty]
        private string logs;

        public void AddLog(DateTime date, string text)
        {
            _values.Add((date, text));
            _values.Sort((t1, t2) => (int)(t2.Item1.Ticks - t1.Item1.Ticks));
            Logs = String.Join('\n', _values.Select(tuple => $"{tuple.Item1:yyyy/MM/dd-HH:mm:ss:fff} : {tuple.Item2}"));
        }
        private List<(DateTime, string)> _values = new List<(DateTime, string)>();
    }
}
