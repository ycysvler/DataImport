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
    /// TargetSelectDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TargetSelectDialog : UserControl
    {
        public TargetSelectDialog()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            this.Loaded += TargetSelectDialog_Loaded;
        }

        void TargetSelectDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if(root.Children.Count == 0)
            foreach (var item in Target) {
                CheckBox cb = new CheckBox();
                cb.Foreground = new SolidColorBrush(Colors.White);
                cb.Margin = new Thickness(8);
                cb.Content = item.ColumnName;
                cb.Tag = item.ColumnName;
                root.Children.Add(cb);
                cbList.Add(cb);
            }
        }

        List<CheckBox> cbList = new List<CheckBox>();

        public void show() {
            this.Visibility = System.Windows.Visibility.Visible;

            foreach (string item in Map.Keys) {
                CheckBox cb = cbList.FirstOrDefault(it => it.Tag.ToString() == item);
                if (cb != null)
                    cb.IsChecked = true;
                else
                {
                    cb.IsChecked = false;
                }
            }
        }

        public List<Structure> Target { get; set; }
        public Dictionary<string, string> Map { get; set; }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
