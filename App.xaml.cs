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

                // Register the GeneratedFilesHelper service
                services.AddSingleton<IGeneratedFilesHelper, GeneratedFilesHelper>();

                // Update the TextPartHelper registration to remove dependency on IProjectRepository
                services.AddTransient<ITextPartHelper, TextPartHelper>();

                // Register the Python helper
                services.AddSingleton<IPythonHelper, PythonHelper>();

                // Register the AppPathsHelper
                services.AddSingleton<IAppPathsHelper, AppPathsHelper>();

                // Register HTML document creator service
                services.AddSingleton<IHtmlDocumentCreatorService, HtmlDocumentCreatorService>();
                
                // Register browser service
                services.AddSingleton<IBrowserService, BrowserService>();

                // Add the TextPartHtmlRenderer service
                services.AddSingleton<ITextPartHtmlRenderer, TextPartHtmlRenderer>();

                // Add these lines to your DI container registration
                services.AddTransient<IProjectHelper, ProjectHelper>();

                // Update the MainWindowViewModel registration in App.xaml.cs
                // Register MainWindowViewModel correctly with all its dependencies
                services.AddTransient<MainWindowViewModel>(provider => 
                    new MainWindowViewModel(
                        provider.GetRequiredService<IProjectRepository>(),
                        provider.GetRequiredService<ITextPartHelper>(),
                        provider.GetRequiredService<IProjectHelper>(),
                        provider.GetRequiredService<IAppPathsHelper>(),
                        provider.GetRequiredService<IGeneratedFilesHelper>(),
                        provider.GetRequiredService<IBrowserService>()
                    )
                );
                
                services.AddTransient<MainWindow>();

                _provider = services.BuildServiceProvider();

                var window = _provider.GetRequiredService<MainWindow>();
                window.Show();

                base.OnStartup(e);
            }
        }

    }


}
