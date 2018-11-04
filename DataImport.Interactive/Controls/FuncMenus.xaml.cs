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

namespace DataImport.Interactive.Controls
{
    /// <summary>
    /// FuncMenus.xaml 的交互逻辑
    /// </summary>
    public partial class FuncMenus : UserControl
    {
        int selectedIndex = 0;

        public FuncMenus()
        {
            InitializeComponent();

            menu0.MouseEnter += menu_MouseEnter;
            menu1.MouseEnter += menu_MouseEnter;
            menu2.MouseEnter += menu_MouseEnter;
            menu3.MouseEnter += menu_MouseEnter;
            menu4.MouseEnter += menu_MouseEnter;
            menu5.MouseEnter += menu_MouseEnter;
            
            menu0.MouseLeave += menu_MouseLeave;
            menu1.MouseLeave += menu_MouseLeave;
            menu2.MouseLeave += menu_MouseLeave;
            menu3.MouseLeave += menu_MouseLeave;
            menu4.MouseLeave += menu_MouseLeave;
            menu5.MouseLeave += menu_MouseLeave;

            menu0.MouseLeftButtonDown += menu0_MouseLeftButtonUp;
            menu1.MouseLeftButtonDown += menu1_MouseLeftButtonUp;
            menu2.MouseLeftButtonDown += menu2_MouseLeftButtonUp;
            menu3.MouseLeftButtonDown += menu3_MouseLeftButtonUp;
            menu4.MouseLeftButtonDown += menu4_MouseLeftButtonUp;
            menu5.MouseLeftButtonDown += menu5_MouseLeftButtonUp;

        }

        private bool checkRun() {
            bool result = true;
            MainWindow mw = App.Current.MainWindow as MainWindow;
            if (mw.SequencesView != null)
            {
                if (mw.SequencesView.RunState == 0 || mw.SequencesView.RunState == 1 || mw.SequencesView.RunState == 3)
                { 
                    result= false;
                }
                else
                {
                    result= true;
                }
            }
            return result;
        }

        void menu5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (checkRun())
            {
                selectedIndex = 5;
                MenuSelectChanged();
                menu_MouseLeave(null, null);
            }
        }

        void menu4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (checkRun())
            {
                selectedIndex = 4;
                MenuSelectChanged();
                menu_MouseLeave(null, null);
            }
        }

        void menu3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (checkRun())
            {
                selectedIndex = 3;
                MenuSelectChanged();
                menu_MouseLeave(null, null);
            }
        }

        void menu2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (checkRun())
            {
                selectedIndex = 2;
                MenuSelectChanged();
                menu_MouseLeave(null, null);
            }
        }

        void menu1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (checkRun())
            {
                selectedIndex = 1;
                MenuSelectChanged();
                menu_MouseLeave(null, null);
            }
        }

        void menu0_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (checkRun())
            {
                selectedIndex = 0;
                MenuSelectChanged();
                menu_MouseLeave(null, null);
            }
        }

        void menu_MouseLeave(object sender, MouseEventArgs e)
        {
            menu1.Opacity = 0.5;
            menu2.Opacity = 0.5;
            menu3.Opacity = 0.5;
            menu4.Opacity = 0.5;
            menu5.Opacity = 0.5;
            menu0.Opacity = 0.5;

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
            }
            if (selectedIndex == 4)
            {
                menu4.Opacity = 1;
            }
            if (selectedIndex == 5)
            {
                menu5.Opacity = 1;
            }
        }

        void menu_MouseEnter(object sender, MouseEventArgs e)
        {
            ((UIElement)sender).Opacity = 1;
        }

        private void MenuSelectChanged()
        {
            MenuSelectedEventArgs args = new MenuSelectedEventArgs() { SelectedIndex = this.selectedIndex };

            if (this.MenuSelectChangedEvent != null)
                MenuSelectChangedEvent(this, args); 
        }

        public event EventHandler<MenuSelectedEventArgs> MenuSelectChangedEvent;
    }

    public class MenuSelectedEventArgs : EventArgs
    {
        public int SelectedIndex { get; set; }
    }
}
