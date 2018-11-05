using DataImport.BLL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using log4net;

namespace DataImport.Interactive
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            log4net.Config.XmlConfigurator.Configure();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Error encountered! Please contact support." + Environment.NewLine + e.Exception.Message);

            LogHelper.WriteLog(e.Exception.ToString());

            MessageBox.Show("Error encountered! Please contact support." + Environment.NewLine + e.Exception.Message);
           
            e.Handled = true;
        }
    }
}
