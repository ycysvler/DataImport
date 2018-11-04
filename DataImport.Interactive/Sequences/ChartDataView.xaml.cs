using DataImport.Interactive.Controls;
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

namespace DataImport.Interactive.Sequences
{
    /// <summary>
    /// ChartDataView.xaml 的交互逻辑
    /// </summary>
    public partial class ChartDataView : UserControl
    {
        public ChartDataView()
        {
            InitializeComponent();
        }

        List<Source> _sources = new List<Source>();
         

        public List<Source> Sources
        {
            get { return _sources; }
            set
            {
                _sources = value;

                root.Children.Clear();

                foreach (var source in value) {
                    RealDataGroup group = new RealDataGroup();
                    root.Children.Add(group);
                    group.GroupName = string.Format("资源：{0}({1})",source.ResID, source.Name);
                    group.MouseLeftButtonDown += group_MouseLeftButtonDown;
                    group.Cursor = Cursors.Hand;

                    StackPanel sp = new StackPanel();
                    group.Tag = sp;
                    root.Children.Add(sp);

                    foreach (var step in source.Steps.OrderBy(it => it.Time).ToList()) {
                        ChartDateItem item = new ChartDateItem();
                        sp.Children.Add(item);
                        item.TargetValue = step.TargetValue.ToString();
                        item.PreTime = step.PreTime.ToString();
                        //item.ChangeTime = step.ChangeTime.ToString();
                        item.HodeTime = step.HodeTime.ToString();
                        item.Time = step.Time.ToString();
                    }
                }
            }

        }

        void group_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RealDataGroup group = sender as RealDataGroup;
            StackPanel sp = group.Tag as StackPanel;

            sp.Visibility = sp.Visibility == System.Windows.Visibility.Visible ? System.Windows.Visibility.Collapsed:System.Windows.Visibility.Visible; 
             
        }
    }
}
