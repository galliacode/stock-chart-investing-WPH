using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trading_charts
{
    internal class FileItem
    {
        public string Symbol {  get; set; }
        public string Title { get; set; }
        public string ParentDirectory { get; set; }
        public string FullPath { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

}
