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
    /// ColumnTargetItem.xaml 的交互逻辑
    /// </summary>
    public partial class ColumnTargetItem : UserControl
    {
        public ColumnTargetItem()
        {
            InitializeComponent();

             
        }
        public string ColumnName
        {
            get { return columnName.Text;  }
            set { columnName.Text = value; root.Tag = value; }
        }

        public string ColumnValue
        {
            get { return columnValue.Text; }
            set { columnValue.Text = value; }
        }

        public bool NullAble { set {
            if (!value)
            {
                cb.IsChecked = true;
            }
        } }

        public string DataType { get { return columnType.Text; } set { columnType.Text = value; } }

        public int Index { get; set; }

        
    }
}
