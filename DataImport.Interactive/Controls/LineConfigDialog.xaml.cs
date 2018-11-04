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

namespace DataImport.Interactive.Controls
{
    /// <summary>
    /// LineConfigDialog.xaml 的交互逻辑
    /// </summary>
    public partial class LineConfigDialog : UserControl
    {
        public LineConfigDialog()
        {
            InitializeComponent();
             
        }

        public Line currentLine { get; set; }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        public string ColumnName { get; set; }
        public string SourceName { get; set; }
          
    }
}
