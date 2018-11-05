using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys;
using DataImport.Interactive.DataScriptInteractive;
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
    /// DataScriptInfo.xaml 的交互逻辑
    /// </summary>
    public partial class DataScriptInfo : UserControl
    {
        public DataScriptInfo()
        {
            InitializeComponent();
            tables = new List<TableInfo>();
            this.Loaded += DataScriptInfo_Loaded;

            saveScript = false;
        }
        private bool checkCode()
        {
            return DataScriptDAL.CheckCode(MidsScriptCode.Text.Trim()) == 0;
        }
        public static bool saveScript = false;

        public string FID { get; set; }
        private List<TableInfo> tables = new List<TableInfo>();
        DataScript dScript;
        DataScriptRule dRule;

        void DataScriptInfo_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取这个项目唯一使用目标表
            string singleTable = DataLogDAL.getSingleTableName(TaskCenter.TaskID);

            if (tables.Count == 0)
            {
                tables.Add(new TableInfo() { TableDesc = "新建表", TableName = "new" });

                var objTableList = ObjtableInfoDAL.getList();


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



                TableInfo singleTinfo = tables.FirstOrDefault(it => it.TableName == singleTable);
                if (singleTinfo != null)
                {
                    Des_Table.SelectedItem = singleTinfo;
                    Des_Table.IsEnabled = false;
                }
                else
                {

                    Des_Table.SelectedIndex = 0;
                }

                ApplyTestProject.SelectedIndex = 0;
            }
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
            MainWindow window = App.Current.MainWindow as MainWindow;
            UIElement item = ImportStack.Pop();
            window.StartPage(item);
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            string erromsg = "";

            if (string.IsNullOrEmpty(txtTemplageFile.Text.Trim()))
            {
                erromsg += "请选择样例文件！\r\n";
            }

            if (Des_Table.SelectedIndex == -1)
            {
                erromsg += "请选择目标数据表！\r\n";
            }

            if (Des_Table.SelectedIndex == 0 && string.IsNullOrEmpty(txtTableName.Text))
            {
                erromsg += "请输入数据表名称！\r\n";
            }

            if (Des_Table.SelectedIndex == 0 && string.IsNullOrEmpty(txtTableCode.Text))
            {
                erromsg += "请输入数据表代码！\r\n";
            }


            if (cbSave.IsChecked.Value)
            {
                if (string.IsNullOrEmpty(MidsScriptCode.Text.Trim()))
                {
                    erromsg += "请输入解析器编码！\r\n";
                }
                else
                { 
                    if (!checkCode())
                    {
                        erromsg += "解析器编码重复！\r\n";
                    }

                }
                if (string.IsNullOrEmpty(MidsScriptName.Text.Trim()))
                {
                    erromsg += "请输入解析器名称！\r\n";
                }
            }

            if (!string.IsNullOrEmpty(erromsg))
            {
                MessageBox.Show(erromsg);
                return;
            }

            builderScript();


            if (cbSave.IsChecked.Value)
            {
                insert();
            }
            DataTable dt = new DataTable();

            if (FileType.SelectedValue.ToString() == "xls/xlsx")
            {
                dt = ExcelImportHelper.GetDataTable(txtTemplageFile.Text.Trim());
            }
            else
            {
                dt = TextImportHelper.GetDataTable(txtTemplageFile.Text.Trim(), DataScriptRule.getColSeperatorChar(GetColSperator()));
            }

            TableDAL.CreateTable(txtTableCode.Text.ToUpper(), dt);

            int count = ObjtableInfoDAL.Count(txtTableCode.Text.ToUpper());
            if (count == 0)
            {
                ObjtableInfo oinfo = new ObjtableInfo();
                oinfo.FID = Guid.NewGuid().ToString().Replace("-", "");
                oinfo.CreatedBy = MainWindow.UserID;
                oinfo.LastUpdatedBy = MainWindow.UserID;
                oinfo.ObjectTableCode = txtTableCode.Text.ToUpper();
                oinfo.ObjectTableName = txtTableName.Text;
                oinfo.Status = "02";
                oinfo.Version = 1;
                oinfo.LastUpdateIp = "127.0.0.1";
                oinfo.LastUpdateDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                oinfo.CreationDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                ObjtableInfoDAL.Insert(oinfo);
            }

            ImportMapModify mapModify = new ImportMapModify();
            mapModify.FID = FID;
            mapModify.DataScriptRule = dRule;
            mapModify.DataScript = dScript;
            mapModify.sourceFile = txtTemplageFile.Text.Trim();
            mapModify.isAutoDrawLine = true;

            ImportStack.Push(this);
            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(mapModify);

        }

        private void Des_Table_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Des_Table.SelectedIndex == 0)
            {
                lblTableName.Visibility = System.Windows.Visibility.Visible;
                txtTableName.Visibility = System.Windows.Visibility.Visible;
                lblTableCode.Visibility = System.Windows.Visibility.Visible;
                txtTableCode.Visibility = System.Windows.Visibility.Visible;

                txtTableCode.Text = "";
                txtTableName.Text = "";
            }
            else
            {
                lblTableName.Visibility = System.Windows.Visibility.Hidden;
                txtTableName.Visibility = System.Windows.Visibility.Hidden;
                lblTableCode.Visibility = System.Windows.Visibility.Hidden;
                txtTableCode.Visibility = System.Windows.Visibility.Hidden;

                TableInfo tinfo = Des_Table.SelectedItem as TableInfo;
                txtTableCode.Text = tinfo.TableName;
                txtTableName.Text = tinfo.TableDesc;
            }
        }

        private void builderScript()
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



            dRule = new DataScriptRule();
            dRule.FID = dScript.FID;
            dRule.MdsImpDataScriptID = dScript.FID;
            dRule.ColSperator = GetColSperator();
            dRule.ColnameLines = int.Parse(ColName_Lines.Text.Trim());
            dRule.DesTable = txtTableCode.Text.ToUpper();

            dRule.DesFile = txtTemplageFile.Text;
            dRule.CreationDate = DateTime.Now;
            dRule.LastUpdateDate = DateTime.Now;
            dRule.LastUpdateIp = "127.0.0.1";
            dRule.Version = 1;
            dRule.DesBusinessPk = "";
            dRule.CreatedBy = MainWindow.UserID;
            dRule.LastUpdatedBy = MainWindow.UserID;

        }
        private void insert()
        {

            DataScriptDAL.Insert(dScript);
            DataScriptRuleDAL.Insert(dRule);
        }

        private void cbSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbSave.IsChecked.Value)
            {
                gdSave.Visibility = System.Windows.Visibility.Visible;
                saveScript = true;
            }
            else
            {
                gdSave.Visibility = System.Windows.Visibility.Hidden;
                saveScript = false;
            }
        }

        private void txtTableName_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtTableName.Text = txtTableName.Text.ToUpper();
        }

        private void txtTableCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tinfo = tables.FirstOrDefault(it => it.TableName == txtTableCode.Text.ToUpper());
            if (tinfo != null && Des_Table.SelectedIndex == 0)
            {
                MessageBox.Show("此表已存在，请通过目标数据表下拉选择此表！");
            }
        }
    }
}
