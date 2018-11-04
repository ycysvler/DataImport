/// **************************************************************************************
/// 通信协议基类：
/// 所有的上行通信协议和下行通信协议均应派生自此类
/// **************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using HyUtilities;
using System.Threading;


namespace HyCommBase
{
    public abstract class ProtocolBase
    {
        #region 构造函数
        /// <summary>
        /// 通信协议基类：
        /// 所有的上行通信协议和下行通信协议均应派生自此类
        /// </summary>
        protected ProtocolBase()
        {

        }

        /// <summary>
        /// 通信协议基类：
        /// 下行通信协议派生类可用此构造函数
        /// </summary>
        /// <param name="HardAddress">
        /// 下行通信协议派生类使用：
        /// 以硬件地址为索引，(子站设备号,通信状态)为内容的地址字典
        /// </param>
        /// <param name="WaitTick">
        /// 用于需要等待返回的报文：
        /// 超时判断毫秒数
        /// </param>
        /// <param name="PollingInterval">
        /// 轮询间隔的周期（毫秒数）
        /// 每一个周期启动一次轮询(可能有多个轮询报文)
        /// 0表示不需要轮询
        /// </param>
        /// <param name="timerDriverInteval">
        /// 协议定时驱动计时器的周期,ms
        /// </param>
        /// <param name="CommIndex">
        /// 该协议定义的通信端口的序号（从0开始）
        /// </param>
        protected ProtocolBase(Dictionary<int, DeviceCommStatus> HardAddress, int WaitTick, int PollingInterval,
            int timerDriverInteval, int CommIndex, bool _AutoCheckCommFlag)
        {
            this.m_HardAddress = HardAddress;
            this.m_WaitTick = WaitTick;
            this.m_PollingInterval = PollingInterval;
            this.m_timerDriverInteval = timerDriverInteval;
            this.timerDriver = new System.Timers.Timer(timerDriverInteval);
            this.timerDriver.AutoReset = true;
            this.timerDriver.Enabled = false;
            this.timerDriver.Elapsed += new System.Timers.ElapsedEventHandler(Main_CommDriver);

            this.m_CommIndex = CommIndex;
            this.AutoCheckCommFlag = _AutoCheckCommFlag;

            if (PollingInterval > 0) Polling_Init();
        }

        /// <summary>
        /// 通信协议基类：
        /// 上行通信协议派生类可用此构造函数
        /// </summary>
        /// <param name="RTUAddress">
        /// 上行通信协议派生类使用：
        /// RTU站地址
        /// </param>
        /// <param name="WaitTick">
        /// 用于需要等待返回的报文：
        /// 超时判断毫秒数
        /// </param>
        /// <param name="PollingInterval">
        /// 轮询间隔的周期（毫秒数）
        /// 每一个周期启动一次轮询(可能有多个轮询报文)
        /// 0表示不需要轮询
        /// </param>
        /// <param name="timerDriverInteval">
        /// 协议定时驱动计时器的周期,ms
        /// </param>
        /// <param name="CommIndex">
        /// 该协议定义的通信端口的序号（从0开始）
        /// </param>
        protected ProtocolBase(int RTUAddress, int WaitTick, int PollingInterval, int timerDriverInteval, int CommIndex, bool _AutoCheckCommFlag)
        {
            this.m_RTUAddress = RTUAddress;
            this.m_WaitTick = WaitTick;
            this.m_PollingInterval = PollingInterval;
            this.m_timerDriverInteval = timerDriverInteval;
            this.timerDriver = new System.Timers.Timer(timerDriverInteval);
            this.timerDriver.AutoReset = true;
            this.timerDriver.Enabled = false;
            this.timerDriver.Elapsed += new System.Timers.ElapsedEventHandler(Main_CommDriver);
            this.m_CommIndex = CommIndex;

            this.AutoCheckCommFlag = _AutoCheckCommFlag;

            if (PollingInterval > 0) Polling_Init();
        }
        #endregion

        #region 属性和定义

        /// <summary>
        /// 用于显示通信报文的委托
        /// </summary>
        /// <param name="CommIndex">通信端口的序号</param>
        /// <param name="item">通信报文帧结构</param>
        public delegate void MessageEventDelegate(int CommIndex, FrameQueueItem item);

        public virtual event MessageEventDelegate MessageEvent;

        /// <summary>
        /// 该协议定义的通信端口的序号（从0开始）
        /// </summary>
        protected int m_CommIndex;

        /// <summary>
        /// 与通信协议类绑定的通信类
        /// </summary>
        public CommBase m_Comm;

        /// <summary>
        /// 仅作为下行通信协议时使用
        /// 以硬件地址为索引，(子站设备号,通信状态)为内容的地址字典
        /// </summary>
        public Dictionary<int, DeviceCommStatus> m_HardAddress;

        /// <summary>
        /// 通信定时驱动是否启动的标志
        /// </summary>
        public bool DriverStartFlag;

        /// <summary>
        /// 协议通信的状态，用于上行通信协议（以RTUAddress构造的）
        /// </summary>
        protected bool m_CommStatus;

        /// <summary>
        /// 报文失败计数，用于上行通信协议（以RTUAddress构造的）
        /// 若连续5个发送报文没有正确回复，则认为该设备通信错误
        /// 一旦正确回复了一个报文，则该计数清零，并认为该设备通信正常
        /// </summary>
        protected int m_ErrorCount = 0;

        /// <summary>
        /// 1、如果是下行通信协议（以HardAddress构造的）——表示所有子站的协议通信状态
        ///     如果所有子站均协议通信正常，则为true
        ///     如果有一个子站协议通信故障，则为false
        /// 2、如果是上行通信协议（以RTUAddress构造的）——表示该设备的协议通信状态
        /// </summary>
        public bool CommStatus
        {
            get
            {
                if (m_HardAddress != null)
                {
                    foreach (DeviceCommStatus var in m_HardAddress.Values)
                    {
                        if (!var.CommStatus) return false;
                    }
                    return true;
                }
                else
                {
                    return m_CommStatus;
                }
            }
            set
            {
                if (m_HardAddress != null)
                {
                    foreach (DeviceCommStatus var in m_HardAddress.Values)
                    {
                        var.CommStatus = value;
                    }
                }
                else
                {
                    m_CommStatus = value;
                }
            }
        }

        /// <summary>
        /// True——作为协议主站
        /// False——作为协议从站
        /// </summary>
        protected bool MasterFlag;

        /// <summary>
        /// 协议定时驱动计时器
        /// </summary>
        protected System.Timers.Timer timerDriver;
        /// <summary>
        /// 协议定时驱动计时器的周期,ms
        /// </summary>
        protected int m_timerDriverInteval;

        /// <summary>
        /// RTU站地址
        /// </summary>
        public int m_RTUAddress;

        /// <summary>
        /// 自动检查通信并试图恢复的标志
        /// </summary>
        public bool AutoCheckCommFlag;

        /// <summary>
        /// 用于需要等待返回的报文：
        /// 超时判断毫秒数
        /// </summary>
        protected int m_WaitTick;

        /// <summary>
        /// 发送缓冲区的队列
        /// </summary>
        protected Queue<ProtocolFrameStruct> SendCache = new Queue<ProtocolFrameStruct>();
        /// <summary>
        /// 发送缓冲区队列的当前数量
        /// </summary>
        public int SendCacheCount
        {
            get { return SendCache.Count; }
        }
        /// <summary>
        /// 发送缓冲区的当前帧
        /// </summary>
        protected ProtocolFrameStruct se;

        /// <summary>
        /// 接收缓冲区的队列
        /// </summary>
        protected Queue<ProtocolFrameStruct> RecvCache = new Queue<ProtocolFrameStruct>();
        /// <summary>
        /// 接收缓冲区队列的当前数量
        /// </summary>
        public int RecvCacheCount
        {
            get { return RecvCache.Count; }
        }
        /// <summary>
        /// 接收缓冲区的当前帧
        /// </summary>
        protected ProtocolFrameStruct re;

        /// <summary>
        /// 发送缓冲区锁
        /// </summary>
        protected object SendCacheLock = new object();
        /// <summary>
        /// 接收缓冲区锁
        /// </summary>
        protected object RecvCacheLock = new object();

        /// <summary>
        /// 用于同步处理的通信状态类
        /// </summary>
        public ProtocolSyncStatus SyncStatus = new ProtocolSyncStatus();

        #endregion

        #region 通信驱动
        /// <summary>
        /// 用于显示通信报文
        /// </summary>
        /// <param name="CommIndex"></param>
        /// <param name="item"></param>
        public void ShowMessage(int CommIndex, FrameQueueItem item)
        {
            if (MessageEvent != null)
                MessageEvent(CommIndex, item);
        }

        /// <summary>
        /// 启动协议驱动计时器循环
        /// </summary>
        public virtual void Main_Start()
        {
            this.timerDriver.Enabled = true;
            DriverStartFlag = true;
        }

        /// <summary>
        /// 停止协议驱动计时器循环
        /// </summary>
        public virtual void Main_Stop()
        {
            this.timerDriver.Enabled = false;
            DriverStartFlag = false;
        }

        /// <summary>
        /// 协议定时驱动Main_CommDriver的防止重入标志
        /// </summary>
        protected object driverlock = new object();

        /// <summary>
        /// 标准的通信协议驱动，由定时器调用
        /// </summary>
        public virtual void Main_CommDriver(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(driverlock))
                return;

            try
            {
             
                //先处理接收
                Main_CommDriver_Recv();

                //处理轮询，或处理同步
                if (this.m_PollingInterval > 0) Polling_Driver();
                else Sync_Driver();

                //再处理发送
                Main_CommDriver_Send();
            }
            catch (Exception ex)
            {
                string aStr = ex.Message;
                //System.Windows.Forms.MessageBox.Show(aStr);
            }
            finally
            {
                Monitor.Exit(driverlock);
            }
        }

        /// <summary>
        /// 标准的通信协议接收处理驱动
        /// </summary>
        public virtual void Main_CommDriver_Recv()
        {
            try
            {
                // 把接收缓冲区中所有的报文都处理完
                while (RecvCache.Count > 0)
                {
                    re = RecvCache.Peek();
                    if (Frame_Analysis())
                    {
                        ///协议通信正常
                        if (m_HardAddress != null)
                        {
                            if (m_HardAddress.ContainsKey(re.Address))
                            {
                                m_HardAddress[re.Address].ErrorCount = 0;
                                m_HardAddress[re.Address].CommStatus = true;
                            }
                        }
                        else
                        {
                            this.m_CommStatus = true;
                            this.m_ErrorCount = 0;
                        }
                    }
                    else
                    {
                    }
                    lock (RecvCacheLock)
                    {
                        RecvCache.Dequeue();
                    }
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("方法Main_CommDriver_Recv()出现了错误！" + ex.Message);
            }
        }

        /// <summary>
        /// 标准的通信协议发送处理驱动
        /// </summary>
        public virtual void Main_CommDriver_Send()
        {
            try
            {
                // 将发送缓冲区中的第一个命令发送出去
                if (SendCache.Count > 0)
                {
                    se = SendCache.Peek();
                    if (se.WaitingFlag)
                    {
                        //当前命令已经发送，正在等待
                        Main_CommDriver_Resend();
                    }
                    else
                    {
                        //发送一个新的命令
                        se.SendTick = HyTick.TickTimeGet();
                        Main_SendCmd();

                        if (se.NeedReply)
                        {
                            // 需要等待回复的报文：应该置标志并等待接收、重发等处理
                            se.WaitingFlag = true;
                        }
                        else
                        {
                            // 不需要等待回复的报文：应该直接从发送缓冲区清除
                            lock (SendCacheLock)
                            {
                                SendCache.Dequeue();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("方法Main_CommDriver_Send()出现了错误！" + ex.Message);
            }

        }

        /// <summary>
        /// 标准的通信协议重发处理
        /// </summary>
        public virtual void Main_CommDriver_Resend()
        {
            if (HyTick.TickTimeIsArrived(se.SendTick, this.m_WaitTick))
            {
                //判断该子站通信是否出现问题
                if (m_HardAddress != null)
                {
                    if (m_HardAddress.ContainsKey(se.Address))
                    {
                        if (m_HardAddress[se.Address].ErrorCount < 5) m_HardAddress[se.Address].ErrorCount++;
                        if (m_HardAddress[se.Address].ErrorCount >= 5) m_HardAddress[se.Address].CommStatus = false;
                    }
                }
                else
                {
                    if (this.m_ErrorCount < 5) this.m_ErrorCount++;
                    if (this.m_ErrorCount >= 5) this.m_CommStatus = false;
                }
                if (this.IsPolling(se.AFN))
                {
                    //轮询的报文不需要重发处理，可等待下一次轮询
                    if (SendCache.Count > 0)
                    {
                        lock (SendCacheLock)
                        {
                            SendCache.Dequeue();
                        }
                    }
                }
                else
                {
                    if (se.ResendCount >= CommConst.MAXRESEND)
                    {
                        //超过最大重发次数，当前命令失败，从发送缓冲区清除
                        if (SendCache.Count > 0)
                        {
                            lock (SendCacheLock)
                            {
                                SendCache.Dequeue();
                            }
                        }
                    }
                    else
                    {
                        //重发当前命令
                        se.ResendCount++;
                        se.SendTick = HyTick.TickTimeGet();

                        Main_SendCmd();
                    }
                }
            }
        }

        /// <summary>
        /// 将命令发送到发送缓冲区
        /// </summary>
        /// <param name="Address">
        /// 地址：
        /// 作为下行通信协议，应填写设备的硬件地址
        /// 作为上行通信协议，可不填
        /// </param>
        /// <param name="AFN">功能码</param>
        /// <param name="DataArea">
        /// 包括帧头、帧尾（但不包括校验）的完整报文数据
        /// important:根据协议的具体实现有所不同
        /// </param>
        /// <param name="NeedReply">该报文是否需要回复</param>
        /// <param name="IPAddr">
        /// 通信数据目标的IP地址：仅供TCP Server通信方式使用
        /// </param>
        /// <returns></returns>
        public virtual bool Main_SendToCache(int Address, int AFN, byte[] DataArea, bool NeedReply, string IPAddr)
        {
            /// 通信未连接则不发送到缓冲区
            if (!this.m_Comm.Connected) return false;

            ProtocolFrameStruct se1 = new ProtocolFrameStruct(Address, AFN);
            SyncStatus.Success = false;
            try
            {
                se1.DataArea = DataArea;
                se1.NeedReply = NeedReply;
                se1.IPAddr = IPAddr;

                se1.WaitingFlag = false;
                se1.ResendCount = 0;

                lock (SendCacheLock)
                {
                    SendCache.Enqueue(se1);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual bool Main_SendToCache(int Address, int AFN, byte[] DataArea, bool NeedReply, string IPAddr, int HardAddr)
        {
            /// 通信未连接则不发送到缓冲区
            if (!this.m_Comm.Connected) return false;

            ProtocolFrameStruct se1 = new ProtocolFrameStruct(Address, AFN, HardAddr);
            SyncStatus.Success = false;
            try
            {
                se1.DataArea = DataArea;
                se1.NeedReply = NeedReply;
                se1.IPAddr = IPAddr;

                se1.WaitingFlag = false;
                se1.ResendCount = 0;

                lock (SendCacheLock)
                {
                    SendCache.Enqueue(se1);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将命令发送到发送缓冲区
        /// </summary>
        /// <param name="Address">
        /// 地址：
        /// 作为下行通信协议，应填写设备的硬件地址
        /// 作为上行通信协议，可不填
        /// </param>
        /// <param name="AFN">功能码</param>
        /// <param name="DataArea">
        /// 包括帧头、帧尾（但不包括校验）的完整报文数据
        /// important:根据协议的具体实现有所不同
        /// </param>
        /// <param name="NeedReply">该报文是否需要回复</param>
        /// <param name="IPAddr">
        /// 通信数据目标的IP地址：仅供TCP Server通信方式使用
        /// </param>
        /// <param name="sync">
        /// 用于同步通信的通信状态控制：比如用于顺序发送文件等。平常的数据通信不使用此重载
        /// </param>
        /// <returns></returns>
        public virtual bool Main_SendToCache(int Address, int AFN, byte[] DataArea, bool NeedReply, string IPAddr,
            ref ProtocolSyncStatus sync)
        {
            /// 通信未连接则不发送到缓冲区
            if (!this.m_Comm.Connected) return false;

            ProtocolFrameStruct se1 = new ProtocolFrameStruct(Address, AFN);
            SyncStatus.Success = false;

            try
            {
                se1.DataArea = DataArea;
                se1.NeedReply = NeedReply;
                se1.IPAddr = IPAddr;

                se1.WaitingFlag = false;
                se1.ResendCount = 0;

                lock (SendCacheLock)
                {
                    SendCache.Enqueue(se1);
                }

                sync = SyncStatus;
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// 将一个完整的接收报文发送到接收缓冲区等待处理
        /// </summary>
        /// <param name="Address">
        /// 地址：
        /// 作为下行通信协议，应填写设备的硬件地址
        /// 作为上行通信协议，应填写RTU的站地址
        /// </param>
        /// <param name="AFN">功能码</param>
        /// <param name="DataArea">包括帧头、帧尾（但不包括校验）的完整报文数据</param>
        /// <param name="IPAddr">
        /// 通信数据目标的IP地址：仅供TCP Server通信方式使用
        /// </param>
        /// <returns></returns>
        public virtual bool Main_RecvToCache(int Address, int AFN, byte[] DataArea, string IPAddr)
        {
            ProtocolFrameStruct re1 = new ProtocolFrameStruct(Address, AFN);
            try
            {
                re1.DataArea = DataArea;
                re1.IPAddr = IPAddr;

                lock (RecvCacheLock)
                {
                    RecvCache.Enqueue(re1);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 清空发送和接受缓冲区
        /// </summary>
        public virtual void ClearCache()
        {
            lock (SendCacheLock)
            {
                SendCache.Clear();
            }
            lock (RecvCacheLock)
            {
                RecvCache.Clear();
            }
        }

        /// <summary>
        /// 根据AFN判断是否轮循或同步的报文
        /// </summary>
        /// <param name="AFN"></param>
        /// <returns></returns>
        public virtual bool IsPolling(int AFN)
        {
            return true;
        }

        /// <summary>
        /// 主站的下发控制命令翻译成内部命令，再根据设备通信的相关协议翻译成相应的报文下发到设备
        /// 凡是要转发主站控制命令的协议，必须重写该方法
        /// 目前实现Modbus和ModbusTCP转发
        /// </summary>
        /// <param name="DeviceNo">子站设备号</param>
        /// <param name="HardAddress">子站硬件地址</param>
        /// <param name="InfoAddr">信息体地址</param>
        /// <param name="Cmd">协议转发命令类型：DO，AO</param>
        /// <param name="Value">
        /// 命令的相关值：
        /// DO命令：1/0 分别表示关和开
        /// AO命令：模拟输出的值
        /// </param>
        /// <returns></returns>
        public virtual bool TransCommand(int DeviceNo, int HardAddress, int InfoAddr, TransCommandType Cmd, object Value)
        {
            return false;
        }

        /// <summary>
        /// 将接收到的错误字节进行处理和显示
        /// </summary>
        /// <param name="ErrorBytes">接收到的错误字节</param>
        /// <param name="IPAddr">
        /// 通信数据来源的IP地址：仅供TCP Server通信方式使用
        /// </param>
        protected virtual void ProcessErrorBytes(List<byte> ErrorBytes, string IPAddr)
        {
            if (ErrorBytes.Count == 0) return;
            byte[] aByte = new byte[ErrorBytes.Count];
            ErrorBytes.CopyTo(aByte);
            ErrorBytes.Clear();

            //用于显示
            FrameQueueItem item = new FrameQueueItem(CommOrientaion.RECV, DateTime.Now, 0, 0,
                HexEncoding.Instance.GetString(aByte),
                Encoding.ASCII.GetString(aByte),
                true, false, IPAddr);
            this.ShowMessage(this.m_CommIndex, item);

        }

        /// <summary>
        /// 将接收到的错误字符进行处理和显示
        /// </summary>
        /// <param name="ErrorBytes">接收到的错误字节</param>
        /// <param name="IPAddr">
        /// 通信数据来源的IP地址：仅供TCP Server通信方式使用
        /// </param>
        protected virtual void ProcessErrorString(ref string ErrorString, string IPAddr)
        {
            if (ErrorString.Length == 0) return;

            //用于显示
            FrameQueueItem item;
            item = new FrameQueueItem(CommOrientaion.RECV, DateTime.Now, 0, 0,
                ErrorString, ErrorString, true, false, IPAddr);
            this.ShowMessage(this.m_CommIndex, item);

            ErrorString = string.Empty;
        }

        /// <summary>
        /// 从接收的字节中扫描所有的本协议完整帧，放入相应的接收缓冲区
        /// </summary>
        /// <param name="aByte">接收的字节</param>
        /// <param name="rByte">未处理完的剩余字节</param>
        /// <param name="IPAddr">
        /// 通信数据来源的IP地址：仅供TCP Server通信方式使用
        /// </param>
        public abstract void Frame_ScanFromStream(byte[] aByte, ref byte[] rByte, string IPAddr);

        /// <summary>
        /// 协议帧分析
        /// 若帧分析正确，可认为相应的子站通信状态正常
        /// </summary>
        public abstract bool Frame_Analysis();

        /// <summary>
        /// 轮询报文初始化
        /// </summary>
        public abstract void Polling_Init();

        /// <summary>
        /// 通过通信类发送当前命令
        /// </summary>
        public abstract void Main_SendCmd();

        #endregion

        #region 协议轮询处理
        /// <summary>
        /// 轮询间隔的周期（毫秒数）
        /// 每一个周期启动一次轮询(可能有多个轮询报文)
        /// </summary>
        protected int m_PollingInterval;
        /// <summary>
        /// 上一次轮询周期开始的时刻
        /// </summary>
        protected int m_PollingTick;
        /// <summary>
        /// 构造时生成的轮询报文
        /// </summary>
        public List<ProtocolFrameStruct> PollingList = new List<ProtocolFrameStruct>();
        /// <summary>
        /// 当前轮询的报文索引
        /// </summary>
        protected int m_PollingIndex = 0;

        /// <summary>
        /// 上次轮询完成所花费的时间(ms)
        /// </summary>
        protected int m_LastPollingPeriod;

        /// <summary>
        /// 上次轮询完成所花费的时间(ms)
        /// </summary>
        public int LastPollingPeriod
        {
            get { return m_LastPollingPeriod; }
        }

        /// <summary>
        /// 统计每次轮询完成所花费的时间(ms)的最小值
        /// </summary>
        protected int m_MinPollingPeriod = 9999;

        protected bool m_TimeDifferenceCalcFlag_Polling;

        /// <summary>
        /// 标准的通信协议轮询处理驱动
        /// </summary>
        public virtual void Polling_Driver()
        {
            try
            {
                /// 通信未连接则不进行轮询
                if (!this.m_Comm.Connected) return;

                // 定时把轮询的报文发送到发送缓冲区中
                if (this.PollingList.Count == 0) return;

                // 若发送缓冲区非空，则意味着前次的轮询未完或者有命令正在处理，目前可以不轮询
                if (SendCache.Count > 0) return;

                if (this.m_PollingIndex == 0)
                {
                    ///表示要开始一次新的轮询周期

                    if (m_TimeDifferenceCalcFlag_Polling)
                    {
                        m_TimeDifferenceCalcFlag_Polling = false;
                        //记录上次轮询所花费的时间
                        this.m_LastPollingPeriod = HyTick.TickTimeDifference(this.m_PollingTick, HyTick.TickTimeGet());
                        if (this.m_MinPollingPeriod > this.m_LastPollingPeriod) this.m_MinPollingPeriod = this.m_LastPollingPeriod;
                    }

                    if (!HyTick.TickTimeIsArrived(this.m_PollingTick, this.m_PollingInterval)) return;
                    ///记录本次轮询周期开始的时刻
                    this.m_PollingTick = HyTick.TickTimeGet();
                    m_TimeDifferenceCalcFlag_Polling = true;
                }

                // 现在可以把当前的一个轮询报文发送到缓冲区
                ProtocolFrameStruct pl = PollingList[this.m_PollingIndex].Clone();
                pl.NeedReply = true;
                pl.ResendCount = 0;
                pl.WaitingFlag = false;
                lock (SendCacheLock)
                {
                    SendCache.Enqueue(pl);
                    this.m_PollingIndex++;
                    this.m_PollingIndex %= PollingList.Count;
                }

            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("方法Polling_Driver()出现了错误！" + ex.Message);
            }

        }
        #endregion

        #region 协议数据同步处理
        /// <summary>
        /// 同步报文——由外部定时刷新
        /// </summary>
        public string SyncData;

        /// <summary>
        /// 标准的通信协议同步处理驱动
        /// </summary>
        public virtual void Sync_Driver()
        {
        }

        #endregion

    }

    /// <summary>
    /// 子站设备号和通信状态
    /// </summary>
    public class DeviceCommStatus
    {
        /// <summary>
        /// 子站设备号
        /// </summary>
        public int DeviceNo;
        /// <summary>
        /// 子站协议通信状态
        /// </summary>
        public bool CommStatus = false;
        /// <summary>
        /// 报文失败计数，若连续5个发送报文没有正确回复，则认为该子站设备通信错误
        /// 一旦正确回复了一个报文，则该计数清零，并认为该子站设备通信正常
        /// </summary>
        public int ErrorCount = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceNo">子站设备号</param>
        public DeviceCommStatus(int DeviceNo)
        {
            this.DeviceNo = DeviceNo;
        }
    }

    /// <summary>
    /// 通信协议帧
    /// </summary>
    public class ProtocolFrameStruct
    {
        /// <summary>
        /// 仅供TCP Server通信方式使用，表示：
        /// 发送报文的目标IP地址；或
        /// 接收报文的源IP地址
        /// </summary>
        public string IPAddr;

        /// <summary>
        /// 地址：
        /// 作为下行通信协议，应填写设备的硬件地址
        /// 作为上行通信协议，应填写RTU的站地址
        /// MA8_9协议，应填写RTU路由地址
        /// </summary>
        public int Address;

        /// <summary>
        /// 硬件地址
        /// </summary>
        public int HardAddr;

        /// <summary>
        /// 应用层功能码
        /// </summary>
        public int AFN;
        /// <summary>
        /// 包括帧头、帧尾（但不包括校验）的完整报文数据
        /// </summary>
        public byte[] DataArea;
        /// <summary>
        /// 校验
        /// </summary>
        public int CRC;

        /// <summary>
        /// 该报文是否需要回复
        /// </summary>
        public bool NeedReply;
        /// <summary>
        /// 该报文是否轮询/或同步的报文——一个通信协议不能既轮询又同步
        /// </summary>
        //public bool PollingFlag;
        /// <summary>
        /// 等待标记,=TRUE表示该有效命令正在等待回复
        /// </summary>
        public bool WaitingFlag;
        /// <summary>
        /// 重发次数
        /// </summary>
        public int ResendCount;
        /// <summary>
        /// 发送时间
        /// </summary>
        public int SendTick;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Address">地址：
        /// 作为下行通信协议，应填写设备的硬件地址
        /// 作为上行通信协议，应填写RTU的站地址
        /// MA8_9协议，应填写RTU路由地址
        /// </param>
        /// <param name="AFN">应用层功能码</param>
        public ProtocolFrameStruct(int Address, int AFN)
        {
            this.Address = Address;
            this.AFN = AFN;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Address">地址：
        /// 作为下行通信协议，应填写设备的硬件地址
        /// 作为上行通信协议，应填写RTU的站地址
        /// MA8_9协议，应填写RTU路由地址
        /// </param>
        /// <param name="AFN">应用层功能码</param>
        /// <param name="HardAddr">硬件地址</param>
        public ProtocolFrameStruct(int Address, int AFN, int HardAddr)
        {
            this.Address = Address;
            this.AFN = AFN;
            this.HardAddr = HardAddr;
        }

        /// <summary>
        /// 完全克隆一个协议帧
        /// </summary>
        /// <returns></returns>
        public ProtocolFrameStruct Clone()
        {
            ProtocolFrameStruct item = (ProtocolFrameStruct)this.MemberwiseClone();
            // 因为数组是对象，浅表副本只复制引用，所以要处理一下
            byte[] data = (byte[])this.DataArea.Clone();
            item.DataArea = data;
            return item;
        }

    }

    public class ProtocolSyncStatus
    {
        private bool m_Success;

        /// <summary>
        /// 本次通信成功标志
        /// </summary>
        public bool Success
        {
            get { return m_Success; }
            set { m_Success = value; }
        }

        /// <summary>
        /// 用于同步处理的通信协议状态类
        /// 用户通信协议类通过检测Success标志来判断本次通信是否成功
        /// </summary>
        public ProtocolSyncStatus()
        {
        }
    }

    /// <summary>
    /// 用于显示的通信报文
    /// </summary>
    public class FrameQueueItem
    {
        /// <summary>
        /// 通信方向：SEND or RECV
        /// </summary>
        public CommOrientaion OrientFlag;
        /// <summary>
        /// 报文发送或接收的时间
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// 地址：
        /// 作为下行通信协议，应填写设备的硬件地址
        /// 作为上行通信协议，应填写RTU的站地址
        /// </summary>
        public int Addr;
        /// <summary>
        /// 功能码
        /// </summary>
        public int AFN;
        /// <summary>
        /// 完整的报文数据(HEX串)
        /// </summary>
        public string FrameData_Hex;
        /// <summary>
        /// 完整的报文数据(ASCII串)
        /// </summary>
        public string FrameData_ASCII;
        /// <summary>
        /// 仅用于TCP Server通信方式：接收报文的来源IP地址或发送报文目标IP地址
        /// </summary>
        public string IPAddr;

        /// <summary>
        /// 该帧错误标志
        /// </summary>
        public bool ErrorFlag;

        /// <summary>
        /// 该帧是否轮询报文/或同步报文
        /// </summary>
        public bool PollingFlag;

        /// <summary>
        /// 用于显示的通信报文
        /// </summary>
        /// <param name="OrientFlag">通信方向：SEND or RECV</param>
        /// <param name="Time">报文发送或接收的时间</param>
        /// <param name="Addr">
        /// 地址：
        /// 作为下行通信协议，应填写设备的硬件地址
        /// 作为上行通信协议，应填写RTU的站地址
        /// </param>
        /// <param name="AFN">功能码</param>
        /// <param name="FrameData_Hex">完整的报文数据(HEX串)</param>
        /// <param name="FrameData_ASCII">完整的报文数据(ASCII串)</param>
        /// <param name="ErrorFlag">该帧错误标志</param>
        /// <param name="PollingFlag">该帧是否轮询报文/或同步报文</param>
        /// <param name="IPAddr">
        /// 仅用于TCP Server通信方式：接收报文的来源IP地址或发送报文目标IP地址
        /// </param>
        public FrameQueueItem(CommOrientaion OrientFlag, DateTime Time, int Addr, int AFN, string FrameData_Hex,
            string FrameData_ASCII, bool ErrorFlag, bool PollingFlag, string IPAddr)
        {
            this.OrientFlag = OrientFlag;
            this.Time = Time;
            this.Addr = Addr;
            this.AFN = AFN;
            this.FrameData_Hex = FrameData_Hex;
            this.FrameData_ASCII = FrameData_ASCII;
            this.ErrorFlag = ErrorFlag;
            this.PollingFlag = PollingFlag;
            this.IPAddr = IPAddr;
        }
    }

}
