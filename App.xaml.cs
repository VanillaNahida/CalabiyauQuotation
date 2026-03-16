using System;
using System.Windows;
using CalabiyauQuotation.Services;

namespace CalabiyauQuotation
{
    public partial class App : Application
    {
        private static System.Threading.Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            ToastNotificationHelper.Initialize();

            string mutexName = "CalabiyauQuotationMutex";
            _mutex = new System.Threading.Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                MessageBox.Show("禁止重复运行喵！如无窗口显示，请查看任务栏托盘图标是否存在，或者使用任务管理器结束进程后再打开本软件喵", "警告！", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }

            base.OnExit(e);
        }
    }
}
