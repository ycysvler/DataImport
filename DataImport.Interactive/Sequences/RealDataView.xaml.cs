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
    /// RealDataView.xaml 的交互逻辑
    /// </summary>
    public partial class RealDataView : UserControl
    {
        public RealDataView()
        {
            InitializeComponent();
        }

        public void SetCurrentValue(string resid, string value, string time) {
            var rdi = rdis.FirstOrDefault(it => it.ResID == resid);
            if (rdi != null) {
                rdi.ResValue = value;
                rdi.ResTime = time;
            }
        }

        private Sequence _sequence;
        public Sequence Sequence
        {

            set
            {
                rdis.Clear();
                root.Children.Clear(); 
                _sequence = value; 

                foreach (var proItem in value.ProtocolItems)
                {
                    RealDataGroup group = new RealDataGroup();
                    root.Children.Add(group);
                    group.GroupName = proItem.Name;

                    foreach (var info in _sequence.GetResourceInfoByAddress(proItem.Address))
                    {
                        RealDataItem item = new RealDataItem();
                        root.Children.Add(item);
                        rdis.Add(item);
                        item.ResID = info.ResID;
                        item.ResName = info.Name;
                    }
                }
            }
        }

        private List<RealDataItem> rdis = new List<RealDataItem>();
    }
}
