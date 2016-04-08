using Graphviz4Net.Dot;
using Graphviz4Net.Dot.AntlrParser;
using ResourseScanner.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ResourseScanner
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly XAMLFileProcessor processor = new XAMLFileProcessor();

        public XAMLFileProcessor Processor { get { return processor; } }

        private string pathText;

        public string PathText
        {
            get { return pathText; }
            set
            {
                pathText = value;
                OnPropertyChanged(new PropertyChangedEventArgs("PathText"));
            }
        }

        private string sourceXamlText;

        public string SourceXamlText
        {
            get { return sourceXamlText; }
            set
            {
                sourceXamlText = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SourceXamlText"));
            }
        }

        private string sourceKeyText;

        public string SourceKeyText
        {
            get { return sourceKeyText; }
            set
            {
                sourceKeyText = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SourceKeyText"));
            }
        }

        private int listBoxSelectedIndex;

        public int ListBoxSelectedIndex
        {
            get { return listBoxSelectedIndex; }
            set
            {
                listBoxSelectedIndex = value;
                if (listBoxSelectedIndex >= 0)
                {
                    var item = Processor.Filters[0].FilteredItems[listBoxSelectedIndex];
                    SourceXamlText = XamlWriter.Save(item.Value);
                    SourceKeyText = item.Key.ToString();
                    OnPropertyChanged(new PropertyChangedEventArgs("ListBoxSelectedIndex"));
                }
            }
        }


        public void Scan(string path)
        {
            Processor.Scan(path);

            PathText = path;
        }

        public Task ScanAsync2(string path)
        {
            var tcs = new TaskCompletionSource<object>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    Scan(path);
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public async Task ScanAsync(string path)
        {
            await Task.Run(() =>
            {
                try
                {
                    Processor.Scan(path);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                }
            });

            PathText = path;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }
    }
}
