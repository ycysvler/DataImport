using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataImport.Interactive.TaskInfoInteractive
{
    /// <summary>
    /// TaskInfoGroup.xaml 的交互逻辑
    /// </summary>
    public partial class TaskInfoGroup : UserControl
    {
        public TaskInfoGroup()
        {
            InitializeComponent();
        }

        private void cbZd_Click(object sender, RoutedEventArgs e)
        {
            if (!cbZd.IsChecked.Value)
                root.Visibility = System.Windows.Visibility.Collapsed;
            else
                root.Visibility = System.Windows.Visibility.Visible;
        }

        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedEvent != null) {
                SelectedEvent(this, null);
            }
        }

        public TaskInfo TaskInfo { get; set; }

        public event EventHandler SelectedEvent;
    }
}
