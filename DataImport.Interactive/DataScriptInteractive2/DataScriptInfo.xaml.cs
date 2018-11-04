using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys;
using DataImport.Interactive.DataScriptInteractive;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

namespace DataImport.Interactive.DataScriptInteractive2
{
    /// <summary>
    /// DataScriptInfo.xaml 的交互逻辑
    /// </summary>
    public partial class DataScriptInfo : UserControl
    {
        public DataScriptInfo()
        {
            InitializeComponent();
            this.Loaded += DataScriptInfo_Loaded;

            ApplyTestProject.SelectedIndex = 0;
        }

        public string FID { get; set; }

        private List<TableInfo> tables = new List<TableInfo>();
        private List<DataScript> scripts = new List<DataScript>();

        void DataScriptInfo_Loaded(object sender, RoutedEventArgs e)
        {
            string[] tablelist = System.Configuration.ConfigurationManager.AppSettings["tables"].Split(new char[] { ',' });
            string[] descs = System.Configuration.ConfigurationManager.AppSettings["tabledescs"].Split(new char[] { ',' });
            // 所有Script的列表，用于计算版本号
            scripts = DataScriptDAL.getList();
            var objTableList = ObjtableInfoDAL.getList();
            foreach (var objTable in objTableList)
            {
                tables.Add(new TableInfo()
                {
                    TableDesc = string.Format("{0}({1})", objTable.ObjectTableName, objTable.ObjectTableCode),
                    TableName = objTable.ObjectTableCode
                });
            }

            var projectList = WebHelper.listProject(MainWindow.UserName);
            ProjectCode.ItemsSource = projectList;
            ProjectCode.DisplayMemberPath = "ProjectName";
            ProjectCode.SelectedValuePath = "ProjectCode";
            if (projectList.Count > 0)
                ProjectCode.SelectedIndex = 0;
            ScriptType.SelectedIndex = 0;

            if (string.IsNullOrEmpty(FID))
            {
                // new row
                MidsScriptCode.Text = "";
                getScriptCode();
            }
            else
            {
                nextButton.IsEnabled = false;
                nextButton.Visibility = System.Windows.Visibility.Collapsed;
                // edit row
                showData();

            }
        }

        DataScript dScript;
        DataScriptRule dRule;

        private void getScriptCode()
        {

            if (TaskNameList.SelectedIndex == 0)
            {
                MidsScriptName.Text = "";

                var list = DataScriptDAL.getList();
                var projectCode = ProjectCode.SelectedValue.ToString();

                for (int i = 0; i < 100; i++)
                {

                    string scriptcode = string.Format("{0}{1}{2}", projectCode, DateTime.Now.Year, i.ToString("000"));

                    if (list.FirstOrDefault(it => it.MidsScriptCode == scriptcode) == null)
                    {
                        MidsScriptCode.Text = scriptcode;
                        break;
                    }
                }

                getScriptName();
            }
        }

        private void getScriptName()
        {
            if (ProjectCode.SelectedItem != null)
                MidsScriptName.Text = string.Format("{0}{1}{2}", ProjectCode.Text, TaskName.Text, ScriptType.SelectedItem.ToString());
        }

        private bool checkCode()
        {
            return DataScriptDAL.CheckCode(MidsScriptCode.Text.Trim()) == 0;
        }

        private void showData()
        {
            dScript = DataScriptDAL.getInfo(FID);
            dRule = DataScriptRuleDAL.getInfo(FID);

            ProjectCode.SelectedValue = dScript.ProjectCode;
            TaskName.Text = dScript.TaskName;
            txtTableName.Text = dScript.TableName;
            MidsScriptCode.Text = dScript.MidsScriptCode;
            MidsScriptName.Text = dScript.MidsScriptName;
            MidsScriptVesion.Text = dScript.MidsScriptVesion;
            FileType.SelectedValue = dScript.FileType;
            IndexKey.Text = dScript.IndexKey;
            ValidFlag.Text = dScript.ValidFlag;
            Remark.Text = dScript.Remark;
            ApplyTestProject.SelectedValue = dScript.ApplyTestProject;
            ScriptType.SelectedIndex = dScript.ScriptType;

            SetColSperator(dRule.ColSperator);
            ColName_Lines.Text = dRule.ColnameLines.ToString();

            if (tables.SingleOrDefault(it => it.TableName == dRule.DesTable) == null)
            {
                tables.Add(new TableInfo() { TableDesc = dRule.DesTable, TableName = dRule.DesTable });
            }
            ProjectCode.SelectedValue = dScript.ProjectCode;

            txtTemplageFile.Text = dRule.DesFile;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            string erromsg = "";

            if (string.IsNullOrEmpty(MidsScriptCode.Text.Trim()))
            {
                erromsg += "请输入解析器编码！\r\n";
            }


            if (string.IsNullOrEmpty(TaskName.Text.Trim()))
            {
                erromsg += "请输入实验名称！\r\n";
            }
            if (string.IsNullOrEmpty(MidsScriptName.Text.Trim()))
            {
                erromsg += "请输入解析器名称！\r\n";
            }
            if (string.IsNullOrEmpty(txtTemplageFile.Text.Trim()))
            {
                erromsg += "请选择样例文件！\r\n";
            }
            else if (FileType.SelectedValue.ToString() != "xls/xlsx" && FileType.SelectedValue.ToString() != "mdb")
            {
                string[] columns = TextImportHelper.GetColumns(txtTemplageFile.Text, DataScriptRule.getColSeperatorChar(GetColSperator()));
                if (columns.Length < 2)
                {
                    erromsg += "数据文件列分隔符不存在，请选择相应列分隔符！\r\n";
                }
            } 

            dScript = new DataScript();


            dScript.MidsScriptCode = MidsScriptCode.Text.Trim();
            dScript.MidsScriptName = MidsScriptName.Text.Trim();
            dScript.MidsScriptVesion = MidsScriptVesion.Text.Trim();
            dScript.FileType = FileType.SelectedValue.ToString();
            dScript.IndexKey = IndexKey.Text.Trim();
            dScript.ValidFlag = ValidFlag.Text.Trim();
            dScript.ApplyTestProject = ApplyTestProject.SelectedValue.ToString();
            dScript.CreationDate = DateTime.Now;
            dScript.LastUpdateDate = DateTime.Now;
            dScript.LastUpdateIp = "127.0.0.1";
            dScript.Version = 1;
            dScript.LastUpdatedBy = MainWindow.UserID;
            dScript.CreatedBy = MainWindow.UserID;
            dScript.ScriptType = ScriptType.SelectedIndex;
            dScript.TableName = txtTableName.Text;
            dScript.ProjectCode = ProjectCode.SelectedValue.ToString();
            dScript.TaskName = TaskName.Text.Trim();

            dRule = new DataScriptRule();

            dRule.ColSperator = GetColSperator();
            dRule.ColnameLines = int.Parse(ColName_Lines.Text.Trim());
            dRule.DesTable = txtTableName.Text;
            dRule.DesFile = txtTemplageFile.Text;
            dRule.CreationDate = DateTime.Now;
            dRule.LastUpdateDate = DateTime.Now;
            dRule.LastUpdateIp = "127.0.0.1";
            dRule.Version = 1;
            dRule.DesBusinessPk = "";
            dRule.CreatedBy = MainWindow.UserID;
            dRule.LastUpdatedBy = MainWindow.UserID;

            // 我发现，现在就没有更新逻辑
            //if (string.IsNullOrEmpty(FID))
            //{
            //    var oldScript = scripts.FirstOrDefault(it => it.ScriptType == dScript.ScriptType && it.ProjectCode == dScript.ProjectCode && it.TaskName == dScript.TaskName);
            //    if (oldScript != null)
            //    {
            //        FID = oldScript.FID;
            //        dScript.FID = FID; 

            //    }
            //}

            if (!string.IsNullOrEmpty(erromsg))
            {
                MessageBox.Show(erromsg);
                return;
            }

            var oldScript = scripts.FirstOrDefault(it => it.ScriptType == dScript.ScriptType && it.ProjectCode == dScript.ProjectCode && it.TaskName == dScript.TaskName);
            if (oldScript != null)
            {
                dScript.FID = Guid.NewGuid().ToString().Replace("-", "");
                FID = dScript.FID;
                dRule.FID = dScript.FID;
                dRule.MdsImpDataScriptID = dScript.FID;

                insertScript();
                updateTableStructure();
            }
            else
            {
                dScript.FID = Guid.NewGuid().ToString().Replace("-", "");
                FID = dScript.FID;
                dRule.FID = dScript.FID;
                dRule.MdsImpDataScriptID = dScript.FID;

                insertScript();
                insertTableStructure();
            }


            DataScriptMapModify mapModify = new DataScriptMapModify();
            mapModify.FID = FID;
            mapModify.DataScriptRule = dRule;
            mapModify.DataScript = dScript;
            MainWindow window = App.Current.MainWindow as MainWindow;

            ImportStack.Push(this);

            window.StartPage(mapModify);

        }

        private int getMaxColumnIndex(List<Structure> lse)
        {
            int max = 1;
            foreach (var s in lse)
            {
                string columnName = s.ColumnName;
                if (columnName.IndexOf("COLUMN") > -1)
                {
                    int index = 0;
                    int.TryParse(columnName.Remove(0, 6), out index);
                    if (max < index)
                        max = index;
                }
            }
            return max;
        }

        private void updateScript()
        {
            DataScriptDAL.update(dScript);
            DataScriptRuleDAL.update(dRule);
        }

        private void updateTableStructure()
        {
            DataTable dt = GetDataTable(txtTemplageFile.Text);
            // 增加对应关系

            var structures = TableDAL.getTableStructure(dScript.TableName);

            int max = getMaxColumnIndex(structures);

            for (int i = 0; i < dt.Columns.Count; i++)
            {

                DataColumn column = dt.Columns[i];
                if (structures.FirstOrDefault(it => it.Comments == column.ColumnName) == null)
                {
                    max++;
                    double d = 0.0;
                    if (double.TryParse(dt.Rows[0][column].ToString(), out d))
                    {
                        TableDAL.AddColumn(dScript.TableName, "COLUMN" + max.ToString(), "number");
                    }
                    else
                    {
                        TableDAL.AddColumn(dScript.TableName, "COLUMN" + max.ToString(), "string");
                    }
                    TableDAL.Comment(dScript.TableName, "COLUMN" + max.ToString(), column.ColumnName);
                }
            }

            structures = TableDAL.getTableStructure(dScript.TableName);

            for (int i = 0; i < dt.Columns.Count; i++)
            {

                DataColumn column = dt.Columns[i];

                var structure = structures.FirstOrDefault(it => it.Comments == column.ColumnName);
                if (structure != null)
                {
                    DataScriptMap map = new DataScriptMap();
                    map.FID = Guid.NewGuid().ToString().Replace("-", "");
                    map.MdsImpDataScriptRuleID = dScript.FID;
                    map.TableColName = structure.ColumnName;
                    map.FileColName = structure.Comments;
                    map.TransferType = "02";
                    map.CreatedBy = MainWindow.UserID;
                    map.LastUpdatedBy = MainWindow.UserID;
                    map.LastUpdateIp = "127.0.0.1";

                    DataScriptMapDAL.Insert(map);
                }

            }
        }

        private void insertScript()
        {
            DataScriptDAL.Insert(dScript);
            DataScriptRuleDAL.Insert(dRule);
        }

        private void insertTableStructure()
        {
            DataTable dt = GetDataTable(txtTemplageFile.Text);

            if (scripts.Count(it => it.TableName == txtTableName.Text) == 0)
            {
                // 创建表
                TableDAL.CreateTable(txtTableName.Text, dt);
                // 添加主键
                //TableDAL.SetPrimary(txtTableName.Text, "ID");
                // 添加扩展列
                TableDAL.AddAttribute(txtTableName.Text, 20);

                int count = ObjtableInfoDAL.Count(txtTableName.Text.ToUpper());
                if (count == 0)
                {
                    ObjtableInfo oinfo = new ObjtableInfo();
                    oinfo.FID = Guid.NewGuid().ToString().Replace("-", "");
                    oinfo.CreatedBy = MainWindow.UserID;
                    oinfo.LastUpdatedBy = MainWindow.UserID;
                    oinfo.ObjectTableCode = txtTableName.Text.ToUpper();
                    oinfo.ObjectTableName = txtTableName.Text.ToUpper();
                    oinfo.Status = "02";
                    oinfo.Version = 1;
                    oinfo.LastUpdateIp = "127.0.0.1";
                    oinfo.LastUpdateDate = DateTime.Now.ToString();
                    oinfo.CreationDate = DateTime.Now.ToString();
                    ObjtableInfoDAL.Insert(oinfo);
                }
            }
            else
            {
                // 追加表结构
                TableDAL.CreateTable(txtTableName.Text, dt);
            }

            DataScriptMapDAL.AutoScriptMap(FID, dt, txtTableName.Text, MainWindow.UserID);
        }

        private string GetColSperator()
        {
            if (rbComma.IsChecked.Value)
                return "comma";
            if (rbSemicolon.IsChecked.Value)
                return "semicolon";
            if (rbTab.IsChecked.Value)
                return "tab";
            if (rbSpace.IsChecked.Value)
                return "space";

            return "";
        }

        private void SetColSperator(string colSperator)
        {
            switch (colSperator)
            {
                case "comma": rbComma.IsChecked = true; break;
                case "semicolon": rbSemicolon.IsChecked = true; break;
                case "tab": rbTab.IsChecked = true; break;
                case "space": rbSpace.IsChecked = true; break;
            }
        }

        private DataTable GetDataTable(string fileName)
        {
            DataTable dt;
            if (FileType.SelectedValue.ToString() == "mdb") {
                AccessImportHelper helper = new AccessImportHelper(fileName);
                dt = helper.getDataTable();
            }
            else if (FileType.SelectedValue.ToString() == "xls/xlsx")
            {
                dt = ExcelImportHelper.GetDataTable(fileName);
            }
            else
            {
                dt = TextImportHelper.GetDataTable(fileName, DataScriptRule.getColSeperatorChar(GetColSperator()));
            }
            return dt;
        }

        private void openTemplateFile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            if (FileType.SelectedValue.ToString() == "xls/xlsx")
            {
                dialog.Filter = "(Excel 文件)|*.xls;*.xlsx";
            }
            if (FileType.SelectedValue.ToString() == "txt")
            {
                dialog.Filter = "(文本文件)|*.txt";
            }
            if (FileType.SelectedValue.ToString() == "dat")
            {
                dialog.Filter = "(数据文件)|*.dat";
            }
            if (FileType.SelectedValue.ToString() == "mdb")
            {
                dialog.Filter = "(数据文件)|*.mdb";
            }
            if (FileType.SelectedValue.ToString() == "fws10")
            {
                dialog.Filter = "(fws10文件)|*.fws10";
            }

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtTemplageFile.Text = dialog.FileName;

                if (FileType.SelectedValue.ToString() != "xls/xlsx" && FileType.SelectedValue.ToString() != "mdb")
                {
                    string[] columns = TextImportHelper.GetColumns(txtTemplageFile.Text, DataScriptRule.getColSeperatorChar(GetColSperator()));
                    if (columns.Length < 2)
                    {
                        MessageBox.Show("数据文件列分隔符不存在，请选择相应列分隔符！");
                    }

                    if (columns.Contains(System.Configuration.ConfigurationManager.AppSettings["pk"]))
                    {
                        nextButton.Visibility = System.Windows.Visibility.Visible;

                    }
                    else
                    {
                        nextButton.Visibility = System.Windows.Visibility.Collapsed;
                        MessageBox.Show(string.Format("数据文件中缺少默认主键列（{0}）,请重新选择正确数据文件！", ConfigurationManager.AppSettings["pk"]));
                    }
                }
            }

        }

        private void prvButton_Click(object sender, RoutedEventArgs e)
        {
            DataScriptList mapModify = new DataScriptList();
            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(mapModify);
        }

        private string getTableName()
        {
            //项目编号、试验名称、数据类型、解析器
            string projectcode = ProjectCode.SelectedValue.ToString();
            string taskname = TaskName.Text.Trim();
            int scripttype = ScriptType.SelectedIndex;

            var script = scripts.FirstOrDefault(it => it.ProjectCode == projectcode && it.TaskName == taskname);
            if (script != null)
            {
                txtTableName.Text = script.TableName;
                return script.TableName;
            }
            else
            {
                // 此项目已有几个试验项目 
                string tablename = getTableName(projectcode, 1);
                txtTableName.Text = tablename;
                return tablename;
            }
        }

        private string getTableName(string projectcode, int index)
        {
            string tablename = string.Format("TBL_{0}_DATA{1}", projectcode, index).Replace('-', '_').ToUpper();

            if (TableDAL.getTableStructure(tablename).Count > 0)
            {
                return getTableName(projectcode, index + 1);
            }
            else
                return tablename;
        }

        // 计算版本号
        private void showVersion()
        {
            if (ProjectCode.SelectedValue == null)
                return;
            //项目编号、试验名称、数据类型、解析器
            string projectcode = ProjectCode.SelectedValue.ToString();
            string taskname = TaskName.Text.Trim();
            string filetype = FileType.SelectedValue.ToString();
            int scripttype = ScriptType.SelectedIndex;

            var sc = scripts.FirstOrDefault(it => it.ProjectCode == projectcode && it.TaskName == taskname && it.ScriptType == scripttype);
            if (sc != null)
            {
                double d = 0;
                double.TryParse(sc.MidsScriptVesion, out d);
                int version = (int)d + 1;

                MidsScriptCode.Text = sc.MidsScriptCode;
                MidsScriptName.Text = sc.MidsScriptName;
                MidsScriptVesion.Text = version.ToString("0.0");
            }
            else
            {
                MidsScriptVesion.Text = "1.0";
            }
        }

        private void ProjectCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showVersion();
            if (string.IsNullOrEmpty(FID))
            {
                getTableName();

            }
            string projectcode = ProjectCode.SelectedValue.ToString();
            List<string> list = scripts.Where(it => it.ProjectCode == projectcode).Select(it => it.TaskName).ToList().Distinct().ToList();
            list.Insert(0, "新增实验");

            TaskNameList.ItemsSource = list;
            getScriptCode();
        }

        private void TaskName_TextChanged(object sender, TextChangedEventArgs e)
        {
            showVersion();
            if (string.IsNullOrEmpty(FID))
            {
                getTableName();
                getScriptName();
            }
        }

        private void ScriptType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showVersion();
            getScriptName();
        }

        private void FileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showVersion();
        }

        private void TaskNameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskNameList.SelectedIndex == 0)
            {
                TaskName.Text = "";
                TaskName.IsEnabled = true;
                getScriptCode();
            }
            else
            {
                if (TaskNameList.SelectedItem != null)
                {
                    TaskName.Text = TaskNameList.SelectedItem.ToString();
                    TaskName.IsEnabled = false;
                }
            }
        }
        // 计算主键
    }

    public class TableInfo
    {
        public string TableName { get; set; }
        public string TableDesc { get; set; }
    }
}
