using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DataImport.Interactive.Sequences
{
    public class Sequence
    {
        public Dictionary<string, ResourceInfo> ResourceDictionary = new Dictionary<string, ResourceInfo>();

        public List<List<Source>> SourceList = new List<List<Source>>();

        private int sourceIndex = 0;

        public List<Source> _sources = new List<Source>();

        public List<Source> _clearup = new List<Source>();

        /// <summary>
        /// 给劲松用的，Protocol的集合
        /// </summary>
        public List<ProtocolItem> ProtocolItems = new List<ProtocolItem>();
        /// <summary>
        /// 给劲松用的，返回ResourceInfo
        /// </summary>
        /// <param name="address">address(21)</param>
        /// <returns></returns>
        public List<ResourceInfo> GetResourceInfoByAddress(int address)
        {
            return ResourceDictionary.Values.Where(it => it.Address == address).ToList();
        }

        private int _cycleCount = 0;

        public int CycleCount
        {
            get { return _cycleCount; }
            set { _cycleCount = value; }
        }

        public int StepCount { get; set; }

        /// <summary>
        /// 一次循环的最大时间长度
        /// </summary>
        /// <returns></returns>
        public int GetCycle() { return _cycle; }

        private int _cycle = 0;

        private int _index = 1;


        public List<Source> Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }

        public TreeNode Root = new TreeNode();

        public string Name { get; set; }

        /// <summary>
        /// 给劲松用的所有Step的集合，已经排序了
        /// </summary>
        /// <returns></returns>
        public List<Step> GetStepList()
        {
            List<Step> result = new List<Step>();

            if (Sources.Count > 0)
            {
                StepCount = Sources[0].Steps.Count;
            }

            int hodetime = 0;

            for (int i = 0; i < _cycleCount; i++)
            {
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
                        nstep.Time = i * _cycle + step.Time;
                        result.Add(nstep);
                    }
                }

            }
            return result.OrderBy(it => it.Time).ToList();
        }

        public List<Source> GetCycleSourceList()
        {
            List<Source> result = new List<Source>();

            foreach (var item in Sources)
            {
                Source source = new Source() { ObjID = item.ObjID, ResID = item.ResID, Line = item.Line, Address = item.Address, Name = item.Name };
                result.Add(source);
            }

            if (Sources.Count > 0)
            {
                StepCount = Sources[0].Steps.Count;
            }

            for (int i = 0; i < _cycleCount; i++)
            {
                foreach (var source in Sources)
                {
                    var item = result.First(it => it.ObjID == source.ObjID && it.ResID == source.ResID);

                    foreach (var step in source.Steps)
                    {
                        Step nstep = new Step();
                        nstep.ChangeTime = step.ChangeTime;
                        nstep.HodeTime = step.HodeTime;
                        nstep.PreTime = step.PreTime;
                        nstep.Source = step.Source;
                        nstep.TargetValue = step.TargetValue;
                        nstep.Time = i * _cycle + step.Time;
                        item.Steps.Add(nstep);
                    }
                }
            }

            return result;
        }
        public Sequence() { }
        public Sequence(string sequencepath, string sourcepath)
        {
            XDocument sourceDocument = XDocument.Load(sourcepath);
            foreach (var element in sourceDocument.Descendants("ResourceInfo").Elements())
            {
                ResourceInfo info = new ResourceInfo();
                info.Name = element.Attribute("Name").Value;
                info.ObjID = element.Attribute("ObjID").Value;
                info.ResID = element.Attribute("ResID").Value;
                info.Address = int.Parse(element.Attribute("Address").Value);
                if (element.Attribute("ObjCmd") != null)
                    info.ObjCmd = element.Attribute("ObjCmd").Value;
                if (element.Attribute("DigitType") != null)
                    info.DigitType = int.Parse(element.Attribute("DigitType").Value);


                ResourceDictionary[element.Attribute("ResID").Value] = info;
            }

            // 读取Protocal
            foreach (var element in sourceDocument.Descendants("Protocol").Elements())
            {
                ProtocolItem pItem = new ProtocolItem();
                try
                {
                    pItem.Name = element.Attribute("Name").Value;
                    pItem.Address = int.Parse(element.Attribute("Address").Value);
                    pItem.LocalSendPort = int.Parse(element.Attribute("LocalSendPort").Value);
                    pItem.LocalRecivePort = int.Parse(element.Attribute("LocalRecivePort").Value);
                    pItem.RemoteIP = element.Attribute("RemoteIP").Value;
                    pItem.RemoteSendPort = int.Parse(element.Attribute("RemoteSendPort").Value);
                    pItem.RemoteRecivePort = int.Parse(element.Attribute("RemoteRecivePort").Value);
                    pItem.ComType = element.Attribute("ComType").Value;
                    pItem.remoteSendDateChk = element.Attribute("remoteSendDateChk").Value;
                }
                catch
                {
                }
                ProtocolItems.Add(pItem);
            }

            XDocument sequenceDocument = XDocument.Load(sequencepath);
            // 构建树形结构
            BuilderNode(Root, sequenceDocument.Element("Sequence"));

            // 读取sources
            //var sources = (from sourcelist in sequenceDocument.Descendants("Group") where sourcelist.Attribute("Name").Value == "Main" select sourcelist).Descendants("CommandTable").Elements();
            //foreach (var sourceElement in sources)
            //{
            //    Source source = new Source();
            //    source.ResID = sourceElement.Attribute("ResID").Value;
            //    source.ObjID = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].ObjID : "";
            //    source.Address = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].Address : 0;
            //    source.Name = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].Name : source.ResID;
            //    source.DigitType = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].DigitType : 0;
            //    string objcmdString = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].ObjCmd : "";
            //    int objcmd = 0;
            //    int.TryParse(objcmdString, out objcmd);
            //    source.ObjCmd = objcmd;
            //    _sources.Add(source);

            //    int temp = 0;

            //    if (sourceElement.Attribute("Enable").Value == "True")
            //        source.Line = true;

            //    foreach (var stepElement in sourceElement.Elements())
            //    {
            //        Step step = new Step();
            //        step.Source = source;
            //        step.TargetValue = Math.Round(Convert.ToDouble(stepElement.Attribute("TargetValue").Value), 2);
            //        step.PreTime = Convert.ToInt32(stepElement.Attribute("PreTime").Value);
            //        step.ChangeTime = Convert.ToInt32(stepElement.Attribute("ChangeTime").Value);
            //        step.HodeTime = Convert.ToInt32(stepElement.Attribute("HodeTime").Value);
            //        step.Time = temp + step.PreTime + step.ChangeTime;
            //        temp = step.Time + step.HodeTime;
            //        source.Steps.Add(step);
            //    }
            //    // 单次循环的最大时常
            //    _cycle = temp > _cycle ? temp : _cycle;
            //}

            //// 读取clearup
            //sources = (from sourcelist in sequenceDocument.Descendants("Group") where sourcelist.Attribute("Name").Value == "Cleanup" select sourcelist).Descendants("CommandTable").Elements();

            //foreach (var sourceElement in sources)
            //{
            //    Source source = new Source();
            //    source.ResID = sourceElement.Attribute("ResID").Value;
            //    source.ObjID = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].ObjID : "";
            //    source.Address = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].Address : 0;
            //    source.Name = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].Name : source.ResID;
            //    _clearup.Add(source);

            //    foreach (var stepElement in sourceElement.Elements())
            //    {
            //        Step step = new Step();
            //        step.Source = source;
            //        step.TargetValue = Convert.ToDouble(stepElement.Attribute("TargetValue").Value);
            //        step.PreTime = Convert.ToInt32(stepElement.Attribute("PreTime").Value);
            //        step.ChangeTime = Convert.ToInt32(stepElement.Attribute("ChangeTime").Value);
            //        step.HodeTime = Convert.ToInt32(stepElement.Attribute("HodeTime").Value);
            //        source.Steps.Add(step);
            //    }
            //}
        }


        private void BuilderNode(TreeNode node, XElement parent)
        {
            foreach (var element in parent.Elements())
            {
                //if (element.Name.ToString() == "Group" && element.Attribute("Name").Value != "Main")
                //{
                //    continue;
                //}
                if (element.Name.ToString() == "Property" && node.Type == "Wait")
                {
                    try
                    {
                        node.TimeOut = Convert.ToInt32(element.Attribute("TimeOut").Value);
                        node.TimeOut = node.TimeOut > 0 ? node.TimeOut : 0;
                    }
                    catch { }
                }


                switch (element.Name.ToString())
                {
                    case "Group":
                        //if (element.Attribute("Name").Value != "Main") continue;
                        break;
                    case "Property":
                    case "Step":
                    case "Conditon":
                    case "Validate":
                        continue;
                }

                TreeNode tn = new TreeNode();
                tn.Title = element.Name.ToString();
                tn.Type = element.Name.ToString();
                tn.Code = _index.ToString();
                _index++;
                switch (tn.Type)
                {
                    case "Group":
                        tn.Title = element.Attribute("Name").Value;
                        break;
                    case "For": tn.Title = string.Format("For({0})", element.Attribute("LoopCount").Value);
                        For tag = new For();
                        tag.LoopCount = int.Parse(element.Attribute("LoopCount").Value);
                        tn.Tag = tag;

                        break;
                    case "Source":
                        string resid = element.Attribute("ResID").Value;
                        tn.Title = ResourceDictionary.Keys.Contains(resid) ? ResourceDictionary[resid].Name : resid;
                        tn.Code = resid;
                        break;
                    case "InputPopup":
                        tn.Title = "PopupWindow";
                        tn.PopupText = element.Attribute("Text").Value;
                        break;
                    case "CommandTable":
                        tn.Title = element.Attribute("Name").Value;
                        CommandTable ct = new CommandTable();
                        if (node.Type == "For")
                            ct.LoopCount = (node.Tag as For).LoopCount;

                        tn.Tag = ct;
                        // 读取sources
                        var sources = element.Elements();
                        foreach (var sourceElement in sources)
                        {
                            Source source = new Source();
                            source.ResID = sourceElement.Attribute("ResID").Value;
                            source.ObjID = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].ObjID : "";
                            source.Address = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].Address : 0;
                            source.Name = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].Name : source.ResID;
                            source.DigitType = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].DigitType : 0;
                            string objcmdstring = ResourceDictionary.Keys.Contains(source.ResID) ? ResourceDictionary[source.ResID].ObjCmd : "";

                            source.ObjCmd = string.IsNullOrEmpty(objcmdstring) ? 0 : int.Parse(objcmdstring);

                            ct.Sources.Add(source);

                            int temp = 0;

                            foreach (var stepElement in sourceElement.Elements())
                            {
                                Step step = new Step();
                                step.Source = source;
                                step.TargetValue = Convert.ToDouble(stepElement.Attribute("TargetValue").Value);
                                step.PreTime = Convert.ToInt32(stepElement.Attribute("PreTime").Value);
                                step.ChangeTime = Convert.ToInt32(stepElement.Attribute("ChangeTime").Value);
                                step.HodeTime = Convert.ToInt32(stepElement.Attribute("HodeTime").Value);
                                step.Time = temp + step.PreTime ;
                                //step.Time = temp + step.PreTime + step.ChangeTime;
                                temp = step.Time + step.ChangeTime + step.HodeTime;
                                source.Steps.Add(step);
                            }
                        }
                        break;
                }

                node.Children.Add(tn);

                BuilderNode(tn, element);

            }
        }
    }

    public class TreeNode
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public object Tag { get; set; }
        public string PopupText = "";
        public int TimeOut = 0;
        private List<TreeNode> _children = new List<TreeNode>();

        public List<TreeNode> Children
        {
            get { return _children; }
            set { _children = value; }
        }
    }

    public class For
    {
        public int LoopCount { get; set; }

        private string _description = "";
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
    }

    public class CommandTable
    {

        private int _loopCount = 1;

        public int LoopCount
        {
            get { return _loopCount; }
            set { _loopCount = value; }
        }

        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _description = "";

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private List<Source> _sources = new List<Source>();

        public List<Source> Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }
        private List<Step> _stepList = new List<Step>();
        public List<Step> GetStepList()
        {
            if (_stepList.Count > 0)
                return _stepList;
            else
            {

                List<Step> result = new List<Step>();

                int interval = 0;

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
                        nstep.Time = step.Time+step.HodeTime;
                        result.Add(nstep);
                    }
                }
                interval = result.Max(it => it.Time);
                

                result.Clear();

                for (int i = 0; i < LoopCount; i++)
                {
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
                            nstep.Time = i * interval + step.Time;
                            result.Add(nstep);
                        }
                    }
                }

                result = result.OrderBy(it => it.Time).ToList();

                foreach (var item in result)
                {
                    Console.WriteLine("{0}\t{1}\t{2}", item.Source.Name, item.TargetValue, item.Time);
                }
                _stepList = result;
                return result;
            }
        }
    }


    public class Source
    {

        public string ResID { get; set; }   //资源ID
        public int ObjCmd { get; set; }
        public int Address { get; set; } //资源地址
        public string ObjID { get; set; }  //目标ID
        public string Name { get; set; }    //资源名称 
        public bool Line { get; set; }      //点和线的标识 
        public int DigitType { get; set; }  // 1:开关量， 0：模拟量
        public int StepNum { get { return _steps.Count(); } } //获得工步的数量

        private List<Step> _steps = new List<Step>(); //工步对象

        public List<Step> Steps
        {
            get { return _steps; }
            set { _steps = value; }
        }
    }

    public class Step
    {
        public Source Source { get; set; }          //工步资源
        public double TargetValue { get; set; }      //工步目标值
        public int PreTime { get; set; }            //预处理时间
        public int ChangeTime { get; set; }         //改变时间
        public int HodeTime { get; set; }           //持续时间
        public int Time { get; set; }               //执行时机

    }

    /// <summary>
    /// <Protocol LocalIP="">
    ///   <Item Name="设备管理机" Address ="21" LocalSendPort="" LocalRecivePort="" RemoteIP="127.0.0.1" RemoteSendPort="6010" RemoteRecivePort=""  ProtocolFile="./XXX.mdb"/>
    ///   <Item Name="EEC" Address ="22"  LocalSendPort="" LocalRecivePort="" RemoteIP="127.0.0.1" RemoteSendPort="6010" RemoteRecivePort=""  ProtocolFile="./XXX.mdb"/> 
    /// </Protocol>
    /// </summary>
    public class ProtocolItem
    {
        public string Name { get; set; }        //设备名称
        public int Address { get; set; }        //设备地址
        public int LocalSendPort { get; set; }   //本地发送端口
        public int LocalRecivePort { get; set; } //本地接收端口
        public string RemoteIP { get; set; }    //远程IP
        public int RemoteSendPort { get; set; } //远端发送端口
        public int RemoteRecivePort { get; set; } //远端接收端口

        private string _comType = "";
        public string ComType { get { return _comType; } set { _comType = value; } } //通讯端口类型
        private string _remoteSendDateChk = "";
        public string remoteSendDateChk { get { return _remoteSendDateChk; } set { _remoteSendDateChk = value; } } //通讯端口是否执行反馈标志
    }

    public class ResourceInfo
    {
        public string Name { get; set; }       //资源名称     
        public string ObjID { get; set; }      //目标ID
        public string ResID { get; set; }      //资源ID
        public int Address { get; set; }       //地址
        public string ObjCmd { get; set; }
        public int DigitType { get; set; }  // 1:开关量， 0：模拟量
    }
}
