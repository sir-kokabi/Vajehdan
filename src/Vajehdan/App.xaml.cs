using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GlobalHotKey;
using Microsoft.Win32;
using Vajehdan.Views;


namespace Vajehdan
{
    [SupportedOSPlatform("windows7.0")]
    public partial class App
    {
        private DateTime _lastKeyPressedTime;

        public App()
        {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var uncaughtException = ((Exception)e.ExceptionObject);
                uncaughtException.Log();
                MessageBox.Show($"{uncaughtException.Message}\n-----------\nSaved in the log file");
                Environment.Exit(-1);
            };
#endif
            StartUpByWindows();
            SetupHook();
            Database.LoadData();           
        }

        private void StartUpByWindows()
        {
            try
            {
                var appName = Assembly.GetEntryAssembly()?.GetName().Name;
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (!key.GetValueNames().Contains(appName))
                    return;

                Helper.GetSettings().StartByWindows = true;
                Helper.GetSettings().Save();
                var value = Process.GetCurrentProcess().MainModule?.FileName;
                key.SetValue(appName, value ?? string.Empty);

            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private void SetupHook()
        {
            var hotKeyManager = new HotKeyManager();

            try
            {
                hotKeyManager.Register(Key.None, ModifierKeys.Alt);
                hotKeyManager.KeyPressed += (_, _) =>
                {
                    if (!Helper.GetSettings().OpenByHotKey)
                        return;

                    if (DateTime.Now - _lastKeyPressedTime > TimeSpan.FromMilliseconds(400))
                    {
                        _lastKeyPressedTime = DateTime.Now;
                        return;
                    }

                    foreach (var mainWindow in Helper.GetWindow<MainWindow>())
                    {
                        mainWindow.ShowMainWindow();
                    }
                };
            }
            catch (Exception ex)
            {
                hotKeyManager.Dispose();
                ex.Log();
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (Helper.GetSettings().ClearHistoryOnClose)
            {
                Helper.GetSettings().AutoCompleteList.Clear();
                Helper.GetSettings().Save();
            }

            Process.GetCurrentProcess().Kill();
        }
    }
}
