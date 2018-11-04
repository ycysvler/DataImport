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
    /// InputDataItem.xaml 的交互逻辑
    /// </summary>
    public partial class InputDataItem : UserControl
    {
        public InputDataItem()
        {
            InitializeComponent();

            ColumnType = "string";
        }

        /// <summary>
        /// string, n10, n16
        /// </summary>
        public string ColumnType
        {
            get;
            set;
        }

        public string V1
        {
            set
            {
                v1.Text = value;
                changeType(value);
            }
        }
        public string V2
        {
            set
            {
                v2.Text = value;
                changeType(value);
            }
        }
        public string V3
        {
            set
            {
                v3.Text = value; 
                changeType(value);
            }
        }

        private void changeType(string value)
        {
            if (ColumnType == "string")
            {
                float temp = 0;
                if (float.TryParse(value, out temp))
                {
                    ColumnType = "n10";
                    r_10.IsChecked = true;
                }
            }
        }
    }
}
