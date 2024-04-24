using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics.Eventing.Reader;
using System.Collections.ObjectModel;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf.Charts.Base;
using LiveCharts.Definitions.Charts;
using System.Diagnostics;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using LiveCharts.Geared;
using System.ComponentModel;
using System.Xml.Linq;
using LiveCharts.Helpers;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Windows.Controls.Primitives;
using System.Runtime.CompilerServices;



namespace trading_charts
{

    public partial class MainWindow : Window
    {
        public string LastSelectedFolderPath;
        public string filePath;
        private string search_text;
        private int RSI_period = 14;
        public int data_start = 0;
        public int data_length;
        public int TRACI_period = 50;
        public int TRACI_rs_length = 25;
        public double TRACI_factor = 3;
        public string TRACI_scan_type = "UP";
        public int TRACI_level = 50;
        public bool TRACI_status = false;
        private bool emaCross_status = false;
        public int shortEmaPeriod = 11;
        public int longEmaPeriod = 21;
        private int numberOfDays = 0;
        private long long_x = 638489450518013792;
                              

        private ObservableCollection<DirectoryItem> DirectoryItems { get; set; }
        private ObservableCollection<FileItem> FileItems { get; set; }
        private ObservableCollection<FileItem> ScanFileItems { get; set; }

        public IEnumerable<StockData> StockDataList { get; set; }
        public GearedValues<StockData> GearedStockDataList { get; set; }
        public ObservableCollection<ComputeData> ComputeDataList { get; set; }
        public GearedValues<ComputeData> GearedComputeDataList { get; set; }
        public ObservableCollection<ScanData> ScanDataList { get; set; }
        public GearedValues<ScanData> GearedScanDataList{ get; set; }
        public List<string> ChartLabels { get; set; }
        private Line horizontalLine;
        private Line verticalLine;
        ScanWindow scanWindow;


        public MainWindow()
        {
            InitializeComponent();

            DirectoryItems = new ObservableCollection<DirectoryItem>();
            DirectoryDataGridList.ItemsSource = DirectoryItems;
            FileItems = new ObservableCollection<FileItem>();
            FileDataGridList.ItemsSource = FileItems;

            StockDataList = new ObservableCollection<StockData>();
            ComputeDataList = new ObservableCollection<ComputeData>();
            ScanDataList = new ObservableCollection<ScanData>();
            this.Loaded += (sender, args) =>
            {
                
                //scanWindow
                scanWindow = new ScanWindow
                {
                    Owner=this,
                    ShowInTaskbar=false
                };
                double parentX = this.Left;
                double parentY = this.Top;
                /*double parentCenterY = this.Top + this.Height / 2;*/

                scanWindow.Left = parentX + 50;
                scanWindow.Top = parentY + 80;
            };
            

            search_text = "";
            string keyPath = @"Software\trading_chart_v_2_2_b_2";
            string valueName = "long_x";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath);
            if (key != null)
            {
                object value = key.GetValue(valueName);
                long_x = long.Parse(value.ToString());
                key.Close();
            }
            else
            {
                key = Registry.CurrentUser.CreateSubKey(keyPath);
                if (key != null)
                {
                    long_x = DateTime.UtcNow.Ticks;
                    key.SetValue("long_x", "" + long_x);
                    key.Close();
                }
                else
                {
                    System.Windows.MessageBox.Show("Error execute program.");
                    System.Windows.Application.Current.Shutdown();
                }

            }

            DateTime dt = new DateTime(long_x);

            /*if (dt.AddHours(2) < DateTime.UtcNow)
            {
                System.Windows.Application.Current.Shutdown();
            }*/


            this.DataContext = this;
        }
        

        //select folder button click
        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (!string.IsNullOrEmpty(LastSelectedFolderPath))
            {
                folderBrowserDialog.SelectedPath = LastSelectedFolderPath;
            }
            var result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                LastSelectedFolderPath = selectedFolderPath;
                PathLabel.Content = selectedFolderPath;

                DirectoryItems.Clear();

                string[] directories = Directory.GetDirectories(selectedFolderPath);

                foreach (string directory in directories)
                {
                    string directoryName = System.IO.Path.GetFileName(directory);

                    DirectoryItem directoryItem = new DirectoryItem
                    {
                        Name = directoryName,
                        FullPath = directory,
                        IsSelected = false
                    };

                    DirectoryItems.Add(directoryItem);
                }
                ProcessCheckedDirectories();
            }
        }

        private void DirectoryCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;
            if (checkBox.DataContext is DirectoryItem directoryItem)
            {
                ProcessCheckedDirectories();
            }
        }

        private void DirectoryCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;
            if (checkBox.DataContext is DirectoryItem directoryItem)
            {
                ProcessCheckedDirectories();
            }
        }


        //Process Checked Directories
        private void ProcessCheckedDirectories()
        {
            FileItems.Clear();
            AddFileItem(LastSelectedFolderPath, "");
            foreach (DirectoryItem directoryItem in DirectoryItems)
            {
                if (directoryItem.IsSelected)
                {
                    AddFileItem(directoryItem.FullPath, directoryItem.Name);
                }
            }

        }
        private void AddFileItem(string fullPath, string parentDirectory)
        {
            try
            {
                var files = Directory.GetFiles(fullPath, search_text + "*.csv");
                foreach (var file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    string value= System.IO.Path.GetFileNameWithoutExtension(file);
                    var name_arr = value.Split(' ');
                    FileItem fileItem = new FileItem
                    {
                        Symbol = name_arr[0],
                        Title = fileName,
                        ParentDirectory = parentDirectory,
                        FullPath = file
                    };

                    // Check if the file is already in the list
                    bool alreadyExists = FileItems.Cast<FileItem>().Any(item => item.FullPath == file);

                    if (!alreadyExists)
                    {
                        FileItems.Add(fileItem);
                    }
                }
            }
            catch (Exception ex)
            {
                 System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void FileDataGridList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (var cell in e.AddedCells)
            {
                var columnIndex = cell.Column.DisplayIndex;
                var rowIndex = FileDataGridList.Items.IndexOf(cell.Item);
                if (rowIndex>=0||rowIndex<FileDataGridList.Items.Count)
                {
                    filePath = FileItems[rowIndex].FullPath;
                    TRACI_status = false;
                    LoadChartData(filePath);
                    SelectGrid.Visibility = Visibility.Hidden;
                    SelectFolderName.Content = FileItems[rowIndex].ParentDirectory;
                    SelectFileName.Content = FileItems[rowIndex].Symbol;
                    
                }
                
            }
        }
        public void LoadChartData(string filePath)
        {
            try
            {
                var reader = new CsvStockDataReader();
                StockDataList = reader.ReadData(filePath);
                data_length = StockDataList.ToList().Count;
                DateTime date = StockDataList.ToList()[data_length - 1].Date;
                var new_date = date.AddDays(1);
                StockDataList = StockDataList.Append(new StockData { Date = new_date, Open = 0, Close = 0, High = 0, Low = 0, Volume = 0 });
                new_date = new_date.AddDays(1);
                StockDataList = StockDataList.Append(new StockData { Date = new_date, Open = 0, Close = 0, High = 0, Low = 0, Volume = 0 });
                data_length = StockDataList.ToList().Count;
                InitializeData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void InitializeData()
        {
            /*DateTime dt = new DateTime(long_x);
            if (dt.AddHours(2) < DateTime.UtcNow)
            {
                return;
            }*/
            if (StockDataList.Count() < 1)
            {
                return;
            }
            var tempDataList = CalculateMACD(StockDataList.Take(data_length-2).Select((x => x.Close)).ToList());
            var rsiValues = CalculateRsi(StockDataList.Take(data_length - 2).Select((x => x.Close)).ToList(), RSI_period);
            var traciValues = CalculateTraci(StockDataList.Take(data_length - 2).Select((x => x.Close)).ToList(), TRACI_period, TRACI_rs_length);
            var shortEmaValues = CalculateEMA(StockDataList.Take(data_length - 2).Select((x => x.Close)).ToList(), shortEmaPeriod);
            var longEmaValues = CalculateEMA(StockDataList.Take(data_length - 2).Select((x => x.Close)).ToList(), longEmaPeriod);
            factorTextbox.Text = TRACI_factor.ToString();
            periodTextbox.Text = TRACI_period.ToString();
            TraciLevelTextBox.Text = TRACI_level.ToString();
            for(int i = 0; i < TraciScanType.Items.Count; i++)
            {
                var traci_scan_type = (ComboBoxItem)TraciScanType.Items[i];
                if(traci_scan_type.Name==TRACI_scan_type) TraciScanType.SelectedItem = traci_scan_type;
                
            }
            ComputeDataList.Clear();
            double rsiValue;
            double traciValue;
            double shortEmaValue;
            double longEmaValue;
            for (int i = 0; i < tempDataList.Count; i++)
            {


                if (i < RSI_period) rsiValue = 0;
                else rsiValue = rsiValues[i - RSI_period];
                if (i < TRACI_rs_length + 1) traciValue = 0;
                else traciValue = traciValues[i - TRACI_rs_length - 1];
                if (i < shortEmaPeriod - 1) shortEmaValue = 0;
                else shortEmaValue = shortEmaValues[i - shortEmaPeriod + 1];
                if (i < longEmaPeriod - 1) longEmaValue = 0;
                else longEmaValue = longEmaValues[i - longEmaPeriod + 1];
                ComputeDataList.Add(new ComputeData
                {
                    MACD = tempDataList[i].MACD,
                    Signal = tempDataList[i].Signal,
                    Histogram = tempDataList[i].Histogram,
                    RSI = rsiValue,
                    TRACI = traciValue,
                    ShortEma = shortEmaValue,
                    LongEma = longEmaValue
                });

            }
            ComputeDataList.Add(new ComputeData { MACD = 0, Signal = 0, Histogram = 0, RSI = 0, TRACI = 0, ShortEma = 0, LongEma = 0 });
            ComputeDataList.Add(new ComputeData { MACD = 0, Signal = 0, Histogram = 0, RSI = 0, TRACI = 0, ShortEma = 0, LongEma = 0 });
            
            if (numberOfDays < 1) numberOfDays = 250;
            
            if (data_length < numberOfDays)
            {
                data_start = 0;
                numberOfDays = data_length;
                numberOfDaysTextbox.Text = data_length.ToString();
            }
            else
            {
                data_start = data_length - numberOfDays;
                numberOfDaysTextbox.Text = "" + numberOfDays;
            }

            GearedStockDataList = StockDataList.AsGearedValues();
            GearedComputeDataList = ComputeDataList.AsGearedValues();
            ChartLabels = StockDataList.Skip(data_start).Take(data_length - data_start).Select((x => x.Date.ToString("dd MMM yy"))).ToList();
            DrawBasicChart();
            DrawComputedChart();
        }

        private void SelectFolderName_Click(object sender, RoutedEventArgs e)
        {
            if (SelectGrid.Visibility != Visibility.Visible)
            {
                SelectGrid.Visibility = Visibility.Visible;
            }
            else
            {
                SelectGrid.Visibility = Visibility.Hidden;
            }

        }

        private void SelectFileName_Click(object sender, RoutedEventArgs e)
        {
            if (SelectGrid.Visibility != Visibility.Visible)
            {
                SelectGrid.Visibility = Visibility.Visible;
            }
            else
            {
                SelectGrid.Visibility = Visibility.Hidden;
            }
        }
        private void BasicChartType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (BasicChartType.SelectedItem != null)
            {
                BasicLabel.Content = "";
                ComputedLabel.Content = "";
                DrawBasicChart();
            }
            else return;


        }
        private void ComputedChartType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComputedChartType.SelectedItem != null)
            {
                BasicLabel.Content = "";
                ComputedLabel.Content = "";
                DrawComputedChart();
            }
            else return;


        }

        private void DrawBasicChart()
        {
            if (filePath != null && StockDataList != null)
            {
                ComboBoxItem cbi = (ComboBoxItem)BasicChartType.SelectedItem;
                BasicChart.AxisY.Clear();

                var list = StockDataList.Skip(data_start).Take(data_length - data_start).Select((x => x.Close)).ToList();

                list = list.Where(value => value != 0).ToList();
                var (minY, maxY) = CalculateMinMax(list);
                minY -= (maxY - minY) * 0.2;
                maxY += (maxY - minY) * 0.2;
                var PriceYAxis = new Axis
                {
                    Foreground = Brushes.Blue,
                    MinValue = minY,
                    MaxValue = maxY

                };

                // Creating a secondary Y-axis (right side)
                var TRACIYAxis = new Axis
                {
                    Foreground = Brushes.Transparent,
                    ShowLabels = false
                };
                var EmaCrossYAxis = new Axis
                {
                    Foreground = Brushes.Transparent,
                    ShowLabels = false
                };

                // Adding axes to the CartesianChart
                BasicChart.AxisY.Add(PriceYAxis);
                BasicChart.AxisY.Add(TRACIYAxis);
                BasicChart.AxisY.Add(EmaCrossYAxis);

                BasicChart.AxisX.Clear();
                var xAxis = new Axis
                {
                    Labels = ChartLabels
                };
                BasicChart.AxisX.Add(xAxis);

                BasicChart.Series.Clear();
                BasicChart.Series.Add(
                    new GLineSeries
                    {
                        Title = "TRACI",
                        Values = new GearedValues<double> (),
                        Fill = Brushes.Transparent,
                        StrokeThickness = 1,
                        FontWeight = FontWeights.Normal,
                        PointGeometry = null,
                        ScalesYAt = 1,
                    });
                switch (cbi.Name)
                {
                    case "Line":
                        BasicChart.Series.Add(
                            new GLineSeries
                            {
                                Title = "Line",
                                Values = new GearedValues<double>(),
                                PointGeometry = null,
                                Stroke = Brushes.DarkOrange,
                                Fill = Brushes.Transparent,
                                StrokeThickness = 1,
                                FontWeight= FontWeights.Normal,
                                LineSmoothness = 0,
                                ScalesYAt = 0,
                            });
                        break;
                    case "Bar":
                        BasicChart.Series.Add(
                            new GOhlcSeries
                            {
                                Title = "Bar",
                                Values = new GearedValues<OhlcPoint>(),
                                StrokeThickness = 1,
                                FontWeight = FontWeights.Normal,
                                DecreaseBrush = Brushes.Red,
                                IncreaseBrush = Brushes.Green,
                                ScalesYAt = 0

                            });

                        break;
                    case "Candle":
                        BasicChart.Series.Add(
                            new GCandleSeries
                            {
                                Title = "Candle",
                                Values = new GearedValues<OhlcPoint>(),
                                StrokeThickness = 1,
                                FontWeight = FontWeights.Normal,
                                ScalesYAt = 0
                            });
                        break;
                    default:
                        break;
                }
                BasicChart.Series.Add(
                    new GLineSeries
                    {
                        Values = new GearedValues<double>(),
                        Fill = Brushes.Transparent,
                        PointGeometry = null,
                        Stroke=Brushes.Red,
                        StrokeThickness = 1,
                        FontWeight = FontWeights.Normal,
                    });
                BasicChart.Series.Add(
                    new GLineSeries
                    {
                        Values = new GearedValues<double>(),
                        Fill = Brushes.Transparent,
                        PointGeometry = null,
                        Stroke = Brushes.Green,
                        StrokeThickness = 1,
                        FontWeight = FontWeights.Normal,
                    });
                BasicChart.Series.Add(
                new GLineSeries
                {
                    Title = "Scan",
                    Values = new GearedValues<double>(),
                    Fill = Brushes.Transparent,
                    StrokeThickness = 1,
                    FontWeight = FontWeights.Normal,
                    PointGeometry = null,
                    ScalesYAt = 1,
                });
                bool first = true;
                for (int i = data_start; i < data_length; i++)
                {
                    var data = GearedStockDataList[i];
                    var traci_val = GearedComputeDataList[i].TRACI;
                    if (TRACI_status) traci_val = GearedScanDataList[i].Traci;
                    if ((first == true && traci_val == 0&&data_start < TRACI_rs_length) || i > data_length - 3) {
                        BasicChart.Series[0].Values.Add(double.NaN);
                        if(TRACI_status) BasicChart.Series[4].Values.Add(double.NaN);
                    } 
                    else
                    {
                        if (first == true) first = false;
                        BasicChart.Series[0].Values.Add(traci_val);
                        if (TRACI_status)
                        {
                            if (TRACI_scan_type == "UP")
                            {
                                if (GearedScanDataList[i].ScanUp == 0&& traci_val>0) BasicChart.Series[4].Values.Add(double.NaN);
                                else BasicChart.Series[4].Values.Add(GearedScanDataList[i].ScanUp);
                            }
                            else if (TRACI_scan_type == "DOWN")
                            {
                                if (GearedScanDataList[i].ScanDown == 0) BasicChart.Series[4].Values.Add(double.NaN);
                                else BasicChart.Series[4].Values.Add(GearedScanDataList[i].ScanDown);
                            }
                        }
                    }
                    
                    switch (cbi.Name)
                    {
                        case "Line":
                            if (i > data_length - 3) BasicChart.Series[1].Values.Add(double.NaN);
                            else BasicChart.Series[1].Values.Add(data.Close);
                            break;
                        case "Bar":
                            //if (i > data_length - 2) BasicChart.Series[0].Values.Add(new OhlcPoint(double.NaN, double.NaN, double.NaN, double.NaN));
                            BasicChart.Series[1].Values.Add(new OhlcPoint(data.Open, data.High, data.Low, data.Close));

                            break;
                        case "Candle":
                            //if (i > data_length - 2) BasicChart.Series[0].Values.Add(new OhlcPoint(double.NaN, double.NaN, double.NaN, double.NaN));
                            BasicChart.Series[1].Values.Add(new OhlcPoint(data.Open, data.High, data.Low, data.Close));
                            break;
                        default:
                            break;
                    }
                    if (emaCross_status)
                    {
                        if (i < (shortEmaPeriod- 1)|| i > data_length - 3) BasicChart.Series[2].Values.Add(double.NaN);
                        else BasicChart.Series[2].Values.Add(GearedComputeDataList[i].ShortEma);

                        if (i < (longEmaPeriod - 1)|| i > data_length - 3) BasicChart.Series[3].Values.Add(double.NaN);
                        else BasicChart.Series[3].Values.Add(GearedComputeDataList[i].LongEma);
                    }
                    
                }
            }
            else return;
        }
        private (double min, double max) CalculateMinMax(List<double> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("The list of values is empty.");
            }

            double min = values.Min();
            double max = values.Max();

            return (min, max);
        }
        private void DrawComputedChart()
        {
            if (filePath != null && StockDataList != null)
            {
                int i = 0;
                ComboBoxItem cbi = (ComboBoxItem)ComputedChartType.SelectedItem;
                ComputedChart.AxisX.Clear();
                var xAxis = new Axis
                {
                    Labels = ChartLabels

                };
                ComputedChart.AxisX.Add(xAxis);
                ComputedChart.Series.Clear();
                switch (cbi.Name)
                {
                    case "MACD":
                        ComputedChart.Series.Add(
                            new GLineSeries
                            {
                                Title = "MACD",
                                Values = new GearedValues<double>(),
                                Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 255)),
                                StrokeThickness = 1,
                                FontWeight = FontWeights.Normal,
                                PointGeometry = null,
                                Fill = Brushes.Transparent

                            });
                        ComputedChart.Series.Add(
                            new GLineSeries
                            {
                                Title = "Signal",
                                Values = new GearedValues<double>(),
                                Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                                StrokeThickness = 1,
                                FontWeight = FontWeights.Normal,
                                PointGeometry = null,
                                Fill = Brushes.Transparent
                            });
                        var histogramMapper = Mappers.Xy<double>()
                               .X((value, index) => index)
                               .Y(value => value)
                               .Fill(value => value > 0 ? Brushes.Green : Brushes.Red);
                        var histogramSeries = new GColumnSeries
                        {
                            Title = "Histogram",
                            Values = new GearedValues<double>(),
                            LabelPoint = point => point.Y.ToString(),
                            Fill= Brushes.Green,
                            StrokeThickness = 1,
                            FontWeight = FontWeights.Normal,
                            Configuration = histogramMapper,
                        };
                        /*histogramSeries.Configuration = new CartesianMapper<double>()
                            .X((value, index) => index)
                            .Y(value => value)
                            .Fill((value, index) => value > 0 ? Brushes.Green : Brushes.Red);*/
                        ComputedChart.Series.Add(histogramSeries);
                        break;
                    case "TRACI":
                        ComputedChart.Series.Add(
                            new GLineSeries
                            {
                                Title = "TRACI",
                                Values = new GearedValues<double>(),
                                Fill = Brushes.Transparent,
                                StrokeThickness = 1,
                                FontWeight = FontWeights.Normal,
                                PointGeometry = null
                            });
                        break;
                    case "RSI":
                        ComputedChart.Series.Add(
                            new GLineSeries
                            {
                                Title = "RSI",
                                Values = new GearedValues<double>(),
                                Fill = Brushes.Transparent,
                                StrokeThickness = 1,
                                FontWeight = FontWeights.Normal,
                                PointGeometry = null
                            });
                        break;
                    default:
                        break;
                }
                bool first = true;
                bool signal_first = true;
                bool histogram_first = true;
                for (i = data_start; i < data_length; i++)
                {
                    switch (cbi.Name)
                    {
                        case "MACD":
                            if ((first == true && GearedComputeDataList[i].MACD == 0) || i > data_length - 3) ComputedChart.Series[0].Values.Add(double.NaN);
                            else
                            {
                                if (first == true) first = false;
                                ComputedChart.Series[0].Values.Add(GearedComputeDataList[i].MACD);
                            }

                            if ((signal_first == true && GearedComputeDataList[i].Signal == 0) || i > data_length - 3) ComputedChart.Series[1].Values.Add(double.NaN);
                            else
                            {
                                if (signal_first == true) signal_first = false;
                                ComputedChart.Series[1].Values.Add(GearedComputeDataList[i].Signal);
                            }
                            if ((histogram_first == true && GearedComputeDataList[i].Histogram == 0) || i > data_length - 3) ComputedChart.Series[2].Values.Add(double.NaN);
                            else
                            {
                                if (histogram_first == true) histogram_first = false;
                                ComputedChart.Series[2].Values.Add(GearedComputeDataList[i].Histogram);
                            }
                            
                            break;
                        case "TRACI":
                            if ((first == true && GearedComputeDataList[i].TRACI == 0 && i< TRACI_rs_length) || i > data_length - 3) ComputedChart.Series[0].Values.Add(double.NaN);
                            else
                            {
                                if (first == true) first = false;
                                ComputedChart.Series[0].Values.Add(GearedComputeDataList[i].TRACI);
                            }
                            break;
                        case "RSI":
                            if ((first == true && GearedComputeDataList[i].RSI == 0) || i > data_length - 3) ComputedChart.Series[0].Values.Add(double.NaN);
                            else
                            {
                                if (first == true) first = false;
                                ComputedChart.Series[0].Values.Add(GearedComputeDataList[i].RSI);
                            }
                            break;
                        default:
                            break;
                    }
                }
                
            }
            else return;
        }

        //get MACD data 
        public static List<(double MACD, double Signal, double Histogram)> CalculateMACD(List<double> prices, int shortPeriod = 12, int longPeriod = 26, int signalPeriod = 9)
        {
            var macdValues = new List<(double MACD, double Signal, double Histogram)>();

            var shortEma = CalculateEMA(prices, shortPeriod);
            var longEma = CalculateEMA(prices, longPeriod);

            for (int i = 0; i < prices.Count; i++)
            {
                double macd = i < (longPeriod - 1) ? 0 : shortEma[i - shortPeriod + 1] - longEma[i - longPeriod + 1];
                macdValues.Add((MACD: macd, Signal: 0, Histogram: 0));
            }

            var signalLine = CalculateEMA(macdValues.Select(x => x.MACD).ToList().Skip(longPeriod - 1).ToList(), signalPeriod);

            for (int i = longPeriod - 1 + signalPeriod - 1; i < macdValues.Count; i++)
            {
                double signal = signalLine[i - longPeriod - signalPeriod + 2];
                macdValues[i] = (macdValues[i].MACD, Signal: signal, Histogram: macdValues[i].MACD - signal);
            }

            return macdValues;
        }

        public static List<double> CalculateEMA(List<double> prices, int period)
        {
            var ema = new List<double>();
            double multiplier = 2.0 / (period + 1);

            double sum = prices.Take(period).Sum();
            double firstEma = sum / period;
            ema.Add(firstEma);

            for (int i = period; i < prices.Count; i++)
            {
                double nextEma = (prices[i] - ema.Last()) * multiplier + ema.Last();
                ema.Add(nextEma);
            }

            return ema;
        }

        //get RSI data
        public List<double> CalculateRsi(List<double> prices, int period = 14)
        {
            var rsiValues = new List<double>();
            var gains = new List<double>();
            var losses = new List<double>();
            // Calculate initial gains and losses
            for (int i = 1; i <= period; i++)
            {
                var change = prices[i] - prices[i - 1];
                gains.Add(Math.Max(change, 0));
                losses.Add(Math.Max(-change, 0));
            }

            double avgGain = gains.Average();
            double avgLoss = losses.Average();

            if (avgLoss == 0)
            {
                rsiValues.Add(100); // Maximum RSI value
                return rsiValues;
            }
            // Calculate initial RSI
            double rs = avgGain / avgLoss;
            rsiValues.Add(100 - (100 / (1 + rs)));


            // Calculate RSI for remaining prices
            for (int i = period + 1; i < prices.Count; i++)
            {
                var change = prices[i] - prices[i - 1];
                var gain = Math.Max(change, 0);
                var loss = Math.Max(-change, 0);

                avgGain = ((avgGain * (period - 1)) + gain) / period;
                avgLoss = ((avgLoss * (period - 1)) + loss) / period;

                rs = avgGain / avgLoss;
                rsiValues.Add(100 - (100 / (1 + rs)));
            }

            return rsiValues;
        }

        //get TRACI Data
        public List<double> CalculateTraci(List<double> prices, int period = 22, int rs_length = 11)
        {
            int i;
            var traciValues = new List<double>();
            var gains = new List<double>();
            var losses = new List<double>();

            var ema = CalculateTRACIEMA(prices, period,TRACI_factor);

            for (i = 1; i < ema.Count; i++)
            {
                var change = ema[i] - ema[i - 1];
                var gain = Math.Max(change, 0);
                var loss = Math.Max(-change, 0);
                gains.Add(gain);
                losses.Add(loss);
            }
            for (i = 0; i < (gains.Count - rs_length); i++)
            {
                var gainsSegment = gains.Skip(i).Take(rs_length);
                var lossesSegment = losses.Skip(i).Take(rs_length);
                double av_gain = gainsSegment.Average();
                double av_loss = lossesSegment.Average();
                if (av_loss == 0)
                {
                    traciValues.Add(100.00);
                }
                else
                {
                    double traciValue = 100 - (100 / (1 + av_gain / av_loss));
                    traciValues.Add(traciValue);
                }
            }

            return traciValues;

        }
        public static List<double> CalculateTRACIEMA(List<double> prices, int period, double factor)
        {
            var ema = new List<double>();
            double multiplier = factor / (period + 1);

            double firstEma = prices[0];
            ema.Add(firstEma);

            for (int i = 1; i < prices.Count; i++)
            {
                double nextEma = (prices[i] - ema.Last()) * multiplier + ema.Last();
                ema.Add(nextEma);
            }

            return ema;
        }

        private void drawChartAt(int value)
        {
            data_start = data_length - value;
            if (data_start < 0) {
                data_start = 0;
                numberOfDays = 0;
                numberOfDaysTextbox.Text = data_length.ToString();
            }
            
            ChartLabels = StockDataList.Skip(data_start).Take(data_length - data_start).Select((x => x.Date.ToString("dd MMM yy"))).ToList();
            DrawBasicChart();
            DrawComputedChart();
        }

        /*private void Chart_DataHover(object sender, ChartPoint chartPoint)
        {
            
            int pointX =(int) chartPoint.X;
            string pointDate = ChartLabels[pointX];
            string pointerLabel = String.Format("Date:{0}", pointDate);
            double value;
            if (BasicChart.Series.Count > 0)
            {
                value = (double)BasicChart.Series[0].Values[pointX];
                pointerLabel += ("  " + BasicChart.Series[1].Title + " Open: " + GearedStockDataList[pointX].Open + " High: " + GearedStockDataList[pointX].High + " Low: " + GearedStockDataList[pointX].Low + " Close:" + GearedStockDataList[pointX].Close);
                pointerLabel += ("  " + BasicChart.Series[0].Title + ":" + value.ToString("0.00"));
            }
            BasicLabel.Content = pointerLabel;
            pointerLabel = String.Format("Date:{0}", pointDate);
            if (ComputedChart.Series.Count > 0)
            {
                foreach (var series in ComputedChart.Series)
                {
                    value = (double)series.Values[pointX];
                    pointerLabel += ("  " + series.Title + ":" + value.ToString("0.00"));
                    
                }
                pointerLabel += (" Volume:" + GearedStockDataList[pointX].Volume);
            }
            ComputedLabel.Content = pointerLabel;
        }*/

        /*private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var point = e.GetPosition(drawingCanvas);
            if (point.X > 0 && point.Y > 0 && data_length > 0)
            {
                drawingCanvas.Children.Clear();
                horizontalLine = new Line
                {
                    Stroke = Brushes.Red,
                    StrokeThickness=1,
                    StrokeDashArray = new DoubleCollection(new double[] { 2, 4 }),
                    X1 = 0,
                    Y1 = point.Y,
                    X2 = drawingCanvas.ActualWidth,
                    Y2 = point.Y
                };
                verticalLine = new Line
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection(new double[] { 2, 4 }),
                    X1 = point.X,
                    Y1 = 0,
                    X2 = point.X,
                    Y2 = drawingCanvas.ActualHeight,
                };
                drawingCanvas.Children.Add(horizontalLine);
                drawingCanvas.Children.Add(verticalLine);
            }
            else
            {
                drawingCanvas.Children.Clear();
            }
            
        }*/

        private void EmaCross_Click(object sender, RoutedEventArgs e)
        {
            if (data_length > 0)
            {
                if (emaCross_status == true)
                {
                    BasicChart.Series[2].Values.Clear();
                    BasicChart.Series[3].Values.Clear();
                    emaCross_status = false;
                    EmaCross.Content = "Show Ema";
                }
                else
                {
                    BasicChart.Series[2].Values.Clear();
                    BasicChart.Series[3].Values.Clear();
                    for (int i = data_start; i < data_length; i++)
                    {
                        if (i < (shortEmaPeriod - 1)|| i > data_length - 3) BasicChart.Series[2].Values.Add(double.NaN);
                        else BasicChart.Series[2].Values.Add(GearedComputeDataList[i].ShortEma);

                        if (i < (longEmaPeriod - 1)|| i > data_length - 3) BasicChart.Series[3].Values.Add(double.NaN);
                        else BasicChart.Series[3].Values.Add(GearedComputeDataList[i].LongEma);
                    }
                    emaCross_status = true;
                    EmaCross.Content = "Hide Ema";
                }
            }
        }

        private void ShortEmaPeriod_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                shortEmaPeriod = int.Parse(textShortEmaPeriod.Text);
                if (shortEmaPeriod > 0 && StockDataList.Count() > 0)
                {
                    InitializeData();
                }
            }
        }

        private void longEmaPeriod_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                longEmaPeriod = int.Parse(textlongEmaPeriod.Text);
                if (longEmaPeriod > 0 && StockDataList.Count() > 0)
                {
                    InitializeData();
                }
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            search_text = SearchTextBox.Text;
            ProcessCheckedDirectories();
        }

        private void numberOfDaysTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                numberOfDays = int.Parse(numberOfDaysTextbox.Text);
                if(numberOfDays > 0)
                {
                    drawChartAt(numberOfDays);
                }
                
            }
        }


        private void ToggleScanButton_Click(object sender, RoutedEventArgs e)
        {
            TRACI_status = !TRACI_status;
            if (TRACI_status) {
                ToggleScanButton.Content = "Hide Scan";
            } 
            else {
                ToggleScanButton.Content = "Show Scan";
            }
            int mode = 0;
            InitializeTraciScanData(mode);

        }

        private void ScanSaveCsvButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to save this to csv file?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int mode = 1;
                InitializeTraciScanData(mode);
            }
            else return;
            
        }

        private void ScanOpenCsvButton_Click(object sender, RoutedEventArgs e)
        {

            SelectGrid.Visibility = Visibility.Hidden;
            if (LastSelectedFolderPath == null)
            {
                System.Windows.MessageBox.Show("Select the Folder.");
                return;
            }
            string directoryPath = System.IO.Path.Combine(LastSelectedFolderPath, "ScanList");
            bool directoryExists = Directory.Exists(directoryPath);
            if (!directoryExists)
            {
                System.Windows.MessageBox.Show("ScanList directory doesn't exist.");
                return;
            }
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Multiselect = false;

            openFileDialog.DefaultExt = ".csv";
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            if (!string.IsNullOrEmpty(directoryPath))
            {
                openFileDialog.InitialDirectory = directoryPath;
            }

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                filePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                try
                {
                    var reader = new CsvStockDataReader();
                    StockDataList = reader.ReadData(filePath);
                    data_length = StockDataList.ToList().Count;
                    DateTime date = StockDataList.ToList()[data_length - 1].Date;
                    var new_date = date.AddDays(1);
                    StockDataList = StockDataList.Append(new StockData { Date = new_date, Open = 0, Close = 0, High = 0, Low = 0, Volume = 0 });
                    new_date = new_date.AddDays(1);
                    StockDataList = StockDataList.Append(new StockData { Date = new_date, Open = 0, Close = 0, High = 0, Low = 0, Volume = 0 });
                    data_length = StockDataList.ToList().Count;
                    var reader1 = new CsvScanDataReader();
                    var _scanDataList = reader1.ReadData(filePath);
                    ScanDataList = new ObservableCollection<ScanData>();
                    foreach (var _scanData in _scanDataList)
                    {
                        if (_scanData != null)
                        {
                            ScanDataList.Add(_scanData);
                        }
                    }
                    ScanDataList.Add(new ScanData { Traci = 0, Up = 0, ScanUp = 0, Down = 0, ScanDown = 0 });
                    ScanDataList.Add(new ScanData { Traci = 0, Up = 0, ScanUp = 0, Down = 0, ScanDown = 0 });
                    GearedScanDataList = ScanDataList.AsGearedValues();
                    string[] input_arr = fileName.Split(' ');
                    TRACI_level = int.Parse(input_arr[3]);
                    TRACI_factor = double.Parse(input_arr[4]);
                    TRACI_period = int.Parse(input_arr[5]);
                    TRACI_rs_length = TRACI_period / 2;
                    factorTextbox.Text = TRACI_factor.ToString();
                    periodTextbox.Text = TRACI_period.ToString();
                    scanWindow.TraciLevelTextBox.Text = TRACI_level.ToString();
                    for (int i = 0; i < scanWindow.TraciScanType.Items.Count; i++)
                    {
                        var traciItem = (ComboBoxItem)scanWindow.TraciScanType.Items[i];
                        if (traciItem.Name == input_arr[2]) TRACI_scan_type = traciItem.Name;
                    }
                    TRACI_status = true;
                    InitializeData();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }

        }
        public void InitializeTraciScanData(int mode = 0)
        {
            if (StockDataList==null || StockDataList.Count() <= 0|| TRACI_level < 0|| TRACI_level > 100)
            {
                return;
            }
            double traciValue;
            
            var traciValues = CalculateTraci(StockDataList.Take(data_length - 2).Select((x => x.Close)).ToList(), TRACI_period, TRACI_rs_length);
            List<int> upValues = new List<int>();
            List<double> upScans = new List<double>();
            List<int> downValues = new List<int>();
            List<double> downScans = new List<double>();
            int upValue, downValue;
            double upScan, downScan;
            ScanDataList.Clear();
            for (int i = 1; i < traciValues.Count; i++)
            {
                if (TRACI_scan_type == "UP")
                {
                    if (traciValues[i] <= TRACI_level)
                    {
                        upValues.Add(1);
                        upScans.Add(traciValues[i]);
                    }
                    else
                    {
                        upValues.Add(0);
                        upScans.Add(0);
                    }
                    /*if (traciValues[i] > traciValues[i - 1])
                    {
                        upValues.Add(1);
                        if (traciValues[i] < traci_level) upScans.Add(traciValues[i]);
                        else upScans.Add(0);
                    }
                    else
                    {
                        upValues.Add(0);
                        upScans.Add(0);
                    }*/
                    downValues.Add(0);
                    downScans.Add(0);
                }
                else if (TRACI_scan_type == "DOWN")
                {
                    if (traciValues[i] >= TRACI_level)
                    {
                        downValues.Add(1);
                        downScans.Add(traciValues[i]);
                    }
                    else {
                        downValues.Add(0);
                        downScans.Add(0);
                    }
                    /*if (traciValues[i] < traciValues[i - 1])
                    {
                        downValues.Add(1);
                        if (traciValues[i] > traci_level) downScans.Add(traciValues[i]);
                        else downScans.Add(0);
                    }
                    else
                    {
                        downValues.Add(0);
                        downScans.Add(0);
                    }*/
                    upValues.Add(0);
                    upScans.Add(0);
                }

            }
            for (int i = 0; i < data_length; i++)
            {
                if (i > data_length - 3)
                {
                    ScanDataList.Add(new ScanData
                    {
                        Traci = 0,
                        Up = 0,
                        ScanUp = 0,
                        Down = 0,
                        ScanDown = 0
                    });
                    continue;
                }
                if (i < TRACI_rs_length + 1) traciValue = 0;
                else traciValue = traciValues[i - TRACI_rs_length - 1];
                if (i < TRACI_rs_length + 2)
                {
                    upScan = 0;
                    upValue = 0;
                    downScan = 0;
                    downValue = 0;

                }
                else
                {
                    upScan = upScans[i - TRACI_rs_length - 2];
                    upValue = upValues[i - TRACI_rs_length - 2];
                    downScan = downScans[i - TRACI_rs_length - 2];
                    downValue = downValues[i - TRACI_rs_length - 2];
                }

                ScanDataList.Add(new ScanData
                {
                    Traci = traciValue,
                    Up = upValue,
                    ScanUp = upScan,
                    Down = downValue,
                    ScanDown = downScan
                });

            }
            GearedScanDataList = ScanDataList.AsGearedValues();
            if (mode == 0)
            {
                InitializeData();
            }
            else if (mode == 1)
            {
                string directoryPath = System.IO.Path.Combine(LastSelectedFolderPath, "ScanList");
                directoryPath = System.IO.Path.Combine(directoryPath, DateTime.Now.ToString("yyyy MM dd"));
                ObservableCollection<SaveScanData> tempDataList = new ObservableCollection<SaveScanData>();
                for (int i = 0; i < data_length - 2; i++)
                {
                    tempDataList.Add(new SaveScanData
                    {
                        Date = GearedStockDataList[i].Date,
                        Open = GearedStockDataList[i].Open,
                        Close = GearedStockDataList[i].Close,
                        High = GearedStockDataList[i].High,
                        Low = GearedStockDataList[i].Low,
                        Volume = GearedStockDataList[i].Volume,
                        Traci = GearedScanDataList[i].Traci,
                        Up = GearedScanDataList[i].Up,
                        ScanUp = GearedScanDataList[i].ScanUp,
                        Down = GearedScanDataList[i].Down,
                        ScanDown = GearedScanDataList[i].ScanDown
                    });
                }

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filename = System.IO.Path.GetFileNameWithoutExtension(filePath);
                string dirname = System.IO.Path.GetDirectoryName(filePath);
                if (filename.Split(' ').Length > 1) {
                    return;
                }
                var dir_arr= dirname.Split('\\');
                dirname = dir_arr[dir_arr.Count()-1];
                filename = dirname+" " + filename + " " + TRACI_scan_type + " " + TRACI_level + " " + TRACI_factor + " " + TRACI_period+ ".csv";
                var file_Path = System.IO.Path.Combine(directoryPath, filename);
                try
                {
                    using (var writer = new StreamWriter(file_Path))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(tempDataList);
                    }
                    System.Windows.MessageBox.Show("Save file successed.\n"+file_Path);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }

            }
        }
        /*private void ScanListFileGridList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (var cell in e.AddedCells)
            {
                var columnIndex = cell.Column.DisplayIndex;
                var rowIndex = ScanListFileGridList.Items.IndexOf(cell.Item);
                if (rowIndex >= 0)
                {
                    filePath = ScanFileItems[rowIndex].FullPath;
                    try
                    {
                        var reader = new CsvStockDataReader();
                        StockDataList = reader.ReadData(filePath);
                        data_length = StockDataList.ToList().Count;
                        DateTime date = StockDataList.ToList()[data_length - 1].Date;
                        var new_date = date.AddDays(1);
                        StockDataList = StockDataList.Append(new StockData { Date = new_date, Open = 0, Close = 0, High = 0, Low = 0, Volume = 0 });
                        new_date = new_date.AddDays(1);
                        StockDataList = StockDataList.Append(new StockData { Date = new_date, Open = 0, Close = 0, High = 0, Low = 0, Volume = 0 });
                        data_length = StockDataList.ToList().Count;
                        var reader1 = new CsvScanDataReader();
                        var _scanDataList = reader1.ReadData(filePath);
                        ScanDataList = new ObservableCollection<ScanData>();
                        foreach (var _scanData in _scanDataList)
                        {
                            if (_scanData != null)
                            {
                                ScanDataList.Add(_scanData);
                            }
                        }
                        ScanDataList.Add(new ScanData { Traci= 0, Up = 0, ScanUp = 0, Down = 0, ScanDown = 0});
                        ScanDataList.Add(new ScanData { Traci = 0, Up = 0, ScanUp = 0, Down = 0, ScanDown = 0 });
                        GearedScanDataList = ScanDataList.AsGearedValues();
                        string[] input_arr = ScanFileItems[rowIndex].Title.Split(' ');
                        for (int i = 0; i < TraciScanType.Items.Count; i++)
                        {
                            
                            var traciItem = (ComboBoxItem)TraciScanType.Items[i];
                            if (traciItem.Name == input_arr[2]) TRACI_scan_type = traciItem.Name;
                        }
                        TRACI_status = true;
                        InitializeData();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }
            }

        }*/

        private void TraciScanType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var traci_scan_type = (ComboBoxItem)TraciScanType.SelectedItem;
            TRACI_scan_type = traci_scan_type.Name;
            InitializeTraciScanData();
        }

        private void TraciLevelTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(TraciLevelTextBox.Text))
                {
                    if (int.Parse(TraciLevelTextBox.Text) > 0 && int.Parse(TraciLevelTextBox.Text) <= 100)
                    {
                        TRACI_level = int.Parse(TraciLevelTextBox.Text);
                        TraciLevelTextBox.Text = TRACI_level.ToString();
                        InitializeTraciScanData();
                    }
                    else
                    {
                        TraciLevelTextBox.Text = TRACI_level.ToString();
                    }
                }
            }
        }
		
		public bool CheckTraciScan(string filePath,string traci_scan_type, int traci_level)
        {
            var reader = new CsvStockDataReader();
            var _StockDataList = reader.ReadData(filePath);
            var traciValues = CalculateTraci(_StockDataList.Select((x => x.Close)).ToList(), TRACI_period, TRACI_rs_length);
            if (traci_scan_type == "UP")
            {
                if (traciValues[traciValues.Count - 1] > traciValues[traciValues.Count - 2] && traciValues[traciValues.Count - 1] <= traci_level) return true;
                else return false;
            }
            else if (traci_scan_type == "DOWN")
            {
                if (traciValues[traciValues.Count - 1] < traciValues[traciValues.Count - 2] && traciValues[traciValues.Count - 1] >= traci_level) return true;
                else return false;
            }
            else return false;
        }

        private void factorTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (factorTextbox.Text == "" || double.Parse(factorTextbox.Text) < 1)
            {
                return;
            }
            if (e.Key == Key.Enter)
            {
                decimal val = Decimal.Round(decimal.Parse(factorTextbox.Text),1); ;
                TRACI_factor = double.Parse(val.ToString());
                factorTextbox.Text = val.ToString();
                InitializeData();
            }
        }

        private void periodTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (periodTextbox.Text == "" || int.Parse(periodTextbox.Text) < 1)
            {
                return;
            }
            if (e.Key == Key.Enter)
            {
                TRACI_period = int.Parse(periodTextbox.Text);
                TRACI_rs_length = TRACI_period / 2;
                InitializeData();
            }
        }

        private void Chart_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var chart = sender as CartesianChart;
            
            if (ChartLabels == null||BasicChart.Series.Count<=0) {
                return;
            }
            Point chartMousePosition = e.GetPosition(chart);
            Point chartPoint = BasicChart.ConvertToChartValues(chartMousePosition);
            

            int pointX = (int)chartPoint.X;
            if (pointX < 0 || pointX >= ChartLabels.Count) return;
            string pointDate = ChartLabels[pointX];
            string pointerLabel = String.Format("Date:{0}", pointDate);
            double value;
            if (BasicChart.Series.Count > 0)
            {
                value = (double)BasicChart.Series[0].Values[pointX];
                pointerLabel += ("  " + BasicChart.Series[1].Title + " Open: " + GearedStockDataList[data_start+pointX].Open + " High: " + GearedStockDataList[data_start + pointX].High + " Low: " + GearedStockDataList[data_start + pointX].Low + " Close:" + GearedStockDataList[data_start+pointX].Close);
                pointerLabel += ("  " + BasicChart.Series[0].Title + ":" + value.ToString("0.00"));
            }
            BasicLabel.Content = pointerLabel;
            pointerLabel = String.Format("Date:{0}", pointDate);
            if (ComputedChart.Series.Count > 0)
            {
                foreach (var series in ComputedChart.Series)
                {
                    value = (double)series.Values[pointX];
                    pointerLabel += ("  " + series.Title + ":" + value.ToString("0.00"));

                }
                pointerLabel += (" Volume:" + GearedStockDataList[pointX].Volume);
            }
            ComputedLabel.Content = pointerLabel;
            
            var point = e.GetPosition(drawingCanvas);
            if (point.X > 0 && point.Y > 0 && data_length > 0)
            {
                drawingCanvas.Children.Clear();
                horizontalLine = new Line
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection(new double[] { 2, 4 }),
                    X1 = 0,
                    Y1 = point.Y,
                    X2 = drawingCanvas.ActualWidth,
                    Y2 = point.Y
                };
                verticalLine = new Line
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection(new double[] { 2, 4 }),
                    X1 = point.X,
                    Y1 = 0,
                    X2 = point.X,
                    Y2 = drawingCanvas.ActualHeight,
                };

                if (pointX == (ChartLabels.Count - 3))
                {
                    verticalLine.Stroke = Brushes.Green;
                    verticalLine.StrokeDashArray = new DoubleCollection();
                }
                else
                {

                }

                drawingCanvas.Children.Add(horizontalLine);
                drawingCanvas.Children.Add(verticalLine);
            }
            else
            {
                drawingCanvas.Children.Clear();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectGrid.Visibility = Visibility.Hidden;
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectGrid.Visibility = Visibility.Hidden;
            ShowScanWindow();
        }
        private void ShowScanWindow()
        {
            if (LastSelectedFolderPath == null)
            {
                System.Windows.MessageBox.Show("Select the Folder.");
                return;
            }
            if (scanWindow.IsVisible == false) {
                scanWindow.WindowState = WindowState.Normal;
                scanWindow.ShowDialog();
            }
            
        }
    }
}
