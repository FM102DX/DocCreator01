using DocCreator01.Services;
using DocCreator01.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using DocCreator01.Data;

namespace DocCreator01
{
    using global::DocCreator01.Contracts;
    using global::DocCreator01.ViewModel;
    using global::DocCreator01.Views;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Windows;

    namespace DocCreator01
    {

        public partial class App : Application
        {
            ServiceProvider? _provider;

            protected override void OnStartup(StartupEventArgs e)
            {
                var services = new ServiceCollection();

                services.AddSingleton<IProjectRepository, JsonProjectRepository>();
                services.AddSingleton<IDocGenerator, DocGenerator>();
                services.AddSingleton<ITextPartHelper, TextPartHelper>();

                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<MainWindow>();

                _provider = services.BuildServiceProvider();

                var window = _provider.GetRequiredService<MainWindow>();
                window.Show();

                base.OnStartup(e);
            }
        }

    }


}
