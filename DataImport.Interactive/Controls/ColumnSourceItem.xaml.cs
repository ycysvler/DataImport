using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
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

namespace DataImport.Interactive.Controls
{
    /// <summary>
    /// ColumnSourceItem.xaml 的交互逻辑
    /// </summary>
    public partial class ColumnSourceItem : UserControl
    {
        public ColumnSourceItem()
        {
            InitializeComponent();

            this.Loaded += ColumnSourceItem_Loaded;
             
        }

        public int Index { get; set; }

        void cbTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        void ColumnSourceItem_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

         
         
        public string ColumnName {
            get { return columnName.Text; }
            set { columnName.Text = value; columnName.ToolTip = value; }
        }

        public string ColumnValue {
            get { return columnValue.Text; }
            set { columnValue.Text = value; columnValue.ToolTip = value; }
        }
    }
}
