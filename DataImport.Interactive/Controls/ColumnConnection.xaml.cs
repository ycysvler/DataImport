using DataImport.DataAccess;
using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

namespace DataImport.Interactive.Controls
{
    /// <summary>
    /// ColumnConnection.xaml 的交互逻辑
    /// </summary>
    public partial class ColumnConnection : UserControl
    {
        const string TRANSFER_SCRIPT = @"# coding: utf-8
def TransferScript(obj):
    return obj";
        public ColumnConnection()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += ColumnConnection_Loaded;
        }

        public DataTable Source { get; set; }
        public List<Structure> Target { get; set; }

        public List<ColumnSourceItem> sourceItemList = new List<ColumnSourceItem>();
        public List<ColumnTargetItem> targetItemList = new List<ColumnTargetItem>();

        int sourcecount = 0, targetcount = 0;

        void ColumnConnection_Loaded(object sender, RoutedEventArgs e)
        {
            root.Children.Clear();

            showData();
            sourcecount = Source.Columns.Count;
            targetcount = Target.Count;

            int rowcount = sourcecount >= targetcount ? sourcecount : targetcount;

            root.Height = rowcount * 30 + 30;
            root.Width = 1000;

            lineConfig.btDelete.Click += btDelete_Click;
            lineConfig.btSave.Click += btSave_Click;
        }

        void btSave_Click(object sender, RoutedEventArgs e)
        {
            ScriptMap[lineConfig.ColumnName] = lineConfig.TransferScript.Text.Trim();
            lineConfig.Visibility = System.Windows.Visibility.Hidden;
        }

        void btDelete_Click(object sender, RoutedEventArgs e)
        {
            ColumnMap.Remove(lineConfig.ColumnName);
            ScriptMap.Remove(lineConfig.ColumnName);
            root.Children.Remove(lineConfig.currentLine);
            lineConfig.Visibility = System.Windows.Visibility.Hidden;
        }

        public void autoDrawLine()
        {

            ColumnMap.Clear();

            for (int i = root.Children.Count - 1; i >= 0; i--)
            {
                UIElement element = root.Children[i];
                if (element is Line)
                {
                    root.Children.Remove(element);
                }
            }


            for (int i = 0; i < Source.Columns.Count; i++)
            {
                string sourceName = Source.Columns[i].ColumnName;

                foreach (var item in Target)
                {
                    if (item.Comments.ToUpper() == sourceName.ToUpper())
                    {
                        ColumnMap[item.ColumnName] = sourceName;
                        drawLine(sourceName, item.ColumnName);
                    }
                }
            }
        }

        public bool  drawLine(string sourceName, string targetName)
        {
            Line line = new Line();
            line.StrokeStartLineCap = PenLineCap.Round;
            line.StrokeEndLineCap = PenLineCap.Round;
            line.StrokeDashCap = PenLineCap.Round; 
            line.StrokeThickness = 3;
            line.Stroke = new SolidColorBrush(Colors.Silver);
            

            sourceItem = sourceItemList.FirstOrDefault(it => it.ColumnName == sourceName);

            targetItem = targetItemList.FirstOrDefault(it => it.ColumnName == targetName);


            if (sourceItem == null || targetItem == null)
            { 
                return false;
            }
            

           

            root.Children.Add(line);

            line.X1 = getStartLocation().Width;
            line.Y1 = getStartLocation().Height;
            line.X2 = getEndLocation().Width;
            line.Y2 = getEndLocation().Height;
            line.Tag = targetName;

            line.MouseLeftButtonDown += line_MouseLeftButtonDown;
            line.MouseEnter += line_MouseEnter;
            line.MouseLeave += line_MouseLeave;

            return true;
        }

        void line_MouseLeave(object sender, MouseEventArgs e)
        {
            Line line = sender as Line;
            line.Stroke = new SolidColorBrush(Colors.Silver);
        }

        void line_MouseEnter(object sender, MouseEventArgs e)
        {
            Line line = sender as Line;
            line.Stroke = new SolidColorBrush(Colors.Red);
        }

        void line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Line line = sender as Line; 
            lineConfig.ColumnName = line.Tag.ToString();
            lineConfig.currentLine = line;

            lineConfig.TransferScript.Text = "";

            string transString = "";
            // 判断这个类型的解析器是否存在
            if (ScriptMap.ContainsKey(line.Tag.ToString()))
                transString = ScriptMap[line.Tag.ToString()];

            if (string.IsNullOrEmpty(transString))
            {
                transString = TRANSFER_SCRIPT;
            }

            lineConfig.TransferScript.Text = transString;

            lineConfig.Visibility = System.Windows.Visibility.Visible;
        }

        private void showData()
        {
            showSource();
            showTarget();

            Thread thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1000);
                Dispatcher.Invoke(new Action(() =>
                {
                    showLine();
                    if (ShowComplete != null) {
                        ShowComplete(this, null);
                    }
                }));
            }));
            thread.Start();

        }

        public string FID { get{ return _fid; } set { _fid = value;
               var  dScript = DataScriptDAL.getInfo(FID);
                TableName = dScript.TableName;
            } }
        private string _fid = "";

        private string TableName { get; set; }

        public event EventHandler ShowComplete;

        private void showLine()
        {
            List<DataScriptMap> maps = DataScriptMapDAL.getList(FID);

            foreach (var map in maps)
            {  
                if (this.drawLine(map.FileColName, map.TableColName))
                {
                    // 如果连线成功，才记录这个对应关系，不然就是文件与脚本不匹配
                    this.ColumnMap[map.TableColName] = map.FileColName;
                    this.ScriptMap[map.TableColName] = map.TransferScript;
                }
            }

            if (this.drawLine("时间", "COLUMN0")) {
                this.ColumnMap["COLUMN0"] = "时间";
            }
        }

        private void showTarget()
        {
            if (Target == null)
                return;

            Target.RemoveAll(it => it.ColumnName.ToUpper() == "FID" || it.ColumnName.ToUpper() == "PROJECTID");

            for (int i = 0; i < Target.Count; i++)
            {
                ColumnTargetItem item = new ColumnTargetItem();
                item.ColumnName = Target[i].ColumnName;
                item.ColumnValue = Target[i].Comments;
                item.DataType = Target[i].DataType;
                item.NullAble = Target[i].NullAble;
                item.cbpk.Click += cbpk_Click;
                item.Index = i;
                

                if (_businessPk.Contains(item.ColumnName))
                {
                    item.cbpk.IsChecked = true;
                }

                Canvas.SetTop(item, 30 * i);
                Canvas.SetLeft(item, 554);

                root.Children.Add(item);

                targetItemList.Add(item);
            }
        }

        void cbpk_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            foreach (var item in targetItemList) {
                if (item.cbpk.IsChecked.Value) {
                    count++;
                }
            }
            if (count == targetItemList.Count) {
                MessageBox.Show("所有字段都做联合主键了？");
            }
        }

        private void showSource()
        {

            for (int i = 0; i < Source.Columns.Count; i++)
            {
                ColumnSourceItem item = new ColumnSourceItem();
                item.ColumnName = Source.Columns[i].ColumnName;
                item.ColumnValue = Source.Rows[0][i].ToString();
                item.Index = i;

                item.MouseDown += ColumnSourceItem_MouseDown;

                Canvas.SetLeft(item, 0);
                Canvas.SetTop(item, 30 * i);

                root.Children.Add(item);

                sourceItemList.Add(item);
            }
        }

        Line tempLine;
        ColumnSourceItem sourceItem;
        ColumnTargetItem targetItem;


        private void root_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 已选中项，无反应
            // 未选中项，记录选中项 
        }

        private void root_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(root);
            // 已选中项，红框显示，无法形成链接
            VisualTreeHelper.HitTest(root, null, f =>
            {
                var element = f.VisualHit;

                if (element is Border)
                {
                    object tag = ((Border)element).Tag;
                    if (tag != null)
                    {
                        string columnName = tag.ToString();

                        var target = targetItemList.FirstOrDefault(item => item.ColumnName == columnName);

                        if (target != null && sourceItem != null)
                        {
                            targetItem = target;

                            if (!ColumnMap.ContainsKey(targetItem.ColumnName))
                            { 
                                drawLine(sourceItem.ColumnName, targetItem.ColumnName);
                                ColumnMap.Add(targetItem.ColumnName, sourceItem.ColumnName);

                                targetItem.ColumnValue = sourceItem.ColumnName;

                                TableDAL.Comment(TableName, targetItem.ColumnName, sourceItem.ColumnName);
                            }
                        }
                    }
                }

                return HitTestResultBehavior.Continue;
            }, new PointHitTestParameters(p));
            // 未选中项，记录选中项，画线 
            root.ReleaseMouseCapture();

            root.Children.Remove(tempLine);
            tempLine = null;
            sourceItem = null;

        }

        public Dictionary<string, string> ColumnMap = new Dictionary<string, string>();
        public Dictionary<string, string> ScriptMap = new Dictionary<string, string>();

        private string _businessPk = "";

        public string BusinessPK
        {
            get {
                _businessPk = "";

                foreach(var item in targetItemList){
                    if (item.cbpk.IsChecked.Value) {
                        _businessPk += item.ColumnName + ",";
                    }
                }
                 
                return _businessPk; }
            set { _businessPk = value; }
        } 

        private void root_MouseMove(object sender, MouseEventArgs e)
        {
            // 如果可以开始连线，画线
            if (sourceItem != null)
            {
                if (tempLine != null)
                {

                    // 可滚动区域高度
                    double height = scroll.ActualHeight; 

                    // 当前鼠标高度
                    double top = e.GetPosition(root).Y;
                    Debug.WriteLine(string.Format("scroll.ActualHeight:{0} ， e.GetPosition(root).Y:{1}", height , top));

                    double gridtop = e.GetPosition(grid).Y;

                    if (gridtop > height)
                    {
                        
                        //scroll.ScrollToVerticalOffset(top - height);
                        scroll.ScrollToVerticalOffset(scroll.ContentVerticalOffset+20);
                        Thread.Sleep(100);
                    }
                    if (gridtop < 0)
                    {
                        scroll.ScrollToVerticalOffset(scroll.ContentVerticalOffset - 20);
                        Thread.Sleep(100);
                    }

                    tempLine.X2 = e.GetPosition(root).X;
                    tempLine.Y2 = e.GetPosition(root).Y;

                    Debug.WriteLine(string.Format("x2:{0} , y2:{1}", tempLine.X2, tempLine.Y2));
                }
            }
        }

        private void ColumnSourceItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(root);
            sourceItem = sender as ColumnSourceItem;

            this.Cursor = Cursors.Hand;

            // 已选中项，无反应
            // 未选中项，记录选中项

            if (tempLine == null)
            {
                tempLine = new Line();
                tempLine.StrokeStartLineCap = PenLineCap.Round;
                tempLine.StrokeEndLineCap = PenLineCap.Round;
                tempLine.StrokeDashCap = PenLineCap.Round;
                //tempLine.StrokeDashArray = new DoubleCollection(new double[] { 3, 2 });
                tempLine.StrokeThickness = 3;
                tempLine.Stroke = new SolidColorBrush(Colors.Silver);
                root.Children.Add(tempLine);

                tempLine.X1 = getStartLocation().Width;
                tempLine.Y1 = getStartLocation().Height;
                tempLine.X2 = e.GetPosition(root).X;
                tempLine.Y2 = e.GetPosition(root).Y;

                Debug.WriteLine(string.Format("x1:{0} , y1:{1}", tempLine.X1, tempLine.Y1));
            }
        }

        private void ColumnTargetItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            targetItem = sender as ColumnTargetItem;
            tempLine.X2 = getEndLocation().Width;
            tempLine.Y2 = getEndLocation().Height;

            sourceItem = null;
            targetItem = null;
            tempLine = null;
        }

        private Size getStartLocation()
        {
            Size result = new Size();

            result.Width = Canvas.GetLeft(sourceItem) + sourceItem.ActualWidth;
            result.Height = Canvas.GetTop(sourceItem) + sourceItem.ActualHeight / 2;

            return result;
        }

        private Size getEndLocation()
        {
            Size result = new Size();

            result.Width = Canvas.GetLeft(targetItem);
            result.Height = Canvas.GetTop(targetItem) + targetItem.ActualHeight / 2;

            return result;
        }
    }
}
