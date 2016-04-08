using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ResourseScanner.Model
{
    public class DictionaryNodeViewModel : GraphNodeViewModelBase
    {
        private readonly HashSet<DictionaryNodeViewModel> mergedFiles = new HashSet<DictionaryNodeViewModel>();

        private readonly List<GraphNodeViewModelBase> childs = new List<GraphNodeViewModelBase>();

        private readonly HashSet<string> keys = new HashSet<string>();

        private readonly string path;

        private bool isMerged = false;

        private bool isDictionary;

        public bool IsDictionary
        {
            get { return isDictionary; }
            internal set { isDictionary = value; }
        }

        private double size;

        public double Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Size"));
            }
        }

        private double borderSize;

        public double BorderSize
        {
            get { return borderSize; }
            set
            {
                borderSize = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BorderSize"));
            }
        }

        public string FilePath
        {
            get
            {
                return path;
            }
        }

        public HashSet<DictionaryNodeViewModel> MergedFiles
        {
            get
            {
                return mergedFiles;
            }
        }

        public List<GraphNodeViewModelBase> Childs
        {
            get
            {
                return childs;
            }
        }

        public HashSet<string> Keys
        {
            get
            {
                return keys;
            }
        }

        public string FileName
        {
            get
            {
                return System.IO.Path.GetFileName(path);
            }
        }

        private bool isHighligted;

        public bool IsHighligted
        {
            get { return isHighligted; }
            set
            {
                isHighligted = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsHighligted"));
                foreach (var item in MergedFiles)
                {
                    item.IsHighligted = isHighligted;
                }
            }
        }

        public bool IsMerged
        {
            get
            {
                return isMerged;
            }

            internal set
            {
                isMerged = value;
            }
        }

        public DictionaryNodeViewModel(string path)
        {
            this.path = path;
            RefreshMetrics();
        }


        internal void RefreshMetrics()
        {
            BorderSize = 1 + MergedFiles.Count * 2;
            Size = 80 + ((double)(Keys.Count)) * 2 + BorderSize * 2;
        }
    }
}
