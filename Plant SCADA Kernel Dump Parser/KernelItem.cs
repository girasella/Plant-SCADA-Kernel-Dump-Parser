using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant_SCADA_Kernel_Dump_Parser
{
    internal class KernelItem : ViewModelBase
    {
        string _title;

        StringBuilder _internalContent;

        public string Content { get => _internalContent.ToString(); 
            set {OnPropertyChanged(); } }
        public string Title { get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public StringBuilder InternalContent { get => _internalContent; set => _internalContent = value; }

        public KernelItem()
        {
            _internalContent = new StringBuilder();
        }

        public void AddContentLine(string line)
        {
            _internalContent.AppendLine(line);
            OnPropertyChanged("Content");
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
