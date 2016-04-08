using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourseScanner.Model
{
    public class ResourceNodeViewModel : GraphNodeViewModelBase
    {
        private readonly DictionaryEntry entry;

        public DictionaryEntry Entry
        {
            get
            {
                return entry;
            }
        }

        public ResourceNodeViewModel(DictionaryEntry entry)
        {
            this.entry = entry;
        }
    }
}
