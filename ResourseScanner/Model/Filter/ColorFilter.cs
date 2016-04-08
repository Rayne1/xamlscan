using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ResourseScanner
{
    public class ColorFilter : ResourcesFilterBase
    {
        //MemoryStream MStream { get; set; }

        //StreamWriter SWriter { get; set; }

        //public ResourceDictionary SourceResult { get; set; }

        public ColorFilter()
        {
        }



        public override bool FilterItemsOverride(DictionaryEntry entry)
        {
            if (entry.Value is Color)
            {
                AddFilteredItemThreadSafe(entry);

                return true;
            }

            return false;
        }

        public override void FilterSourceOverride(XDocument xdoc, StreamWriter writer, XmlNamespaceManager nsmgr)
        {
            //StartDictionary();

            foreach (var item in xdoc.XPathSelectElements("//presentation:Color", nsmgr))
            {
                var xaml = item.ToString();

                xaml = xaml.Replace("x:Shared=\"False\"", "");

                if (xaml.Contains('{')) continue;

                if (xaml.Contains("Name")) continue;

                if (!xaml.Contains(":Key")) continue;

                writer.WriteLine(xaml);

                //if (SWriter != null)
                //    SWriter.WriteLine(xaml);
            }

            //SourceResult = EndDictionary();
        }

        //public void StartDictionary()
        //{
        //    MStream = new MemoryStream();
        //    BufferedStream bufstream = new BufferedStream(MStream);
        //    SWriter = new StreamWriter(bufstream);

        //    SWriter.WriteLine("<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'");
        //    SWriter.WriteLine("xmlns:x = 'http://schemas.microsoft.com/winfx/2006/xaml\' >");
        //}

        //public ResourceDictionary EndDictionary()
        //{
        //    if (SWriter == null) return new ResourceDictionary();

        //    SWriter.WriteLine("</ResourceDictionary>");
        //    SWriter.Flush();


        //    MStream.Seek(0, SeekOrigin.Begin);
        //    using (StreamReader sr = new StreamReader(MStream))
        //    {
        //        try
        //        {
        //            var txt = sr.ReadToEnd();
        //            return (ResourceDictionary)XamlReader.Parse(txt);
        //        }
        //        catch (XamlParseException xpe)
        //        {
        //            Trace.WriteLine(String.Format("Could not parce : {0}.", xpe.Message), "error");
        //            return new ResourceDictionary();
        //        }
        //    }
        //}
    }
}
