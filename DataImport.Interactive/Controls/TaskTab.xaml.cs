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

namespace DataImport.Interactive.Controls
{
    /// <summary>
    /// TaskTab.xaml 的交互逻辑
    /// </summary>
    public partial class TaskTab : UserControl
    {
        int selectedIndex = 0;

        public int SelectedIndex {
            get { return selectedIndex; }
            set { selectedIndex = value; }
        }

        public TaskTab()
        {
            InitializeComponent();

            menu0.MouseEnter += menu_MouseEnter;
            menu1.MouseEnter += menu_MouseEnter;
            menu2.MouseEnter += menu_MouseEnter;
            menu3.MouseEnter += menu_MouseEnter;
            menu4.MouseEnter += menu_MouseEnter;
            menu5.MouseEnter += menu_MouseEnter;
            menu6.MouseEnter += menu_MouseEnter;

            menu0.MouseLeave += menu_MouseLeave;
            menu1.MouseLeave += menu_MouseLeave;
            menu2.MouseLeave += menu_MouseLeave;
            menu3.MouseLeave += menu_MouseLeave;
            menu4.MouseLeave += menu_MouseLeave;
            menu5.MouseLeave += menu_MouseLeave;
            menu6.MouseLeave += menu_MouseLeave;

            menu0.MouseLeftButtonDown += menuMouseLeftButtonDown;
            menu1.MouseLeftButtonDown += menuMouseLeftButtonDown;
            menu2.MouseLeftButtonDown += menuMouseLeftButtonDown;
            menu3.MouseLeftButtonDown += menuMouseLeftButtonDown;
            menu4.MouseLeftButtonDown += menuMouseLeftButtonDown;
            menu5.MouseLeftButtonDown += menuMouseLeftButtonDown;
            menu6.MouseLeftButtonDown += menuMouseLeftButtonDown; 
            
        }

        void menuMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedIndex = Convert.ToInt32((sender as StackPanel).Tag);
            MenuSelectChanged();
            menu_MouseLeave(null, null);
        }
        void menu_MouseEnter(object sender, MouseEventArgs e)
        {
            ((UIElement)sender).Opacity = 1;
        }

        void menu_MouseLeave(object sender, MouseEventArgs e)
        {
            menu0.Opacity = 0.5;
            menu1.Opacity = 0.5;
            menu2.Opacity = 0.5;
            menu3.Opacity = 0.5;
            menu4.Opacity = 0.5;
            menu5.Opacity = 0.5;
            menu6.Opacity = 0.5;

            if (selectedIndex == 0)
            {
                menu0.Opacity = 1;

            }
            if (selectedIndex == 1)
            {
                menu1.Opacity = 1;

            }
            if (selectedIndex == 2)
            {
                menu2.Opacity = 1;
            }
            if (selectedIndex == 3)
            {
                menu3.Opacity = 1;
            } if (selectedIndex == 4)
            {
                menu4.Opacity = 1;
            }
            if (selectedIndex == 5)
            {
                menu5.Opacity = 1;
            }
            if (selectedIndex == 6)
            {
                menu6.Opacity = 1;
            }
        }

        private void MenuSelectChanged()
        {
            MenuSelectedEventArgs args = new MenuSelectedEventArgs() { SelectedIndex = this.selectedIndex };

            if (this.MenuSelectChangedEvent != null)
                MenuSelectChangedEvent(this, args); 
        }

        public event EventHandler<MenuSelectedEventArgs> MenuSelectChangedEvent;
    }
}
