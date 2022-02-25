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



    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        public void setItemsSource(ObservableCollection<Dto> dtos) {
            MyListBox.ItemsSource = dtos;
            MyListBox.DataContext = dtos;
        }
        public MainWindow()
        {
            InitializeComponent();
        }

 

        private void MyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            // 選択中のアイテムの ListBoxItem を取得
            var listBoxItem = (ListBoxItem)MyListBox.ItemContainerGenerator.ContainerFromItem(MyListBox.SelectedItem);
            // ヒットテストでアイテム上
            if (listBoxItem != null)
            {
                Dto dto = listBoxItem.Content as Dto;
                var uri = dto.URI;
                // 選択中のアイテム上でダブルクリックされたとき
                System.Diagnostics.Process.Start(uri);
            }
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
                if (MessageBox.Show("Are you sure you want to stop printing?", "NanoDLP Browser", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    using (var client = new HttpClient())
                    {
                        string stopApi = "printer/force-stop";
                        var result = await client.GetAsync(current.URI + "" + stopApi);
                    }
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
                    string stopApi = "z-axis/bottom";
                    var result = await client.GetAsync(current.URI + "" + stopApi);
                }
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
                if(MessageBox.Show("Are you sure you want to restart the printer?", "NanoDLP Browser",MessageBoxButton.YesNo,MessageBoxImage.Question ,MessageBoxResult.No) ==MessageBoxResult.Yes)
                {
                    using (var client = new HttpClient())
                    {
                        string stopApi = "printer/restart";
                        var result = await client.GetAsync(current.URI + "" + stopApi);
                    }
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
                if (MessageBox.Show("Are you sure you want to power off the printer?", "NanoDLP Browser", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    using (var client = new HttpClient())
                    {
                        string stopApi = "printer/off";
                        var result = await client.GetAsync(current.URI + "" + stopApi);
                    }
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
                if (MessageBox.Show("Are you sure you want to stop printing?", "NanoDLP Browser", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    using (var client = new HttpClient())
                    {
                        string stopApi = "printer/stop";
                        var result = await client.GetAsync(current.URI + "" + stopApi);
                    }
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

    }


}
