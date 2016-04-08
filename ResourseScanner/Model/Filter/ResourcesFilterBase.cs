using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace ResourseScanner
{
    public abstract class ResourcesFilterBase
    {
        public bool IsEnabled { get; set; }

        public bool AddToGraph { get; set; }

        public ResourcesGraphBuilder GBuilder { get; set; }

        private ObservableCollection<DictionaryEntry> filteredItems = new ObservableCollection<DictionaryEntry>();

        public ObservableCollection<DictionaryEntry> FilteredItems
        {
            get
            {
                return filteredItems;
            }
        }

        public ResourcesFilterBase()
        {
            IsEnabled = true;
        }


        protected void AddFilteredItemThreadSafe(DictionaryEntry entry)
        {
            if (App.Current.Dispatcher.CheckAccess())
                FilteredItems.Add(entry);
            else
                App.Current.Dispatcher.BeginInvoke((Action)(() => { FilteredItems.Add(entry); }));

        }

        public void FilterSource(XDocument xdoc, StreamWriter writer, XmlNamespaceManager nsmgr)
        {
            if (IsEnabled)
                FilterSourceOverride(xdoc, writer, nsmgr);
        }

        public bool FilterItems(DictionaryEntry entry)
        {
            if (IsEnabled)
            {
                if (GBuilder != null && AddToGraph)
                    GBuilder.AddResourceToCurrentFile(entry);

                return FilterItemsOverride(entry);
            }
            return false;
        }

        public abstract void FilterSourceOverride(XDocument xdoc, StreamWriter writer, XmlNamespaceManager nsmgr);

        public abstract bool FilterItemsOverride(DictionaryEntry entry);

        public virtual void Reset()
        {
            if (App.Current.Dispatcher.CheckAccess())
                FilteredItems.Clear();
            else
                App.Current.Dispatcher.BeginInvoke((Action)(() => { FilteredItems.Clear(); }));
        }
    }
}
