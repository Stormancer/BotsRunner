﻿namespace Stormancer.Bots.Client
{
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
            BindingContext = new AppViewModel();
            MainPage = new AppShell();
           
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<AppViewModel>();
            services.AddTransient<SignInViewModel>();
            services.AddTransient<ManageAgentsViewModel>();
            services.AddTransient<ManageCommandViewModel>();
           

            return services.BuildServiceProvider();
        }
    }
}