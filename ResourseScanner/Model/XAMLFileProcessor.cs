using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows.Markup;
using System.Text.RegularExpressions;

namespace ResourseScanner.Model
{
    public class XAMLFileProcessor
    {
        private readonly List<ResourcesFilterBase> filters = new List<ResourcesFilterBase>() {
            new VisualFilter(),
            new ColorFilter(),
            new SolidColorBrushFilter() { IsEnabled = false},
            new GenericFilter()
        };

        private GenericFilter GFilter { get { return filters[3] as GenericFilter; } }

        public List<ResourcesFilterBase> Filters
        {
            get
            {
                return filters;
            }
        }

        readonly ResourcesGraphBuilder gBuilder = new ResourcesGraphBuilder();

        readonly ViewsGraphBuilder vBuilder = new ViewsGraphBuilder();

        public ResourcesGraphBuilder GBuilder { get { return gBuilder; } }

        public ViewsGraphBuilder VBuilder { get { return vBuilder; } }

        public void Scan(string path)
        {
            if (!Directory.Exists(path))
            {
                Trace.WriteLine(String.Format("Directory {0} does not exists.", path));
                return;
            };

            /* setup graph */
            GBuilder.Reset();
            VBuilder.Reset();


            foreach (var item in Filters)
            {
                item.Reset();
                item.GBuilder = GBuilder;
            }

            foreach (var fileName in Directory.GetFiles(path, "*.xaml", SearchOption.AllDirectories))
            {
                ScanFile(fileName);
            }

            VBuilder.FormAdges();

            TraceSource ts = new TraceSource("GenericFilter");
            ts.TraceInformation(GFilter.ToString());
            ts.Flush();
            ts.Close();
            //Trace.WriteLine(GFilter.ToString());
            Trace.WriteLine(GBuilder.ToString());
            Trace.Flush();
        }

        private void ScanFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Trace.WriteLine(String.Format("File {0} does not exists.", fileName));
                return;
            };

            string text = File.ReadAllText(fileName);

            if (IsView(text))
                HandleView(fileName, text);

            if (IsResourceDictionary(text))
                HandleResourceDictionary(fileName);
            else if (HasResourceDictionary(text))
                HandleControl(fileName);

            ((GenericFilter)filters[3]).ResolveResourcesLinks(text);
        }

        private void HandleView(string fileName, string text)
        {
            Trace.WriteLine(String.Format("Handle {0} view.", fileName), "info");

            var m = Regex.Match(text, @"x:Class=""(.*)""");

            VBuilder.AddView(m.Groups[1].Value, fileName).IsWindow = text.StartsWith("<Window");
        }

        //private void HandleEmbededResourceDictionaries(string text)
        //{
        //    /* handles only first RD */
        //    int start = text.IndexOf("<ResourceDictionary>");
        //    int end = text.IndexOf("</ResourceDictionary>");
        //    string rd = text.Substring(start, end - start);

        //    HandleRDXaml(text);
        //}

        //private void HandleRDXaml(string text)
        //{
        //    using (StreamReader sr = new StreamReader(wstream))
        //    {
        //        var txt = sr.ReadToEnd();
        //        ResourceDictionary rd = (ResourceDictionary)XamlReader.Parse(txt);

        //        foreach (var item in rd)
        //        {
        //            DictionaryEntry entry = (DictionaryEntry)item;
        //            FrameworkElement element = entry.Value as FrameworkElement;
        //            element.Width = 40;
        //            element.Height = 40;
        //            ListBoxItemsSource.Add(element);
        //        }
        //    }
        //}


        private void HandleResourceDictionary(string fileName)
        {
            Trace.WriteLine(String.Format("Handle {0} dictionary.", fileName), "info");
            GBuilder.AddFile(fileName).IsDictionary = true;

            HandleXamlResources(fileName);
        }

        private void HandleControl(string fileName)
        {
            Trace.WriteLine(String.Format("Handle {0} control.", fileName), "info");
            GBuilder.AddFile(fileName);

            HandleXamlResources(fileName);
        }

        private void HandleXamlResources(string fileName)
        {
            Trace.WriteLine(String.Format("Handle {0} xaml.", fileName), "info");

            using (var wstream = new MemoryStream())
            using (BufferedStream bufstream = new BufferedStream(wstream))
            using (StreamWriter writer = new StreamWriter(bufstream))
            using (var reader = File.OpenRead(fileName))
            {
                writer.WriteLine("<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'");
                writer.WriteLine("xmlns:x = 'http://schemas.microsoft.com/winfx/2006/xaml\' >");

                var xdoc = XDocument.Load(reader);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("presentation", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                nsmgr.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");

                foreach (var item in xdoc.XPathSelectElements("//presentation:ResourceDictionary[@Source]", nsmgr))
                {
                    GBuilder.AddReferenceToAnotherFile(item.Attribute(XName.Get("Source")).Value);
                }

                /* filter items */
                foreach (var filter in Filters)
                    filter.FilterSource(xdoc, writer, nsmgr);

                writer.WriteLine("</ResourceDictionary>");
                writer.Flush();


                wstream.Seek(0, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(wstream))
                {
                    try
                    {
                        var txt = sr.ReadToEnd();

                        ResourceDictionary rd = App.Current.Dispatcher.Invoke<ResourceDictionary>(() => { return (ResourceDictionary)XamlReader.Parse(txt); });

                        //ResourceDictionary rd = (ResourceDictionary)XamlReader.Parse(txt);

                        foreach (var item in rd)
                        {
                            DictionaryEntry entry = (DictionaryEntry)item;

                            foreach (var filter in Filters)
                                filter.FilterItems(entry);

                        }
                    }
                    catch (XamlParseException xpe)
                    {
                        Trace.WriteLine(String.Format("Could not parce : {0}.", xpe.Message), "error");
                    }
                }
            }
        }

        private void FilterButtonTemplates(XDocument xdoc, StreamWriter writer, XmlNamespaceManager nsmgr)
        {
            foreach (var item in xdoc.XPathSelectElements("//presentation:ControlTemplate", nsmgr))
            {
                var xaml = item.ToString();

                xaml = xaml.Replace("x:Shared=\"False\"", "");

                if (!xaml.Contains("TargetType=\"{x:Type Button}\"") && !xaml.Contains("TargetType=\"Button\"")) continue;

                //if (xaml.Contains('{')) continue;

                if (xaml.Contains("Name")) continue;

                //if (!xaml.Contains(":Key")) continue;

                writer.WriteLine(xaml);
            }
        }

        private bool IsResourceDictionary(string text)
        {
            if (text.StartsWith("<ResourceDictionary"))
                return true;
            return false;
        }

        private bool IsView(string text)
        {
            if (text.StartsWith("<UserControl") || text.StartsWith("<Window"))
                return true;
            return false;
        }

        private bool HasResourceDictionary(string text)
        {
            if (text.Contains("<ResourceDictionary>"))
                return true;
            return false;
        }

    }
}
