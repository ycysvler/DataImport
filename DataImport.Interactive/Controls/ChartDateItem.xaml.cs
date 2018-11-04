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
    /// ChartDateItem.xaml 的交互逻辑
    /// </summary>
    public partial class ChartDateItem : UserControl
    {
        public ChartDateItem()
        {
            InitializeComponent();
        }

        public string TargetValue { get { return tb_targetvalue.Text; } set { tb_targetvalue.Text = value; } }
        public string PreTime { get { return tb_pretime.Text; } set { tb_pretime.Text = value; } }
        //public string ChangeTime { get { return tb_changetime.Text; } set { tb_changetime.Text = value; } }
        public string HodeTime { get { return tb_hodetime.Text; } set { tb_hodetime.Text = value; } }
        public string Time { get { return tb_time.Text; } set { tb_time.Text = value; } }
    }
}
