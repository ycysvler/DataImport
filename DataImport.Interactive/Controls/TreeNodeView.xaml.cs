using DataImport.Interactive.Sequences;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataImport.Interactive.Controls
{
    /// <summary>
    /// TreeNodeView.xaml 的交互逻辑
    /// </summary>
    public partial class TreeNodeView : UserControl
    {
        public TreeNodeView()
        {
            InitializeComponent();
             
            i_down.Visibility = System.Windows.Visibility.Collapsed;
            i_up.Visibility = System.Windows.Visibility.Collapsed;
        } 

        public TreeNode TreeNode { get; set; }

        public bool IsRoot { set {
            i_down.Visibility = System.Windows.Visibility.Visible;
            i_up.Visibility = System.Windows.Visibility.Visible;
        } }

        public string ParentType = "";

        public string Title
        {
            get { return v_title.Text; }
            set { v_title.Text = value; }
        }

        public string Value {
            set { v_value.Text = value; }
        }

        public string NodeType { get; set; }

        public int State
        {
            set
            {
                switch (value)
                {
                    case -1:
                        i_state.Visibility = System.Windows.Visibility.Hidden;
                        v_state.Text = ""; break;

                    case 0:
                        i_state.Visibility = System.Windows.Visibility.Hidden;
                        //v_state.Text = "○"; 
                        break;  
                    case 1:
                        i_state.Visibility = System.Windows.Visibility.Visible;
                        //v_state.Text = "●"; 
                        break;
                }
            }
        }

        public string Code { get; set; }

        public void AddChild(TreeNodeView node)
        {
            cb.Visibility = System.Windows.Visibility.Visible;
            panel_children.Children.Add(node);
        }

        public List<TreeNodeView> getChildren()
        {
            List<TreeNodeView> result = new List<TreeNodeView>();

            foreach (var ui in panel_children.Children)
            {
                result.Add(ui as TreeNodeView);
            }
            return result;
        }

        public bool IsCollapse {
            set {
                if (value)
                { 
                    panel_children.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    panel_children.Visibility = System.Windows.Visibility.Visible;
                }
                cb.IsChecked = !value;
            }
            get { 
                return panel_children.Visibility == System.Windows.Visibility.Collapsed;
            }
        }

        private void cb_Click(object sender, RoutedEventArgs e)
        {
            if (!cb.IsChecked.Value)
            {
                panel_children.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                panel_children.Visibility = System.Windows.Visibility.Visible;
            }
        }

        DoubleAnimation da = new DoubleAnimation();

        public bool IsActive {
            set {
                if (value) {
                    
                    da.From = 1.0f;
                    da.To = 0.0f;
                    da.Duration = new Duration(TimeSpan.Parse("0:0:2"));
                    da.RepeatBehavior = RepeatBehavior.Forever; 
                    i_state.BeginAnimation(Image.OpacityProperty, da); 
                }
                else {
                    i_state.BeginAnimation(Image.OpacityProperty, null);                    
                }
            
            }
        }
    }
}
