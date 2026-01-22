using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using voskwpf.Models;
using voskwpf.ViewModels;

namespace voskwpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private static IServiceProvider _serviceProvider;

		public static IServiceProvider Services
		{
			get {  return _serviceProvider; }
			private set
			{
				if (_serviceProvider != value)
				{
					_serviceProvider = value;
				}
			}
		}
		
		
		public App()
		{
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			_serviceProvider = serviceCollection.BuildServiceProvider();


		}

		private void ConfigureServices(IServiceCollection services)
		{
			string engine = ConfigurationSettings.AppSettings["engine"];
			if (engine == "vosk")
			{
				services.AddSingleton<VoskModel>();
				services.AddSingleton<VoskViewModel>();

			}
			else
			{
				services.AddSingleton<WhisperModel>();

			}

			services.AddSingleton<MainWindow>();

		}

		private void OnStartup(object sender, StartupEventArgs e)
		{
			var mainWindow = _serviceProvider.GetService<MainWindow>();
			mainWindow.Show();
		}

	}

}
