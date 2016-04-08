using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourseScanner.Model
{
    public class ViewNodeViewModel : GraphNodeViewModelBase
    {
        private readonly HashSet<ViewNodeViewModel> chilsViews = new HashSet<ViewNodeViewModel>();

        private readonly string path;

        private readonly string fullName;

        public string Path
        {
            get
            {
                return path;
            }
        }

        public string FileName
        {
            get
            {
                return System.IO.Path.GetFileName(path);
            }
        }

        public string FullName { get { return fullName; } }

        public string Name { get { return FullName.Split('.').Last(); } }

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

        private double borderSize = 2;

        public double BorderSize
        {
            get { return borderSize; }
            set
            {
                borderSize = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BorderSize"));
            }
        }


        private bool isWindow;

        public bool IsWindow
        {
            get
            {
                return isWindow;
            }
            internal set
            {
                isWindow = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsWindow"));
            }
        }

        public HashSet<ViewNodeViewModel> ChilsViews
        {
            get
            {
                return chilsViews;
            }
        }


        public ViewNodeViewModel(string name, string path)
        {
            this.path = path;
            this.fullName = name;
            RefreshMetrics();
        }


        internal void RefreshMetrics()
        {
            FileInfo fi = new FileInfo(Path);
            Size = 90 + ((double)fi.Length) / 1000;
        }
    }
}
