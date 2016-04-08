using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ResourseScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get { return DataContext as MainWindowViewModel; } }

        static MainWindow()
        {
            CommandManager.RegisterClassCommandBinding(typeof(MainWindow),
                new CommandBinding(ApplicationCommands.Open, OpenExecuted));
        }

        private static async void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindow window = sender as MainWindow;

            VisualStateManager.GoToElementState(window.root, "Loading", false);

            var dialog = new FolderBrowserDialog();            
            if (System.Windows.Forms.DialogResult.OK == dialog.ShowDialog())
            {
                await window.ViewModel.ScanAsync(dialog.SelectedPath);

                window.resourcesGraphView.RefreshGraph();
                window.viewsGraphView.RefreshGraph();
            }

            VisualStateManager.GoToElementState(window.root, "Normal", true);
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Trace.Listeners.Add(new TextBoxTraceListener(trace));

            MainWindowViewModel vm = new MainWindowViewModel();
            DataContext = vm;

            VisualStateManager.GoToElementState(root, "Loading", false);

            //await vm.ScanAsync(@"d:\WKSC-NJD\dev");
            vm.Scan(@"d:\WKSC-NJD\dev");

            VisualStateManager.GoToElementState(root, "Normal", true);
            //vm.Scan(@"d:\WKSC-NJD\dev\AppModules\bk\SOURCE\CoBAppModule\");
        }
    }
}
