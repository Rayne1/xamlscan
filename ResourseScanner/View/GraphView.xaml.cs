using Graphviz4Net.WPF;
using ResourseScanner.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;

namespace ResourseScanner
{
    /// <summary>
    /// Interaction logic for GraphView.xaml
    /// </summary>
    public partial class GraphView : UserControl
    {
        public ObservableCollection<string> SearchResults { get; private set; }


        static GraphView()
        {
            CommandManager.RegisterClassCommandBinding(typeof(GraphView),
                new CommandBinding(ApplicationCommands.Find, FindExecuted));
        }

        public GraphView()
        {
            SearchResults = new ObservableCollection<string>();
            InitializeComponent();
        }

        private static void FindExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            GraphView view = sender as GraphView;
            var vm = view.DataContext as ResourcesGraphBuilder;
            if (vm != null)
            {
                string txt = (string)e.Parameter;
                if (String.IsNullOrEmpty(txt)) return;

                var dict = vm.Graph.AllVertices.OfType<DictionaryNodeViewModel>().Where(n => n.FileName.Contains(txt)).FirstOrDefault();

                if (dict != null)
                {
                    view.nodeDetails.DataContext = dict;
                    dict.Container.BringIntoView();
                    view.searchResultsPopup.IsOpen = false;
                }
            }
        }

        private void DetailsRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            nodeDetails.DataContext = e.Parameter;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                //scaleTransform.CenterX = pos.X/ graph.ActualWidth;
                //scaleTransform.CenterY = pos.Y/ graph.ActualHeight;

                scaleTransform.ScaleX += (double)(e.Delta) / 2000;
                scaleTransform.ScaleY += (double)(e.Delta) / 2000;

                if (scaleTransform.ScaleX <= 0) scaleTransform.ScaleX = 0;
                if (scaleTransform.ScaleY <= 0) scaleTransform.ScaleY = 0;

                var pos = e.GetPosition(graph);
                scrollViewer.ScrollToHorizontalOffset(pos.X);
                scrollViewer.ScrollToVerticalOffset(pos.Y);

                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                ScrollViewer sw = sender as ScrollViewer;
                sw.ScrollToHorizontalOffset(sw.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Set up the WPF Control to be printed
            GraphLayout controlToPrint = new GraphLayout();
            controlToPrint.Graph = graph.Graph;
            controlToPrint.Resources.MergedDictionaries.Add(Resources);

            controlToPrint.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            controlToPrint.Arrange(new Rect(0, 0,
                graph.ActualWidth,
                graph.ActualHeight));

            FixedDocument fixedDoc = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage() { Background = Brushes.Gray };
            fixedPage.Width = graph.ActualWidth;
            fixedPage.Height = graph.ActualHeight;

            //Create first page of document
            fixedPage.Children.Add(controlToPrint);
            ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);
            //Create any other required pages here

            //View the document
            //documentViewer1.Document = fixedDoc;

            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "MyReport"; // Default file name
            dlg.DefaultExt = ".xps"; // Default file extension
            dlg.Filter = "XPS Documents (.xps)|*.xps"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            //Thread.Sleep(1000);

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                XpsDocument xpsd = new XpsDocument(filename, FileAccess.Write);
                System.Windows.Xps.XpsDocumentWriter xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
                xw.Write(fixedDoc);
                xpsd.Close();
            }
        }

        public void RefreshGraph()
        {
            BindingOperations.GetBindingExpression(graph, GraphLayout.GraphProperty).UpdateTarget();
        }


        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;

            var vm = DataContext as ResourcesGraphBuilder;
            if (vm == null) return;

            SearchResults.Clear();
            var results = vm.Graph.AllVertices.OfType<DictionaryNodeViewModel>().Where(n => n.FileName.Contains(tb.Text)).Select(n => n.FileName);
            foreach (var item in results)
            {
                //searchResultsListBox.Items.Add(new GroupItem() { Ite});
                SearchResults.Add(item);
            }
            searchResultsPopup.IsOpen = true;

            //Mouse.Capture(searchGrid, CaptureMode.SubTree);
        }
    }
}
