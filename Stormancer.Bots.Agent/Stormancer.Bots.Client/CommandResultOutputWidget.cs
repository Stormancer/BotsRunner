using Stormancer.Plugins.RemoteControl;
using System.Globalization;
using System.Windows.Markup;

namespace Stormancer.Bots.Client;

public class CommandResultOutputWidget : ContentView
{
    private class AgentCommandOutputEntryValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is AgentCommandOutputEntry entry)
            {
                return entry.Type switch
                {
                    "output" => entry.Result["data"].ToObject<string>(),
                    _ => $"{entry.Type} {entry.Result}"
                };
            }
            else
            {
                return value?.ToString()??String.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    private AgentCommandOutputEntry Model => (AgentCommandOutputEntry)BindingContext;
    public CommandResultOutputWidget()
    {
        var origin = new Label();
        origin.SetBinding(Label.TextProperty, "AgentName");
        var text = new Label();
        text.SetBinding(Label.TextProperty, ".",BindingMode.Default,new AgentCommandOutputEntryValueConverter());
        
       var layout = new HorizontalStackLayout
        {

            Children = {
                origin,
                text
            }

        };

        Content = layout;
    }

    private string GetText()
    {
        return Model.Type switch
        {
            "output" => Model.Result["data"].ToObject<string>(),
            _ => $"{Model.Type} {Model.Result}"
        };
    }
}