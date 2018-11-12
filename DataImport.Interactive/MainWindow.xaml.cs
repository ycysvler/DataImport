using DataImport.BLL;
using DataImport.BLL.TdmToZkWebservice;
using DataImport.DataAccess;
using DataImport.Interactive.BatchInteractive;
using DataImport.Interactive.DataImportInteractive;
using DataImport.Interactive.DataScriptInteractive;
using DataImport.Interactive.Sequences;
using DataImport.Interactive.TaskInfoInteractive;
using Steema.TeeChart.WPF.Styles;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
using System.Xml.Linq;

namespace DataImport.Interactive
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 0：正常执行；1：暂停；-1：强制退出
        /// </summary>
        private static int sequenceIsRunning = -1;
        private static object mainlock = new object();

        public static int SequenceIsRunning
        {
            get { return MainWindow.sequenceIsRunning; }
            set
            {
                lock (mainlock)
                {
                    MainWindow.sequenceIsRunning = value;
                }
            }
        }



        public MainWindow()
        {
            InitializeComponent();

            // 这里牛逼了，遇到一个XP下中文乱码问题
            // 通过写死环境变量的方式，让客户端与服务器端用指定字符集通讯
            string NLS_LANG = System.Configuration.ConfigurationManager.AppSettings["NLS_LANG"];

            if (!string.IsNullOrEmpty(NLS_LANG)) {
                Environment.SetEnvironmentVariable("NLS_LANG", NLS_LANG, EnvironmentVariableTarget.Process);
            }
              
            Process[] pro = Process.GetProcesses();
            int n = pro.Where(p => p.ProcessName.Equals("DataImport.Interactive")).Count();
            if (n > 1)
            {
                MessageBox.Show("程序已启动!");
                Application.Current.Shutdown();
                return;
            }

            this.Loaded += MainWindow_Loaded;
        }

        public SequencesView SequencesView { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        { 
            if (ConfigurationManager.AppSettings["simulatormode"] == "true")
            {
                loginBorder.Visibility = System.Windows.Visibility.Hidden;

                MainWindow window = App.Current.MainWindow as MainWindow;

                UIElement item = new SequencesView();

                ImportStack.clear();
                ImportStack.Push(this);

                window.StartPage(item);
            }

        }
        private void Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SequencesView != null)
            { 
                if (MainWindow.SequenceIsRunning == 0) {
                    MessageBox.Show("模拟程序正在执行，不能关闭系统！");
                    return;
                }
                else
                {
                    SequencesView.Stop();
                }
            }


            this.Close();
            this.Visibility = System.Windows.Visibility.Hidden;
            this.ShowInTaskbar = false;
        }
        private void Max_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = this.WindowState == System.Windows.WindowState.Normal ? System.Windows.WindowState.Maximized : System.Windows.WindowState.Normal;
        }
        private void Min_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }


        private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (System.Exception ex) { }
        }

        private void drop_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(sender as UIElement);
        }

        public void StartPage(UIElement element)
        {

            this.content.Child = element;
        }

        private void FuncMenus_MenuSelectChangedEvent(object sender, Controls.MenuSelectedEventArgs e)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;

            switch (e.SelectedIndex)
            {
                case 0: window.StartPage(new TaskInfoList());
                    break;
                case 1: window.StartPage(new DataScriptList());
                    break;
                case 2: window.StartPage(new ProjectList());
                    break;
                case 3: window.StartPage(new TaskProgressList());
                    break;
                case 4:
                    window.StartPage(new MdbAdapter.MdbAdapterPage());
                    break;
                case 5:
                    window.StartPage(new BetchImport());
                    break;

            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {

            if (ConfigurationManager.AppSettings["loginConfig"] == "0")
            {
                try
                {
                    string result = WebHelper.Login(loginUser.Text, loginPassword.Password);

                    if (!string.IsNullOrEmpty(result))
                    {
                        content.Child = new TaskInfoList();
                        funcMenus.Visibility = System.Windows.Visibility.Visible;
                        loginBorder.Visibility = System.Windows.Visibility.Hidden;

                        UserID = result;
                        UserName = loginUser.Text.Trim();
                    }
                    else
                    {
                        MessageBox.Show("用户名密码不匹配！");
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                content.Child = new DataScriptList();
                funcMenus.Visibility = System.Windows.Visibility.Visible;
                loginBorder.Visibility = System.Windows.Visibility.Hidden;

                UserID = "4028b48152632a160152635092f7000e";
                MainWindow.UserName = "shaozj";

            }

        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 登录成功后，把用户信息保存到这里
        public static string UserID { get; set; }
        public static string UserName { get; set; }

        public static IDictionary<string, string> HexColumns = new Dictionary<string, string>();

        private void loginPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                loginButton_Click(null, null);
            }
        }
    }
}


