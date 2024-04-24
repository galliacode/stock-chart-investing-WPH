using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace trading_charts
{
    
    public partial class ScanWindow : Window
    {
        private string TRACI_scan_type = "UP";
        private int TRACI_level = 50;
        private string selectedFolderPath;
        private ObservableCollection<FileItem> FileItems { get; set; }
        public ScanWindow()
        {
            InitializeComponent();
            FileItems = new ObservableCollection<FileItem>();
            ScanFileGridList.ItemsSource = FileItems;
            TraciLevelTextBox.Text = TRACI_level.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; 
            this.Hide();
        }

        private void ScanOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Owner is MainWindow mw)
            {
                if (string.IsNullOrEmpty(TraciLevelTextBox.Text) || TRACI_level <= 0 || TRACI_level > 100)
                {
                    System.Windows.MessageBox.Show("Set level.");
                    return;
                }
                var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

                if (!string.IsNullOrEmpty(mw.LastSelectedFolderPath))
                {
                    folderBrowserDialog.SelectedPath = mw.LastSelectedFolderPath;
                }
                var result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    selectedFolderPath = folderBrowserDialog.SelectedPath;
                    FolderPathTextBox.Text = selectedFolderPath;
                    ProcessScanList(selectedFolderPath);
                }
            }
            
        }
        private void FolderPathTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                selectedFolderPath = FolderPathTextBox.Text;
                bool directoryExists = Directory.Exists(selectedFolderPath);
                if (directoryExists)
                {
                    ProcessScanList(selectedFolderPath);
                }
                else return;
            }
        }
        private void ProcessScanList(string selectedFolderPath)
        {
            if (this.Owner is MainWindow mw) 
            {
                try
                {
                    FileItems.Clear();
                    var files = Directory.GetFiles(selectedFolderPath, "*.csv");
                    foreach (var file in files)
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        string value = System.IO.Path.GetFileNameWithoutExtension(file);
                        var name_arr = value.Split(' ');
                        FileItem fileItem = new FileItem
                        {
                            Symbol = name_arr[0],
                            Title = fileName,
                            ParentDirectory = "",
                            FullPath = file
                        };

                        // Check if the file is already in the list
                        bool alreadyExists = FileItems.Cast<FileItem>().Any(item => item.FullPath == file);

                        if (!alreadyExists)
                        {
                            if (mw.CheckTraciScan(file, TRACI_scan_type, TRACI_level)) FileItems.Add(fileItem);
                        }

                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }
        
        private void TraciLevelTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                if (!string.IsNullOrEmpty(TraciLevelTextBox.Text))
                {
                    if (int.Parse(TraciLevelTextBox.Text) > 0 && int.Parse(TraciLevelTextBox.Text) <= 100)
                    {
                        TRACI_level = int.Parse(TraciLevelTextBox.Text);
                        TraciLevelTextBox.Text = TRACI_level.ToString();
                        if(!string.IsNullOrEmpty(selectedFolderPath)) ProcessScanList(selectedFolderPath);
                    }
                    else
                    {
                        TraciLevelTextBox.Text = TRACI_level.ToString();
                    }
                }
            }
        }
        private void TraciScanType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var traciScanType = (ComboBoxItem)TraciScanType.SelectedItem;
            TRACI_scan_type = traciScanType.Name;
            if (!string.IsNullOrEmpty(selectedFolderPath)) ProcessScanList(selectedFolderPath);
        }
        private void ScanFileGridList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (this.Owner is MainWindow mw)
            {
                if (TRACI_level <= 0 || TRACI_level > 100)
                {
                    return;
                }
                foreach (var cell in e.AddedCells)
                {
                    var columnIndex = cell.Column.DisplayIndex;
                    var rowIndex = ScanFileGridList.Items.IndexOf(cell.Item);
                    if (rowIndex >= 0 || rowIndex < ScanFileGridList.Items.Count)
                    {
                        mw.filePath = FileItems[rowIndex].FullPath;
                        mw.TRACI_level = TRACI_level;
                        mw.TRACI_scan_type = TRACI_scan_type;
                        try
                        {
                            var reader = new CsvStockDataReader();
                            mw.StockDataList = reader.ReadData(mw.filePath);
                            mw.data_length = mw.StockDataList.ToList().Count;
                            DateTime date = mw.StockDataList.ToList()[mw.data_length - 1].Date;
                            var new_date = date.AddDays(1);
                            mw.StockDataList = mw.StockDataList.Append(new StockData { Date = new_date, Open = 0, Close = 0, High = 0, Low = 0, Volume = 0 });
                            new_date = new_date.AddDays(1);
                            mw.StockDataList = mw.StockDataList.Append(new StockData { Date = new_date, Open = 0, Close = 0, High = 0, Low = 0, Volume = 0 });
                            mw.data_length = mw.StockDataList.ToList().Count;
                            mw.TRACI_status = true;
                            int mode = 0;
                            mw.InitializeTraciScanData(mode);
                            this.Hide();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                }
            }

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized) 
            {
                this.Hide();
            }

        }
    }
}
