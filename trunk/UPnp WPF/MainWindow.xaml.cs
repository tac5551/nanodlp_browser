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

using NOTIFY = Notifications.Wpf;
namespace UPnp_WPF
{

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string status_api = "status";
        private DeviceCollector _col;
        private ObservableCollection<Dto> _dtos = new ObservableCollection<Dto>();
        private readonly NOTIFY.NotificationManager notificationManager = new NOTIFY.NotificationManager();
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

            using (var client = new HttpClient())
            {
                foreach (var each in _dtos)
                {
                    try
                    {

                        if (each.Enable)
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
                                each.Plate = stat.Path;
                                each.Remaining = stat.getRemainingTime + " of " + stat.getTotalTime + " min";
                                each.Layer = stat.LayerID + " of " + stat.LayersCount;
                                each.Height = Math.Round(stat.getCurrentHeight, 1) + " of " + Math.Round(stat.PlateHeight, 1) + " mm";
                                each.ETA = stat.getETA;
                                this.MyListBox.Items.Refresh();
                            }
                            else
                            {
                                each.visibleStop = false;
                                each.visibleMove = true;
                                each.Plate = "";
                                each.Remaining = "";
                                each.Layer = "";
                                each.Height = "";
                                each.ETA = "";
                                this.MyListBox.Items.Refresh();
                            }
                        }

                    }
                    catch
                    {
                        each.Enable = false;
                    }
                }
            }


        }
        public void DeviceAdded(UPnPDevice device)
        {
            if (device.ManufacturerName == "NanoDLP") {
                bool flag = true;
                foreach (Dto each in MyListBox.Items)
                {
                    if (each.UUID == device.UniqueDeviceName)
                    {
                        flag = false;
                    }
                }
                if (flag) {
                    _dtos.Add(
                        new Dto()
                        {
                            Name = device.FriendlyName,
                            URI = device.PresentationURL,
                            Discription = device.ManufacturerName,
                            visibleMove = true,
                            visibleStop = false,
                            UUID = device.UniqueDeviceName,
                            Enable = true
                        //Device = device
                    }
                        );
                }
                MyListBox.Items.Refresh();
            }
        }

        public void SearchCompleted()
        {
            System.Diagnostics.Debug.WriteLine("SearchCompleted");
            _col.DeviceSearchStart();
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

        private async void Reboot_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            try
            {
                using (var client = new HttpClient())
                {
                    string stopApi = "printer/restart";
                    var result = await client.GetAsync(current.URI + "" + stopApi);
                }
            }
            catch { }
        }

        private async void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            try
            {
                using (var client = new HttpClient())
                {
                    string stopApi = "printer/off";
                    var result = await client.GetAsync(current.URI + "" + stopApi);
                }
            }
            catch { }
        }

        private async void PrintStop_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            try
            {
                using (var client = new HttpClient())
                {
                    string stopApi = "printer/stop";
                    var result = await client.GetAsync(current.URI + "" + stopApi);
                }
            }
            catch { }
        }

        private async void PrintPause_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            try
            {
                using (var client = new HttpClient())
                {
                    string stopApi = "printer/pause";
                    var result = await client.GetAsync(current.URI + "" + stopApi);
                }
            }
            catch { }
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            var content = new NOTIFY.NotificationContent
            {
                Type = NOTIFY.NotificationType.Information,
                Title = "タイトル",
                Message = "こんにちは",
            };

            // メッセージを2秒表示
            notificationManager.Show(
                content, expirationTime: TimeSpan.FromSeconds(2));
        }
    }


}
