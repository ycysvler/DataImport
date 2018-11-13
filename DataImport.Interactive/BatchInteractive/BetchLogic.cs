using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys; 
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.Interactive.BatchInteractive
{
    public class CompleteArgs : EventArgs {
        public string Message { get; set; }
    }
    public class ProgressArgs : EventArgs {
        public int Message { get; set; }
    }
    public class MessageArgs : EventArgs {
        public Hashtable Message { get; set; }
    }

    public class BetchLogic
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
        int baseline = 0;
        int times = 0;
        int allcount = 0;
        int successCount = 0;

        public event EventHandler<MessageArgs> MessageEvent;
        public event EventHandler<ProgressArgs> ProgressEvent;
        public event EventHandler<CompleteArgs> CompleteEvent;

        bool isUpdate = false;  // 当实验次数重复时候,是更新逻辑这里为true,那么注意一下表名,用临时表名

        string tableName = "";

        public static void BetchDir(string userID, string userName, string dir)
        { 
            betchDirFile(userID, userName, dir);
        }

        private static void betchDirFile(string userID, string userName, string dir)
        {
            
            foreach (string file in System.IO.Directory.GetFiles(dir))
            {
                string ext = System.IO.Path.GetExtension(file);
                if (ext == ".txt" || ext == ".xls" || ext == ".xlsx" || ext == ".mdb")
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

                        BetchLogic bl = new BetchLogic(userID, userName, projectCode, taskCode, scriptCode, times, file);
                        bl.run();
                    }
                }
            }
        }
          
        public BetchLogic(string userID, string userName, string projectCode, string taskCode, string scriptCode, int times, string sourceFile)
        {
            this.userID = userID;
            this.userName = userName;
            this.taskCode = taskCode;
            this.times = times;
            this.scriptCode = scriptCode;
            this.sourceFile = sourceFile; 
        }

        public bool init() {
            var taskInfoList = TaskinfoDAL.getList(userName);

            if (taskInfoList == null || taskInfoList.Count == 0) {
                SendMessageEvent(false, string.Format("用户[{0}],无任务数据", userName));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" }); 
                return false;
            }

            this.taskInfo = taskInfoList.FirstOrDefault(it => it.taskCode == taskCode);

            if (taskInfo == null )
            {
                SendMessageEvent(false, string.Format("任务[{0}],不存在",taskCode));
                CompleteEvent(this, new CompleteArgs() { Message = "数据导入失败" });
                return false;
            }

            var dataSource = WebHelper.listTdmTaskTimesInfo(taskInfo.id);

            if (dataSource.Count(it => it.TestTime == this.times.ToString()) < 1) {
                SendMessageEvent(false, string.Format("任务 [ {0} ] ,实验次数 [ {1} ] 不存在，", taskCode, this.times));
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

            if (dataScript==null)
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

        private List<string> getHeaders(DataTable dt) {
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

        private void SendMessageEvent( string message)
        {
            SendMessageEvent(true, message);
        }
        private void SendMessageEvent(bool flg, string message) {
            if (MessageEvent != null) {
                Hashtable ht = new Hashtable();
                ht["code"] = flg;
                ht["message"] = message;
                MessageEvent(this,new MessageArgs() { Message = ht });
            }
        }

        private void SendCompleteEvent(string message)
        {
            if (CompleteEvent != null)
            {
                CompleteEvent(this, new CompleteArgs() { Message = message});
            }
        }

        public void run()
        {
            SendMessageEvent(string.Format("开始处理数据文件：{0}", sourceFile));
            DateTime begin = DateTime.Now, end=DateTime.Now;
            this.tableName = this.dataScriptRule.DesTable;
            this.isUpdate = checkTestTimes();

            // 判断源表是否存在 
            if (TableDAL.getTableStructure(this.dataScriptRule.DesTable).Count == 0) {
                // 源表不存在
                log.Error(string.Format("BetchLogic > run > 目标表 [ {0} ] 不存在", this.dataScriptRule.DesTable));
                SendMessageEvent(false, string.Format("目标表 [ {0} ] 不存在", this.dataScriptRule.DesTable));
                SendCompleteEvent("导入失败");
                return;
            } 

            // 写入导入数据日志
            DataLog dataLog = createLog(0, System.IO.Path.GetFileName(sourceFile));
            // 上传文件
            //WebHelper.uploadFile(sourceFile, dataLog.FID, TaskCenter.TaskID); 
            // 一个处理半物力实验数据的逻辑
            uploadFile();
            end = DateTime.Now;
            SendMessageEvent(string.Format("上传数据文件，耗时：{0}秒", (end - begin).TotalSeconds));

            begin = DateTime.Now;

            string fileType = System.IO.Path.GetExtension(this.sourceFile);

            // 判断本次实验，有没有解析器执行过, 创建临时表
            if (isUpdate)
            { 
                Structure st = structList.FirstOrDefault(it => it.Comments == System.Configuration.ConfigurationManager.AppSettings["pk"]);
                if (st != null)
                {  
                    tableName += "_" + DateTime.Now.Millisecond.ToString();
                    if (tableName.Length > 30)
                    {
                        tableName = tableName.Remove(0, tableName.Length - 29);
                    }  
                    TableDAL.createtempTable(dataScriptRule.DesTable, tableName);

                    log.Info(string.Format("BetchLogic > run > 本次为更新操作,创建临时表:{0}", tableName));

                    TableDAL.AddIndex(tableName, string.Format("{0},TASKTIMES,PROJECTID", st.ColumnName));
                }
            }

            switch (fileType)
            {
                case ".xls":
                case ".xlsx":
                    xls2db();
                    if (isUpdate)
                    {
                        updateDbByID();
                    }
                    break;
                case ".txt":
                case ".dat":
                    txt2db();
                    if (isUpdate)
                    {
                        updateDbByID();
                    }
                    break;
                case ".mdb":
                    acc2db();
                    break;
            }

            end = DateTime.Now;
            SendMessageEvent(string.Format("数据导入，耗时：{0}秒", (end - begin).TotalSeconds));

            if(!isUpdate)
                SendCompleteEvent("数据导入成功！");
             
        }

        

        private void updateDbByID()
        {
            System.Data.OracleClient.OracleParameter[]
            ps = new System.Data.OracleClient.OracleParameter[7];
            System.Data.OracleClient.OracleParameter p1 = new System.Data.OracleClient.OracleParameter("I_EnTest_Work_Id", this.taskInfo.id);
            System.Data.OracleClient.OracleParameter p2 = new System.Data.OracleClient.OracleParameter("I_EnTest_Times", System.Data.OracleClient.OracleType.Int32);
            p2.Value = this.times;
            System.Data.OracleClient.OracleParameter p3 = new System.Data.OracleClient.OracleParameter("I_Fk_Time_Name", "COLUMN0");
            System.Data.OracleClient.OracleParameter p4 = new System.Data.OracleClient.OracleParameter("I_Object_Table_Name", this.dataScriptRule.DesTable);
            System.Data.OracleClient.OracleParameter p5 = new System.Data.OracleClient.OracleParameter("I_Source_Table_Name", tableName);
            System.Data.OracleClient.OracleParameter p6 = new System.Data.OracleClient.OracleParameter("O_Return_Int", System.Data.OracleClient.OracleType.Int32);
            p6.Direction = ParameterDirection.Output;
            System.Data.OracleClient.OracleParameter p7 = new System.Data.OracleClient.OracleParameter("O_Return_String", System.Data.OracleClient.OracleType.VarChar, 100);
            p7.Direction = ParameterDirection.Output;
            ps[0] = p1;
            ps[1] = p2;
            ps[2] = p3;
            ps[3] = p4;
            ps[4] = p5;
            ps[5] = p6;
            ps[6] = p7;

            log.Info(string.Format("BetchLogic > updateDbByID > 更新数据:{0} -> {1}", tableName, this.dataScriptRule.DesTable));
            SendMessageEvent(string.Format("更新数据:{0} -> {1} , {2}", tableName, this.dataScriptRule.DesTable, DateTime.Now.ToString("HH:mm:ss")));

            object[] prout = OracleHelper.ExcuteProc("Process_Date_Server.Process_Temp_Into_Oragin", 2, ps);

            log.Info(string.Format("BetchLogic > updateDbByID > 更新结束:{0} : {1} {2} {3}", this.dataScriptRule.DesTable, DateTime.Now.ToString("HH:mm:ss"), prout[0], prout[1]));
            SendMessageEvent(string.Format("更新结束:{0} : {1}", this.dataScriptRule.DesTable, DateTime.Now.ToString("HH:mm:ss")));

            SendCompleteEvent(prout[1].ToString());
            //TableDAL.DropTable(tableName);
        }

        private bool xls2db()
        {
            DataTable dt = ExcelImportHelper.GetDataTable(this.sourceFile);
            this.calColumnMap(dt);
            return insertDataTable(dt, structList, this.tableName);
             
        }
        private bool txt2db()
        {
            char separator = this.dataScriptRule.getColSeperatorChar();

            DataTable dt = TextImportHelper.GetDataTable(this.sourceFile, this.dataScriptRule.getColSeperatorChar());
            if (dt.Columns.Count <= 1) {
                log.Error(string.Format("BetchLogic > txt2db > 文件与规则的分隔符 [ {0} ] 不匹配", this.dataScriptRule.ColSperator));
                SendMessageEvent(false, string.Format("文件与规则的分隔符 [ {0} ] 不匹配", this.dataScriptRule.ColSperator));
                SendCompleteEvent("导入失败");
                return false;
            }

            this.calColumnMap(dt);

            
            DataTable dataTable = new DataTable();
            // 获取列头，创建表结构
            string[] columnNames = TextImportHelper.GetColumns(sourceFile, separator);

            if (columnNames.Length <= 1) {
                log.Error(string.Format("BetchLogic > txt2db > 文件与规则的分隔符 [ {0} ] 不匹配", this.dataScriptRule.ColSperator));
                SendMessageEvent(false, string.Format("文件与规则的分隔符 [ {0} ] 不匹配", this.dataScriptRule.ColSperator));
                SendCompleteEvent("导入失败");
                return false;
            }
             
            // 如果是temp表，去掉无用列
            if (tableName != this.dataScriptRule.DesTable)
            {
                dropColumn(tableName, columnNames, structList);
                log.Info(string.Format("BetchLogic > run > 判断临时表，去掉扩展列"));
            }

            for (int i = 0; i < columnNames.Length; i++)
            {
                DataColumn col = new DataColumn(columnNames[i].TrimEnd('\n').TrimEnd('\r'));
                dataTable.Columns.Add(col);
            }
            DateTime begin = DateTime.Now;
            Console.WriteLine(begin);

            log.Info(string.Format("BetchLogic > run > 开始导入:{0}", begin));

            StreamReader sr = new StreamReader(sourceFile, Encoding.Default);
            sr.ReadLine();

            int count = 0;
            while (true)
            {
                // 读一行数据
                string row = sr.ReadLine();
                // 空数据，结束读取
                if (string.IsNullOrEmpty(row))
                {
                    insertDataTable(dataTable, structList, tableName);
                    break;
                }
                row = row.Trim();
                // 根据分隔符取数据
                string[] columnDatas = row.Split(separator);
                // 创建一个新行
                DataRow dr = dataTable.NewRow();
                for (int i = 0; i < columnDatas.Length; i++)
                {
                    dr[i] = columnDatas[i];
                }
                dataTable.Rows.Add(dr);

                if (dataTable.Rows.Count >= 10000)
                {
                    insertDataTable(dataTable, structList, tableName);
                    dataTable.Rows.Clear();
                    log.Info(string.Format("BetchLogic > run > 凑够10000行写一次库 :{0}", count));
                    SendMessageEvent(string.Format("写入数据：{0} , 表 [ {1} ]", count, tableName));
                }
                count++;
            }
            sr.Close();

            log.Info(string.Format("BetchLogic > run > 全部写入完成:{0}", count));
            SendMessageEvent(string.Format("写完数据：{0} , 表 [ {1} ]", count, tableName));

            return true;
        }

        private void acc2db()
        {
            AccessImportHelper accHelper = new AccessImportHelper(this.sourceFile);

            DataTable dt = accHelper.getAllDataTable();
            this.calColumnMap(dt);

            insertDataTable(dt, structList, this.tableName);

            if (isUpdate)
            {
                // 这次是更新逻辑
                updateDbByID();
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

        private void dropColumn(string tableName, string[] txtColumnNames, List<Structure> structList)
        {
            foreach (var structure in structList)
            {
                switch (structure.ColumnName)
                {
                    case "PROJECTID":
                    case "CREATED_BY":
                    case "CREATION_DATE":
                    case "LAST_UPDATED_BY":
                    case "LAST_UPDATE_DATE":
                    case "TASKTIMES":
                    case "COLUMN0":
                        continue;
                }
                switch (structure.Comments)
                {
                    case "时间":
                        continue;
                }

                if (!txtColumnNames.Contains(structure.Comments))
                    TableDAL.dropColumn(tableName, structure.ColumnName);

                else if (!columnMap.Keys.Contains(structure.ColumnName))
                {
                    TableDAL.dropColumn(tableName, structure.ColumnName);
                }
            }
        }

        /// <summary>
        /// 不知道为什么有个半物力实验数据的上传逻辑
        /// </summary>
        private void uploadFile()
        {
            if (this.taskInfo != null)
            {
                var deliver = this.taskInfo.delivers.FirstOrDefault(it => it.deliverType == "半物理试验数据");
                // 半物理实验数据标志为以上传
                if (deliver != null)
                {
                    FileInfo finfo = new FileInfo(sourceFile);
                    string serverpath = string.Format(@"{0}\file{1}\{2}", System.Configuration.ConfigurationManager.AppSettings["deliverpath"],
                        deliver.deliverId, System.IO.Path.GetFileName(sourceFile));

                    WebHelper.addAtachFileInfo2Tdm(deliver.deliverId, System.IO.Path.GetFileName(sourceFile), (finfo.Length / 1024).ToString(), serverpath);
                    WebHelper.modifyTdmDeliveryListState(deliver.deliverId, "1");
                }
                // 修改完上传状态，释放
                TaskCenter.CurrentInfo = null;
            }
        }

        /// <summary>
        /// 当实验次数重复时候,是更新逻辑这里为true,那么注意一下表名,用临时表名
        /// </summary>
        /// <returns></returns>
        private bool checkTestTimes()
        {
            // 判断本次实验，有没有解析器执行过
            if (DataLogDAL.getList(this.taskInfo.id).Count(it => Convert.ToInt32(it.Version) == this.times) > 0)
            { 
                return true;
            }
            return false;
        }

        private DataLog createLog(int count, string impFileName)
        {
            DataLog dataLog = new DataLog();
            dataLog.FID = Guid.NewGuid().ToString().Replace("-", "");
            dataLog.ImpDateTime = DateTime.Now;
            dataLog.FullFloderName = string.Format("{0}{1}", dataLog.FID, System.IO.Path.GetExtension(sourceFile));
            dataLog.CreationDate = DateTime.Now;
            dataLog.LastUpdateDate = DateTime.Now;
            dataLog.ImpRowsCount = count;
            dataLog.TestProjectID = this.taskInfo.id;// ProjectList.FID;
            dataLog.ImpFileName = impFileName;
            dataLog.ObjectTable = this.dataScriptRule.DesTable;
            dataLog.LastUpdatedBy = this.userID;
            dataLog.CreatedBy = this.userID;
            dataLog.LastUpdateIp = "127.0.0.1";
            dataLog.TypeName = this.dataScript.FID;
            dataLog.Version = this.times;
            DataLogDAL.Insert(dataLog);
            return dataLog;
        }

        private OracleDbType getOracleDbType(string type)
        {
            switch (type)
            {
                case "DATE":
                    return OracleDbType.Date;
                case "NUMBER":
                    return OracleDbType.Single;
                default:
                    return OracleDbType.Varchar2;
            }
        }

        private object getRowValue(string txtColumnName, Structure structure, DataRow dataRow)
        {
            string value = dataRow[txtColumnName].ToString();
            if (string.IsNullOrEmpty(value))
                return null;

            if (MainWindow.HexColumns.Keys.Contains(txtColumnName))
            {
                try
                {
                    //处理16进制数据
                    int i = Convert.ToInt32(value, 16);

                    return i;
                }
                catch
                {
                    return 0;
                }
            }

            switch (structure.DataType)
            {
                case "DATE":
                    DateTime dt;
                    DateTime.TryParse(value, out dt);
                    return dt;
                case "NUMBER":
                    float f;
                    float.TryParse(value, out f);
                    return f;
                default:
                    return value;
            }
        }

        private bool insertDataTable(DataTable dataTable, List<Structure> structList, string tableName)
        { 
            allcount += dataTable.Rows.Count;

            List<string> columnNameList = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                columnNameList.Add(column.ColumnName);
            }

            List<OracleParameter> oParams = new List<OracleParameter>();
            Dictionary<string, object[]> oValues = new Dictionary<string, object[]>();
            Dictionary<string, Structure> structures = new Dictionary<string, Structure>();

            string sql_columns = "";
            string sql_values = "";

            foreach (string tablecolumnname in columnMap.Keys)
            {
                Structure structure = structList.FirstOrDefault(it => it.ColumnName == tablecolumnname);

                if (structure == null)
                    continue;

                string sourcecolumname = columnMap[tablecolumnname];

                if (!columnNameList.Contains(sourcecolumname)) // 导入数据中，没有这个字段
                    continue;

                // 搞出来SQL
                // 搞Parameter
                object[] oValue = new object[dataTable.Rows.Count];

                OracleParameter oParam = new OracleParameter(tablecolumnname, getOracleDbType(structure.DataType));
                oParam.Direction = ParameterDirection.Input;
                oParam.Value = oValue;

                oParams.Add(oParam);
                oValues[sourcecolumname] = oValue;
                structures[sourcecolumname] = structure;

                sql_columns += tablecolumnname + ",";
                sql_values += ":" + tablecolumnname + ",";
            }

            OracleParameter oParamCreateBy = new OracleParameter("CREATED_BY", OracleDbType.Varchar2);
            oParamCreateBy.Direction = ParameterDirection.Input;
            object[] CreateByValues = new object[dataTable.Rows.Count];
            oParamCreateBy.Value = CreateByValues;
            oParams.Add(oParamCreateBy);

            OracleParameter oParamCreationDate = new OracleParameter("CREATION_DATE", OracleDbType.Varchar2);
            oParamCreationDate.Direction = ParameterDirection.Input;
            object[] CreationDateValues = new object[dataTable.Rows.Count];
            oParamCreationDate.Value = CreationDateValues;
            oParams.Add(oParamCreationDate);

            OracleParameter oParamLastUpdateBy = new OracleParameter("LAST_UPDATED_BY", OracleDbType.Varchar2);
            oParamLastUpdateBy.Direction = ParameterDirection.Input;
            object[] ParamLastUpdateByValues = new object[dataTable.Rows.Count];
            oParamLastUpdateBy.Value = ParamLastUpdateByValues;
            oParams.Add(oParamLastUpdateBy);

            OracleParameter oParamLastUpdateDate = new OracleParameter("LAST_UPDATE_DATE", OracleDbType.Varchar2);
            oParamLastUpdateDate.Direction = ParameterDirection.Input;
            object[] LastUpdateDateValues = new object[dataTable.Rows.Count];
            oParamLastUpdateDate.Value = LastUpdateDateValues;
            oParams.Add(oParamLastUpdateDate);

            OracleParameter oParamProjectID = new OracleParameter("PROJECTID", OracleDbType.Varchar2);
            oParamProjectID.Direction = ParameterDirection.Input;
            object[] ProjectIDValues = new object[dataTable.Rows.Count];
            oParamProjectID.Value = ProjectIDValues;
            oParams.Add(oParamProjectID);

            OracleParameter oParamID = new OracleParameter("TASKTIMES", OracleDbType.Int32);
            oParamID.Direction = ParameterDirection.Input;
            int[] TaskTimesValues = new int[dataTable.Rows.Count];
            oParamID.Value = TaskTimesValues;
            oParams.Add(oParamID);

            string sql = string.Format("INSERT INTO {0}({1}CREATED_BY,LAST_UPDATED_BY,CREATION_DATE, LAST_UPDATE_DATE, PROJECTID,TASKTIMES) VALUES({2}:CREATED_BY,:LAST_UPDATED_BY,:CREATION_DATE,:LAST_UPDATE_DATE,:PROJECTID,:TASKTIMES)",
                tableName, sql_columns, sql_values);

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {

                foreach (string txtColumnName in structures.Keys)
                {
                    var list = oValues[txtColumnName];
                    var structure = structures[txtColumnName];

                    list[i] = getRowValue(txtColumnName, structure, dataTable.Rows[i]);
                }

                CreateByValues[i] = this.userID;
                CreationDateValues[i] = DateTime.Now;
                ParamLastUpdateByValues[i] = this.userID;
                LastUpdateDateValues[i] = DateTime.Now;
                ProjectIDValues[i] = this.taskInfo.id;
                TaskTimesValues[i] = this.times;
            }
            try
            {
                if (OracleAccess.ExecuteSqlBat(dataTable.Rows.Count, sql, oParams.ToArray()))
                {
                    successCount += dataTable.Rows.Count;
                }

                //TableDAL.DropTable(tableName);
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format("BetchLogic > run > insertDataTable > {0}", ex));
                SendMessageEvent(false, "遇到异常：" + ex.ToString());
            }
            baseline += dataTable.Rows.Count;
            int btcount = baseline;
             
            return true;
        }

    }
}
