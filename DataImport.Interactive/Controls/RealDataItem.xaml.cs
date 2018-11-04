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
    /// RealDataItem.xaml 的交互逻辑
    /// </summary>
    public partial class RealDataItem : UserControl
    {
        public RealDataItem()
        {
            InitializeComponent();
        }

        public string ResID { get { return tb_resid.Text; } set { tb_resid.Text = value; } }
        public string ResName{ get { return tb_name.Text; } set { tb_name.Text = value; } }
        public string ResValue { get { return tb_value.Text; } set { tb_value.Text = value; } }
        public string ResTime { get { return tb_time.Text; } set { tb_time.Text = value; } }


       
        
    }
}
