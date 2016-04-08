using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ResourseScanner
{
    public class SolidColorBrushFilter : ResourcesFilterBase
    {
        public override bool FilterItemsOverride(DictionaryEntry entry)
        {
            if (entry.Value is SolidColorBrush)
            {
                AddFilteredItemThreadSafe(entry);
                return true;
            }

            return false;
        }

        public override void FilterSourceOverride(XDocument xdoc, StreamWriter writer, XmlNamespaceManager nsmgr)
        {
            foreach (var item in xdoc.XPathSelectElements("//presentation:SolidColorBrush", nsmgr))
            {
                var xaml = item.ToString();

                xaml = xaml.Replace("x:Shared=\"False\"", "");

                //Colors should be filtered
                //if (xaml.Contains('{')) continue;

                if (xaml.Contains("Name")) continue;

                if (!xaml.Contains(":Key")) continue;

                writer.WriteLine(xaml);
            }
        }
    }
}
