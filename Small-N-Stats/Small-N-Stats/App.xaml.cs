using Small_N_Stats.ViewModel;
using System.Windows;

namespace Small_N_Stats
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new MainWindow();

            MainWindowViewModel viewModel = new MainWindowViewModel();
            /* Start Interface */
            viewModel._interface = window;
            viewModel.MainWindow = window;
            /* End Interface */

            window.DataContext = viewModel;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }
    }
}
