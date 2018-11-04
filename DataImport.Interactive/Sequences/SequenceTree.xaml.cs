using DataImport.Interactive.Controls;
using System;
using System.Collections.Generic;
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

namespace DataImport.Interactive.Sequences
{
    /// <summary>
    /// SequenceTree.xaml 的交互逻辑
    /// </summary>
    public partial class SequenceTree : UserControl
    {
        public SequenceTree()
        {
            InitializeComponent();
        }

        public Sequence Sequence { get; set; }

        public TreeNodeView forTnv { get; set; }

        List<TreeNodeView> tnvs = new List<TreeNodeView>();
        TreeNodeView tv;

        public void Render()
        {
            tnvs.Clear();
            tree.Children.Clear();

            tv = new TreeNodeView();
            tv.IsRoot = true;
            tv.i_down.MouseLeftButtonDown += i_down_MouseLeftButtonDown;
            tv.i_up.MouseLeftButtonDown += i_up_MouseLeftButtonDown;
            tnvs.Add(tv);
            tree.Children.Add(tv);
            tv.Title = Sequence.Name;
            tv.State = -1;

            AdapterTree(Sequence.Root, tv);
        }

        public bool IsCollapse
        {
            get { return tv.IsCollapse; }
            set { tv.IsCollapse = value; }
        }

        public bool CanOrderBy
        {
            set
            {
                if (value)
                {
                    tv.i_down.Visibility = System.Windows.Visibility.Visible;
                    tv.i_up.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    tv.i_down.Visibility = System.Windows.Visibility.Collapsed;
                    tv.i_up.Visibility = System.Windows.Visibility.Collapsed;
                }

            }
        }

        void i_down_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TreeOrderByEvent != null)
            {
                TreeOrderByEvent(this, new StringEventArgs() { Message = "down" });
            }
        }
        void i_up_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TreeOrderByEvent != null)
            {
                TreeOrderByEvent(this, new StringEventArgs() { Message = "up" });
            }
        }
        public event EventHandler<StringEventArgs> TreeOrderByEvent;

        void AdapterTree(TreeNode source, TreeNodeView parent)
        {
            foreach (var tn in source.Children)
            {
                if (tn.Type == "Source")
                    continue;

                TreeNodeView tnv = new TreeNodeView();
                tnv.ParentType = source.Type;

                tnv.TreeNode = tn;

                if (tn.Type == "For")
                {
                    forTnv = tnv;
                }

                tnvs.Add(tnv);
                parent.AddChild(tnv);
                tnv.Title = tn.Title;
                tnv.NodeType = tn.Type;
                tnv.Code = tn.Code;
                tnv.State = 0;
                AdapterTree(tn, tnv);
            }
        }

        private Thread thread;

        /// <summary>
        /// 0:正常执行，1：等待，2：执行完成，-1：结束,3:正在执行command
        /// </summary>
        public int RunState { get { return _runstate; } set { _runstate = value; } }
        public int _runstate = -2;

        public event EventHandler<StringEventArgs> WaitEventHandler;
        public event EventHandler<CommandEventArgs> CommandEventHandler;
        
        public event EventHandler CompleteEventHandler;

        public void Run()
        {
            RunState = 0;
            thread = new Thread(new ThreadStart(() =>
            {
                foreach (TreeNodeView tnv in tnvs)
                {

                    Console.WriteLine("{0}", tnv.NodeType);
                }

                foreach (TreeNodeView tnv in tnvs)
                {
                    Dispatcher.BeginInvoke((Delegate)new Action(() =>
                    {
                        tnv.State = 1;
                        SetTreeAcitve(tnv.Code, true);
                        Console.WriteLine("{0}\t{1}", tnv.Title, tnv.Code);

                    }));

                    switch (RunState)
                    {
                        case 0:
                            switch (tnv.NodeType)
                            {
                                case "Source": break;

                                case "For":
                                    forTnv = tnv; break;

                                case "Wait":
                                    Thread.Sleep(tnv.TreeNode.TimeOut * 1000);
                                    break;

                                case "InputPopup":
                                    if (WaitEventHandler != null)
                                    {
                                        Dispatcher.BeginInvoke((Delegate)new Action(() =>
                                        {
                                            WaitEventHandler(this, new StringEventArgs() { Message = tnv.TreeNode.PopupText });
                                        }));
                                        RunState = 1;
                                        // 循环等对话框确认
                                        while (RunState == 1)
                                        {
                                            Thread.Sleep(500);
                                        }
                                        // 确认为继续执行
                                        if (RunState == 0)
                                        {
                                            Dispatcher.BeginInvoke((Delegate)new Action(() =>
                                            {
                                                tnv.State = 1;
                                                SetTreeAcitve(tnv.Code, true);

                                            }));
                                        }
                                        // 确认为取消
                                        if (RunState == -1)
                                        {
                                            Dispatcher.BeginInvoke((Delegate)new Action(() =>
                                            {
                                                SetTreeAcitve(tnv.Code, false);

                                            }));
                                            return;
                                        }
                                    }
                                    break;
                                case "CommandTable":
                                    Dispatcher.BeginInvoke((Delegate)new Action(() =>
                                    {
                                        CommandEventHandler(this, new CommandEventArgs() { CommandTable = tnv.TreeNode.Tag as CommandTable });
                                    }));

                                    RunState = 3;

                                    // 循环等对话框确认
                                    while (RunState == 3)
                                    {
                                        Thread.Sleep(1000);
                                    }

                                    break;

                            }

                            break;
                        case 1:
                            break;
                        case -1:
                            return;
                    }
                    // 循环等对话框确认
                    while (RunState == 1)
                    {
                        Thread.Sleep(1000);
                    }
                    for (int i = 0; i < 10; i++)
                    { 
                        while (MainWindow.SequenceIsRunning != 0)
                        {
                            Thread.Sleep(100);
                            if (MainWindow.SequenceIsRunning == -1)
                            {
                                return;
                            }
                        }
                        Thread.Sleep(100);

                        i++;
                    }
                    

                }
                if (CompleteEventHandler != null)
                {
                    Dispatcher.BeginInvoke((Delegate)new Action(() =>
                    {
                        this.IsCollapse = true;
                        SetTreeAcitve(tnvs.Last().Code, false);
                        CompleteEventHandler(this, null);
                    }));
                }
            }));

            thread.Start();
        }

        public void Abort()
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
            }
        }

        public void SetCycleStep(int i)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                if (forTnv != null)
                {
                    For f = forTnv.TreeNode.Tag as For;
                    if (f != null)
                    {
                        forTnv.Title = string.Format("For({0}/{1})", i, f.LoopCount);
                    }
                }
            });

            clearSteps();
        }

        private void clearSteps()
        {
            foreach (var step in tnvs.Where(it => it.NodeType == "Step"))
            {
                step.State = 0;
            }
        }

        public event EventHandler RunCompleteEvent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resid"></param>
        /// <param name="value"></param>
        public void SetTreeValue(string resid, string value)
        {
            foreach (var item in tnvs)
            {
                if (item.Code == resid)
                    item.Value = value;
            }
        }

        public void SetTreeProgress(string resid)
        {
            foreach (var item in tnvs)
            {
                if (item.Code == resid)
                    item.State = 1;
            }
        }

        public void SetTreeAcitve(string resid, bool active)
        {
            foreach (var item in tnvs)
            {
                if (item.Code == resid)
                {
                    if (active)
                    {
                        item.IsActive = true;
                    }
                    else
                    {
                        item.IsActive = false;
                    }
                    break;
                }
                else
                {
                    item.IsActive = false;
                }
            }
        }
    }
    public class StringEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
    public class CommandEventArgs : EventArgs
    {
        public CommandTable CommandTable { get; set; }
    }

}
