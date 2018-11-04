using DataImport.Interactive.Controls;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace DataImport.Interactive.DataImportInteractive
{
    /// <summary>
    /// ImportDataGrid.xaml 的交互逻辑
    /// </summary>
    public partial class ImportDataGrid : UserControl
    {
        public ImportDataGrid()
        {
            InitializeComponent();
             
        }

        public DataTable ItemsSource {
            set {

                MainWindow.HexColumns.Clear();

                root.Children.Clear();

                int rowcount = 3;

                rowcount = value.Rows.Count < rowcount ? value.Rows.Count : rowcount;

                foreach (DataColumn column in value.Columns) {
                    InputDataItem item = new InputDataItem();
                    item.title.Text = column.ColumnName;
                    item.Tag = column.ColumnName;

                    if (rowcount > 0) {
                        item.V1 = value.Rows[0][column].ToString();
                    }
                    if (rowcount > 1)
                    {
                        item.V2 = value.Rows[1][column].ToString();
                    }
                    if (rowcount > 2)
                    {
                        item.V3 = value.Rows[2][column].ToString();
                    }

                    root.Children.Add(item);
                }
            
            }
        }

        public void getHexColumn() {

            foreach (InputDataItem item in root.Children) {
                if (item.r_16.IsChecked.HasValue && item.r_16.IsChecked.Value) {
                    MainWindow.HexColumns[item.Tag.ToString()] = null;
                }
            }
        }
    }
}
