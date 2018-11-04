using DataImport.BLL;
using DataImport.Interactive.Controls;
using HyCommBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TcpClientTest;

namespace DataImport.Interactive.Sequences
{
    /// <summary>
    /// SequencesView.xaml 的交互逻辑
    /// </summary>
    public partial class SequencesView : UserControl
    {
        FlowDocument fd = new FlowDocument();

        Dictionary<string, string> files = new Dictionary<string, string>();

        private CommandTable CurrentCommandTable = null;

        public SequencesView()
        {

            Dictionary<string, string> files = new Dictionary<string, string>();

            string basePath = AppDomain.CurrentDomain.BaseDirectory + @"data";

            foreach (var file in System.IO.Directory.GetFiles(basePath))
            {
                if (System.IO.Path.GetFileNameWithoutExtension(file).ToUpper().IndexOf("SOURCE") > -1) continue;
                if (System.IO.Path.GetExtension(file).ToUpper() != ".XML") continue;

                files.Add(System.IO.Path.GetFileNameWithoutExtension(file), file);
            }
            //files.Add("工步1", basePath + @"data\Sequence.xml");
            //files.Add("工步2", basePath + @"data\Sequence.xml");

            this.files = files;

            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += SequencesView_Loaded;

            this.Unloaded += SequencesView_Unloaded;

            popWindow.cancelEvent += popWindow_cancelEvent;
            popWindow.runEvent += popWindow_runEvent;

            stopWindow.cancelEvent += stopWindow_cancelEvent;
            stopWindow.runEvent += stopWindow_runEvent;

            MainWindow mw = Application.Current.MainWindow as MainWindow;
            mw.SequencesView = this;
        }

        public int RunState
        {
            get
            {
                if (this.CurrentTree == null)
                    return -2;
                return this.CurrentTree.RunState;
            }
        }

        public SequencesView(Dictionary<string, string> files)
        {
            this.files = files;

            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += SequencesView_Loaded;

            this.Unloaded += SequencesView_Unloaded;

            popWindow.cancelEvent += popWindow_cancelEvent;
            popWindow.runEvent += popWindow_runEvent;

            stopWindow.cancelEvent += stopWindow_cancelEvent;
            stopWindow.runEvent += stopWindow_runEvent;

            MainWindow mw = Application.Current.MainWindow as MainWindow;
            mw.SequencesView = this;
        }

        void stopWindow_runEvent(object sender, EventArgs e)
        {
            MainWindow.SequenceIsRunning = -1;
            m_TcpMain.stop();
            chart.Complete(); 
            CurrentTree.Abort();
            CurrentTree.Render();
            runButton.Visibility = Visibility.Visible;
            stopButton.Visibility = System.Windows.Visibility.Collapsed;
            pauseButton.Visibility = System.Windows.Visibility.Collapsed;
            
            pauseButton.Content = "暂停任务";
            foreach (var item in sts)
            {
                item.CanOrderBy = true;
            }
        }

        void stopWindow_cancelEvent(object sender, EventArgs e)
        {
            // 继续执行
            m_TcpMain.appdata.ProcessStopRun("02");
            MainWindow.SequenceIsRunning = 0;
        }

        public void Stop()
        { 
            m_TcpMain.stop();
            CurrentTree.Abort(); 
            pauseButton.Content = "暂停任务";
        }

        void SequencesView_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                m_TcpMain.stop();
            }
            catch (System.Exception ex) { }

        }

        List<Sequence> sequences = new List<Sequence>();
        List<TreeNodeView> tnvs = new List<TreeNodeView>();
        List<SequenceTree> sts = new List<SequenceTree>();

        private TcpClientAppMain m_TcpMain;


        int stsIndex = 0;

        private Sequence CurrentSequence { get { return sts[stsIndex].Sequence; } }
        private SequenceTree CurrentTree
        {
            get
            {
                if (stsIndex < sts.Count)
                    return sts[stsIndex];
                else return null;
            }
        }


        void SequencesView_Loaded(object sender, RoutedEventArgs e)
        {
            outLog.Document = fd;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var file in files)
            {
                Sequence s = new Sequence(file.Value, basePath + @"data\Source.xml");
                s.Name = file.Key;
                sequences.Add(s);
            }


            foreach (var sequence in sequences)
            {
                SequenceTree stree = new SequenceTree();
                stree.Sequence = sequence;
                stree.TreeOrderByEvent += stree_TreeOrderByEvent;
                stree.WaitEventHandler += stree_WaitEventHandler;
                stree.CommandEventHandler += stree_CommandEventHandler;
                stree.CompleteEventHandler += stree_CompleteEventHandler;
                sts.Add(stree);
                tree.Children.Add(stree);
                stree.Render();

            }

            //DrawChart();
            DrawProtocol();
            DrawRealData();

            m_TcpMain = new TcpClientAppMain();
            m_TcpMain.init(CurrentSequence);

            m_TcpMain.appdata.CmdIndexStatusEvent += appdata_CmdIndexStatusEvent;
            m_TcpMain.appdata.RecRt_DataEvent += appdata_RecRt_DataEvent;
            m_TcpMain.appdata.EquComm_StatusEnvent += appdata_EquComm_StatusEnvent;
            m_TcpMain.appdata.EquComm_MessageEnvent += appdata_EquComm_MessageEnvent;
            m_TcpMain.appdata.EquComm_RunStatusEnvent += appdata_EquComm_RunStatusEnvent;
        }

        void appdata_EquComm_RunStatusEnvent(object sender, TcpClientAppData.EquComm_RunStatusEnventArgs e)
        {
            switch (e.CommFlag)
            {
                case "02":
                    // 继续执行
                    m_TcpMain.appdata.ProcessStopRun("02");
                    MainWindow.SequenceIsRunning = 0;
                    if (CurrentTree.RunState == 1)
                        CurrentTree.RunState = 0;
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        pauseButton.Content = "暂停任务";
                    });
                    
                    chart.Pause = false;
                    break;
                case "01":
                    // 暂停
                    m_TcpMain.appdata.ProcessStopRun("01");
                    if (CurrentTree.RunState == 0)
                        CurrentTree.RunState = 1;
                    MainWindow.SequenceIsRunning = 1;
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        pauseButton.Content = "继续执行";
                    });
                   
                    chart.Pause = true; 
                    break;
                case "03":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        MessageBox.Show("通讯故障 程序中止");
                        stopWindow_runEvent(this, null);
                        
                    });
                    
                    break;
            }
        }



        void stree_TreeOrderByEvent(object sender, StringEventArgs e)
        {
            SequenceTree st = sender as SequenceTree;
            int index = sts.IndexOf(st);

            if (e.Message == "down")
            {
                if (index < sts.Count - 1)
                {
                    SequenceTree temp = sts[index];
                    sts[index] = sts[index + 1];
                    sts[index + 1] = temp;
                }
            }
            else
            {
                if (index > 0)
                {
                    SequenceTree temp = sts[index];
                    sts[index] = sts[index - 1];
                    sts[index - 1] = temp;
                }
            }

            tree.Children.Clear();
            foreach (var item in sts)
            {
                tree.Children.Add(item);
            }
        }




        private void DrawRealData()
        {
            rDataView.Sequence = CurrentSequence;
        }

        private void DrawProtocol()
        {
            Protocols.SetSequence(CurrentSequence);
        }

        private void DrawChart(List<Source> list)
        {
            chart.Sources = list;// CurrentSequence.Sources; //CurrentSequence.GetCycleSourceList();     //
            chart.ChartName = CurrentSequence.Name;
            chart.Render();


        }



        /// <summary>
        /// 通讯日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void appdata_EquComm_MessageEnvent(object sender, TcpClientAppData.EquComm_MessageEnventArgs e)
        {
            Dispatcher.BeginInvoke((Delegate)new Action(() =>
            {
                fd.Blocks.Add(new Paragraph(new Run(e.Message)));

                if (fd.Blocks.Count > 100)
                {
                    fd.Blocks.Clear();
                }
                if (e.ViewFlag)
                    tb_message.Text = e.Message;
            }));
        }
        /// <summary>
        /// 通讯状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void appdata_EquComm_StatusEnvent(object sender, TcpClientAppData.EquComm_StatusEnventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                Protocols.SetState(e.Address, e.CommFlag);
            });

        }
        /// <summary>
        /// 实时值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void appdata_RecRt_DataEvent(object sender, TcpClientAppData.RecRt_DataEnventArgs e)
        {
            Console.WriteLine(string.Format("RecRt_DataEvent:{0}\t{1}", e.ResID, e.Value));

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                //CurrentTree.SetTreeValue(e.ResID, e.Value.ToString());
                CurrentTree.SetTreeAcitve(e.ResID, true);
                rDataView.SetCurrentValue(e.ResID, e.Value.ToString(), DateTime.Now.ToShortTimeString());
            });
        } 

        void appdata_CmdIndexStatusEvent(object sender, TcpClientAppData.CmdIndexStatusEventArgs e)
        {
            if (e.CmdIndex == 0)
            {
                chart.run();
            }

            Console.WriteLine("index:{0}", e.CmdIndex);

            Thread thread = new Thread(new ThreadStart(() =>
            {
                var steps = CurrentCommandTable.GetStepList();
                int stepcount = CurrentCommandTable.Sources.Sum(it => it.Steps.Count);


                if (e.CmdIndex < steps.Count)
                {
                    int index = e.CmdIndex % stepcount;

                    var step = steps[index];

                    if (index == 0)
                    {
                        chart.setProgress(step.Time);
                    }
                    else
                    {
                        chart.Progress = step.Time;
                    }
                }

                CurrentTree.SetCycleStep(e.CmdIndex / stepcount + 1);

                if (e.CmdIndex + 1 >= steps.Count)
                {
                    var step = steps[e.CmdIndex];
                    if (step.HodeTime > 0) {
                        Thread.Sleep(step.HodeTime * 1000);
                    }
                    // 完事了
                    CurrentTree.RunState = 0;
                    CurrentTree.forTnv = null;
                }

            }));

            thread.Start();


        }
         
        #region 右上角三个单选按钮切换事件
        private void cb_report_Click(object sender, RoutedEventArgs e)
        {
            commFS.Visibility = System.Windows.Visibility.Visible;
            realData.Visibility = System.Windows.Visibility.Hidden;
            chartConter.Visibility = System.Windows.Visibility.Hidden;
            chartData.Visibility = System.Windows.Visibility.Hidden;
        }
        private void cb_chart_Click(object sender, RoutedEventArgs e)
        {
            commFS.Visibility = System.Windows.Visibility.Hidden;
            realData.Visibility = System.Windows.Visibility.Hidden;
            chartConter.Visibility = System.Windows.Visibility.Visible;
            chartData.Visibility = System.Windows.Visibility.Hidden;
        }
        private void cb_data_Click(object sender, RoutedEventArgs e)
        {
            commFS.Visibility = System.Windows.Visibility.Hidden;
            realData.Visibility = System.Windows.Visibility.Visible;
            chartConter.Visibility = System.Windows.Visibility.Hidden;
            chartData.Visibility = System.Windows.Visibility.Hidden;

        }

        private void cb_chartdata_Click(object sender, RoutedEventArgs e)
        {
            commFS.Visibility = System.Windows.Visibility.Hidden;
            realData.Visibility = System.Windows.Visibility.Hidden;
            chartConter.Visibility = System.Windows.Visibility.Hidden;
            chartData.Visibility = System.Windows.Visibility.Visible;

            // chartDataView.Sources = CurrentSequence.GetCycleSourceList();
        }
        #endregion

        #region 右下角两个大按钮事件
        /// <summary>
        /// 实时数据按钮事件（现在就是跑测试用）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monitorButton_Click(object sender, RoutedEventArgs e)
        { 
            appdata_CmdIndexStatusEvent(this, new TcpClientAppData.CmdIndexStatusEventArgs() { CmdIndex = aa });
            aa++;
             
        }
        int aa = 0;
        /// <summary>
        /// 启动执行按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SequenceIsRunning = 0;
            foreach (var item in sts)
            {
                item.CanOrderBy = false;
            }

            m_TcpMain.start();
            CurrentTree.Run();
            runButton.Visibility = System.Windows.Visibility.Hidden;
            pauseButton.Visibility = System.Windows.Visibility.Visible;
            stopButton.Visibility = System.Windows.Visibility.Visible; 
        }

        #endregion

        void stree_CompleteEventHandler(object sender, EventArgs e)
        {
            if (stsIndex + 1 < sequences.Count)
            {
                chart.Complete();
                stsIndex++;
                DrawRealData();
                CurrentTree.Run();
            }

            else
            {
                CurrentTree.RunState = -2;
            }
        }

        
        void stree_CommandEventHandler(object sender, CommandEventArgs e)
        {
            CurrentCommandTable = e.CommandTable;
            chartDataView.Sources = e.CommandTable.Sources;
            aa = 0;
            DrawChart(e.CommandTable.Sources);
            m_TcpMain.appdata.CreateCmdByCommandTable(e.CommandTable);
            
        } 
        /// <summary>
        /// tree wait事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void stree_WaitEventHandler(object sender, StringEventArgs e)
        {
            popWindow.Visibility = System.Windows.Visibility.Visible;
            popWindow.txtMessage.Text = e.Message;
        }
        /// <summary>
        /// 弹窗ok事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void popWindow_runEvent(object sender, EventArgs e)
        {
            CurrentTree.RunState = 0;
        }
        /// <summary>
        /// 弹窗cancel事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void popWindow_cancelEvent(object sender, EventArgs e)
        {
            CurrentTree.RunState = -1;
        }
         
        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.SequenceIsRunning == 0)
            {
                // 暂停
                m_TcpMain.appdata.ProcessStopRun("01"); 
                MainWindow.SequenceIsRunning = 1;
                pauseButton.Content = "继续执行"; 
            }
            else
            {
                // 继续
                m_TcpMain.appdata.ProcessStopRun("02");
                MainWindow.SequenceIsRunning = 0; 
                pauseButton.Content = "暂停任务"; 
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopWindow.Visibility = System.Windows.Visibility.Visible;
            m_TcpMain.appdata.ProcessStopRun("01");
            MainWindow.SequenceIsRunning = 1;
        }


    }
}
