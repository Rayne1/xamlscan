using ResourseScanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ResourseScanner
{
    public class ResourceNodeTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var node = item as GraphNodeViewModelBase;
            node.Container = container as FrameworkElement;
            var fe = container as FrameworkElement;
            if (item == null || fe == null) return base.SelectTemplate(item, container);

            if (node is DictionaryNodeViewModel)
                return fe.FindResource("App.Template.Node.Dictionary") as DataTemplate;

            if (node is ResourceNodeViewModel)
                if (((ResourceNodeViewModel)node).Entry.Value is Color)
                    return fe.FindResource("App.Template.Node.Color") as DataTemplate;
                else if (((ResourceNodeViewModel)node).Entry.Value is Viewbox)
                    return fe.FindResource("App.Template.Node.ViewBox") as DataTemplate;

            if (node is ViewNodeViewModel)
                return fe.FindResource("App.Template.Node.View") as DataTemplate;

            if (node is ViewModelNodeViewModel)
                return fe.FindResource("App.Template.Node.ViewModel") as DataTemplate;
            

            return base.SelectTemplate(item, container);

        }
    }
}
