using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HyCommBase;
using HyUtilities;
using DataImport.Interactive.Sequences;

namespace TcpClientTest
{
    public class TcpClientAppData
    {

        public class deviceconfig_autocmditem  //自动命令数据类
        {
            public int secs;    //命令执行时间
            public string ObjIdent; //执行层识别码
            public int ObjCmd; //对象操作指令
            public string ObjID; //对象信号编号
            public double DestValue; //命令数据
            public double HoldTime; //持续时间
            public double ChangeTime;//变换时间
            public int devIndex; //设备的序号
            public bool ExeFlag; //执行标志
            public int devAddress; //设备地址
            public int RemoteSendPort;//远程发送接口
        };

        public class deviceconfig
        {
            //设备的基本参数
            public string RemoteIp ;  //远端设备IP
            public string LocalIp; //本地接收数据IP
            public int Sendport; //设备发送端口
            public int Recvport; //设备接收端口
            public int devAddr; //设备地址
            public bool CommFlag; //通讯标识
            public string devName; //设备名称
            public string ComType; //通讯方式
            public string remoteSendDateChk; //是否接收反馈的标志

            //设备数据
            public float [] afl ; //设备的实时数据
            public bool CmdFlag; //命令标识
            public bool CmdResultFlag; //命令执行结果

            public int CmdNumberCount; //命令数量
            public int tick = 0; //设定计时的ticket;
        };

        public deviceconfig[] devd; //实时数据实例
        private Sequence m_sequence;
     
        public List<deviceconfig_autocmditem> SequenceCmdList = new List<deviceconfig_autocmditem>();

        public int m_process_tick = 0; //设定计时的ticket;
        public int m_CmdIndex = 0; //当前cmdIndex的序号
        public int m_autorunfalg = 0;  //定义当前自动运行的状态标识
        public int m_StopPeriod_tick = 0; //当前停止的时间
        public int m_lastStopTicket = 0; //记录上次停止的ticket


        //初始化Dev设备的信息 Source
        public void init(Sequence sequence)
        {
            m_sequence = sequence;
            int sumdevd = m_sequence.ProtocolItems.Count;

            //根据XML配置文件 进行文件的解析，XML配置中的记录数据的先后顺序进行处理
            devd = new deviceconfig[sumdevd];
            for (int i = 0; i < sumdevd; i++)
            {
                devd[i] = new deviceconfig();
                devd[i].CmdFlag = false;
                devd[i].afl = new float[16];
                devd[i].RemoteIp = m_sequence.ProtocolItems[i].RemoteIP;
                devd[i].LocalIp = "127.0.0.1";
                devd[i].Sendport = m_sequence.ProtocolItems[i].LocalSendPort;
                devd[i].Recvport = m_sequence.ProtocolItems[i].LocalRecivePort;
                devd[i].devAddr = m_sequence.ProtocolItems[i].Address;
                devd[i].devName = m_sequence.ProtocolItems[i].Name;
                devd[i].ComType = m_sequence.ProtocolItems[i].ComType.ToUpper();
                devd[i].remoteSendDateChk = m_sequence.ProtocolItems[i].remoteSendDateChk.ToUpper();
            }
        }

        //资源状态信息事件
        public class CmdIndexStatusEventArgs : EventArgs
        {
            public int CmdIndex{get;set;}
            public bool ExeStatus{get;set;}
        }

        public event EventHandler<CmdIndexStatusEventArgs> CmdIndexStatusEvent;


        //处理实时数据消息事件
        public class RecRt_DataEnventArgs : EventArgs
        {
            public string ResID { get; set; }
            public float Value { get; set; }
            public DateTime Current_Date { get; set; }
        }
        public event EventHandler<RecRt_DataEnventArgs> RecRt_DataEvent;


       //处理设备的通讯状态消息事件
        public class EquComm_StatusEnventArgs : EventArgs
        {
            public int Address { get; set; }
            public bool CommFlag { get; set; }
        }
        public event EventHandler<EquComm_StatusEnventArgs> EquComm_StatusEnvent;


        //处理设备的通讯状态中断事件  01表示暂停，02 表示继续，03表示停止
        public class EquComm_RunStatusEnventArgs : EventArgs
        {
            public string CommFlag { get; set; }
        }
        public event EventHandler<EquComm_RunStatusEnventArgs> EquComm_RunStatusEnvent;

        //处理通讯报文的中断处理
        public void Process_EquComm_RunStatus(string m_CommFlag)
        {
            if (EquComm_RunStatusEnvent != null)
            {
                EquComm_RunStatusEnvent(this,new EquComm_RunStatusEnventArgs(){ CommFlag = m_CommFlag});
            }
        }


        //处理通讯报文消息事件
        public class EquComm_MessageEnventArgs : EventArgs
        {
            public DateTime  Current_Date { get; set; }
            public String Message { get; set; }
            public bool ViewFlag { get; set; }
        }
        public event EventHandler<EquComm_MessageEnventArgs> EquComm_MessageEnvent;

        //处理通讯报文方法
        public void Process_EquComm_Message(DateTime m_Current_Date, string m_Message, bool m_SendFlag, bool m_ViewFlag)
        {
            if (EquComm_MessageEnvent != null)
            {
                if (m_SendFlag)  //发送报文
                {
                    m_Message = m_Current_Date.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "  SEND : " + m_Message;
                }
                else   //接收报文
                {
                    m_Message = m_Current_Date.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "  REV : " + m_Message;
                }
                EquComm_MessageEnvent(this, new EquComm_MessageEnventArgs() { Current_Date = m_Current_Date, Message = m_Message, ViewFlag = m_ViewFlag });
            }
        }
       
        
        //处理设备的通讯状态
        public void  Process_EquCommFlag(int m_address, bool m_commflag)
        {
            if (EquComm_StatusEnvent != null)
            {
                EquComm_StatusEnvent(this, new EquComm_StatusEnventArgs() { Address = m_address, CommFlag = m_commflag });
            }
        }

        public Dictionary<string, float> rtdataDict = new Dictionary<string, float>();
        public Dictionary<string, bool> ResouceStatus = new Dictionary<string, bool>();


        //获取当前设备的实时数据更新发布
        public void process(int m_appdateindex, int devAddress)
        {
            //按照设备进行实时数据的更新
            int devCount = m_sequence.ProtocolItems.Count;
            int RtDataCount = 0;
            List<ResourceInfo> Reslist = new List<ResourceInfo>();

            Reslist = m_sequence.GetResourceInfoByAddress(devAddress);
            RtDataCount = Reslist.Count;
            for (int j = 0; j < RtDataCount; j++)
            {
                if (RecRt_DataEvent != null)
                {
                    RecRt_DataEvent(this, new RecRt_DataEnventArgs() { ResID = Reslist[j].ResID, Value = devd[m_appdateindex].afl[j], Current_Date = System.DateTime.Now});
                }
            }
        }


        //命令执行完成后进行状态的更新
        public void CmdIndexStatus(int m_cmdIndex, bool exeFlag)
        {
            if (CmdIndexStatusEvent != null)
            {
                DateTime begin = DateTime.Now;
                Console.WriteLine("CmdIndexStatus begin:{0}", begin);
                CmdIndexStatusEvent(this, new CmdIndexStatusEventArgs() { CmdIndex = m_cmdIndex, ExeStatus = exeFlag });
                DateTime end = DateTime.Now;
                Console.WriteLine("CmdIndexStatus end:{0}", end);
                Console.WriteLine("CmdIndexStatus interval:{0}", (end-begin).TotalMilliseconds);

            }
        }

        //执行端发送Command控制命令节点
        public bool CreateCmdByCommandTable(CommandTable ct) {

            //进行多命令信息的初始化 
            List<Step> CmdStepList = new List<Step>();

            CmdStepList = ct.GetStepList();

            int StepCount = CmdStepList.Count;

            m_CmdIndex = 0; //重新开始执行
            int devIndex = 0;
            SequenceCmdList.Clear();//清空原来执行的内容

            if (StepCount > 0)  //说明具有多缓冲命令信息
            {
                //进行命令消息的存储
                for (int i = 0; i < StepCount; i++)
                {

                    devIndex = GetDevIndexByAddress(CmdStepList[i].Source.Address);
                    deviceconfig_autocmditem SequenceCmdItem = new deviceconfig_autocmditem();

                    SequenceCmdItem.secs = CmdStepList[i].Time; //执行时间S
                    SequenceCmdItem.ChangeTime = CmdStepList[i].ChangeTime; //变换时间
                    SequenceCmdItem.HoldTime = CmdStepList[i].HodeTime; //持续时间
                    SequenceCmdItem.DestValue = CmdStepList[i].TargetValue; //目标值
                    SequenceCmdItem.ExeFlag = false;   //执行标志为false
                    SequenceCmdItem.devAddress = CmdStepList[i].Source.Address;
                    SequenceCmdItem.devIndex = GetDevIndexByAddress(CmdStepList[i].Source.Address);
                    SequenceCmdItem.ObjID = CmdStepList[i].Source.ObjID;
                    SequenceCmdItem.ObjCmd = CmdStepList[i].Source.ObjCmd;
                    SequenceCmdItem.ObjIdent = devd[SequenceCmdItem.devIndex].devName;
                    SequenceCmdItem.RemoteSendPort = devd[SequenceCmdItem.devIndex].Sendport;
                    SequenceCmdList.Add(SequenceCmdItem);
                }
            }
          
            m_process_tick = 0; //设定计时的ticket;
            m_CmdIndex = 0; //当前cmdIndex的序号
            m_autorunfalg = 1;  //定义当前自动运行的状态标识
            m_StopPeriod_tick = 0; //当前停止的时间
            m_lastStopTicket = 0; //记录上次停止的ticket
            LogHelper.Log("************" + System.DateTime.Now.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "程序运行开始" + "***********");
            return true;
        }


        //初始化获得的多命令信息
        public bool CreateCmdBySequence(Sequence sequence)
        {
            //进行多命令信息的初始化
          
            List <Step> CmdStepList = new List<Step> ();

            CmdStepList = sequence.GetStepList();
            int StepCount = CmdStepList.Count;
           
            m_CmdIndex = 0; //重新开始执行
            int devIndex = 0;
            SequenceCmdList.Clear();//清空原来执行的内容

            if (StepCount > 0)  //说明具有多缓冲命令信息
            {
                //进行命令消息的存储
                for (int i = 0; i < StepCount; i++)
                {

                    devIndex = GetDevIndexByAddress(CmdStepList[i].Source.Address);
                    deviceconfig_autocmditem SequenceCmdItem = new deviceconfig_autocmditem();
                    
                    SequenceCmdItem.secs = CmdStepList[i].Time; //执行时间S
                    /*
                    = CmdStepList[i].Source.ResID; //资源ID
                    = CmdStepList[i].PreTime; //预使用时间
                    = CmdStepList[i].ChangeTime; //改变时间
                     */

                    SequenceCmdItem.ChangeTime = CmdStepList[i].ChangeTime; //变换时间
                    SequenceCmdItem.HoldTime = CmdStepList[i].HodeTime; //持续时间
                    SequenceCmdItem.DestValue = Convert.ToInt32(CmdStepList[i].TargetValue * 100); //目标值
                    SequenceCmdItem.ExeFlag = false;   //执行标志为false
                    SequenceCmdItem.devAddress = CmdStepList[i].Source.Address;
                    SequenceCmdItem.devIndex = GetDevIndexByAddress(CmdStepList[i].Source.Address);
                    SequenceCmdItem.ObjID = CmdStepList[i].Source.ObjID;
                    SequenceCmdItem.ObjCmd = CmdStepList[i].Source.ObjCmd;
                                 
                    SequenceCmdList.Add(SequenceCmdItem);
                }
            }
            m_autorunfalg = 1;
            LogHelper.Log("************" + System.DateTime.Now.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "程序运行开始" + "***********");
            return true;
        }


        //根据设备地址获得设备序号
        private int GetDevIndexByAddress(int devAddress)
        {
            int retDevIndex = 0;
            for (int i = 0; i < devd.Length; i++)
            {
                if (devd[i].devAddr == devAddress)
                {
                    retDevIndex = i;
                    return retDevIndex;
                }
            }
            return retDevIndex;
        }

        //获取命令缓冲区中的命令信息
        public int cmdmany_getcmd(int devindex)
        {
            int ExecSec = 0;

            if (m_autorunfalg == 0)
            {
                m_CmdIndex = 0 ;
                return 0;
            }
            else if (m_autorunfalg == 1)
            {
                if (m_CmdIndex == 0 && m_process_tick == 0)
                {
                    m_process_tick = HyTick.GetTickCount();  //设定当前的Ticket时钟
                    m_StopPeriod_tick = 0;
                }

                if (SequenceCmdList.Count > 0 && m_CmdIndex < SequenceCmdList.Count)  //表示有多命令要开始执行了
                {
                    if (devindex == SequenceCmdList[m_CmdIndex].devIndex) //
                    {
                        ExecSec = SequenceCmdList[m_CmdIndex].secs * 1000 + m_StopPeriod_tick;
                        if (HyTick.TickTimeIsArrived(m_process_tick, ExecSec))  //说明时间到了需要下发命令了,进行下发命令的组织。
                        {
                            devd[devindex].CmdFlag = true;
                            m_CmdIndex = m_CmdIndex + 1;
                            return m_CmdIndex;
                        }
                    }
                }

                if (m_CmdIndex == SequenceCmdList.Count)  //表示已经执行完成
                {
                    m_autorunfalg = 0;
                    m_CmdIndex = 0;
                    LogHelper.Log("************" + System.DateTime.Now.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "程序运行成功结束" + "***********");
                    return m_CmdIndex;
                }
            }
            return 0;
        }


        //进行程序启动，停止的控制
        //01表示停止
        //02表示继续
        public void ProcessStopRun(string RunOrStop)
        {
            //进行停止程序的控制
            if (RunOrStop == "01")
            {
                m_autorunfalg = 2;
                m_lastStopTicket = HyTick.GetTickCount();
                LogHelper.Log("************" + System.DateTime.Now.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "程序运行中止" + "***********");
            } //进行启动程序的控制
            else if (RunOrStop == "02")
            {
                m_autorunfalg = 1;
                m_StopPeriod_tick = m_StopPeriod_tick + HyTick.GetTickCount() - m_lastStopTicket;
                LogHelper.Log("************" + System.DateTime.Now.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "程序运行继续" + "***********");
            }
        }


        //根据设备号获得发送的巡检命令
        public byte[] GetCmd_Bydev( int devindex )
        {
            int ResouceCount = 0;
            List <ResourceInfo> Reslist = new List<ResourceInfo> ();

            byte[] retByte = null;
            int devAddress = devd[devindex].devAddr;
            Reslist = m_sequence.GetResourceInfoByAddress(devAddress);
            ResouceCount = Reslist.Count;
            int bytecount = 5 + ResouceCount * 2;
            retByte = new byte[bytecount];
            retByte[0] = 0xAA; //起始位
            retByte[1] = 0x55;
            retByte[2] = (byte)devAddress;
            retByte[3] = 0x03; //指令类型
            retByte[4] = (byte)ResouceCount; //指令数量
            for (int i = 0; i < ResouceCount; i++)  //指令报文
            {
                retByte[4 + i * 2 + 1] = (byte)HyStringUtility.ConvertStringToInt("0x" + Reslist[i].ResID.Substring(0, 1)); 
                retByte[4 + i * 2 + 2] = byte.Parse(Reslist[i].ResID.Substring(1));
            }
            return retByte;
        }


        //按照时间调度来组织控制命令的报文
        public byte[] Create_cmdbyte_ByIndex(int m_CmdIndex)
        {
            byte[] retByte = null;
            int index = 0;
            string ObjIdent = SequenceCmdList[m_CmdIndex].ObjIdent.ToString();
            string ObjIdStr = SequenceCmdList[m_CmdIndex].ObjID.ToString();
            retByte = new byte[145];
            ///////////ObjIdent报文的组织0---60////////////////////////
            index = 0; 
            retByte[index] = (byte)ObjIdent.Length;
            for (int i = 0; i < ObjIdent.Length; i++)
            {
                retByte[index + 1 + i] = (byte)Convert.ToInt32(Convert.ToChar(ObjIdent.Substring(i, 1)));
            }
            ////////////ObjCmd报文的组织////////////////////////////////
            index = 61;
            retByte[index] = (byte)SequenceCmdList[m_CmdIndex].ObjCmd; //ObjCmd
            ///////////OjbId报文的组织62---122/////////////////////////////////
            index = 62;
            retByte[index] = (byte)ObjIdStr.Length; //ObjCmd
            for (int i = 0; i < ObjIdStr.Length; i++)
            {
                retByte[index + 1 + i] = (byte)Convert.ToInt32(Convert.ToChar(ObjIdStr.Substring(i, 1)));
            }
            /////////////PkgType报文组织123---124////////////////////////////
            index = 123;
            retByte[index] = 0x01; //PkgNum
            /////////////AckPort报文组织125---128////////////////////////////
            index = 125;
            byte[] ConvertByte = new byte[4];
            ConvertByte = BitConverter.GetBytes(SequenceCmdList[m_CmdIndex].RemoteSendPort);
            for (int i = 0; i < ConvertByte.Length; i++)
            {
                retByte[index + i] = ConvertByte[i];
            }
            /////////////Destval报文组织129---136////////////////////////////
            index = 129;
            ConvertByte = new byte[8];
            ConvertByte = BitConverter.GetBytes(SequenceCmdList[m_CmdIndex].DestValue);
            for (int i = 0; i < ConvertByte.Length; i++)
            {
                retByte[index + i] = ConvertByte[i];
            }
            /////////////Destval报文组织137---144////////////////////////////
            index = 137;
            ConvertByte = new byte[8];
            ConvertByte = BitConverter.GetBytes(SequenceCmdList[m_CmdIndex].ChangeTime);
            for (int i = 0; i < ConvertByte.Length; i++)
            {
                retByte[index + i] = ConvertByte[i];
            }
            return retByte;
        }


        //根据时间轮巡的时间来获得返回的报文（旧版本）
        public byte[] Create_Sendbyte_ByIndex(int m_CmdIndex)
        {
            byte[] retByte = null;
           
            string ObjIdStr = SequenceCmdList[m_CmdIndex].ObjID.ToString();
            retByte = new byte[16 + ObjIdStr.Length];

            //1,获取该时间段的所有命令记录信息
            retByte[0] = 0xAA;
            retByte[1] = 0x55;
            retByte[2] = (byte)SequenceCmdList[m_CmdIndex].devAddress;
            retByte[3] = 0x06; //指令类型
            retByte[4] = 0x01; //PkgType
            retByte[5] = 0x01; //PkgNum
            retByte[6] = 0x00; //ObjIdent
            retByte[7] = (byte)SequenceCmdList[m_CmdIndex].ObjCmd; //ObjCmd
            
            for (int i = 0; i < ObjIdStr.Length; i++)
            {
                retByte[8 + i] = (byte)Convert.ToInt32(Convert.ToChar(ObjIdStr.Substring(i, 1)));
            }
            //Destval
            //Int32ByteArrayConverter.LConvertToByte(SequenceCmdList[m_CmdIndex].DestValue, retByte, 8 + ObjIdStr.Length);
            //DestTime
            //Int32ByteArrayConverter.LConvertToByte(SequenceCmdList[m_CmdIndex].ChangeTime, retByte, 12 + ObjIdStr.Length);
            
            return retByte;
        }

        //解析发送控制报文
        public string GetSendMessage(int m_CmdIndex)
        {
            string SendMessage = string.Empty;
            SendMessage = "目标对象:" + devd[SequenceCmdList[m_CmdIndex].devIndex].devName + " 目标值:" + SequenceCmdList[m_CmdIndex].DestValue + " 时间:" + SequenceCmdList[m_CmdIndex].ChangeTime;
            return SendMessage;
        }

    }
}
