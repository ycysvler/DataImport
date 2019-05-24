using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys;
using DataImport.Interactive.DataImportInteractive;
using DataImport.Interactive.DataScriptInteractive;
using DataImport.Interactive.Sequences;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// TaskInfoList.xaml 的交互逻辑
    /// </summary>
    public partial class TaskInfoList : UserControl
    {
        static string s_qProjectName = "";
        static string s_qTaskName = "";
        static int? s_cbGongbu;
        static int? s_cbStatus;
        static DateTime? s_qBegin;
        static DateTime? s_qEnd;
        static int? selectIndex;

        log4net.ILog log = log4net.LogManager.GetLogger("RollingLogFileAppender");

        public TaskInfoList()
        {
            InitializeComponent();
            this.Loaded += TaskInfoList_Loaded;

            this.popWindow.cancelEvent += popWindow_cancelEvent;
            this.popWindow.runEvent += popWindow_runEvent;
             

            taskTab.MenuSelectChangedEvent += taskTab_MenuSelectChangedEvent;
        }

        void popWindow_runEvent(object sender, EventArgs e)
        {
            TaskInfo info = currentInfo;
            if (string.IsNullOrEmpty(info.interfaceState))
            {
                MessageBox.Show("任务尚未接收，无法上报！");
                return;
            }


            if (info.interfaceState == "3")
            {
                MessageBox.Show("任务已上报，无法重复上报！");
                return;
            }

            List<string> scriptCodes = DataLogDAL.getScriptCodes(info.id);

            

            string result = WebHelper.verificationReportState(info.id);
            if (result.IndexOf("Y") > -1)
            {
                WebHelper.modifyActualDate(info.id, popWindow.begin.Text, popWindow.end.Text, popWindow.actualWorkHours.Text);

                //if (WebHelper.updateTdmTaskState(info.id, "3"))
                if (WebHelper.taskReportByClient(info.id))
                {
                    element.Text = "已上报";
                    info.interfaceState = "3";
                    //dataSource = WebHelper.listTdmTasks(MainWindow.UserName);
                    //dataGrid.DataContext = dataSource;
                    //taskGrid.TaskInfoList = dataSource;
                    //query_Click(null, null);
                }
                else
                {
                    MessageBox.Show("接收任务失败！");
                }
            }
            else
            {
                MessageBox.Show(result);
            
            }
        }

        void popWindow_cancelEvent(object sender, EventArgs e)
        {
            
        }

        
        void taskTab_MenuSelectChangedEvent(object sender, Controls.MenuSelectedEventArgs e)
        {
            showDetail();
        }

        void showDetail()
        {
            dgResources.Visibility = System.Windows.Visibility.Hidden;
            dgDelivers.Visibility = System.Windows.Visibility.Hidden;
            dgScripts.Visibility = System.Windows.Visibility.Hidden;
            dgUsers.Visibility = System.Windows.Visibility.Hidden;
            dgTimes.Visibility = System.Windows.Visibility.Hidden;
            gdTimes.Visibility = System.Windows.Visibility.Hidden;
            addTimes.Visibility = System.Windows.Visibility.Hidden;
            dgTechFiles.Visibility = System.Windows.Visibility.Hidden;
            dgArithmetics.Visibility = System.Windows.Visibility.Hidden;
             
            switch (taskTab.SelectedIndex)
            {
                case 0:
                    dgTimes.Columns[4].Visibility = System.Windows.Visibility.Collapsed;
                    dgTimes.Columns[5].Visibility = System.Windows.Visibility.Collapsed;
                    dgTimes.Columns[6].Visibility = System.Windows.Visibility.Collapsed;
                    dgTimes.Columns[7].Visibility = System.Windows.Visibility.Collapsed; 
                    
 
                    dgTimes.Visibility = System.Windows.Visibility.Visible;
                    if (currentInfo != null)
                    { 
                        // 已经导入过得日志名称
                        //var scriptNames = DataLogDAL.getScriptCodes(currentInfo.id);
                        var logs = DataLogDAL.getDistinctList(currentInfo.id);


                        if (currentInfo.resolvers.Count(it => it.scriptType == "0") > 0) dgTimes.Columns[4].Visibility = System.Windows.Visibility.Visible;
                        if (currentInfo.resolvers.Count(it => it.scriptType == "1") > 0) dgTimes.Columns[5].Visibility = System.Windows.Visibility.Visible;
                        if (currentInfo.resolvers.Count(it => it.scriptType == "2") > 0) dgTimes.Columns[6].Visibility = System.Windows.Visibility.Visible;
                        if (currentInfo.resolvers.Count(it => it.scriptType == "3") > 0) dgTimes.Columns[7].Visibility = System.Windows.Visibility.Visible;
                         
                        var dataSource = WebHelper.listTdmTaskTimesInfo(currentInfo.id);
                        foreach (var item in dataSource) {
                            item.scriptRunds = logs;
                            item.resolvers.AddRange( currentInfo.resolvers);
                        }
                        dgTimes.DataContext = dataSource;
                         
                        int max = 0;
                        try
                        {
                            max = dataSource.Max(it => it.TestTimeInt);
                        }
                        catch { }

                        txtTimes.Text = (max + 1).ToString();

                        if (currentInfo != null)
                        {
                            if (currentInfo.taskType == "工步")
                                addTimes.Visibility = System.Windows.Visibility.Visible;
                            else
                                addTimes.Visibility = System.Windows.Visibility.Hidden;
                        }
                        if (dataSource.Count > logs.Count)
                        {
                            // 说明有的试验任务，还没执行过
                            // 有人说时有时无，先去掉这个判断逻辑
                            //addTimes.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                    break;
                case 1:

                    dgResources.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 2: dgDelivers.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 3: dgUsers.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 4: dgScripts.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 5:
                    dgTechFiles.Visibility = System.Windows.Visibility.Visible;
                    break;
                    case 6:
                    dgArithmetics.Visibility = System.Windows.Visibility.Visible;
                    break;
                    
            }
        }

        List<TaskInfo> dataSource = new List<TaskInfo>(); 
        List<KV> gongbuSource = new List<KV>();

        void TaskInfoList_Loaded(object sender, RoutedEventArgs e)
        {
            taskGrid.UpEvent += taskGrid_UpEvent;
            taskGrid.StateEvent += taskGrid_StateEvent;
            taskGrid.SelectionEvent += taskGrid_SelectionEvent;
            // 隐藏执行脚本按钮
            if (System.Configuration.ConfigurationManager.AppSettings["simulator"] != "true") {
                scriptrun.Visibility = System.Windows.Visibility.Hidden;
            }

            cbStatus.SelectedIndex = s_cbStatus.HasValue ? s_cbStatus.Value: 1;

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                log.Info(string.Format("TaskInfoList > getList > user name:{0}", MainWindow.UserName));

                dataSource = TaskinfoDAL.getList(MainWindow.UserName);

                log.Info(string.Format("TaskInfoList > getList > data count:{0}", dataSource.Count));

                List<KV> gongbus = getGongBu(dataSource);
                cbGongbu.ItemsSource = gongbus;
                cbGongbu.DisplayMemberPath = "value";

                log.Info(string.Format("TaskInfoList > gongbu List > data count:{0}", gongbus.Count));

                cbGongbu.SelectedIndex = s_cbGongbu.HasValue? s_cbGongbu.Value : 0; 
                 
                dpBegin.SelectedDate = s_qBegin.HasValue ? s_qBegin : DateTime.Now.AddDays(-1);
                dpEnd.SelectedDate = s_qEnd.HasValue? s_qEnd: DateTime.Now.AddDays(1);

                if (!string.IsNullOrEmpty(s_qProjectName)) { qProjectName.Text = s_qProjectName; }
                if (!string.IsNullOrEmpty(s_qTaskName)) { qTaskName.Text = s_qTaskName; }

                query_Click(null, null); 
                showDetail();
            }
        }

        void taskGrid_SelectionEvent(object sender, EventArgs e)
        {
            TaskInfo ti = sender as TaskInfo; 
            if (ti!=null)
            {
                // 加载扩展信息
                TaskinfoDAL.loadEx(ti);

                currentInfo = ti;
                if (currentInfo != null)
                {
                    if (currentInfo.taskType == "工步")
                        addTimes.Visibility = System.Windows.Visibility.Visible;
                    else
                        addTimes.Visibility = System.Windows.Visibility.Hidden;

                    dgResources.DataContext = currentInfo.resources;
                    dgDelivers.DataContext = currentInfo.delivers;
                    dgUsers.DataContext = currentInfo.users;
                    dgScripts.DataContext = currentInfo.scripts;
                    dgTechFiles.DataContext = currentInfo.techFiles;
                    dgArithmetics.DataContext = currentInfo.arithmetics;
                    showDetail();
                }
            }
        }

        void taskGrid_StateEvent(object sender, EventArgs e)
        {
            State_MouseLeftButtonUp(sender, null);
        }

        void taskGrid_UpEvent(object sender, EventArgs e)
        {
            up_MouseLeftButtonUp(sender, null);
        }

        private List<KV> getGongBu(List<TaskInfo> source) {

            List<KV> result = new List<KV>();
            result.Add(new KV() { key = "", value = "全部" });

            foreach(var item in source) {
                log.Debug(string.Format("TaskInfoList > getGongBu > taskType:{0}", item.taskType));
            }

            foreach (var item in source.Where(it => it.taskType == "工序"))
            { 
                result.Add(new KV() { key = item.taskCode, value = "[" + item.taskCode +"]"+ item.taskName });
            }

            return result;//.OrderBy(it=>it.key).ToList(); 
        }

        private void url_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { 
            TextBlock img = (TextBlock)sender;
            DataImport.DataAccess.Entitys.TaskInfo.script info = img.Tag as DataImport.DataAccess.Entitys.TaskInfo.script;

            System.Diagnostics.Process.Start("iexplore.exe",
                info.url);
        }

        private void url2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock img = (TextBlock)sender;
            DataImport.DataAccess.Entitys.TaskInfo.techFile info = img.Tag as DataImport.DataAccess.Entitys.TaskInfo.techFile;

            System.Diagnostics.Process.Start("iexplore.exe",
                info.url);
        }
        private void url3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock img = (TextBlock)sender;
            DataImport.DataAccess.Entitys.TaskInfo.arithmetic info = img.Tag as DataImport.DataAccess.Entitys.TaskInfo.arithmetic;

            System.Diagnostics.Process.Start("iexplore.exe",
                info.url);
        } 
        private void delete_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { 
            TextBlock img = (TextBlock)sender;
            TasktimeInfo info = img.Tag as TasktimeInfo;

            if (WebHelper.updateTdmTaskTimesInfo(info, TdmTaskTimeMethod.delete))
            { 
                var dataSource = WebHelper.listTdmTaskTimesInfo(currentInfo.id);
                dgTimes.DataContext = dataSource;
            }
            else
            {
                MessageBox.Show("服务器异常，删除失败");
            }
        }

        private void deliverupload_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            TextBlock img = (TextBlock)sender;
            DataImport.DataAccess.Entitys.TaskInfo.deliver info = img.Tag as DataImport.DataAccess.Entitys.TaskInfo.deliver;

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
             

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (WebHelper.uploadFile(dialog.FileName, "", info.deliverId, "deliver"))
                {


                    FileInfo finfo = new FileInfo(dialog.FileName);
                    long length = finfo.Length / 1024;
                     
                    string serverpath = string.Format(@"{0}\groupTrailDate\{1}",
                       System.Configuration.ConfigurationManager.AppSettings["deliverpath"],
                        System.IO.Path.GetFileName(dialog.FileName));

                    
                    WebHelper.addAtachFileInfo2Tdm(info.deliverId, System.IO.Path.GetFileName(dialog.FileName), length.ToString(), serverpath);
                    WebHelper.modifyTdmDeliveryListState(info.deliverId, "1");

                    List<DataImport.DataAccess.Entitys.TaskInfo.deliver> delivers = dgDelivers.DataContext as List<DataImport.DataAccess.Entitys.TaskInfo.deliver>;
                    foreach (var deliver in delivers) {
                        if (deliver.deliverId == info.deliverId) {
                            deliver.deliverState = "已提交";
                            deliver.attachNames = System.IO.Path.GetFileName(dialog.FileName);
                        }
                    }
                    dgDelivers.DataContext = delivers.ToList();
                    
                }
            }
        }

        private void detail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock img = (TextBlock)sender;

            TaskInfo info = img.Tag as TaskInfo;
            currentInfo = info; 
            dgResources.DataContext = info.resources;
            dgDelivers.DataContext = info.delivers;
            dgUsers.DataContext = info.users;
            dgScripts.DataContext = info.scripts; 

            showDetail(); 
        }

        TaskInfo currentInfo;

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            imgClose.Visibility = System.Windows.Visibility.Hidden;
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            txtTimes.Text = "1";
            dpBegin.SelectedDate = DateTime.Now.AddDays(-1);
            dpEnd.SelectedDate = DateTime.Now.AddDays(1);
            gdTimes.Visibility = System.Windows.Visibility.Hidden;
            addTimes.Visibility = System.Windows.Visibility.Visible;
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            int temp = 0;
            if (!int.TryParse(txtTimes.Text.Trim(), out temp)) {
                MessageBox.Show("试验次数请输入整数！");
                return;
            }

            //currentInfo.t
            //DataLogDAL.getList(TaskCenter.TaskID).Count(it => Convert.ToInt32(it.Version) == TaskCenter.TaskTimes)

            TasktimeInfo info = new TasktimeInfo();
            info.Id = Guid.NewGuid().ToString().Replace("-", "");
            info.TaskId = currentInfo.id;
            info.TestTime = txtTimes.Text;
            info.Remark = txtDesc.Text;
            info.CreationDate = DateTime.Now.ToString("yyyy-MM-dd");
            info.LastUpdateDate = DateTime.Now.ToString("yyyy-MM-dd");
            info.LastUpdateIp = "127.0.0.1";
            info.Version = "1";
            info.TestPersion = txtPersion.Text;
            info.CreatedBy = MainWindow.UserID;
            info.LastUpdateBy = MainWindow.UserID;
            info.BeginDate = dpBegin.SelectedDate.Value.ToString("yyyy-MM-dd");
            info.EndDate = dpEnd.SelectedDate.Value.ToString("yyyy-MM-dd");


            if (WebHelper.updateTdmTaskTimesInfo(info, TdmTaskTimeMethod.add)) {
                showDetail();
                MessageBox.Show("添加试验次数成功");
            }
        }

        private void addTimes_Click(object sender, RoutedEventArgs e)
        {
            addTimes.Visibility = System.Windows.Visibility.Hidden;
            gdTimes.Visibility = System.Windows.Visibility.Visible;

            txtPersion.Text = MainWindow.UserName;
        }

        private bool downLoadScriptFile(TaskInfo info) {
            string basePath = AppDomain.CurrentDomain.BaseDirectory + @"scripts";
            string scriptPath = basePath + @"\" + info.id;
            
            if (!System.IO.Directory.Exists(basePath)) {
                Directory.CreateDirectory(basePath);
            }
            if (!System.IO.Directory.Exists(scriptPath))
            {
                Directory.CreateDirectory(scriptPath);
            }

            string scriptName = scriptPath + @"\Sequence.xml";

            if (File.Exists(scriptName)) {
                File.Delete(scriptName);
            }

            if (info.scripts.Count > 0) {
                WebHelper.DownFile(info.scripts[0].url, scriptName);
                return true;
            }
            return false;
        }

        private void State_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock element = (TextBlock)sender;

            TaskInfo info = element.Tag as TaskInfo;
            currentInfo = info;

            switch (info.interfaceState) { 
                case "1":
                    MessageBox.Show("任务已接收，无法重复接收！");
                    return;
                case "3": 
                    MessageBox.Show("任务已上报，无法重复接收！");
                    return;

            }


            if (WebHelper.updateTdmTaskState(info.id, MainWindow.UserName))
            {
                element.Text = "已接收";

                info.interfaceState = "1";

                //dataSource = WebHelper.listTdmTasks(MainWindow.UserName);
                //dataGrid.DataContext = dataSource; 
                //taskGrid.TaskInfoList = dataSource;
                //query_Click(null, null);
            }
            else
            {
                MessageBox.Show("接收任务失败！");
            }
        }

        TextBlock element;

        private void up_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            element = (TextBlock)sender;
             

            TaskInfo info = element.Tag as TaskInfo;
            currentInfo = info;
             
            this.popWindow.Visibility = System.Windows.Visibility.Visible;
            this.popWindow.actualWorkHours.Text = "0";
            this.popWindow.begin.Text = string.IsNullOrEmpty(info.actualSdate) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:00") : info.actualSdate;
            this.popWindow.end.Text = string.IsNullOrEmpty(info.actualEdate) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:00") : info.actualEdate;

            DateTime begin, end;
            if (DateTime.TryParse(this.popWindow.begin.Text, out begin) && DateTime.TryParse(this.popWindow.end.Text, out end)) {
                var span = (end - begin);
                this.popWindow.actualWorkHours.Text = (int)span.TotalHours + "小时" + span.Minutes + "分" + span.Seconds + "秒";
            }
        }
        


        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectIndex = dataGrid.SelectedIndex;

            if (e.AddedItems.Count > 0)
            {
                currentInfo = e.AddedItems[0] as TaskInfo;
                if (currentInfo != null)
                {
                    if (currentInfo.taskType == "工步")
                        addTimes.Visibility = System.Windows.Visibility.Visible;
                    else
                        addTimes.Visibility = System.Windows.Visibility.Hidden;

                    dgResources.DataContext = currentInfo.resources;
                    dgDelivers.DataContext = currentInfo.delivers;
                    dgUsers.DataContext = currentInfo.users;
                    dgScripts.DataContext = currentInfo.scripts;
                    dgTechFiles.DataContext = currentInfo.techFiles;
                    dgArithmetics.DataContext = currentInfo.arithmetics;
                    showDetail();
                }
            }
        }

        private void query_Click(object sender, RoutedEventArgs e)
        {
            List<TaskInfo> queryList = dataSource.ToList();

            if (cbGongbu.SelectedIndex == 0) {
                queryList = queryList.Where(it => it.taskType == "工序" || it.taskType == "工步").OrderBy(it => it.planCodeGantt).ToList(); 
            }
            if (cbGongbu.SelectedIndex > 0) {
                string taskcode = (cbGongbu.SelectedItem as KV).key;
                queryList = queryList.Where(it => it.taskCode.IndexOf(taskcode) == 0).OrderBy(it=>it.planCodeGantt).ToList();

                //queryList = queryList.Where(it => it.taskCode == (cbGongbu.SelectedItem as KV).key).ToList();
            }
            if (!string.IsNullOrEmpty(qProjectName.Text.Trim())) {
                queryList = queryList.Where(it => it.projectName.IndexOf(qProjectName.Text.Trim()) > -1).ToList();
            }
            if (!string.IsNullOrEmpty(qTaskName.Text.Trim()))
            {
                queryList = queryList.Where(it => it.taskName.IndexOf(qTaskName.Text.Trim()) > -1).ToList();
            }
            if (!string.IsNullOrEmpty(qRwName.Text.Trim()))
            {
                queryList = queryList.Where(it => it.parentName.IndexOf(qRwName.Text.Trim()) > -1).ToList();
            }

            
            if (qBegin.SelectedDate.HasValue) {
                queryList = queryList.Where(it => Convert.ToDateTime(it.planSdate) >= qBegin.SelectedDate.Value).ToList();
            }
            if (qEnd.SelectedDate.HasValue)
            {
                queryList = queryList.Where(it => Convert.ToDateTime(it.planSdate) <= qEnd.SelectedDate.Value).ToList();
            }
            switch (cbStatus.SelectedIndex) { 
                
                case 1:
                queryList = queryList.Where(it => string.IsNullOrEmpty( it.interfaceState)).ToList();break;
                case 2:
                queryList = queryList.Where(it => it.interfaceState == "1").ToList();break;
                case 3:
                queryList = queryList.Where(it => it.interfaceState == "3").ToList();break;
                 
            }


            s_qProjectName = qProjectName.Text;
            s_qTaskName = qTaskName.Text;
            s_cbGongbu = cbGongbu.SelectedIndex;
            s_cbStatus = cbStatus.SelectedIndex;
            s_qBegin = qBegin.SelectedDate;
            s_qEnd = qEnd.SelectedDate;

            int listLengh = queryList.Count;

            for (int i = listLengh-1; i >= 0; i--) {
                var info = queryList[i];
                if (info.taskType == "工步")
                {
                    if (!queryList.Exists(it => it.id == info.parentId)) {
                        var result = dataSource.ToList().FirstOrDefault(it => it.id == info.parentId);
                        if (result != null)
                            queryList.Add(result);
                    }
                }
            }
             
            var list = queryList.OrderBy(it => it.planCodeGantt).ToList();
            taskGrid.TaskInfoList = list;

            log.Info(string.Format("TaskInfoList > taskGrid > data count:{0}", list.Count));
        }
        private void clear_Click(object sender, RoutedEventArgs e)
        {
            List<TaskInfo> queryList = dataSource.ToList();
            qProjectName.Text = "";
            qTaskName.Text = "";
            qRwName.Text = "";
            qBegin.SelectedDate = null;
            qEnd.SelectedDate = null;
            cbStatus.SelectedIndex = 0;
               
            query_Click(null, null);
        }

        //private TaskInfo 

        private void resolver_MouseLeftButtonDown_0(object sender, MouseButtonEventArgs e)
        {
            TextBlock btn = sender as TextBlock; 
            TasktimeInfo ttimeInfo = btn.Tag as TasktimeInfo;
            TaskCenter.TaskTimes = ttimeInfo.TestTimeInt;
            resolver(btn.Text,ttimeInfo.resolver0code);
        }
        private void resolver_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            TextBlock btn = sender as TextBlock;
            TasktimeInfo ttimeInfo = btn.Tag as TasktimeInfo;
            TaskCenter.TaskTimes = ttimeInfo.TestTimeInt;
            resolver(btn.Text, ttimeInfo.resolver1code);
        }
        private void resolver_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            TextBlock btn = sender as TextBlock;
            TasktimeInfo ttimeInfo = btn.Tag as TasktimeInfo;
            TaskCenter.TaskTimes = ttimeInfo.TestTimeInt;
            resolver(btn.Text, ttimeInfo.resolver2code);
        }
        private void resolver_MouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {
            TextBlock btn = sender as TextBlock;
            TasktimeInfo ttimeInfo = btn.Tag as TasktimeInfo;
            TaskCenter.TaskTimes = ttimeInfo.TestTimeInt;
            resolver(btn.Text, ttimeInfo.resolver3code);
        }
        void resolver(string title, string scriptId) {

            bool retest = System.Configuration.ConfigurationManager.AppSettings["retest"] == "true";

            if (title == "已执行" && !retest)
            {
                MessageBox.Show("解析器已执行，无法重复执行！");
                return;
            }

            string taskid = currentInfo.id;

            DataImport.DataAccess.Entitys.TaskInfo.resolver ar = currentInfo.resolvers.FirstOrDefault(it => it.resolverId == scriptId);
            if (ar != null)
            {
                TaskCenter.TaskID = currentInfo.id;
                TaskCenter.CurrentInfo = currentInfo;
                TaskCenter.ScriptCode = ar.resolverCode;
                TaskCenter.ScriptID = ar.resolverId;

                MainWindow window = App.Current.MainWindow as MainWindow;

                UIElement item = new ImportFileSelecte();
                 
                ImportStack.clear();
                ImportStack.Push(this);

                window.StartPage(item);
            }
        
        }

        private void scriptrun_Click(object sender, RoutedEventArgs e)
        {
            var taskinfos = taskGrid.getTaskInfos(); // dataGrid.SelectedItems;

            Dictionary<string, string> files = new Dictionary<string, string>();

            if (taskinfos.Count == 0)
            {
                MessageBox.Show("您未选择任何工序！");
                return;
            }
               
            foreach (TaskInfo info in taskinfos) {
                string scriptPath = string.Format(@"{0}\{1}\Sequence.xml", AppDomain.CurrentDomain.BaseDirectory + @"scripts", info.id);
                if (downLoadScriptFile(info)) {
                    files.Add(info.taskName, scriptPath); 
                }
            }

            if (files.Count == taskinfos.Count)
            {

                MainWindow window = App.Current.MainWindow as MainWindow;

                UIElement item = new SequencesView(files);

                ImportStack.clear();
                ImportStack.Push(this);

                window.StartPage(item);
            }
            else {
                MessageBox.Show("您选择的工序没有配置脚本文件！");
            }
        }
    }

    public class KV {
        public string key { get; set; }
        public string value { get; set; }
    }
}
