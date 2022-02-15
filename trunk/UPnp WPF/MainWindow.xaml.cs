using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
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
using System.Windows.Threading;
using UPNPLib;
using Newtonsoft.Json;
namespace UPnp_WPF
{
    public class NanoDLPStatus
    {
        public bool Printing;
        public string Path;
        public double LayersCount;
        public double LayerID;
        public double LayerStartTime;
        public double LayerTime;
        public double Layer;
        public double PlateHeight;

        public double getLayerTime
        { 
            get {
                return LayerTime / 1000000000;
             }
        }
        public double getRemainingTime { 
            get {
                return Math.Round((this.LayersCount - this.LayerID) * this.getLayerTime / 60);
            }
        }
        public double getTotalTime
        {
            get
            {
                return Math.Round(this.LayersCount * this.getLayerTime / 60);
            }
        }

        public double getCurrentHeight
        {
            get
            {
                return this.PlateHeight / this.LayersCount * this.LayerID;
            }
        }
        public string getETA {
            get {
                DateTime est = DateTime.Now;
                return est.AddMinutes(this.getRemainingTime).ToString("HH:mm");

            }
        }

    }
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string status_api = "status";
        private DeviceCollector _col;
        private ObservableCollection<Dto> _dtos = new ObservableCollection<Dto>();
        private DispatcherTimer _timer;
        private Dto selected;

        public MainWindow()
        {
            InitializeComponent();

            MyListBox.ItemsSource = _dtos;

            _col = new DeviceCollector();

            _col.DeviceAdded += new DeviceCollector.DeviceAddedEventHandler(this.DeviceAdded);
            _col.SearchCompleted += new DeviceCollector.SearchCompletedEventHandler(this.SearchCompleted);

            // タイマのインスタンスを生成
            _timer = new DispatcherTimer(); // 優先度はDispatcherPriority.Background
                                            // インターバルを設定
            _timer.Interval = new TimeSpan(0, 0, 1);
            // タイマメソッドを設定
            _timer.Tick += new EventHandler(MyTimerMethod);
            // タイマを開始
            _timer.Start();

            // 画面が閉じられるときに、タイマを停止
            this.Closing += new CancelEventHandler(StopTimer);
        }
        // タイマを停止
        private void StopTimer(object sender, CancelEventArgs e)
        {
            _timer.Stop();
        }

        // タイマメソッド
        private async void MyTimerMethod(object sender, EventArgs e)
        {
            try
            {

                using (var client = new HttpClient())
                {
                    foreach (var each in _dtos)
                    {
                        NanoDLPStatus stat = new NanoDLPStatus();
                        var result = await client.GetAsync(each.URI + "" + status_api);
                        var json = await result.Content.ReadAsStringAsync();
                        // インスタンス person にデシリアライズ
                        stat = JsonConvert.DeserializeObject<NanoDLPStatus>(json);
                        if (stat.Printing == true)
                        {
                            System.Diagnostics.Debug.Write("Plate : " + stat.Path);
                            System.Diagnostics.Debug.Write(" Layer : " + stat.LayerID + " of " + stat.LayersCount);
                            System.Diagnostics.Debug.Write(" RemainingTime : " + stat.getRemainingTime + " of " + stat.getTotalTime + " min");
                            System.Diagnostics.Debug.Write(" Height : " + Math.Round(stat.getCurrentHeight, 1) + " of " + Math.Round(stat.PlateHeight, 1) + " mm");
                            System.Diagnostics.Debug.Write(" ETA : " + stat.getETA);

                            System.Diagnostics.Debug.WriteLine("");
                            each.visibleStop = true;
                            each.visibleMove = false;
                            this.MyListBox.Items.Refresh();
                        }
                        else {
                            each.visibleStop = false;
                            each.visibleMove = true;
                            this.MyListBox.Items.Refresh();
                        }
                    }
                }

            }
            catch {
                _timer.Stop();
            }

        }
        public void DeviceAdded(UPnPDevice device)
        {
            if (device.ManufacturerName == "NanoDLP") {
                _dtos.Add(
                    new Dto()
                    {
                        Name = device.FriendlyName,
                        URI = device.PresentationURL,
                        Discription = device.ManufacturerName,
                        visibleMove = true,
                        visibleStop = false,
                        Enable = true
                        //Device = device
                    }
                    );
                MyListBox.Items.Refresh();
            }
        }

        public void SearchCompleted()
        {
            System.Diagnostics.Debug.WriteLine("SearchCompleted");
        }

        public void DeviceRemoved(string sUDN)
        {
            System.Diagnostics.Debug.WriteLine("DeviceRemoved : " + sUDN);
        }

        private void MyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 選択中のアイテムの ListBoxItem を取得
            var listBoxItem = (ListBoxItem)MyListBox.ItemContainerGenerator.ContainerFromItem(MyListBox.SelectedItem);
            // ヒットテストでアイテム上
            if (listBoxItem.InputHitTest(e.GetPosition(listBoxItem)) != null)
            {
                Dto dto = listBoxItem.Content as Dto;
                var uri = dto.URI;
                // 選択中のアイテム上でダブルクリックされたとき
                System.Diagnostics.Process.Start(uri);
            }
            else
            {
                // アイテム以外でダブルクリックされたとき
            }
        }

        private void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 選択中のアイテムの ListBoxItem を取得
            var listBoxItem = (ListBoxItem)MyListBox.ItemContainerGenerator.ContainerFromItem(MyListBox.SelectedItem);
            selected = listBoxItem.Content as Dto;
            _timer.Start();
        }

        private T FindParent<T>(DependencyObject d) where T : DependencyObject
        {
            while (d != null)
            {
                d = VisualTreeHelper.GetParent(d);
                if (d is T)
                {
                    return (T)d;
                }
            }
            return null;
        }
        private async void forceStop_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            try
            {
                using (var client = new HttpClient())
                {
                    string stopApi = "printer/force-stop";
                    var result = await client.GetAsync(current.URI + "" + stopApi);
                }
            }
            catch { }
        }

        private async void moveTop_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            try
            {
                using (var client = new HttpClient())
                {
                    string stopApi = "z-axis/top";
                    var result = await client.GetAsync(current.URI + "" + stopApi);
                }
            }
            catch { }
        }

        private async void moveBottom_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            try
            {
                using (var client = new HttpClient())
                {
                    string stopApi = "z-axis/top";
                    var result = await client.GetAsync(current.URI + "" + stopApi);
                }
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyListBox.DataContext = _dtos;
        }

        private void MyListBox_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }


}
