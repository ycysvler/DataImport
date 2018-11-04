using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataImport.Interactive.MdbAdapter
{
    /// <summary>
    /// MdbAdapterPage.xaml 的交互逻辑
    /// </summary>
    public partial class MdbAdapterPage : UserControl
    {
        [DllImport("user32.DLL")]
        public static extern IntPtr FindWindow(String lpszClass, string lpszWnidow);

        [DllImport("user32.DLL")]
        public static extern IntPtr SetParent(IntPtr hwndChild, IntPtr hwndNewParent);

        [DllImport("user32.DLL")]
        public static extern long GetWindowLong(IntPtr hwndChild, int index);

        [DllImport("user32.DLL")]
        public static extern long SetWindowLong(IntPtr hwndChild, int oldStyle, long newStyle);

        const int GWL_STYPE = (-16);
        const long WS_CAPTION = 0x00C00L;
        const long WS_CHILD = 0x40000000L;

        public MdbAdapterPage()
        {
            InitializeComponent();

            this.Loaded += MdbAdapterPage_Loaded;
        }

        private void MdbAdapterPage_Loaded(object sender, RoutedEventArgs e)
        {
            //http://localhost:8080/Platform_V6/avicit/platform6/modules/system/sysdashboard/mpm_ribbon_index.jsp

            //WindowsFormsHost host = new WindowsFormsHost();
            //System.Windows.Forms.WebBrowser web = new System.Windows.Forms.WebBrowser();
            
            //web.Url = new Uri("http://office.qq.com/download.html");

            //host.Child = web;

            //root.Children.Add(host);

           return;

            Process process = Process.Start("iexplore.exe", "http://localhost:8080/Platform_V6/avicit/platform6/modules/system/sysdashboard/mpm_ribbon_index.jsp");

            process.WaitForInputIdle();

            Thread.Sleep(3000);

            var ieHandle = FindWindow("ieframe", null);

            IntPtr hwnd = new WindowInteropHelper(App.Current.MainWindow).Handle;

            SetParent(ieHandle, hwnd);

            long style = GetWindowLong(ieHandle, GWL_STYPE);

           // SetWindowLong(ieHandle, GWL_STYPE, (style| WS_CHILD));

           

        }
    }
}
