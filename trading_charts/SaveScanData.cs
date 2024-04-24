using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trading_charts
{
    public class SaveScanData
    {
        public DateTime Date { get; set; }
        public double Open {  get; set; }
        public double Close { get; set; }
        public double High { get; set; } 
        public double Low { get; set; }
        public double Volume { get; set; }
        public double Traci { get; set; }
        public int Up {  get; set; }
        public double ScanUp {  get; set; }
        public int Down { get; set; }
        public double ScanDown { get; set; }


    }
}
