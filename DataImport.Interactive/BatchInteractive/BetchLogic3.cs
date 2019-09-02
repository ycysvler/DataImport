using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys; 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Interactive.BatchInteractive
{ 
    public class BetchLogic3
    {
        log4net.ILog log = log4net.LogManager.GetLogger("RollingLogFileAppender");

        TaskInfo taskInfo = null;
        DataScript dataScript = null;
        DataScriptRule dataScriptRule = null;
        List<Structure> structList = new List<Structure>();
        Dictionary<string, string> columnMap = new Dictionary<string, string>();
        string sourceFile = ""; // 本次的导入数据文件
        string userID = "";
        string taskCode = "";
        string scriptCode = "";
        string userName = "";
        string projectCode = "";
        int baseline = 0;
        int times = 0;
        int allcount = 0;
        int successCount = 0;
        string lastmessage = "";

        public event EventHandler<MessageArgs> MessageEvent;
        public event EventHandler<ProgressArgs> ProgressEvent;
        public event EventHandler<CompleteArgs> CompleteEvent;

        bool isUpdate = false;  // 当试验次数重复时候,是更新逻辑这里为true,那么注意一下表名,用临时表名

        string tableName = "";
         
        public BetchLogic3(string userID, string userName, string projectCode, string taskCode, string scriptCode, int times, string sourceFile)
        {
            this.userID = userID;
            this.userName = userName;
            this.taskCode = taskCode;
            this.times = times;
            this.projectCode = projectCode;
            this.scriptCode = scriptCode;
            this.sourceFile = sourceFile;
        }

        public bool init()
        {
            var taskInfoList = TaskinfoDAL.getList(userName);

            if (taskInfoList == null || taskInfoList.Count == 0)
            {
                SendMessageEvent(false, string.Format("用户[{0}],无任务数据", userName));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" });
                return false;
            }
             
            this.taskInfo = taskInfoList.FirstOrDefault(it => it.projectCode == projectCode && it.taskCode == taskCode);
              
            if (taskInfo == null)
            {
                SendMessageEvent(false, string.Format("任务[{0}],不存在", taskCode));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" });
                return false;
            }

            if (DataScriptDAL.getTaskScriptCount(taskInfo.id, scriptCode) == 0) {
                SendMessageEvent(false, string.Format("解析器[{0}],没有配置在任务[{1}]下", scriptCode,taskCode));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" });
                return false;
            }

            var dataSource = WebHelper.listTdmTaskTimesInfo(taskInfo.id);

            if (dataSource.Count(it => it.TestTime == this.times.ToString()) < 1)
            {
                SendMessageEvent(false, string.Format("任务 [ {0} ] ,试验次数 [ {1} ] 不存在，", taskCode, this.times));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" });
                return false;
            }

            string fid = scriptCode2Fid(scriptCode);

            if (string.IsNullOrEmpty(fid))
            {
                SendMessageEvent(false, string.Format("任务[{0}],不存在", taskCode));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" });
                return false;
            }

            this.dataScript = DataScriptDAL.getInfo(fid);

            if (dataScript == null)
            {
                SendMessageEvent(false, string.Format("任务[{0}],不存在", taskCode));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" });
                return false;
            }

            this.dataScriptRule = DataScriptRuleDAL.getInfo(fid);

            if (dataScriptRule == null)
            {
                SendMessageEvent(false, string.Format("任务规则[{0}],不存在", fid));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" });
                return false;
            }

            this.columnMap = getColumnMap();

            if (this.dataScriptRule != null)
            {
                this.structList = TableDAL.getTableStructure(this.dataScriptRule.DesTable);
            }

            return true;
        }

        private string scriptCode2Fid(string code)
        {
            string result = "";
            var item = DataScriptDAL.getList().LastOrDefault(it => it.MidsScriptCode == code);
            if (item != null)
            {
                result = item.FID;
            }

            return result;
        }

        /// <summary>
        /// 动态计算对应关系
        /// </summary>
        private void calColumnMap(DataTable dt)
        {
            // 获取文件表头
            List<string> headers = getHeaders(dt);

            List<Structure> structures = TableDAL.getTableStructure(this.dataScriptRule.DesTable);

            foreach (string colName in headers)
            {
                // 文件字段在对应关系里面不存在                
                if (!this.columnMap.ContainsValue(colName))
                {
                    var structure = structures.FirstOrDefault(it => it.Comments == colName);
                    if (structure == null)
                    {
                        // 文件字段在表备注中不存在
                        modifyComment(structures, colName);
                    }
                    else
                    {
                        this.columnMap[structure.ColumnName] = structure.Comments;
                    }
                }
            }
        }

        private List<string> getHeaders(DataTable dt)
        {
            List<string> result = new List<string>();
            foreach (DataColumn column in dt.Columns)
            {
                result.Add(column.ColumnName);
            }
            return result;
        }

        private void modifyComment(List<Structure> structures, string fileColumn)
        {
            // 表有空余字段
            var structure = structures.FirstOrDefault(it => string.IsNullOrEmpty(it.Comments));

            if (structure != null)
            {
                // 更新备注到数据库里面去
                TableDAL.Comment(this.dataScriptRule.DesTable, structure.ColumnName, fileColumn);
                // 更新表备注
                structure.Comments = fileColumn;
                // columnMap增加记录
                this.columnMap[structure.ColumnName] = fileColumn;
            }
        }

        private Dictionary<string, string> getColumnMap()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            var list = DataScriptMapDAL.getList(this.dataScriptRule.MdsImpDataScriptID);

            foreach (var item in list)
            {
                result[item.TableColName] = item.FileColName;
            }

            return result;
        }

        private void SendMessageEvent(string message)
        {
            SendMessageEvent(true, message);
        }
        private void SendMessageEvent(bool flg, string message)
        {
            if (MessageEvent != null)
            {
                Hashtable ht = new Hashtable();
                ht["code"] = flg;
                ht["message"] = message;
                MessageEvent(this, new MessageArgs() { Message = ht });
            }
        }

        private void SendCompleteEvent(string message)
        {
            if (CompleteEvent != null)
            {
                CompleteEvent(this, new CompleteArgs() { Message = message });
            }
        }

        public void run() {
            run(System.IO.Path.GetFileName(sourceFile));
        }

        public void run(string fname)
        {
            SendMessageEvent(string.Format("开始处理数据文件：{0}", sourceFile));
            DateTime begin = DateTime.Now, end = DateTime.Now;
            this.tableName = this.dataScriptRule.DesTable;
            log.Info(string.Format("BetchLogic > updateDbByID > 删除数据:{0} -> taskid: {1}  times: {2}", tableName,this.taskInfo.id, this.times));
            SendMessageEvent(string.Format("目标表 [ {0} ] ，开始计算，请稍后！", this.dataScriptRule.DesTable));
            
            
            // 上传文件
            try
            {
                SendMessageEvent("开始上传文件："+ sourceFile);
                WebHelper.uploadFile(sourceFile, fname, TaskCenter.TaskID);
                SendMessageEvent( "上传文件结束! ");
            }
            catch (System.Exception uex) {
                SendMessageEvent(false, "上传文件异常! " + uex.Message);
            }


            // 启动导入 
            string importurl =  string.Format("{0}?filename={1}&username={2}&userid={3}",
                ConfigurationManager.AppSettings["importuri"], fname, this.userName, this.userID );


            WebHelper.GetHttp(importurl);

            while (true) {
                string msgurl = string.Format("{0}?filename={1}",
                ConfigurationManager.AppSettings["messageuri"],
                fname);

                string result = WebHelper.GetHttp(msgurl);

                if (result.IndexOf("end:") > -1)
                {

                    SendCompleteEvent(result);
                    break;
                }
                else {
                    if (lastmessage != result) {
                        lastmessage = result;
                        SendMessageEvent(result);
                    } 
                }
                Thread.Sleep(200);
            }


            
        } 
    }
}
