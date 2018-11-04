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
using Visifire.Charts;

namespace DataImport.Interactive.Sequences
{
    /// <summary>
    /// ChartView.xaml 的交互逻辑
    /// </summary>
    public partial class ChartView : UserControl
    {
        // 这个chart上的最大时间，在设置sources时候计算
        private int _maxTime = 0;
        // 这个chart上的最大Y值，在设置sources时候计算
        private double _maxY = 0;

        public ChartView()
        {
            InitializeComponent();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += ChartView_Loaded;
        }

        public void Complete()
        {
            Progress = 0;
            root.Children.Clear();
            if (threadTest1 != null && threadTest1.IsAlive)
            {
                threadTest1.Abort();
            }
        }



        public void run()
        {
            threadTest1 = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
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
                    }
                    Progress++;
                }
            }));
            threadTest1.Start();
        }



        public void Render()
        {
            DateTime begin = DateTime.Now;


            if (threadTest1 != null)
            {
                threadTest1.Abort();
                threadTest1 = null;
            }

            root.Children.Clear();
            //创建一个图标
            Chart chart = new Chart();


            chart.DataPointWidth = 0.2;
            //chart.Background = 

            //设置图标的宽度和高度
            //是否启用打印和保持图片
            chart.ToolBarEnabled = false;
            chart.BorderThickness = new Thickness(0);

            //设置图标的属性
            chart.ScrollingEnabled = false;//是否启用或禁用滚动
            chart.View3D = false;//3D效果显示

            //创建一个标题的对象
            Title title = new Title();

            //设置标题的名称
            title.Text = ChartName;
            title.Padding = new Thickness(0, 10, 5, 0);

            //向图标添加标题
            chart.Titles.Add(title);

            Axis yAxis = new Axis();
            //设置图标中Y轴的最小值永远为0          
            yAxis.AxisMinimum = 0;
            //设置图表中Y轴的后缀          
            //yAxis.Suffix = "斤";
            chart.AxesY.Add(yAxis);

            for (int i = 0; i < _sources.Count; i++)
            {
                var source = _sources[i];
                if (source.Steps[0].TargetValue > 1)
                    source.Line = true;



                DataSeries dataSeries = new DataSeries();
                dataSeries.Name = source.Name;

                // 0 : 模拟量， 1：开关量
                if (source.DigitType == 0)
                    dataSeries.RenderAs = RenderAs.Line;
                else
                    dataSeries.RenderAs = RenderAs.Bubble;

                DataPoint dataPoint;

                double pre_value = 0;

                for (int s = 0; s < source.Steps.Count; s++)
                {
                    var step = source.Steps[s];
                    Console.WriteLine("{0}:value（{1}）\tpretime（{2}）\tchangetime（{3}）\thodetime（{4}）\ttime（{5}）",
                       source.ResID, step.TargetValue, step.PreTime, step.ChangeTime, step.HodeTime, step.Time);

                    double y = 0;
                    // 补changeTime点逻辑
                    if (source.DigitType == 0)
                    {
                        //if (step.ChangeTime > 0)
                        //{
                        DataPoint changePrePoint = new DataPoint();
                        changePrePoint.MarkerScale = 1.5;
                        changePrePoint.XValue = step.Time;// -step.ChangeTime;

                        y = pre_value;

                        changePrePoint.YValue = y;

                        changePrePoint.ToolTipText = string.Format("[{2}] , [时间 : {0}] , [目标值 : {1}]", step.Time, y, source.Name);

                        dataSeries.DataPoints.Add(changePrePoint);
                        //}
                    }

                    dataPoint = new DataPoint();
                    dataPoint.MarkerScale = 1.1;
                    dataPoint.XValue = step.Time + step.ChangeTime;

                    y = step.TargetValue;
                    //y = y == 0 ? 2 : y;
                    if (source.DigitType != 0)
                    {
                        y = y == 0 ? _maxY * 0.2 : _maxY * 0.5;
                    }
                    dataPoint.YValue = y;
                    dataPoint.ToolTipText = string.Format("[{2}] , [时间 : {0}] , [目标值 : {1}]", step.Time + step.ChangeTime, y, source.Name);

                    if (step.TargetValue == 1.0f && source.DigitType == 1)
                    {
                        dataPoint.ToolTipText = string.Format("[{2}] , [时间 : {0}] , [目标值 : {1}]", step.Time + step.ChangeTime, "开", source.Name);
                    }
                    if (step.TargetValue == 0 && source.DigitType == 1)
                    {
                        dataPoint.ToolTipText = string.Format("[{2}] , [时间 : {0}] , [目标值 : {1}]", step.Time + step.ChangeTime, "关", source.Name);
                    }
                    dataSeries.DataPoints.Add(dataPoint);

                    pre_value = step.TargetValue;

                    if (source.DigitType == 0 && s == source.Steps.Count - 1)
                    {
                        DataPoint changePrePoint = new DataPoint();
                        changePrePoint.MarkerScale = 1.5;
                        changePrePoint.XValue = step.Time + step.HodeTime;

                        y = step.TargetValue;

                        changePrePoint.YValue = y;

                        changePrePoint.ToolTipText = string.Format("[{2}] , [时间 : {0}] , [目标值 : {1}]", step.Time + step.HodeTime, y, source.Name);

                        dataSeries.DataPoints.Add(changePrePoint);

                    }

                }
                chart.Series.Add(dataSeries);
            }

            timeSeries.Name = "进度";

            timePoint.XValue = 0;
            timePoint.YValue = _maxY + 5;
            timeSeries.DataPoints.Add(timePoint);
            chart.Series.Add(timeSeries);

            chart.Rendered += chart_Rendered;

            if (_maxTime > 60)
                chart.ZoomingEnabled = true;

            root.Children.Add(chart);

            DateTime end = DateTime.Now;

            Console.WriteLine((end - begin).TotalMilliseconds);
        }

        DataSeries timeSeries = new DataSeries();
        DataPoint timePoint = new DataPoint();

        private float _progress;



        public void setProgress(float value)
        {
            _progress = value;

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                timePoint.XValue = _progress;
            });
        }

        public float Progress
        {
            get { return _progress; }
            set
            {
                if (value > _progress && value <= _maxTime)
                {
                    _progress = value;

                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        timePoint.XValue = _progress;
                    });
                }
            }
        }

        void chart_Rendered(object sender, EventArgs e)
        {
            var chart = sender as Chart;
            chart.Legends[0].HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            chart.Legends[0].VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
        }
        void ChartView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public bool Pause { get; set; }

        Thread threadTest1;

        List<Source> _sources = new List<Source>();

        public string ChartName { get; set; }

        public List<Source> Sources
        {
            get { return _sources; }
            set
            {
                _sources = value;
                Pause = false;

                List<Step> result = new List<Step>();

                _maxTime = 0;

                foreach (var source in Sources)
                {
                    foreach (var step in source.Steps)
                    {
                        Step nstep = new Step();
                        nstep.ChangeTime = step.ChangeTime;
                        nstep.HodeTime = step.HodeTime;
                        nstep.PreTime = step.PreTime;
                        nstep.Source = step.Source;
                        nstep.TargetValue = step.TargetValue;
                        nstep.Time = step.Time + step.HodeTime;
                        result.Add(nstep);
                    }
                }
                _maxTime = result.Max(it => it.Time);
                _maxY = result.Max(it => it.TargetValue);
            }

        }
    }
}
