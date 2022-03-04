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


namespace NanoDLP_Browser
{



    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dtos _dtos =new Dtos();
        public void setItemsSource(Dtos dtos) {
            MyListBox.ItemsSource = dtos;
            MyListBox.DataContext = dtos;
            _dtos = dtos;
        }
        public MainWindow()
        {
            InitializeComponent();

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
        AddMachine addform;
        private void MyListBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            // 選択中のアイテムの ListBoxItem を取得
            var listBoxItem = (ListBoxItem)MyListBox.ItemContainerGenerator.ContainerFromItem(MyListBox.SelectedItem);
            Dto dto = null;
            // ヒットテストでアイテム上
            if (listBoxItem != null)
            {
                dto = listBoxItem.Content as Dto;
            }
            addform = new AddMachine(this,dto);
            addform.ShowDialog();

        }

        private void MyListBoxStack_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)sender;
            var parent = stackPanel.Parent;
        }

        private void MyListBox_EditClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            // 選択中のアイテムの ListBoxItem を取得
            var listBoxItem = (ListBoxItem)MyListBox.ItemContainerGenerator.ContainerFromItem(MyListBox.SelectedItem);
            Dto dto = null;
            // ヒットテストでアイテム上
            if (listBoxItem != null)
            {
                dto = listBoxItem.Content as Dto;
            }
            if (dto!=null || dto.ManualAdd) {
                addform = new AddMachine(this, dto);
                addform.ShowDialog();
            }
        }

        private void MyListBox_AddClick(object sender, RoutedEventArgs e)
        {
            Dto dto = null;
            addform = new AddMachine(this, dto);
            addform.ShowDialog();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            addform = new AddMachine(this, current);
            addform.ShowDialog();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Dto current = btn.DataContext as Dto;
            if (MessageBox.Show("Are you sure you want to delete printer?", "NanoDLP Browser", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _dtos.Remove(current);
                FileIO.SaveFile(_dtos);
            }
        }
    }


}
