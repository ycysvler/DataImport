using DataImport.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// BetchItem.xaml 的交互逻辑
    /// </summary>
    public partial class BetchItem : UserControl
    {
        log4net.ILog log = log4net.LogManager.GetLogger("RollingLogFileAppender");

        string projectCode = "";
        string taskCode = "";
        string scriptCode = "";
        int times = 0;

        public BetchItem()
        {
            InitializeComponent();

            this.Loaded += BetchItem_Loaded;
        }
        FlowDocument fd = new FlowDocument();

        public event EventHandler CompleteEvent; 

        private void BetchItem_Loaded(object sender, RoutedEventArgs e)
        {
            outLog.Document = fd;
        }
         
        public void run() {
            

            log.Info(string.Format("BetchItem > fileName - {0} ********************", this.fileName.Text)); 

            string[] t1 = System.IO.Path.GetFileNameWithoutExtension(this.fileName.Text).Split('@');
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

                BetchLogic3 bl = new BetchLogic3(
                    MainWindow.UserID, MainWindow.UserName,
                    projectCode, taskCode, scriptCode, times, this.fileName.Text);

                log.Info(string.Format("BetchItem > projectCode:{0}  tackCode:{1}  scriptCode:{2}  times:{3}  fileName:{4}",
                    projectCode, taskCode, scriptCode, times, fileName.Text));

                bl.CompleteEvent += Bl_CompleteEvent;
                bl.MessageEvent += Bl_MessageEvent;

                Thread thread = new Thread(new ThreadStart(() =>
                {
                    if (bl.init()) {
                        try
                        {
                            bl.run();
                        }
                        catch (System.Exception ex)
                        {
                            Dispatcher.BeginInvoke((Delegate)new Action(() =>
                            {
                                Paragraph paragraph = new Paragraph();
                                Run runflg = new Run("数据导入失败\r\n" + ex.ToString());
                                runflg.Foreground = new SolidColorBrush(Colors.Red);
                                paragraph.Inlines.Add(runflg);
                                fd.Blocks.Add(paragraph);
                            }));
                            if (this.CompleteEvent != null)
                            {
                                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败!" });
                            }
                        }
                        finally {
                             
                        }
                    } 
                }));
                thread.Start();
            }

        }

        private void Bl_MessageEvent(object sender, MessageArgs e)
        {
            Dispatcher.BeginInvoke((Delegate)new Action(() =>
            {
                Paragraph paragraph = new Paragraph();
                Run runflg = new Run(e.Message["message"].ToString());
                if (Convert.ToBoolean(e.Message["code"]))
                {
                    runflg.Foreground = new SolidColorBrush(Colors.Green);
                }
                else {
                    runflg.Foreground = new SolidColorBrush(Colors.Red);
                }
                
                paragraph.Inlines.Add(runflg);
                fd.Blocks.Add(paragraph);

                if (fd.Blocks.Count > 100)
                {
                    fd.Blocks.Clear();
                } 
            }));
        }

        private void Bl_CompleteEvent(object sender, CompleteArgs e)
        {
            
            Dispatcher.BeginInvoke((Delegate)new Action(() =>
            {
                Paragraph paragraph = new Paragraph();
                Run runflg = new Run(e.Message);
                runflg.Foreground = new SolidColorBrush(Colors.Green);
                paragraph.Inlines.Add(runflg);
                fd.Blocks.Add(paragraph);

                var tr = new TextRange(fd.ContentStart, fd.ContentEnd);
                string log = tr.Text;

                string serverpath = string.Format(@"{0}\groupTrailDate\{1}", 
                    System.Configuration.ConfigurationManager.AppSettings["deliverpath"],
                         System.IO.Path.GetFileName(this.fileName.Text));

                ImportLogDAL.Insert(new DataAccess.Entitys.ImportLog() {
                    FileName = serverpath,
                    CreatedBy = MainWindow.UserName,
                    Content = tr.Text.Replace("\r\n", "<br />"),
                    ProjectCode = projectCode,
                    TaskCode = taskCode,
                    Times = times.ToString()
                });

                if (fd.Blocks.Count > 100)
                {
                    fd.Blocks.Clear();
                }

                if (this.CompleteEvent != null)
                {
                    this.CompleteEvent(this, null);
                }
            }));

            
        }
    }
}
