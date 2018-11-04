using DataImport.Interactive.Controls;
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
    /// Protocols.xaml 的交互逻辑
    /// </summary>
    public partial class Protocols : UserControl
    {
        public Protocols()
        {
            InitializeComponent();
        }

        public void SetSequence(Sequence sequence) {
            sp_protocol.Children.Clear();
            foreach (var item in sequence.ProtocolItems) {
                ProtocolView pv = new ProtocolView();
                sp_protocol.Children.Add(pv);
                pv.Address = item.Address;
                pv.ProtocolName = item.Name;
                pv.State = false;
            }
        }

        public void SetState(int address, bool flag) {
            foreach (var element in sp_protocol.Children) {
                ProtocolView pv = element as ProtocolView;
                if (pv != null) {
                    if (pv.Address == address) {
                        pv.State = flag;
                    }
                }
            }
        }
 
    }
}
