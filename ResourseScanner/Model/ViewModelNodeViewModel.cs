using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourseScanner.Model
{
    public class ViewModelNodeViewModel : GraphNodeViewModelBase
    {
        private readonly string fullName;

        private double size = 90;

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

        public string FullName
        {
            get
            {
                return fullName;
            }
        }

        public string Name { get { return FullName.Split('.').Last(); } }

        public ViewModelNodeViewModel(string value)
        {
            this.fullName = value;
        }
    }
}
