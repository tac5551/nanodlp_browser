namespace NanoDLP_Browser
{
    using NanoDLP_Browser.Properties;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.InteropServices;
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
        // 外部プロセスのメイン・ウィンドウを起動するためのWin32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        // ShowWindowAsync関数のパラメータに渡す定義値
        private const int SW_RESTORE = 9;  // 画面を元の大きさに戻す

        private DeviceCollector _col;
        public Dtos _dtos = new Dtos();
        public readonly NOTIFY.NotificationManager notificationManager = new NOTIFY.NotificationManager();
        private DispatcherTimer _timer;
        private MainWindow wnd;
        private System.Threading.Mutex mutex = new System.Threading.Mutex(false, "NanoDLPBrowser");

        // 外部プロセスのウィンドウを起動する
        public static void WakeupWindow(IntPtr hWnd)
        {
            // メイン・ウィンドウが最小化されていれば元に戻す
            if (IsIconic(hWnd))
            {
                ShowWindowAsync(hWnd, SW_RESTORE);
            }

            // メイン・ウィンドウを最前面に表示する
            SetForegroundWindow(hWnd);
        }

        // 実行中の同じアプリケーションのプロセスを取得する
        public static Process GetPreviousProcess()
        {
            Process curProcess = Process.GetCurrentProcess();
            Process[] allProcesses = Process.GetProcessesByName(curProcess.ProcessName);

            foreach (Process checkProcess in allProcesses)
            {
                // 自分自身のプロセスIDは無視する
                if (checkProcess.Id != curProcess.Id)
                {
                    // プロセスのフルパス名を比較して同じアプリケーションか検証
                    if (String.Compare(
                        checkProcess.MainModule.FileName,
                        curProcess.MainModule.FileName, true) == 0)
                    {
                        // 同じフルパス名のプロセスを取得
                        return checkProcess;
                    }
                }
            }

            // 同じアプリケーションのプロセスが見つからない！
            return null;
        }

        /// <summary>
        /// NotifyIconWrapper クラス を生成、初期化します。
        /// </summary>
        public NotifyIconWrapper()
        {


            //// ミューテックスの所有権を要求
            //if (!mutex.WaitOne(0, false))
            //{
            //    // 既に起動しているため終了させる
            //    MessageBox.Show("NanoDLPBrowser is already running", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    mutex.Close();
            //    mutex = null;
            //    Application.Current.Shutdown();
            //}
            Process prevProcess = GetPreviousProcess();
            if (prevProcess != null)
            {
                WakeupWindow(prevProcess.MainWindowHandle);
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
            //_dtos.Clear();
            FileIO.LoadFile(ref _dtos);

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

        // タイマメソッド
        private async void MyTimerMethod(object sender, EventArgs e)
        {
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
                                //NanoDLP
                                if (!each.isOctprint && !each.isNova3D) {
                                    NanoDLPStatus stat = new NanoDLPStatus();

                                    //try
                                    //{
                                    //    WebRequest request = WebRequest.Create(each.URI + "" + "status");

                                    //    request.Proxy = null;
                                    //    request.Method = "GET";

                                    //    WebResponse response = request.GetResponse();
                                    //    Stream dataStream = response.GetResponseStream();
                                    //    StreamReader reader = new StreamReader(dataStream);
                                    //    var json = reader.ReadToEnd() as string;

                                    //}
                                    //catch (Exception ex) {
                                    //    System.Diagnostics.Debug.WriteLine(ex.Message);
                                    //}
                                    string URI = each.URI;
                                    if (URI.Substring(URI.Length - 1) != "/") {
                                        URI += "/";
                                    }

                                    var result = await client.GetAsync(URI + "" + "status");
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
                                else if(each.isNova3D){

                                    try {
                                        string URI = each.URI;
                                        if (URI.Substring(URI.Length - 1) == "/")
                                        {
                                            if (URI.Substring(URI.Length - 6) != ":8081/") {
                                                URI = URI.Substring(0, URI.Length - 1) + ":8081/";
                                            }
                                        }

                                        WebRequest request = WebRequest.Create(URI + "job/list/");

                                        request.Proxy = null;
                                        request.Method = "GET";

                                        WebResponse response = request.GetResponse();
                                        Stream dataStream = response.GetResponseStream();
                                        StreamReader reader = new StreamReader(dataStream);
                                        var json = reader.ReadToEnd() as string;

                                        //よくわからない大かっこがあるので切り取り
                                        json = json.Substring(1, json.Length - 1-3).Trim();

                                        //json = "{";
                                        //json += "\"currentSlice\" : 76,";
                                        //json += "\"exposureTime\" : 1600,";
                                        //json += "\"thickness\" : 0.029999999999999999,";
                                        //json += "\"totalSlices\" : 77,";
                                        //json += "\"jobName\" : \"焼き魚.CWS\",";
                                        //json += "\"status\" : \"Printing\",";
                                        //      json += "\"zliftDistance\" : 7.0,";
                                        //json += "\"zliftSpeed\" : 120.0,";
                                        //json += "\"lastNAverageSliceTime\" : 0,";
                                        //json += "\"elapsedTime\" : 1666324";
                                        //json += "}";
                                        if (json != "null")
                                        {
                                            JObject Nova3Dstatus = JObject.Parse(json);

                                            if (Nova3Dstatus["status"].ToString() == "Printing")
                                            {
                                                each.Printing = true;
                                                each.Plate = Nova3Dstatus["jobName"].ToString();

                                                each.LayerMax = ((int)Nova3Dstatus["totalSlices"]);
                                                each.LayerNow = ((int)Nova3Dstatus["currentSlice"]);
                                                double thickness = (double)((int)(((double)Nova3Dstatus["thickness"]) * 1000 + 0.9)) / 1000;
                                                each.Height = Math.Round(((double)(((double)Nova3Dstatus["currentSlice"]) * thickness)), 1).ToString() + "mm";
                                                each.Height += " / " + Math.Round(((double)(((double)Nova3Dstatus["totalSlices"]) * thickness)), 1).ToString() + "mm";
                                                each.Layer = ((int)Nova3Dstatus["currentSlice"]) + " of " + ((int)Nova3Dstatus["totalSlices"]);
                                                each.Remaining = "";
                                                each.ETA = "";
                                            }
                                            else
                                            {
                                                each.Printing = false;
                                                each.LayerMax = 100;
                                                each.LayerNow = 0;
                                            }
                                        }
                                        else {
                                            each.Printing = false;
                                            each.LayerMax = 100;
                                            each.LayerNow = 0;
                                        }
                                    } catch (Exception ex) {
                                        System.Diagnostics.Debug.WriteLine(ex.Message);
                                        each.Printing = false;
                                        each.LayerMax = 100;
                                        each.LayerNow = 0;

                                    }

                                }
                                // for Octprint
                                else if (each.isOctprint)
                                {

                                    WebRequest request = WebRequest.Create(each.URI + "/" + "api/job");
                                    request.ContentType = "application/json";
                                    //request.Headers.Add("X-Api-key: " + App.settings.API_key);
                                    request.Headers.Add("X-Api-key: 2BA8A606462B48D3980EF8C22162C409");
                                    request.Proxy = null;
                                    request.Method = "GET";

                                    WebResponse response = request.GetResponse();
                                    Stream dataStream = response.GetResponseStream();
                                    StreamReader reader = new StreamReader(dataStream);
                                    var json = reader.ReadToEnd() as string;




                                    jobMain stat = new jobMain();
                                    stat = JsonConvert.DeserializeObject<jobMain>(json);
                                    if (stat.state == "Printing")
                                    {
                                        each.Printing = true;
                                        each.Plate = stat.job.file.name;
                                        each.Height = "Print Time Left: " + stat.progress.printTimeLeft / 60 + " :" + stat.progress.printTimeLeft % 60;
                                        each.Layer = "Print Time: " + stat.progress.printTime / 60 + " : " + stat.progress.printTime % 60;
                                        each.Remaining = "Total Print Time : " + (int)(stat.job.averagePrintTime / 60) + " : " + (int)(stat.job.averagePrintTime % 60);
                                        each.ETA = "";

                                        each.LayerMax = (int)stat.job.file.size;
                                        each.LayerNow = (int)stat.progress.filepos;
                                    }
                                    else {
                                        each.Printing = false;
                                    }
                                    if (wnd != null) wnd.MyListBox.Items.Refresh();
                                }


                            }
 
                        }
                        catch(Exception ex)
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
