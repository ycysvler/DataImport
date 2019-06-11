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

namespace DataImport.Interactive
{
    /// <summary>
    /// TestControl.xaml 的交互逻辑
    /// </summary>
    public partial class TestControl : UserControl
    {
        public TestControl()
        {
            InitializeComponent();
        }

        private List<Project> projectList = new List<Project>();

        private void ProjectCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectCode.SelectedValue == null) { return; }

            string projectcode = ProjectCode.SelectedValue.ToString();
            Console.WriteLine("projectcode:{0}", projectcode);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

            projectList.Add(new Project() {
                ProjectName="pn1",
                ProjectCode="pc1"
            });
            projectList.Add(new Project()
            {
                ProjectName = "pn2",
                ProjectCode = "pc2"
            });
            projectList.Add(new Project()
            {
                ProjectName = "pn3",
                ProjectCode = "pc3"
            });
            projectList.Add(new Project()
            {
                ProjectName = "pn4",
                ProjectCode = "pc4"
            });
            projectList.Add(new Project()
            {
                ProjectName = "pn5",
                ProjectCode = "pc5"
            });

            ProjectCode.ItemsSource = projectList;
            ProjectCode.DisplayMemberPath = "ProjectName";
            ProjectCode.SelectedValuePath = "ProjectCode";
        }

        private void ProjectCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (ProjectCode.Text != null && (!string.IsNullOrEmpty(ProjectCode.Text)))
                ProjectCode.ItemsSource = projectList.Where((i) => i.ProjectName.Contains(ProjectCode.Text.Trim()));

            if (ProjectCode.Text == null || string.IsNullOrEmpty(ProjectCode.Text))
            {
                ProjectCode.ItemsSource = projectList;
            }
            ProjectCode.DisplayMemberPath = "ProjectName";
            ProjectCode.SelectedValuePath = "ProjectCode";

            ProjectCode.IsDropDownOpen = true;
        }
    }
}
