using DataImport.DataAccess;
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

namespace DataImport.Interactive.DataScriptInteractive
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

        void DataScriptInfo_Loaded(object sender, RoutedEventArgs e)
        { 
            string[] tablelist = System.Configuration.ConfigurationManager.AppSettings["tables"].Split(new char[]{','});
            string[] descs = System.Configuration.ConfigurationManager.AppSettings["tabledescs"].Split(new char[]{','});

            //for (int i = 0; i < tablelist.Length; i++) {
            //    tables.Add(new TableInfo() {  TableDesc=descs[i], TableName = tablelist[i]});
            //}

            var objTableList = ObjtableInfoDAL.getList();

            ScriptType.SelectedIndex = 0;


            foreach (var objTable in objTableList)
            {
                tables.Add(new TableInfo()
                {
                    TableDesc = string.Format("{0}({1})", objTable.ObjectTableName, objTable.ObjectTableCode),
                    TableName = objTable.ObjectTableCode
                });
            }

            Des_Table.ItemsSource = tables;
            Des_Table.DisplayMemberPath = "TableDesc"; 

            if (string.IsNullOrEmpty(FID))
            {
                // new row
            }
            else
            {
                // edit row
                showData();
            }
        }

        DataScript dScript;
        DataScriptRule dRule;

        private bool checkCode() {  
            return DataScriptDAL.CheckCode(MidsScriptCode.Text.Trim()) == 0; 
        }

        private void showData()
        {
            dScript = DataScriptDAL.getInfo(FID);
            dRule = DataScriptRuleDAL.getInfo(FID);

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

            if (tables.SingleOrDefault(it => it.TableName == dRule.DesTable) == null) {
                tables.Add(new TableInfo() { TableDesc = dRule.DesTable, TableName = dRule.DesTable });
            }
            Des_Table.SelectedValue = tables.Single(it=>it.TableName == dRule.DesTable);
            txtTemplageFile.Text = dRule.DesFile;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            string erromsg = "";


            if (string.IsNullOrEmpty(MidsScriptCode.Text.Trim()))
            {
                erromsg += "请输入解析器编码！\r\n";
            }
            else {
                if (string.IsNullOrEmpty(FID) && (!checkCode()))
                {
                    erromsg += "解析器编码重复！\r\n";
                }
            }
            if (string.IsNullOrEmpty(MidsScriptName.Text.Trim()))
            {
                erromsg += "请输入解析器名称！\r\n";
            }
            if (string.IsNullOrEmpty(txtTemplageFile.Text.Trim()))
            {
                erromsg += "请选择样例文件！\r\n";
            }
            if (Des_Table.SelectedIndex == -1)
            {
                erromsg += "请选择目标数据表！\r\n";
            }

            if (!string.IsNullOrEmpty(erromsg)) {
                MessageBox.Show(erromsg);
                return;
            }

            if (string.IsNullOrEmpty(FID))
            {
                insert();
            }
            else
            {
                Update();
            }

            DataScriptMapModify mapModify = new DataScriptMapModify();
            mapModify.FID = FID;
            mapModify.DataScriptRule = dRule;
            mapModify.DataScript = dScript;
            MainWindow window = App.Current.MainWindow as MainWindow;

            ImportStack.Push(this);

            window.StartPage(mapModify);

        }

        private void Update()
        {
            dScript.MidsScriptCode = MidsScriptCode.Text.Trim();
            dScript.MidsScriptName = MidsScriptName.Text.Trim();
            dScript.MidsScriptVesion = MidsScriptVesion.Text.Trim();
            dScript.FileType = FileType.SelectedValue.ToString();
            dScript.IndexKey = IndexKey.Text.Trim();
            dScript.ValidFlag = ValidFlag.Text.Trim();
            dScript.Remark = Remark.Text.Trim();
            dScript.ApplyTestProject = ApplyTestProject.SelectedValue.ToString();
            dScript.CreationDate = DateTime.Now;
            dScript.LastUpdateDate = DateTime.Now;
            dScript.LastUpdateIp = "127.0.0.1";
            dScript.Version = 1;
            dScript.ScriptType = ScriptType.SelectedIndex;
             
            dRule.ColSperator = GetColSperator();
            dRule.ColnameLines = int.Parse(ColName_Lines.Text.Trim());
            dRule.DesTable = (Des_Table.SelectedItem as TableInfo).TableName;
            dRule.DesFile = txtTemplageFile.Text;
            dRule.CreationDate = DateTime.Now;
            dRule.LastUpdateDate = DateTime.Now;
            dRule.LastUpdateIp = "127.0.0.1";
            dRule.Version = 1;
            dRule.Remark = Remark.Text.Trim();

            DataScriptDAL.update(dScript);
            DataScriptRuleDAL.update(dRule);
        }

        private void insert()
        {
            dScript = new DataScript();

            dScript.FID = Guid.NewGuid().ToString().Replace("-", "");
            FID = dScript.FID;
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

            dRule = new DataScriptRule();
            dRule.FID = dScript.FID;    // Guid.NewGuid().ToString().Replace("-", "");
            dRule.MdsImpDataScriptID = dScript.FID;
            dRule.ColSperator = GetColSperator();
            dRule.ColnameLines = int.Parse(ColName_Lines.Text.Trim());
            dRule.DesTable = (Des_Table.SelectedItem as TableInfo).TableName;
            dRule.DesFile = txtTemplageFile.Text;
            dRule.CreationDate = DateTime.Now;
            dRule.LastUpdateDate = DateTime.Now;
            dRule.LastUpdateIp = "127.0.0.1";
            dRule.Version = 1;
            dRule.DesBusinessPk = "";
            dRule.CreatedBy = MainWindow.UserID;
            dRule.LastUpdatedBy = MainWindow.UserID;

            DataScriptDAL.Insert(dScript);
            DataScriptRuleDAL.Insert(dRule); 
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

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtTemplageFile.Text = dialog.FileName;
            }
        }

        private void prvButton_Click(object sender, RoutedEventArgs e)
        {
            DataScriptList mapModify = new DataScriptList(); 
            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(mapModify);
        }

      
    }

    public class TableInfo {
        public string TableName { get; set; }
        public string TableDesc { get; set; }
    }
}
