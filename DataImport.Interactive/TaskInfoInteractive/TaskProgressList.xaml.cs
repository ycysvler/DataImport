using DataImport.BLL;
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
    /// TaskProgressList.xaml 的交互逻辑
    /// </summary>
    public partial class TaskProgressList : UserControl
    {
        public TaskProgressList()
        {
            InitializeComponent();

            this.Loaded += TaskInfoList_Loaded; 
        }

        List<TaskInfo> dataSource = new List<TaskInfo>();

        void TaskInfoList_Loaded(object sender, RoutedEventArgs e)
        {
            cbStatus.SelectedIndex = 0;
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                dataSource = WebHelper.listTdmTasks(MainWindow.UserName); 
                dataGrid.DataContext = dataSource;

                query_Click(null, null);
            }
        }

        private void query_Click(object sender, RoutedEventArgs e)
        {
            List<TaskInfo> queryList = dataSource.ToList();

            if (!string.IsNullOrEmpty(qProjectName.Text.Trim()))
            {
                queryList = queryList.Where(it => it.projectName.IndexOf(qProjectName.Text.Trim()) > -1).ToList();
            }
            if (!string.IsNullOrEmpty(qTaskName.Text.Trim()))
            {
                queryList = queryList.Where(it => it.taskName.IndexOf(qTaskName.Text.Trim()) > -1).ToList();
            }
            if (qBegin.SelectedDate.HasValue)
            {
                queryList = queryList.Where(it => Convert.ToDateTime(it.planSdate) >= qBegin.SelectedDate.Value).ToList();
            }
            if (qEnd.SelectedDate.HasValue)
            {
                queryList = queryList.Where(it => Convert.ToDateTime(it.planSdate) <= qEnd.SelectedDate.Value).ToList();
            }
            switch (cbStatus.SelectedIndex)
            {
                    
                case 0:
                    queryList = queryList.Where(it => it.interfaceState == "1"||it.interfaceState == "3").ToList(); break;
                case 1:
                    queryList = queryList.Where(it => it.interfaceState == "1").ToList(); break;
                case 2:
                    queryList = queryList.Where(it => it.interfaceState == "3").ToList(); break;

            }
            dataGrid.DataContext = queryList;
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            List<TaskInfo> queryList = dataSource.ToList();
            qProjectName.Text = "";
            qTaskName.Text = "";
            qBegin.SelectedDate = null;
            qEnd.SelectedDate = null;

            dataGrid.DataContext = queryList;
        }

        private void State_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock element = (TextBlock)sender;

            TaskInfo info = element.Tag as TaskInfo;
            

            if (WebHelper.updateTdmTaskState(info.id, "3", MainWindow.UserName))
            {
                element.Text = "已完成";
            }
            else
            {
                MessageBox.Show("完成任务失败！");
            }
        }
    }
}
