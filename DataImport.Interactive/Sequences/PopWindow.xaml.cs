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

namespace DataImport.Interactive.Sequences
{
    /// <summary>
    /// PopWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PopWindow : UserControl
    {
        public PopWindow()
        {
            InitializeComponent();
        }

        public event EventHandler runEvent;
        public event EventHandler cancelEvent;

        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            if (runEvent != null)
            {
                runEvent(this, null);
            } 
            
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            if (cancelEvent != null)
            {
                cancelEvent(this, null);

            }
            
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            if (cancelEvent != null)
            {
                cancelEvent(this, null);
            }
            
        }
    }
}
