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

namespace DataImport.Interactive.DataScriptInteractive
{
    /// <summary>
    /// DataScriptMapModify.xaml 的交互逻辑
    /// </summary>
    public partial class DataScriptMapModify : UserControl
    {
        public DataScriptMapModify()
        {
            InitializeComponent();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += DataScriptMapModify_Loaded;
        }

        private string fid;

        public string FID
        {
            get { return fid; }
            set
            {
                fid = value;
            }
        }
         

        public DataScript DataScript { get; set; }
        public DataScriptRule DataScriptRule { get; set; }

        void DataScriptMapModify_Loaded(object sender, RoutedEventArgs e)
        {
            // columnConnection.Source = TextImportHelper.GetDataTable(@"G:\workspace\动控实现数据管理\src\TestData\data.txt", ',');
            //columnConnection.Target = TableDAL.getTableStructure("MDS_IMP_DATA_TEST");

            if (DataScript.FileType == "mdb") {
                AccessImportHelper helper = new AccessImportHelper(this.DataScriptRule.DesFile);
                columnConnection.Source = helper.getDataTable();
            }
            else if (DataScript.FileType == "xls/xlsx")
            {
                columnConnection.Source = ExcelImportHelper.GetDataTable(this.DataScriptRule.DesFile); 
            }
            else {
                columnConnection.Source = TextImportHelper.GetDataTable(this.DataScriptRule.DesFile, this.DataScriptRule.getColSeperatorChar()); 
            }

            columnConnection.FID = FID;
            columnConnection.BusinessPK = this.DataScriptRule.DesBusinessPk;
            columnConnection.Target = TableDAL.getTableStructure(this.DataScriptRule.DesTable); 
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
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
                map.LastUpdateIp = "127.0.0.1";
                map.LastUpdatedBy = MainWindow.UserID;
                map.CreatedBy = MainWindow.UserID;

               // 如果有算式，保存
                if(columnConnection.ScriptMap.ContainsKey(key))
                    map.TransferScript = columnConnection.ScriptMap[key];

                DataScriptMapDAL.Insert(map);
            }

            MessageBox.Show("对应关系已经保存！");

            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(new DataScriptList());
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(new DataScriptList());
        }

        private void autoLine_Click(object sender, RoutedEventArgs e)
        {
            columnConnection.autoDrawLine();
        }
    }
}
