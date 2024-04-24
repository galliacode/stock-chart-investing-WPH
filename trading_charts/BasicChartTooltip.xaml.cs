using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace trading_charts
{
    /// <summary>
    /// Interaction logic for BasicChartTooltip.xaml
    /// </summary>
    public partial class BasicChartTooltip : UserControl
    {
        public BasicChartTooltip()
        {
            InitializeComponent();
        }
        public void UpdateTooltip(string title, double value)
        {
            TitleTextBlock.Text = title;
            ValueTextBlock.Text = $"Value: {value:N2}";
        }
    }
}
