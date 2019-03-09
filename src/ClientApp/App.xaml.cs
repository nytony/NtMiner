﻿using NTMiner.OverClock;
using NTMiner.Views;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace NTMiner {
    public partial class App : Application, IDisposable {
        public App() {
            Logging.LogDir.SetDir(System.IO.Path.Combine(VirtualRoot.GlobalDirFullName, "Logs"));
            AppHelper.Init(this);
            InitializeComponent();
        }

        private bool createdNew;
        private Mutex appMutex;
        private static string s_appPipName = "ntminerclient";
        protected override void OnExit(ExitEventArgs e) {
            AppHelper.NotifyIcon?.Dispose();
            NTMinerRoot.Current.Exit();
            HttpServer.Stop();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e) {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            if (!string.IsNullOrEmpty(CommandLineArgs.Upgrade)) {
                Vms.AppStatic.Upgrade(CommandLineArgs.Upgrade, () => {
                    Environment.Exit(0);
                });
            }
            else {
                try {
                    appMutex = new Mutex(true, s_appPipName, out createdNew);
                }
                catch (Exception) {
                    createdNew = false;
                }
                if (createdNew) {
                    Vms.AppStatic.IsMinerClient = true;
                    SplashWindow splashWindow = new SplashWindow();
                    splashWindow.Show();
                    NTMinerRoot.AppName = "开源矿工挖矿客户端";
                    NTMinerRoot.Current.Init(()=> {
                        NTMinerRoot.KernelDownloader = new KernelDownloader();
                        UIThread.Execute(() => {
                            MainWindow window = new MainWindow();
                            IMainWindow mainWindow = window;
                            this.MainWindow = window;
                            this.MainWindow.Show();
                            this.MainWindow.Activate();
                            System.Drawing.Icon icon = new System.Drawing.Icon(GetResourceStream(new Uri("pack://application:,,,/NTMiner;component/logo.ico")).Stream);
                            AppHelper.NotifyIcon = ExtendedNotifyIcon.Create(icon, "NTMiner挖矿端", isControlCenterApp: false);
                            #region 处理显示主界面命令
                            VirtualRoot.Accept<ShowMainWindowCommand>(
                                "处理显示主界面命令",
                                LogEnum.None,
                                action: message => {
                                    UIThread.Execute(() => {
                                        Dispatcher.Invoke((ThreadStart)mainWindow.ShowThisWindow);
                                    });
                                });
                            #endregion
                            try {
                                HttpServer.Start($"http://localhost:{WebApiConst.MinerClientAppPort}");
                                NTMinerRoot.Current.Start();
                            }
                            catch (Exception ex) {
                                Logger.ErrorDebugLine(ex.Message, ex);
                            }
                            splashWindow?.Close();
                        });
                    });
                    VirtualRoot.Accept<CloseNTMinerCommand>(
                        "处理关闭NTMiner客户端命令",
                        LogEnum.Console,
                        action: message => {
                            Write.DevLine("关闭客NTMiner客户端");
                            UIThread.Execute(() => {
                                if (MainWindow != null) {
                                    MainWindow.Close();
                                }
                                else {
                                    Shutdown();
                                    Environment.Exit(0);
                                }
                            });
                        });
                    VirtualRoot.On<MineStartedEvent>(
                        "开始挖矿后启动1080ti小药丸",
                        LogEnum.Console,
                        action: message => {
                            OhGodAnETHlargementPill.OhGodAnETHlargementPillUtil.Start();
                        });
                    VirtualRoot.On<MineStopedEvent>(
                        "停止挖矿后停止1080ti小药丸",
                        LogEnum.Console,
                        action: message => {
                            OhGodAnETHlargementPill.OhGodAnETHlargementPillUtil.Stop();
                        });
                    NTMinerOverClockUtil.ExtractResource();
                }
                else {
                    try {
                        AppHelper.ShowMainWindow(this, WebApiConst.MinerClientAppPort);
                    }
                    catch (Exception) {
                        DialogWindow.ShowDialog(message: "另一个NTMiner正在运行，请手动结束正在运行的NTMiner进程后再次尝试。", title: "alert", icon: "Icon_Error");
                        Process currentProcess = Process.GetCurrentProcess();
                        Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
                        foreach (var process in processes) {
                            if (process.Id != currentProcess.Id) {
                                NTMiner.Windows.TaskKill.Kill(process.Id);
                            }
                        }
                    }
                }
            }
            base.OnStartup(e);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                if (appMutex != null) {
                    appMutex.Dispose();
                }
            }
        }
    }
}
