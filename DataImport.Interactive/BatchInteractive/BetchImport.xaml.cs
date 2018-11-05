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

namespace DataImport.Interactive.BatchInteractive
{
    /// <summary>
    /// BetchImport.xaml 的交互逻辑
    /// </summary>
    public partial class BetchImport : UserControl
    {
        public BetchImport()
        {
            InitializeComponent();
        }

        List<BetchItemFlg> flgs = new List<BetchItemFlg>();

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog m_Dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();

            addButton.Visibility = Visibility.Hidden;
             
            foreach (string file in System.IO.Directory.GetFiles(m_Dir))
            {
                if (System.IO.Path.GetFileName(file)[0] == '.') {
                    // 这是一个隐藏文件
                    continue;   
                }

                string ext = System.IO.Path.GetExtension(file);
                if (ext == ".txt" || ext == ".xls" || ext == ".xlsx" || ext == ".mdb" || ext == ".dat")
                {
                    string projectCode = "";
                    string taskCode = "";
                    string scriptCode = "";
                    int times = 0;

                    string[] t1 = System.IO.Path.GetFileNameWithoutExtension(file).Split('@');
                    if (t1.Length > 1)
                    {
                        string[] t2 = t1[1].Split(',');
                        if (t2.Length > 3)
                        {
                            projectCode = t2[0];
                            taskCode = t2[1];
                            scriptCode = t2[2];
                            int.TryParse(t2[3], out times);
                        }

                        BetchItem item = new BetchItem();
                        item.fileName.Text = file;
                        item.CompleteEvent += Item_CompleteEvent;
                        panel.Children.Add(item);

                        flgs.Add(new BetchItemFlg() { Item = item });
                    }
                }
            }

            if (flgs.Count > 0)
            {
                flgs[0].Item.run();
            }
        }

        private void Item_CompleteEvent(object sender, EventArgs e)
        { 
            Dispatcher.BeginInvoke((Delegate)new Action(() =>
            {
                Console.WriteLine("complete : {0}", (sender as BetchItem).fileName.Text);

                foreach (var item in flgs)
                {
                    if (item.Item == sender)
                    {
                        item.Flg = 2;
                    }
                }

                foreach (var item in flgs)
                {
                    if (item.Flg == 0)
                    {
                        item.Item.run();
                        return;
                    }
                }

                addButton.Visibility = Visibility.Visible;

            }));

            
        }

        class BetchItemFlg {
            public BetchItem Item{get;set;}
            /// <summary>
            /// 0 等待执行，1 执行中， 2 完成
            /// </summary>
            public int Flg { get; set; }
        }
    }
}
