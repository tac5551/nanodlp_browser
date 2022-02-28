﻿namespace NanoDLP_Browser
{
    using NanoDLP_Browser.Properties;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using UPNPLib;
    using NOTIFY = Notifications.Wpf;


    /// <summary>
    /// タスクトレイ通知アイコン
    /// </summary>
    public partial class NotifyIconWrapper : Component
    {
        private DeviceCollector _col;
        public ObservableCollection<Dto> _dtos = new ObservableCollection<Dto>();
        public readonly NOTIFY.NotificationManager notificationManager = new NOTIFY.NotificationManager();
        private DispatcherTimer _timer;
        private MainWindow wnd;
        private System.Threading.Mutex mutex = new System.Threading.Mutex(false, "NanoDLPBrowser");

        string fileName = @"sample.xml";

        public void SaveToXML(string fileName, List<Dto> saveDtos)
        {
            //XmlSerializerオブジェクトを作成
            //オブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(List<Dto>));
            //書き込むファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                fileName, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(sw, saveDtos);
            //ファイルを閉じる
            sw.Close();

        }

        public List<Dto> LoadFromXML(string fileName)
        {
            //XmlSerializerオブジェクトを作成
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(List<Dto>));
            //読み込むファイルを開く
            System.IO.StreamReader sr = new System.IO.StreamReader(
                fileName, new System.Text.UTF8Encoding(false));
            //XMLファイルから読み込み、逆シリアル化する
            List<Dto> obj = (List<Dto>)serializer.Deserialize(sr);
            //ファイルを閉じる
            sr.Close();
            return obj;
        }

        public void SaveFile(string fileName)
        {
            List<Dto> saveDtos = new List<Dto>();
            foreach (var each in _dtos)
            {
                if (each.ManualAdd == true)
                {
                    saveDtos.Add(each);
                }
            }

            SaveToXML(fileName, saveDtos);
        }

        public void LoadFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                List<Dto> saveDtos = LoadFromXML(fileName);

                foreach (var each in saveDtos)
                {
                    _dtos.Add(new Dto
                    {
                        Name = each.Name,
                        URI = each.URI,
                        UUID = each.UUID,
                        Discription = each.Discription,
                        ManualAdd = true,
                        Enable = true,
                    });
                }
            }
        }

        /// <summary>
        /// NotifyIconWrapper クラス を生成、初期化します。
        /// </summary>
        public NotifyIconWrapper()
        {
            LoadFile(fileName);

            // ミューテックスの所有権を要求
            if (!mutex.WaitOne(0, false))
            {
                // 既に起動しているため終了させる
                MessageBox.Show("NanoDLPBrowser is already running", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                mutex.Close();
                mutex = null;
                Application.Current.Shutdown();
            }

            // コンポーネントの初期化
            this.InitializeComponent();

            // コンテキストメニューのイベントを設定
            this.toolStripMenuItem_Open.Click += this.toolStripMenuItem_Open_Click;
            this.toolStripMenuItem_Exit.Click += this.toolStripMenuItem_Exit_Click;

            _col = new DeviceCollector();

            _col.DeviceAdded += new DeviceCollector.DeviceAddedEventHandler(this.DeviceAdded);
            _col.SearchCompleted += new DeviceCollector.SearchCompletedEventHandler(this.SearchCompleted);
            _col.DeviceRemoved += new DeviceCollector.DeviceRemovedEventHandler(this.DeviceRemoved);


            // タイマのインスタンスを生成
            _timer = new DispatcherTimer(); // 優先度はDispatcherPriority.Background
                                            // インターバルを設定
            _timer.Interval = new TimeSpan(0, 0, 1);
            // タイマメソッドを設定
            _timer.Tick += new EventHandler(MyTimerMethod);
            // タイマを開始
            _timer.Start();

            // 画面が閉じられるときに、タイマを停止
            //this.Closing += new CancelEventHandler(StopTimer);

            // MainWindow を生成、表示
            wnd = new MainWindow();
            wnd.setItemsSource(_dtos);
            wnd.Show();
        }

        ~NotifyIconWrapper()
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
            _timer.Stop();
        }



        public void DeviceAdded(UPnPDevice device)
        {
            if (device.ManufacturerName == "NanoDLP")
            {
                bool flag = true;
                foreach (Dto each in _dtos)
                {
                    if (each.UUID == device.UniqueDeviceName)
                    {
                        flag = false;
                    }
                    if (each.Enable == false)
                    {
                        each.Enable = true;
                    }
                }
                if (flag)
                {
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
                    var content = new NOTIFY.NotificationContent
                    {
                        Type = NOTIFY.NotificationType.Information,
                        Title = "[" + device.FriendlyName + "]",
                        Message = "A new printer has been connected.",
                    };

                    // メッセージを2秒表示
                    notificationManager.Show(
                        content, expirationTime: TimeSpan.FromSeconds(5));
                }
                if (wnd != null) wnd.MyListBox.Items.Refresh();
            }
        }
        public void DeviceRemoved(string UUID)
        {
            var content = new NOTIFY.NotificationContent
            {
                Type = NOTIFY.NotificationType.Information,
                Title = "[" + UUID + "]",
                Message = "A new printer has been removed.",
            };

            // メッセージを2秒表示
            notificationManager.Show(
                content, expirationTime: TimeSpan.FromSeconds(5));
        }

        private bool exec = false;
        // タイマメソッド
        private async void MyTimerMethod(object sender, EventArgs e)
        {
            exec = true;
            try
            {
                using (var client = new HttpClient())
                {
                    foreach (Dto each in _dtos)
                    {
                        try
                        {

                            if (each.Enable)
                            {
                                NanoDLPStatus stat = new NanoDLPStatus();
                                var result = await client.GetAsync(each.URI + "" + "status");
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
                                    each.Printing = stat.Printing;
                                    each.LayerMax = (int)stat.LayersCount;
                                    each.LayerNow = (int)stat.LayerID;
                                    if (wnd != null) wnd.MyListBox.Items.Refresh();
                                }
                                // NOTE : 印刷終了
                                else if (stat.Printing != each.Printing && stat.Printing == false)
                                {
                                    var content = new NOTIFY.NotificationContent
                                    {
                                        Type = NOTIFY.NotificationType.Information,
                                        Title = "[" + each.Name + "]",
                                        Message = "The plate \"" + each.Plate + "\" is printing done.",
                                    };

                                    // メッセージを2秒表示
                                    notificationManager.Show(
                                        content, expirationTime: TimeSpan.FromSeconds(5));
                                    each.Printing = stat.Printing;
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
                                    each.Printing = stat.Printing;
                                    each.LayerMax = 100;
                                    each.LayerNow = 0;
                                    if (wnd != null) wnd.MyListBox.Items.Refresh();
                                }
                            }
                        }
                        catch
                        {
                            if (!each.ManualAdd) each.Enable = false;
                            each.LayerMax = 100;
                            each.LayerNow = 0;
                        }

                    }
                }
            }
            catch
            {
                
            }

            exec = false;
        }


        public void SearchCompleted()
        {
            System.Diagnostics.Debug.WriteLine("SearchCompleted");

            _col.DeviceSearchStop();
            _col.DeviceSearchStart();
        }

        ///// <summary>
        ///// コンテナ を指定して NotifyIconWrapper クラス を生成、初期化します。
        ///// </summary>
        ///// <param name="container">コンテナ</param>
        //public NotifyIconWrapper(IContainer container)
        //{
        //    container.Add(this);

        //    this.InitializeComponent();

        //}

        /// <summary>
        /// コンテキストメニュー "表示" を選択したとき呼ばれます。
        /// </summary>
        /// <param name="sender">呼び出し元オブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void toolStripMenuItem_Open_Click(object sender, EventArgs e)
        {
            if (!wnd.IsLoaded)
            {
                wnd = new MainWindow();
                wnd.setItemsSource(_dtos);
                wnd.Show();
            }
        }

        /// <summary>
        /// コンテキストメニュー "終了" を選択したとき呼ばれます。
        /// </summary>
        /// <param name="sender">呼び出し元オブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            // NotifyIcon を非表示
            this.notifyIcon1.Visible = false;

            // 現在のアプリケーションを終了
            Application.Current.Shutdown();
        }
    }
}
