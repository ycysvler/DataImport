using DataImport.DataAccess;
using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// DataScriptList.xaml 的交互逻辑
    /// </summary>
    public partial class DataScriptList : UserControl
    {
        public DataScriptList()
        {
            InitializeComponent();

            this.Loaded += DataScriptList_Loaded;
            
        }

        List<DataScript> dataSource = new List<DataScript>();

        void DataScriptList_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                dataSource = DataScriptDAL.getList();

                dataGrid.DataContext = dataSource;

               FileType.SelectedIndex = 0;

                FileType.SelectionChanged += FileType_SelectionChanged;
                MidsScriptCode.TextChanged +=MidsScriptCode_TextChanged;
                MidsScriptName.TextChanged +=MidsScriptName_TextChanged;
                IndexKey.TextChanged +=IndexKey_TextChanged;
            }
        }

        void bindData()
        {
            List<DataScript> tempsource = dataSource.ToList();

            if (FileType.SelectedIndex != 0)
            {
                tempsource = tempsource.Where(it => it.FileType.IndexOf(FileType.SelectedValue.ToString()) > -1).ToList();
            }
            if(MidsScriptCode!=null&&!string.IsNullOrEmpty(MidsScriptCode.getText().Trim())){
                tempsource = tempsource.Where(it => it.MidsScriptCode.IndexOf(MidsScriptCode.getText().Trim()) > -1).ToList();
            }
              if(MidsScriptName != null && !string.IsNullOrEmpty(MidsScriptName.getText().Trim())){
                  tempsource = tempsource.Where(it => it.MidsScriptName.IndexOf(MidsScriptName.getText().Trim()) > -1).ToList();
            }
             if(IndexKey!=null&&!string.IsNullOrEmpty(IndexKey.getText().Trim())){
                 tempsource = tempsource.Where(it => it.IndexKey.IndexOf(IndexKey.getText().Trim()) > -1).ToList();
            }
             dataGrid.DataContext = tempsource;               
        }

        private void detail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;

            DataScriptInteractive2.DataScriptInfo info = new DataScriptInteractive2.DataScriptInfo();
            info.FID = img.Tag.ToString();

            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(info);
        }

        private void InvalidText_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock img = (TextBlock)sender;

            DataScript info = img.Tag as DataScript;

            DataScriptDAL.updateInvalid(info.FID, info.Invalid == 0 ? 1 : 0);

            dataSource = DataScriptDAL.getList();
            bindData();
        }
        
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;

            window.StartPage(new DataScriptInteractive2.DataScriptInfo());
        }

        private void delButton_Click(object sender, RoutedEventArgs e)
        {
            string error = "";
            foreach (DataScript item in dataGrid.SelectedItems)
            {
                if (item.CreatedBy != MainWindow.UserID)
                {
                    error += string.Format("解析器[{0}],是别人创建的，无法删除\r\n", item.DisplayName);
                    continue;
                }
                if (item.Release == "02")
                {
                    error += string.Format("解析器[{0}],已发布，无法删除\r\n", item.DisplayName);
                    continue;
                }

                List<string> allMaps = DataScriptDAL.getMapColName(item.TableName);
                var currMaps = DataScriptMapDAL.getList(item.FID);
                var struceures = TableDAL.getTableStructure(item.TableName);

                foreach (var cm in currMaps) {
                    if (allMaps.Count(it => it == cm.TableColName) > 1)
                    {
                        // 多个并用
                        continue;
                    }
                    var s = struceures.FirstOrDefault(it=>it.ColumnName == cm.TableColName);
                    if (s != null && s.IsKey)
                    {
                        // 主键不能删
                        continue;
                    }
                    TableDAL.dropColumn(item.TableName, cm.TableColName);
                }

                DataScriptDAL.Delete(item.FID);
                DataScriptRuleDAL.Delete(item.FID);
                DataScriptMapDAL.delAll(item.FID);
            }

            if (!string.IsNullOrEmpty(error)) {
                MessageBox.Show(error);
            }

            dataGrid.DataContext = DataScriptDAL.getList();
        }

      

        private void FileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dataGrid != null)
            bindData();
        }

        private void MidsScriptCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dataGrid != null)
            bindData();
        }

        private void MidsScriptName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dataGrid != null)
            bindData();
        }

        private void IndexKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dataGrid != null)
            bindData();
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock block = sender as TextBlock;
            string fid = block.Tag.ToString();
            if (block.Text == "未发布")
            {
                DataScriptDAL.Release(fid);
                block.Text = "已发布";
                MessageBox.Show("新建解析器发布后,请及时进行同步数据操作!");
            }
            else {
                MessageBox.Show("解析器已发布，无法重复发布");
            }
        }
    }
}
