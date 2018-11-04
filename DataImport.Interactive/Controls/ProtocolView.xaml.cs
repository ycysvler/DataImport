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
    /// ProtocolView.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolView : UserControl
    {
        public ProtocolView()
        {
            InitializeComponent();
        }

        public string ProtocolName {
            get { return protocolName.Text; }
            set { protocolName.Text = value; }
        }
        public int Address { get; set; }
        public bool State
        {
            set
            {
                if (value)
                {
                    v_state.Text = "■"; 
                }
                else {
                    v_state.Text = "□"; 
                } 
            }
        }

    }
}
