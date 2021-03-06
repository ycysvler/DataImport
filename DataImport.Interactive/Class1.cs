﻿using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys; 
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* 
 1、文件名解析字段
 2、userID 获取 
 4、access 文件转txt文件
     */
namespace DataImport.Interactive
{ 
    public class Class1
    {
        TaskInfo taskInfo = null;
        DataScript dataScript = null;
        DataScriptRule dataScriptRule = null;
        List<Structure> structList = new List<Structure>();
        Dictionary<string, string> columnMap = new Dictionary<string, string>();
        string sourceFile = ""; // 本次的导入数据文件
        string userID = "";
        int baseline = 0;
        int times = 0;
        int allcount = 0;
        int successCount = 0;


        bool isUpdate = false;  // 当试验次数重复时候,是更新逻辑这里为true,那么注意一下表名,用临时表名

        string tableName = "";

        public Class1(string userID,string userName, string projectCode, string taskCode, string scriptCode, int times, string sourceFile)
        {
            this.userID = userID;
            var taskInfoList = WebHelper.listTdmTasks(userName);
            this.taskInfo = taskInfoList.FirstOrDefault(it => it.taskCode == taskCode); 
            
            // "b1190f6a66a94b6eb110b3ffc0739887";
            string fid = scriptCode2Fid(scriptCode);
            this.dataScript = DataScriptDAL.getInfo(fid);
            this.dataScriptRule = DataScriptRuleDAL.getInfo(fid);
            this.times = times;
            this.sourceFile = sourceFile;
            this.columnMap = getColumnMap();
            this.calColumnMap();

            if (this.dataScriptRule != null)
            {
                this.structList = TableDAL.getTableStructure(this.dataScriptRule.DesTable);
            }
        }

        private string scriptCode2Fid(string code) {
            string result = "";
            var item = DataScriptDAL.getList().LastOrDefault(it => it.MidsScriptCode == code);
            if (item != null) {
                result = item.FID;
            }

            return result;
        }

        /// <summary>
        /// 动态计算对应关系
        /// </summary>
        private void calColumnMap() {
            // 获取文件表头
            List<string> headers = getHeaders();
            List<Structure> structures = TableDAL.getTableStructure(this.dataScriptRule.DesTable);

            foreach (string colName in headers) {
                // 文件字段在对应关系里面不存在                
                if (!this.columnMap.ContainsValue(colName)) {
                    var structure = structures.FirstOrDefault(it => it.Comments == colName);
                    if (structure == null)
                    {
                        // 文件字段在表备注中不存在
                        modifyComment(structures, colName);
                    }
                    else {
                        this.columnMap[structure.ColumnName] = structure.Comments;
                    }
                } 
            } 
        }

        private void modifyComment(List<Structure> structures, string fileColumn) {
            // 表有空余字段
            var structure = structures.FirstOrDefault(it => string.IsNullOrEmpty(it.Comments));

            if (structure != null) {
                // 更新备注到数据库里面去
                TableDAL.Comment(this.dataScriptRule.DesTable, structure.ColumnName, fileColumn);
                // 更新表备注
                structure.Comments = fileColumn;
                // columnMap增加记录
                this.columnMap[structure.ColumnName] = fileColumn;
            }
        }

        private List<string> getHeaders() {
            List<string> result = new List<string>(); 
            DataTable dt = new DataTable();
            string fileType = System.IO.Path.GetExtension(this.sourceFile);
            if (fileType == ".xls" || fileType == ".xlsx")
            {
                dt = ExcelImportHelper.GetDataTable(this.sourceFile);
            }
            else
            {
                dt = TextImportHelper.GetDataTable(this.sourceFile, this.dataScriptRule.getColSeperatorChar());
            }

            foreach (DataColumn column in dt.Columns) {
                result.Add(column.ColumnName);
            }
            return result;
        }


        public void run()
        {
            this.tableName = this.dataScriptRule.DesTable;
            this.isUpdate = checkTestTimes();

            // 写入导入数据日志
            DataLog dataLog = createLog(0, System.IO.Path.GetFileName(sourceFile));
            // 上传文件
            WebHelper.uploadFile(sourceFile, dataLog.FID, TaskCenter.TaskID);
            // 一个处理半物力试验数据的逻辑
            uploadFile();

            string fileType = System.IO.Path.GetExtension(this.sourceFile);
             
            if (fileType == ".xls" || fileType == ".xlsx")
            {
                xls2db();
            }
            else
            {
                txt2db();
            }

            if (isUpdate)
            {
                // 这次是更新逻辑
                updateDbByID();
            }

        }

        private void updateDbByID()
        {
            System.Data.OracleClient.OracleParameter[]
            ps = new System.Data.OracleClient.OracleParameter[7];
            System.Data.OracleClient.OracleParameter p1 = new System.Data.OracleClient.OracleParameter("I_EnTest_Work_Id", this.taskInfo.id);
            System.Data.OracleClient.OracleParameter p2 = new System.Data.OracleClient.OracleParameter("I_EnTest_Times", System.Data.OracleClient.OracleType.Int32);
            p2.Value = this.times;
            System.Data.OracleClient.OracleParameter p3 = new System.Data.OracleClient.OracleParameter("I_Fk_Time_Name", this.dataScriptRule.DesBusinessPk);
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
            object[] prout = OracleHelper.ExcuteProc("Process_Date_Server.Process_Temp_Into_Oragin", 2, ps);
        }

        private void xls2db()
        {
            DataTable dataTable = ExcelImportHelper.GetDataTable(sourceFile);
            if (!insertDataTable(dataTable, structList, this.tableName))
            {
                return;
            }
        }
        private void txt2db()
        {
            char separator = this.dataScriptRule.getColSeperatorChar();
            DataTable dataTable = new DataTable();
            // 获取列头，创建表结构
            string[] columnNames = TextImportHelper.GetColumns(sourceFile, separator);

            // 如果是temp表，去掉无用列
            if (tableName != this.dataScriptRule.DesTable)
            {
                dropColumn(tableName, columnNames, structList);
            }

            for (int i = 0; i < columnNames.Length; i++)
            {
                DataColumn col = new DataColumn(columnNames[i].TrimEnd('\n').TrimEnd('\r'));
                dataTable.Columns.Add(col);
            }
            DateTime begin = DateTime.Now;
            Console.WriteLine(begin);
            StreamReader sr = new StreamReader(sourceFile, Encoding.Default);
            sr.ReadLine();

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

                if (dataTable.Rows.Count >= 1000)
                {
                    insertDataTable(dataTable, structList, tableName);
                    dataTable.Rows.Clear();
                }
            }
            sr.Close();
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
        /// 不知道为什么有个半物力试验数据的上传逻辑
        /// </summary>
        private void uploadFile()
        {
            if (this.taskInfo != null)
            {
                var deliver = this.taskInfo.delivers.FirstOrDefault(it => it.deliverType == "半物理试验数据");
                // 半物理试验数据标志为以上传
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
        /// 当试验次数重复时候,是更新逻辑这里为true,那么注意一下表名,用临时表名
        /// </summary>
        /// <returns></returns>
        private bool checkTestTimes()
        {
            // 判断本次试验，有没有解析器执行过
            if (DataLogDAL.getList(this.taskInfo.id).Count(it => Convert.ToInt32(it.Version) == this.times) > 0)
            {

                Structure st = structList.FirstOrDefault(it => it.Comments == System.Configuration.ConfigurationManager.AppSettings["pk"]);
                if (st != null)
                {
                    this.dataScriptRule.DesBusinessPk = st.ColumnName;


                    tableName += "_" + DateTime.Now.Millisecond.ToString();
                    if (tableName.Length > 30)
                    {
                        tableName = tableName.Remove(0, tableName.Length - 29);
                    }

                    TableDAL.createtempTable(this.dataScriptRule.DesTable, tableName);
                    TableDAL.AddIndex(tableName, string.Format("{0},TASKTIMES,PROJECTID", st.ColumnName));
                }
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
                Structure structure = structList.Single(it => it.ColumnName == tablecolumnname);
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
                //if (OracleAccess.ExecuteSqlBat(dataTable.Rows.Count, sql, oParams.ToArray()))
                //{
                //    successCount += dataTable.Rows.Count;
                //}
            }
            catch 
            {

            }
            baseline += dataTable.Rows.Count;
            int btcount = baseline;

            //Dispatcher.BeginInvoke((Delegate)new Action(() =>
            //{
            //    Paragraph paragraph = new Paragraph();
            //    Run runflg = new Run(btcount.ToString());
            //    runflg.Foreground = new SolidColorBrush(Colors.Green);
            //    paragraph.Inlines.Add(runflg);
            //    fd.Blocks.Add(paragraph);

            //    if (fd.Blocks.Count > 100)
            //    {
            //        fd.Blocks.Clear();
            //    }
            //}));


            return true;
        }

    }
}
