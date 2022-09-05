namespace Stormancer.Bots.Client;

public partial class SignInPage : ContentPage
{
	public SignInPage()
	{
		InitializeComponent();
		var vm = App.Current.Services.GetRequiredService<SignInViewModel>();
		BindingContext = vm;
		_ = vm.SignIn();
	}
}