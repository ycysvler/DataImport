using DataImport.BLL;
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

namespace DataImport.Interactive.DataImportInteractive
{
    /// <summary>
    /// ImportMapModify.xaml 的交互逻辑
    /// </summary>
    public partial class ImportMapModify : UserControl
    {
        public string FID { get; set; }

        public bool isAutoDrawLine { get; set; }

        public ImportMapModify()
        {
            InitializeComponent();

            this.Loaded += ImportMapModify_Loaded;
        }

        public DataScript DataScript { get; set; }
        public DataScriptRule DataScriptRule { get; set; }
        public String sourceFile { get; set; }

        void ImportMapModify_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataScript.FileType == "mdb") {
                AccessImportHelper helper = new AccessImportHelper(sourceFile);
                columnConnection.Source = helper.getDataTable();
            }
            else if (DataScript.FileType == "db")
            {
                columnConnection.Source = SQLiteImportHelper.GetDataTable(sourceFile);
            }
            else if (DataScript.FileType == "xls/xlsx")
            {
                columnConnection.Source = ExcelImportHelper.GetDataTable(sourceFile);
            }
            else
            {
                columnConnection.Source = TextImportHelper.GetDataTable(sourceFile, this.DataScriptRule.getColSeperatorChar());
            }

            columnConnection.Target = TableDAL.getTableStructure(this.DataScriptRule.DesTable);
            columnConnection.BusinessPK = DataScriptRule.DesBusinessPk;
            columnConnection.FID = FID;

            columnConnection.ShowComplete += columnConnection_ShowComplete;
            
        }

        void columnConnection_ShowComplete(object sender, EventArgs e)
        {
            if (isAutoDrawLine)
                columnConnection.autoDrawLine();
        }

        private void btnPrv_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;
            UIElement item = ImportStack.Pop();
            window.StartPage(item); 
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            ImportRun mapModify = new ImportRun();
            mapModify.DataScript = this.DataScript;
            mapModify.DataScriptRule = this.DataScriptRule;
            mapModify.ColumnMap = columnConnection.ColumnMap;
            mapModify.ScriptMap = columnConnection.ScriptMap;
            mapModify.sourceFile = sourceFile;
            mapModify.DataScriptRule.DesBusinessPk = columnConnection.BusinessPK;

            if (!mapModify.ColumnMap.ContainsKey("COLUMN0")) {
                MessageBox.Show("您需要指定时间列的关联关系，才可以导入数据！");
                return;
            }

            foreach (string key in mapModify.ScriptMap.Keys)
            {
                string script = mapModify.ScriptMap[key].Trim();
                if (!string.IsNullOrEmpty(script))
                {
                    DataScriptMapDAL.SaveScriptFile(key, script);
                }
            }

            // 保存对应关系
            if (DataScriptInfo.saveScript||true) {


                DataScriptMapDAL.delAll(FID);

                DataScriptRuleDAL.updateBusinessPK(FID, columnConnection.BusinessPK);

                foreach (var key in columnConnection.ColumnMap.Keys)
                {

                    DataScriptMap map = new DataScriptMap();
                    map.FID = Guid.NewGuid().ToString().Replace("-", "");
                    map.MdsImpDataScriptRuleID = FID;
                    map.TableColName = key;
                    map.FileColName = columnConnection.ColumnMap[key];
                    map.TransferType = "02";
                    map.CreatedBy = MainWindow.UserID;
                    map.LastUpdatedBy = MainWindow.UserID;
                    map.LastUpdateIp = "127.0.0.1";

                    // 如果有算式，保存
                    if (columnConnection.ScriptMap.ContainsKey(key))
                        map.TransferScript = columnConnection.ScriptMap[key];

                    DataScriptMapDAL.Insert(map);
                }
                //MessageBox.Show("对应关系已经保存！");
            }

            ImportStack.Push(this);

            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(mapModify);
        }
    }
}
