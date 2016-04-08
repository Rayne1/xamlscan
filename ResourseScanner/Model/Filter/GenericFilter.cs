using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ResourseScanner
{
    class GenericFilter : ResourcesFilterBase
    {
        private readonly HashSet<string> allKeys = new HashSet<string>();

        private readonly HashSet<string> duplicatedKeys = new HashSet<string>();

        private readonly HashSet<string> notUsedKeys = new HashSet<string>();

        private readonly HashSet<string> staticResources = new HashSet<string>();

        private readonly HashSet<string> dynamicResources = new HashSet<string>();

        public HashSet<string> AllKeys
        {
            get
            {
                return allKeys;
            }
        }

        public HashSet<string> NotUsedKeys
        {
            get
            {
                return notUsedKeys;
            }
        }

        public HashSet<string> UnresolvedStaticResources
        {
            get
            {
                return staticResources;
            }
        }

        public HashSet<string> UnresolvedDynamicResources
        {
            get
            {
                return dynamicResources;
            }
        }

        public HashSet<string> DuplicatedKeys
        {
            get
            {
                return duplicatedKeys;
            }
        }

        public override bool FilterItemsOverride(DictionaryEntry entry)
        {
            //throw new NotImplementedException();
            return false;
        }

        public override void FilterSourceOverride(XDocument xdoc, StreamWriter writer, XmlNamespaceManager nsmgr)
        {
            foreach (var item in xdoc.XPathSelectElements("//*[@x:Key]", nsmgr))
            {
                string key = item.Attribute(XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml")).Value;
                if (AllKeys.Add(key))
                    NotUsedKeys.Add(key);
                else
                    DuplicatedKeys.Add(key);

                GBuilder.AddResourceToCurrentFile(key);
            }


            //foreach (var item in xdoc.XPathSelectElements("//presentation:SolidColorBrush", nsmgr))
            //{
            //    var xaml = item.ToString();

            //    xaml = xaml.Replace("x:Shared=\"False\"", "");

            //    //Colors should be filtered
            //    //if (xaml.Contains('{')) continue;

            //    if (xaml.Contains("Name")) continue;

            //    if (!xaml.Contains(":Key")) continue;

            //    writer.WriteLine(xaml);
            //}
        }

        public void ResolveResourcesLinks(string text)
        {
            /* check existing unresolved */
            foreach (var item in UnresolvedDynamicResources.ToList())
                if (NotUsedKeys.Remove(item))
                    UnresolvedDynamicResources.Remove(item);

            foreach (var item in UnresolvedStaticResources.ToList())
                if (NotUsedKeys.Remove(item))
                    UnresolvedStaticResources.Remove(item);

            NotUsedKeys.RemoveWhere(k => k.Contains('{'));


            /* add new unresolved */
            var matches = Regex.Matches(text, @"[{<](\w+)Resource (?:ResourceKey="")?([\w\d\.]*)[}(?:""/>)]");
            foreach (var item in matches.OfType<Match>())
            {
                if (item.Groups[1].Value == "Static")
                {
                    if (!NotUsedKeys.Remove(item.Groups[2].Value))
                        UnresolvedStaticResources.Add(item.Groups[2].Value);
                }
                else if (item.Groups[1].Value == "Dynamic")
                    if (!NotUsedKeys.Remove(item.Groups[2].Value))
                        UnresolvedDynamicResources.Add(item.Groups[2].Value);
            }
        }

        public override void Reset()
        {
            base.Reset();

            AllKeys.Clear();
            NotUsedKeys.Clear();
            UnresolvedStaticResources.Clear();
            UnresolvedDynamicResources.Clear();
            DuplicatedKeys.Clear();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(" -- Generic filter report --------");

            sb.AppendLine().AppendFormat(" -- Not used keys : {0}", NotUsedKeys.Count.ToString()).AppendLine();

            foreach (var item in NotUsedKeys)
                sb.Append("\t").AppendLine(item);

            sb.AppendLine().AppendFormat(" -- Duplicated keys : {0}", DuplicatedKeys.Count.ToString()).AppendLine();

            foreach (var item in DuplicatedKeys)
                sb.Append("\t").AppendLine(item);

            sb.AppendLine().AppendFormat(" -- Not resolved static links : {0}", UnresolvedStaticResources.Count.ToString()).AppendLine();

            //foreach (var item in UnresolvedStaticResources)
            //    sb.Append("\t").AppendLine(item);

            sb.AppendLine().AppendFormat(" -- Not resolved dynamic links : {0}", UnresolvedDynamicResources.Count.ToString()).AppendLine();

            //foreach (var item in UnresolvedDynamicResources)
            //    sb.Append("\t").AppendLine(item);

            return sb.ToString();
        }
    }
}
