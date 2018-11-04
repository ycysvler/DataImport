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
    /// TaskInfoGrid.xaml 的交互逻辑
    /// </summary>
    public partial class TaskInfoGrid : UserControl
    {
        public TaskInfoGrid()
        {
            InitializeComponent();
        }

        private static TaskInfo selected = null;

        public List<TaskInfo> getTaskInfos()
        {
            List<TaskInfo> result = new List<TaskInfo>();

            foreach (UIElement ui in root.Children)
            {
                TaskInfoGroup group = ui as TaskInfoGroup;
                if (group != null)
                {
                    if (group.cbSelect.IsChecked.Value)
                    {
                        result.Add(group.cbSelect.Tag as TaskInfo);
                    }
                    foreach (TaskInfoGroup item in group.root.Children)
                    {
                        if (item.cbSelect.IsChecked.Value)
                        {
                            result.Add(item.cbSelect.Tag as TaskInfo);
                        }
                    }
                }
            }
            return result;

        }

        public List<TaskInfo> TaskInfoList
        {
            set
            {
                root.Children.Clear();

                List<TaskInfo> gx = value.Where(it => it.taskType == "工序").ToList();

                foreach (var item in gx)
                {
                    TaskInfoGroup group = new TaskInfoGroup();
                    group.TaskInfo = item;
                    group.deptName.Text = item.deptName;
                    group.loginName.Text = item.loginName;
                    group.planEdate.Text = item.planEdate;
                    group.planSdate.Text = item.planSdate;
                    group.taskType.Text = item.taskType;
                    group.taskName.Text = item.taskName;
                    group.planName.Text = item.parentName;
                    group.taskCode.Text = item.taskCode;
                    group.projectName.Text = item.projectName;
                    group.projectCode.Text = item.projectCode;
                    group.taskType.Text = item.taskType;
                    group.interfaceStateText.Text = item.interfaceStateText;
                    group.up.Tag = item;
                    if (item.interfaceState == "3")
                    {
                        //"已上报"
                        group.up.Visibility = System.Windows.Visibility.Hidden;
                        group.cbSelect.Visibility = System.Windows.Visibility.Hidden;
                    }
                    group.up.MouseLeftButtonDown += up_MouseLeftButtonDown;
                    group.interfaceStateText.Tag = item;
                    group.interfaceStateText.MouseLeftButtonDown += interfaceStateText_MouseLeftButtonDown;
                    group.cbSelect.Tag = item;
                    group.cbSelect.Visibility = System.Windows.Visibility.Collapsed;
                    group.cbSelect.Click += cbSelect_Click;
                    group.SelectedEvent += group_SelectedEvent;

                    root.Children.Add(group);
                    Rectangle r = new Rectangle();
                    r.Height = 1;
                    r.Fill = new SolidColorBrush(Colors.Black);
                    root.Children.Add(r);

                    foreach (var child in value.Where(it => it.parentId == item.id))
                    {
                        TaskInfoGroup tg = new TaskInfoGroup();
                        tg.TaskInfo = child;
                        tg.deptName.Text = child.deptName;
                        tg.taskType.Text = child.taskType;
                        tg.loginName.Text = child.loginName;
                        tg.planEdate.Text = child.planEdate;
                        tg.planSdate.Text = child.planSdate;
                        tg.taskName.Text = child.taskName;
                        tg.planName.Text = child.parentName;
                        tg.taskCode.Text = child.taskCode;
                        tg.projectName.Text = child.projectName;
                        tg.projectCode.Text = child.projectCode;
                        tg.taskType.Text = child.taskType;
                        tg.interfaceStateText.Text = child.interfaceStateText;
                        if (child.interfaceState == "3") {
                            //"已上报"
                            tg.up.Visibility = System.Windows.Visibility.Hidden;
                            tg.cbSelect.Visibility = System.Windows.Visibility.Hidden;
                        }
                        tg.up.Tag = child;
                        tg.up.MouseLeftButtonDown += up_MouseLeftButtonDown;
                        tg.interfaceStateText.Tag = child;
                        tg.interfaceStateText.MouseLeftButtonDown += interfaceStateText_MouseLeftButtonDown;
                        tg.cbSelect.Tag = child;
                        tg.cbSelect.Click += cbSelect_Click;
                        tg.SelectedEvent += group_SelectedEvent;
                        tg.cbZd.Visibility = System.Windows.Visibility.Collapsed;
                        group.root.Children.Add(tg);

                        if (selected != null && selected.taskCode == child.taskCode) {
                             
                            group_SelectedEvent(tg, null);
                        }
                    }
                }
            }
        }

        void group_SelectedEvent(object sender, EventArgs e)
        {
            TaskInfoGroup tg = sender as TaskInfoGroup;


            System.Windows.Media.Color color = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#01333333");
            SolidColorBrush brush = new SolidColorBrush(color);

            if (SelectionEvent != null) {
                SelectionEvent(tg.TaskInfo, null);
                selected = tg.TaskInfo;
            }
             
            foreach (UIElement ui in root.Children)
            {
                TaskInfoGroup group = ui as TaskInfoGroup;
                if (group != null)
                {
                    group.grid.Background = brush; 
                    foreach (TaskInfoGroup item in group.root.Children)
                    {
                        item.grid.Background = brush; 
                    }
                }
            }
            tg.grid.Background = new SolidColorBrush(Colors.Blue);
            
        }

        void cbSelect_Click(object sender, RoutedEventArgs e)
        {
            
        }

        void interfaceStateText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (StateEvent != null)
            {
                StateEvent(sender, null);
            }
        }

        void up_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (UpEvent != null)
            {
                UpEvent(sender, null);
            }
        }

        public event EventHandler UpEvent;
        public event EventHandler StateEvent;
        public event EventHandler SelectionEvent;

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                scroll.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            catch (System.Exception ex) { }

           
        }
    }
}
