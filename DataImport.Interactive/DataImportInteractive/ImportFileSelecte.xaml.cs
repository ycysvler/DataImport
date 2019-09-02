using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys;
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

namespace DataImport.Interactive.DataImportInteractive
{
    /// <summary>
    /// ImportFileSelecte.xaml 的交互逻辑
    /// </summary>
    public partial class ImportFileSelecte : UserControl
    {
        public ImportFileSelecte()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += ImportFileSelecte_Loaded;
        }

        void ImportFileSelecte_Loaded(object sender, RoutedEventArgs e)
        {
            if (ScriptName.Items == null || ScriptName.Items.Count == 0)
            {
                var dsList = DataScriptDAL.getList();

                ScriptName.Items.Clear();

                foreach (var item in dsList)
                {
                    ScriptName.Items.Add(item);
                }

                ScriptName.SelectedIndex = 0;

                if (!String.IsNullOrEmpty(TaskCenter.ScriptID))
                {
                    var script = dsList.FirstOrDefault(it => it.FID == TaskCenter.ScriptID);
                    if (script != null)
                    {
                        ScriptName.SelectedIndex = dsList.IndexOf(script);

                        switch (script.FileType)
                        {
                            case "txt": FileType.SelectedIndex = 0;
                                break;
                            case "dat": FileType.SelectedIndex = 2;
                                break;
                            default: FileType.SelectedIndex = 1;
                                break;
                        }
                    }
                }
            }
        }

        DataScript dataScript;
        DataScriptRule dataRule;

        private void openTemplateFile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dataScript = ScriptName.SelectedItem as DataScript;

            dataRule = DataScriptRuleDAL.getInfo(dataScript.FID);

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            if (FileType.SelectedValue.ToString() == "xls/xlsx")
            {
                dialog.Filter = "(Excel 文件)|*.xls;*.xlsx";
            }
            else if (FileType.SelectedValue.ToString() == "dat")
            {
                dialog.Filter = "(dat 文件)|*.dat";
            }
            else if (FileType.SelectedValue.ToString() == "db")
            {
                dialog.Filter = "(sqlite 文件)|*.db";
            }
            else if (FileType.SelectedValue.ToString() == "mdb")
            {
                dialog.Filter = "(mdb 文件)|*.mdb";
            }
            else if (FileType.SelectedValue.ToString() == "fws10")
            {
                dialog.Filter = "(fws10 文件)|*.fws10";
            }
            else
            {
                dialog.Filter = "(文本文件)|*.txt";
            }

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            DataTable dt = new DataTable();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtTemplageFile.Text = dialog.FileName;

                if (FileType.SelectedValue.ToString() == "mdb")
                {
                    AccessImportHelper helper = new AccessImportHelper(dialog.FileName);
                    dt = helper.getDataTable(); 
                }
                else if (FileType.SelectedValue.ToString() == "xls/xlsx")
                {
                    dt = ExcelImportHelper.GetDataTable(dialog.FileName); 
                }
                else if (FileType.SelectedValue.ToString() == "db")
                {
                    dt = SQLiteImportHelper.GetDataTable(dialog.FileName);
                }
                else
                {
                    dt = TextImportHelper.GetDataTable(dialog.FileName, dataRule.getColSeperatorChar()); 
                }

                dataGrid.ItemsSource = dt.AsDataView();
                importGrid.ItemsSource = dt;
                checkStruct(dt, dataScript);
            } 
        }

        List<Structure> structures = new List<Structure>();
        DataTable sourceDt = new DataTable();
        private void Comments() { 
            foreach (DataColumn column in sourceDt.Columns)
            {
                if (structures.FirstOrDefault(it => it.Comments == column.ColumnName) == null)
                {
                    var st = structures.FirstOrDefault(it => string.IsNullOrEmpty(it.Comments));
                    TableDAL.Comment(dataScript.TableName, st.ColumnName, column.ColumnName);
                    st.Comments = column.ColumnName;

                    if (update.IsChecked.Value) {

                        DataScriptMap map = new DataScriptMap();
                        map.FID = Guid.NewGuid().ToString().Replace("-", "");
                        map.MdsImpDataScriptRuleID = dataScript.FID;
                        map.TableColName = st.ColumnName;
                        map.FileColName = column.ColumnName;
                        map.TransferType = "02";
                        map.CreatedBy = MainWindow.UserID;
                        map.LastUpdatedBy = MainWindow.UserID;
                        map.LastUpdateIp = "127.0.0.1";
                        DataScriptMapDAL.Insert(map);
                    }
                }
            }
        }

        private void checkStruct(DataTable dt, DataScript script)
        {
            fuck.Visibility = System.Windows.Visibility.Hidden;
            update.Visibility = System.Windows.Visibility.Hidden;

            structures = TableDAL.getTableStructure(script.TableName);
            List<string> difference = new List<string>();
            sourceDt = dt;

            string error = "";

            bool havePk = false;

            foreach (DataColumn column in dt.Columns)
            {
                if (structures.FirstOrDefault(it => it.Comments == column.ColumnName) == null)
                {
                    difference.Add(column.ColumnName);
                    error += column.ColumnName + "\r\n";
                }

                if (column.ColumnName == System.Configuration.ConfigurationManager.AppSettings["pk"]) {
                    havePk = true;
                }
            }

            if (!havePk) { 
                error += string.Format("数据文件中缺少默认主键列（{0}）,请重新选择正确数据文件！\r\n",ConfigurationManager.AppSettings["pk"]); 
            }

            var dsms = DataScriptMapDAL.getList(script.FID);

            if (difference.Count * 10 / dsms.Count >= 6)
            {
                string msg = "您选择的数据文件与解析器严重不匹配，请重新选择数据文件!\r\n差异字段：\r\n";
                msg += error;

                MessageBox.Show(msg);
                fuck.Visibility = System.Windows.Visibility.Visible;
                update.Visibility = System.Windows.Visibility.Visible;

                btNext.Visibility = System.Windows.Visibility.Collapsed;

            }
            else if (difference.Count > 0)
            {
                string msg = "您选择的数据文件与解析器不匹配,差异字段：\r\n";
                msg += error;

                MessageBox.Show(msg);

                fuck.Visibility = System.Windows.Visibility.Visible;
                update.Visibility = System.Windows.Visibility.Visible;

                btNext.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ScriptName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataScript datascript = ScriptName.SelectedItem as DataScript;
            desc.Text = datascript.Remark;
            FileType.SelectedValue = datascript.FileType;

        }

        private void btNext_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTemplageFile.Text.Trim()))
            {
                MessageBox.Show("请选择导入文件");
                return;
            }

            if (fuck.IsChecked.Value && false) {

                List<string> difference = new List<string>();

                foreach (DataColumn column in sourceDt.Columns)
                {
                    if (structures.FirstOrDefault(it => it.Comments == column.ColumnName) == null)
                    {
                        difference.Add(column.ColumnName);

                    }
                }

                if (difference.Count > structures.Count(it => string.IsNullOrEmpty(it.Comments)))
                {
                    MessageBox.Show("剩余空闲字段不足，无法导入全部字段，请重新选择数据文件！");
                    return;
                }
                Comments();

                if (update.IsChecked.Value)
                {
                    float newVersion = float.Parse(dataScript.MidsScriptVesion) + 0.1f;
                    DataScriptDAL.updateLevel(dataScript.FID, newVersion.ToString());
                }
            
            }

            
            dataScript = (DataScript)ScriptName.SelectedItem;

            ImportMapModify mapModify = new ImportMapModify();
            mapModify.FID = dataScript.FID;
            mapModify.DataScriptRule = dataRule;
            mapModify.DataScript = dataScript;
            mapModify.sourceFile = txtTemplageFile.Text;

            importGrid.getHexColumn();

            ImportStack.Push(this);
            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(mapModify);
        }

        private void prvButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;
            UIElement item = ImportStack.Pop();
            window.StartPage(item);
        }

        private void fuck_Click(object sender, RoutedEventArgs e)
        {
            if (fuck.IsChecked.Value)
                btNext.Visibility = System.Windows.Visibility.Visible;
            else
                btNext.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
