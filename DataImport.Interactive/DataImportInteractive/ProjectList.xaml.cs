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
    /// ProjectList.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectList : UserControl
    {
        public ProjectList()
        {
            
            InitializeComponent(); 
           
            this.Loaded += ProjectList_Loaded;
        }

        void ProjectList_Loaded(object sender, RoutedEventArgs e)
        {
            //ScriptID = "";

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            { 
                dataSource = ProjectDAL.getList();
                 

                if (MainWindow.UserID != "test") {
                    dataSource = dataSource.Where(it => it.UserCode == MainWindow.UserName).ToList();
                }
                  
                dataGrid.DataContext = dataSource;
            }
        }
        List<Project> dataSource = new List<Project>();
        //public static string FID { get; set; }
        //public static string ScriptID { get; set; }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;

            TaskCenter.TaskID = img.Tag.ToString();
            var project = dataSource.SingleOrDefault(it => it.FID == TaskCenter.TaskID);
            if (project != null)
            { 
                TaskCenter.ScriptID = project.ScriptID;
            }

            MainWindow window = App.Current.MainWindow as MainWindow;

            UIElement item = new ImportFileSelecte();

            ImportStack.clear();
            ImportStack.Push(this);
            
            window.StartPage(item);
        }

        private void Direct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;

            TaskCenter.TaskID = img.Tag.ToString();
            var project = dataSource.SingleOrDefault(it => it.FID == TaskCenter.TaskID);
            if (project != null) {
                TaskCenter.ScriptID = project.ScriptID;
            }

            MainWindow window = App.Current.MainWindow as MainWindow;

            UIElement item = new DataImportInteractive.DataScriptInfo();

            ImportStack.clear();
            ImportStack.Push(this);
           
            window.StartPage(item);
        }
    }
}
