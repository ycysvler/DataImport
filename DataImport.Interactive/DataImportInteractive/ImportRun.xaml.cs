using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys;
using DataImport.Interactive.TaskInfoInteractive;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting; 
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DataImport.Interactive.DataImportInteractive
{
    /// <summary>
    /// ImportRun.xaml 的交互逻辑
    /// </summary>
    public partial class ImportRun : UserControl
    {
        public ImportRun()
        {
            InitializeComponent();
            progresssChanged += ImportRun_progresssChanged;
            this.Loaded += ImportRun_Loaded;
        }
        FlowDocument fd = new FlowDocument();

        void ImportRun_Loaded(object sender, RoutedEventArgs e)
        {
            outLog.Document = fd;
            title.Text = string.Format("您已选择导入文件：{0} ,正在校验数据合法性，请稍后！", sourceFile);

            runTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Test();
        }

        void ImportRun_progresssChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            progressBar.Value = e.NewValue;
            int nvalue = (int)e.NewValue;
            nvalue = nvalue >= (int)progressBar.Maximum ? (int)progressBar.Maximum : nvalue;
            currentIndex.Text = nvalue.ToString();
        }

        public DataScript DataScript { get; set; }
        public DataScriptRule DataScriptRule { get; set; }
        public String sourceFile { get; set; }
        public String fileType { get; set; }
        public Dictionary<string, string> ColumnMap { get; set; }
        public Dictionary<string, string> ScriptMap { get; set; }

        private void btRun_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(runTime.Text.Trim()))
            {
                DateTime time;
                if(DateTime.TryParse(runTime.Text, out time)){

                    double s = (time - DateTime.Now).TotalSeconds;

                    while (s > 0) {
                        Thread.Sleep(1000);
                        s = (time - DateTime.Now).TotalSeconds;
                        runTime.Text = s.ToString();
                    }
                    btRun.IsEnabled = false;
                    btRun.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Buttons/button_disable.png")));
                    outLog.Document.Blocks.Clear();
                    Run();

                }
                else{
                    MessageBox.Show("执行时间格式不正确！");
                }
            }
            else
            {  
                btRun.IsEnabled = false; 
                btRun.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Buttons/button_disable.png"))); 
                outLog.Document.Blocks.Clear();
                Run();

            }

        }

        event RoutedPropertyChangedEventHandler<double> progresssChanged;

        int baseline = 0;
        int allcount = 0;
        int successCount = 0;

        private bool checkDataTable(DataTable dataTable, List<Structure> structList, string pk)
        {
            // 理论上的表的主键
            string pkcolumnName = System.Configuration.ConfigurationManager.AppSettings["pk"];

            if (!dataTable.Columns.Contains(pkcolumnName))
            {
                pkcolumnName = this.ColumnMap["COLUMN0"];
            }

            // 循环插入数据
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                DataRow dr = dataTable.Rows[i];



                Dictionary<Structure, string> insertValue = new Dictionary<Structure, string>();

                StringBuilder stringBuilder = new StringBuilder();

                foreach (string tablecolumnname in ColumnMap.Keys.OrderBy(it => it))
                {
                    Structure structure = structList.Single(it => it.ColumnName == tablecolumnname);

                    string sourcecolumname = ColumnMap[tablecolumnname];

                    string value = dr[sourcecolumname].ToString();

                    insertValue.Add(structure, value);

                    // 如果一行的原始数据没有送进来 
                    stringBuilder.Append(dr[sourcecolumname].ToString());
                    stringBuilder.Append("\t");
                }

                Structure spk = new Structure() { ColumnName = pk, DataType = "VARCHAR2", DataLength = "40" };
                string svalue = Guid.NewGuid().ToString().Replace("-", "");
                insertValue[spk] = svalue;

                string err = "";

                string timeValue = dr[pkcolumnName].ToString();
                if (timeKey.ContainsKey(timeValue))
                {
                    err += string.Format("行{2}:({0})字段有重复数据：{1}\t", System.Configuration.ConfigurationManager.AppSettings["pk"], timeValue, baseline + i + 1);
                }
                else
                {
                    timeKey[timeValue] = timeValue;
                }

                foreach (var item in structList)
                {
                    if (!insertValue.ContainsKey(item) && !item.NullAble)
                    {
                        err += string.Format("{0} 字段为必填项\t", item.Comments);
                    }
                }
                foreach (var item in insertValue.Keys)
                {
                    if (item.DataType.Equals("NVARCHAR2") && item.DataType.Equals("VARCHAR2") && item.DataType.Equals("CHAR") && int.Parse(item.DataLength) < insertValue[item].Length)
                        err += string.Format("{0} 字段内容超长\t", item.Comments);

                    switch (item.DataType.ToUpper())
                    {
                        case "DATE":
                            if (string.IsNullOrEmpty(insertValue[item]))
                            {

                                break;
                            }

                            DateTime temp;
                            if (!DateTime.TryParse(insertValue[item], out temp))
                            {
                                err += string.Format("{0} 字段内容({1})无法转换时间格式\t", item.Comments, insertValue[item]);
                            }
                            break;
                        case "LONG":
                        case "NUMBER":
                            if (string.IsNullOrEmpty(insertValue[item]))
                            {

                                break;
                            }

                            float number;

                            if (!float.TryParse(insertValue[item], out number))
                            {
                                try
                                {
                                    Convert.ToInt32(insertValue[item], 16);
                                }
                                catch
                                {
                                    err += string.Format("{0} 字段内容({1})无法转数字\t", item.Comments, insertValue[item]);
                                }

                            }
                            break;
                    }

                    string value = insertValue[item].ToString();
                }

                //添加一行数据
                Dispatcher.BeginInvoke((Delegate)new Action(() =>
                {
                    if (!string.IsNullOrEmpty(err))
                    {
                        fd.Blocks.Add(new Paragraph(new Run(stringBuilder.ToString())));
                        Paragraph paragraph = new Paragraph();
                        Run runflg = new Run(err);
                        runflg.Foreground = new SolidColorBrush(Colors.Red);
                        paragraph.Inlines.Add(runflg);
                        fd.Blocks.Add(paragraph);
                        fd.Blocks.Add(new Paragraph(new Run("---------------------------------------------------------------------------------------------------------")));

                    }
                }));

                if (!string.IsNullOrEmpty(err))
                    return false;
            }
            Dispatcher.BeginInvoke((Delegate)new Action(() =>
                {
                    Paragraph paragraph = new Paragraph();
                    baseline += dataTable.Rows.Count;
                    Run runflg = new Run(baseline.ToString());
                    runflg.Foreground = new SolidColorBrush(Colors.Green);
                    paragraph.Inlines.Add(runflg);
                    fd.Blocks.Add(paragraph);

                    if (fd.Blocks.Count > 100)
                    {
                        fd.Blocks.Clear();
                    }
                }));

            return true;
        }

        Hashtable timeKey = new Hashtable();


        private void Test()
        {
            timeKey = new Hashtable();

            btRun.IsEnabled = false;
            btRun.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Buttons/button_disable.png")));


            baseline = 0;
            outLog.Document.Blocks.Clear();
            // 拿到主键
            String pk = TableDAL.getTablePK(DataScriptRule.DesTable);

            // 拿到表结构
            List<Structure> structList = TableDAL.getTableStructure(DataScriptRule.DesTable);
            // 源数据
            DataTable dataTable;
            // 读取数据
            fileType = DataScript.FileType;

            //progressBar.Maximum = dataTable.Rows.Count;
            //allCount.Text = dataTable.Rows.Count.ToString();

            Thread thread = new Thread(new ThreadStart(() =>
            {
                if (fileType == "mdb") {
                    AccessImportHelper helper = new AccessImportHelper(sourceFile);
                    dataTable = helper.getAllDataTable();
                }
                else if (fileType == "xls/xlsx")
                {
                    dataTable = ExcelImportHelper.GetDataTable(sourceFile);
                    if (!checkDataTable(dataTable, structList, pk))
                    {
                        MessageBox.Show("数据检查失败，请修正数据后重新导入");
                        return;
                    }
                }
                else
                {

                    char separator = DataScriptRule.getColSeperatorChar();
                    dataTable = new DataTable();
                    // 获取列头，创建表结构
                    string[] columnNames = TextImportHelper.GetColumns(sourceFile, separator);

                    for (int i = 0; i < columnNames.Length; i++)
                    {
                        DataColumn col = new DataColumn(columnNames[i].TrimEnd('\n').TrimEnd('\r'));
                        dataTable.Columns.Add(col);
                    }

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
                            checkDataTable(dataTable, structList, pk);
                            break;
                        }

                        // 先拿10行，然后每1000行拿一行
                        if (count < 10 || count % 1000 == 1)
                        {
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
                        }
                        count++;

                        if (dataTable.Rows.Count >= 10000)
                        {
                            if (!checkDataTable(dataTable, structList, pk))
                            {
                                sr.Close();
                                MessageBox.Show("数据检查失败，请修正数据后重新导入");

                                return;
                            }

                            dataTable.Rows.Clear();
                        }
                    }
                    sr.Close();

                }

                Dispatcher.BeginInvoke((Delegate)new Action(() =>
                {
                    btRun.IsEnabled = true;
                    btRun.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Buttons/button.png")));

                    title.Text = string.Format("数据检查完毕，请点击导入数据按钮！");

                }));

                MessageBox.Show("数据检查完毕，请点击导入数据按钮！");
            }));

            thread.Start();
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

        private bool insertDataTable2(DataTable dataTable, List<Structure> structList, string pk, string tableName)
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

            foreach (string tablecolumnname in ColumnMap.Keys)
            {
                Structure structure = structList.Single(it => it.ColumnName == tablecolumnname);
                string sourcecolumname = ColumnMap[tablecolumnname];

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

                CreateByValues[i] = MainWindow.UserID;
                CreationDateValues[i] = DateTime.Now;
                ParamLastUpdateByValues[i] = MainWindow.UserID;
                LastUpdateDateValues[i] = DateTime.Now;
                ProjectIDValues[i] = TaskCenter.TaskID;
                TaskTimesValues[i] = TaskCenter.TaskTimes;
            }
            try
            {
                if (OracleAccess.ExecuteSqlBat(dataTable.Rows.Count, sql, oParams.ToArray()))
                {
                    successCount += dataTable.Rows.Count;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(sql);
                MessageBox.Show(ex.ToString());
            }
            baseline += dataTable.Rows.Count;
            int btcount = baseline;

            Dispatcher.BeginInvoke((Delegate)new Action(() =>
            {
                Paragraph paragraph = new Paragraph();
                Run runflg = new Run(btcount.ToString());
                runflg.Foreground = new SolidColorBrush(Colors.Green);
                paragraph.Inlines.Add(runflg);
                fd.Blocks.Add(paragraph);

                if (fd.Blocks.Count > 100)
                {
                    fd.Blocks.Clear();
                }
            }));


            return true;
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


        private bool insertDataTable3(DataTable dataTable, List<Structure> structList, string pk)
        {
            allcount += dataTable.Rows.Count;

            List<string> columnNameList = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                columnNameList.Add(column.ColumnName);
            }

            List<string> sqls = new List<string>();


            using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["oracle"].ConnectionString))
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                OracleTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;

                try
                {

                    // 循环插入数据
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {

                        // 这个错误字符串作用于有点长，在计算插入值得时候，可能出错，在插库的时候，可能出错
                        string err = "";

                        DataRow dr = dataTable.Rows[i];
                        Dictionary<Structure, string> insertValue = new Dictionary<Structure, string>();

                        StringBuilder stringBuilder = new StringBuilder();

                        // 制造了一条插入数据
                        foreach (string tablecolumnname in ColumnMap.Keys)
                        {
                            Structure structure = structList.Single(it => it.ColumnName == tablecolumnname);
                            string sourcecolumname = ColumnMap[tablecolumnname];

                            if (!columnNameList.Contains(sourcecolumname)) // 导入数据中，没有这个字段
                                continue;
                            string value = dr[sourcecolumname].ToString();
                            // 构造每一列数据
                            insertValue.Add(structure, value);
                        }

                        try
                        {
                            // 如果转换数据时候已经出错，就不操作了
                            if (string.IsNullOrEmpty(err))
                            {
                                // projectid
                                Structure projectid = new Structure() { ColumnName = System.Configuration.ConfigurationManager.AppSettings["projectid_columnname"], DataType = "VARCHAR2" };
                                insertValue[projectid] = TaskCenter.TaskID;

                                Structure tasktimes = new Structure() { ColumnName = "TASKTIMES", DataType = "NUMBER" };
                                insertValue[tasktimes] = TaskCenter.TaskTimes.ToString();

                                int count = 0;

                                if (!string.IsNullOrEmpty(DataScriptRule.DesBusinessPk))
                                {
                                    cmd.CommandText = TableDAL.getUpdateSql(DataScriptRule.DesTable, insertValue, DataScriptRule.DesBusinessPk + "," + projectid.ColumnName);
                                    count = cmd.ExecuteNonQuery();
                                    if (count > 0) successCount++;
                                }
                                // 更新操作，没有更新，需要插入数据
                                if (count == 0)
                                {
                                    cmd.CommandText = TableDAL.getInsertSql(DataScriptRule.DesTable, insertValue, MainWindow.UserID);
                                    count = cmd.ExecuteNonQuery();
                                    if (count > 0) successCount++;
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            err = e.Message;

                            Dispatcher.BeginInvoke((Delegate)new Action(() =>
                            {
                                Paragraph paragraph = new Paragraph();
                                Run runflg = new Run(err);
                                runflg.Foreground = new SolidColorBrush(Colors.Red);
                                paragraph.Inlines.Add(runflg);
                                fd.Blocks.Add(paragraph);
                            }));
                        }
                    }

                    tx.Commit();

                }
                catch (System.Data.OracleClient.OracleException e)
                {
                    tx.Rollback();
                }
                finally
                {
                    conn.Close();
                }
            }
            baseline += dataTable.Rows.Count;

            int btcount = baseline;

            Dispatcher.BeginInvoke((Delegate)new Action(() =>
            {
                Paragraph paragraph = new Paragraph();

                Run runflg = new Run(btcount.ToString());
                runflg.Foreground = new SolidColorBrush(Colors.Green);
                paragraph.Inlines.Add(runflg);
                fd.Blocks.Add(paragraph);

                if (fd.Blocks.Count > 100)
                {
                    fd.Blocks.Clear();
                }
            }));

            return true;
        }

        private bool insertDataTable(DataTable dataTable, List<Structure> structList, string pk)
        {
            allcount += dataTable.Rows.Count;

            List<string> columnNameList = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                columnNameList.Add(column.ColumnName);
            }

            List<string> sqls = new List<string>();

            // 循环插入数据
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                // 这个错误字符串作用于有点长，在计算插入值得时候，可能出错，在插库的时候，可能出错
                string err = "";

                DataRow dr = dataTable.Rows[i];
                Dictionary<Structure, string> insertValue = new Dictionary<Structure, string>();

                StringBuilder stringBuilder = new StringBuilder();

                // 制造了一条插入数据
                foreach (string tablecolumnname in ColumnMap.Keys)
                {
                    Structure structure = structList.Single(it => it.ColumnName == tablecolumnname);
                    string sourcecolumname = ColumnMap[tablecolumnname];

                    if (!columnNameList.Contains(sourcecolumname)) // 导入数据中，没有这个字段
                        continue;
                    string value = dr[sourcecolumname].ToString();

                    // 通过算式计算value的值
                    //if (ScriptMap.ContainsKey(structure.ColumnName) && !string.IsNullOrEmpty(ScriptMap[structure.ColumnName]))
                    //{
                    //    value = scriptExec(structure.ColumnName, structure.DataType, value, ref err);
                    //}
                    // 构造每一列数据
                    insertValue.Add(structure, value);
                }

                try
                {
                    // 如果转换数据时候已经出错，就不操作了
                    if (string.IsNullOrEmpty(err))
                    {
                        // projectid
                        Structure projectid = new Structure() { ColumnName = System.Configuration.ConfigurationManager.AppSettings["projectid_columnname"], DataType = "VARCHAR2" };
                        insertValue[projectid] = TaskCenter.TaskID;

                        Structure tasktimes = new Structure() { ColumnName = "TASKTIMES", DataType = "NUMBER" };
                        insertValue[tasktimes] = TaskCenter.TaskTimes.ToString();

                        int count = 0;

                        if (!string.IsNullOrEmpty(DataScriptRule.DesBusinessPk))
                        {
                            // 更新
                            count = TableDAL.update(DataScriptRule.DesTable, insertValue, DataScriptRule.DesBusinessPk + "," + projectid.ColumnName);
                            if (count > 0)
                            {
                                // 记录一条更新成功
                                successCount++;
                            }
                        }
                        // 更新操作，没有更新，需要插入数据
                        if (count == 0)
                        {
                            // 插入主键
                            if (!string.IsNullOrEmpty(pk))
                            {
                                Structure spk = new Structure() { ColumnName = pk, DataType = "VARCHAR2" };
                                string svalue = Guid.NewGuid().ToString().Replace("-", "");
                                insertValue[spk] = svalue;
                            }
                            // 插入 
                            sqls.Add(TableDAL.getInsertSql(DataScriptRule.DesTable, insertValue, MainWindow.UserID));
                        }
                    }
                }
                catch (System.Exception e)
                {
                    err = e.Message;

                    Dispatcher.BeginInvoke((Delegate)new Action(() =>
                    {
                        Paragraph paragraph = new Paragraph();
                        Run runflg = new Run(err);
                        runflg.Foreground = new SolidColorBrush(Colors.Red);
                        paragraph.Inlines.Add(runflg);
                        fd.Blocks.Add(paragraph);
                    }));
                }
            }
            if (sqls.Count > 0)
                successCount += OracleHelper.Excuse(sqls);

            Dispatcher.BeginInvoke((Delegate)new Action(() =>
            {
                Paragraph paragraph = new Paragraph();
                baseline += dataTable.Rows.Count;
                Run runflg = new Run(baseline.ToString());
                runflg.Foreground = new SolidColorBrush(Colors.Green);
                paragraph.Inlines.Add(runflg);
                fd.Blocks.Add(paragraph);

                if (fd.Blocks.Count > 100)
                {
                    fd.Blocks.Clear();
                }
            }));

            return true;
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

                else if (!ColumnMap.Keys.Contains(structure.ColumnName))
                {
                    TableDAL.dropColumn(tableName, structure.ColumnName);
                }
            }
        }

        private void Run()
        {
            baseline = 0;
            allcount = 0;
            successCount = 0;

            title.Text = string.Format("数据正在导入，请稍后！");

            fd.Blocks.Clear();
            try
            {
                // 拿到主键 (废弃逻辑)
                String pk = "";
                // 拿到表结构
                List<Structure> structList = TableDAL.getTableStructure(DataScriptRule.DesTable);

                // 判断是第几次执行（任务ID,试验次数，）
                // 得到业务主键

                Thread thread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        var r = DataScriptRule.DesBusinessPk;
                        string tableName = DataScriptRule.DesTable;


                        // 判断本次实验，有没有解析器执行过
                        if (DataLogDAL.getList(TaskCenter.TaskID).Count(it => Convert.ToInt32(it.Version) == TaskCenter.TaskTimes) > 0)
                        {

                            Structure st = structList.FirstOrDefault(it => it.Comments == System.Configuration.ConfigurationManager.AppSettings["pk"]);
                            if (st != null)
                            {
                                DataScriptRule.DesBusinessPk = st.ColumnName;


                                tableName += "_" + DateTime.Now.Millisecond.ToString();
                                if (tableName.Length > 30)
                                {
                                    tableName = tableName.Remove(0, tableName.Length - 29);
                                }

                                TableDAL.createtempTable(DataScriptRule.DesTable, tableName);
                                TableDAL.AddIndex(tableName, string.Format("{0},TASKTIMES,PROJECTID", st.ColumnName));
                            }
                        }

                        DataTable dataTable = null;
                        TimeSpan span = new TimeSpan();

                        // 写入导入数据日志
                        DataLog dataLog = createLog(0, System.IO.Path.GetFileName(sourceFile));
                        // 上传文件 
                        try
                        {
                            //SendMessageEvent("开始上传文件：" + sourceFile);
                            WebHelper.uploadFile(sourceFile, dataLog.FID, TaskCenter.TaskID);
                            //SendMessageEvent("上传文件结束! ");
                        }
                        catch (System.Exception uex)
                        {
                            //SendMessageEvent(false, "上传文件异常! " + uex.Message);
                        }


                        if (TaskCenter.CurrentInfo != null)
                        {
                            var deliver = TaskCenter.CurrentInfo.delivers.FirstOrDefault(it => it.deliverType == "半物理试验数据");
                            // 半物理实验数据标志为以上传
                            if (deliver != null)
                            {
                                FileInfo finfo = new FileInfo(sourceFile);
                                string serverpath = string.Format(@"{0}\groupDeliver\file{1}\{2}", System.Configuration.ConfigurationManager.AppSettings["deliverpath"],
                                    deliver.deliverId, System.IO.Path.GetFileName(sourceFile));

                                WebHelper.addAtachFileInfo2Tdm(deliver.deliverId, System.IO.Path.GetFileName(sourceFile), (finfo.Length / 1024).ToString(), serverpath);
                                WebHelper.modifyTdmDeliveryListState(deliver.deliverId, "1");
                            }
                            // 修改完上传状态，释放
                            TaskCenter.CurrentInfo = null;
                        }
                        DateTime begin = DateTime.Now;

                        if (fileType == "mdb")
                        {
                            AccessImportHelper helper = new AccessImportHelper(sourceFile);
                            dataTable = helper.getAllDataTable();
                            insertDataTable2(dataTable, structList, pk, tableName);
                        }
                        else if (fileType == "xls/xlsx")
                        {
                            dataTable = ExcelImportHelper.GetDataTable(sourceFile);
                            if (!insertDataTable(dataTable, structList, pk))
                            {
                                MessageBox.Show("数据插入失败，部分数据未能保存！");
                                return;
                            }
                        }
                        else
                        {

                            char separator = DataScriptRule.getColSeperatorChar();
                            dataTable = new DataTable();
                            // 获取列头，创建表结构
                            string[] columnNames = TextImportHelper.GetColumns(sourceFile, separator);

                            // 如果是temp表，去掉无用列
                            if (tableName != DataScriptRule.DesTable)
                            {
                                dropColumn(tableName, columnNames, structList);
                            }

                            for (int i = 0; i < columnNames.Length; i++)
                            {
                                DataColumn col = new DataColumn(columnNames[i].TrimEnd('\n').TrimEnd('\r'));
                                dataTable.Columns.Add(col);
                            }
                            
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
                                    insertDataTable2(dataTable, structList, pk, tableName); 
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
                                    insertDataTable2(dataTable, structList, pk, tableName); 
                                    dataTable.Rows.Clear();
                                }
                            }
                            sr.Close();

                        }

                        if (!string.IsNullOrEmpty(DataScriptRule.DesBusinessPk))
                            {
                                Dispatcher.BeginInvoke((Delegate)new Action(() =>
                                {
                                    fd.Blocks.Clear();

                                    Paragraph paragraph = new Paragraph();
                                    Run runflg = new Run("开始合并数据");
                                    runflg.Foreground = new SolidColorBrush(Colors.Green);
                                    paragraph.Inlines.Add(runflg);
                                    fd.Blocks.Add(paragraph);
                                }));

                                System.Data.OracleClient.OracleParameter[] ps = new System.Data.OracleClient.OracleParameter[7];
                                System.Data.OracleClient.OracleParameter p1 = new System.Data.OracleClient.OracleParameter("I_EnTest_Work_Id", TaskCenter.TaskID);
                                System.Data.OracleClient.OracleParameter p2 = new System.Data.OracleClient.OracleParameter("I_EnTest_Times", System.Data.OracleClient.OracleType.Int32);
                                p2.Value = TaskCenter.TaskTimes;
                                System.Data.OracleClient.OracleParameter p3 = new System.Data.OracleClient.OracleParameter("I_Fk_Time_Name", DataScriptRule.DesBusinessPk);
                                System.Data.OracleClient.OracleParameter p4 = new System.Data.OracleClient.OracleParameter("I_Object_Table_Name", DataScriptRule.DesTable);
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


                                Dispatcher.BeginInvoke((Delegate)new Action(() =>
                                {

                                    Paragraph paragraph = new Paragraph();


                                    Run runflg = null;
                                    if (Convert.ToInt32(prout[0]) == 1)
                                    {
                                        runflg = new Run(string.Format("{0}", prout[1]));
                                        runflg.Foreground = new SolidColorBrush(Colors.Green);
                                    }
                                    else
                                    {
                                        runflg = new Run(string.Format("{0}", prout[1]));
                                        runflg.Foreground = new SolidColorBrush(Colors.Red);
                                    }
                                    paragraph.Inlines.Add(runflg);
                                    fd.Blocks.Add(paragraph);
                                }));
                            }

                            DateTime end = DateTime.Now;
                            Console.WriteLine(end);

                            span = end - begin;

                            Console.WriteLine(span.TotalSeconds);

                       

                        // 算一下，一共插入了多少列
                        List<Structure> strucs = new List<Structure>();
                        foreach (string tablecolumnname in ColumnMap.Keys)
                        {
                            Structure structure = structList.Single(it => it.ColumnName == tablecolumnname);
                            strucs.Add(structure);
                        }
                        // 把列数据记录到数据库
                        ColumInfoDAL.Insert(DataScriptRule.DesTable, DataScriptRule.FID, MainWindow.UserID, strucs);


                        // 显示最终提示
                        Dispatcher.BeginInvoke((Delegate)new Action(() =>
                        {
                            title.Text = string.Format("数据导入完成，原始文件总记录：（{0}条），导入成功：（{1}条），耗时：（{2}秒）", allcount, successCount, span.TotalSeconds);
                        }));

                        // 调用web services
                        // 暂时屏蔽掉了，服务器上没这个接口【20170913】
                        //upLoad();

                        MessageBox.Show("导入数据结束");

                        Dispatcher.BeginInvoke((Delegate)new Action(() =>
                        {
                            try {
                                TaskInfoList mapModify = new TaskInfoList();
                                MainWindow window = App.Current.MainWindow as MainWindow;
                                window.StartPage(mapModify);
                            }
                            catch (System.Exception ex) {

                                LogHelper.WriteLog(ex.ToString());
                                MessageBox.Show(ex.ToString());
                            }
                           
                        }));
                    }
                    catch (System.Exception e1)
                    {
                        LogHelper.WriteLog(e1.ToString());
                        MessageBox.Show(e1.ToString());
                        
                    }
                }));

                thread.Start();
            }
            catch (System.Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
            }
        }
        /// <summary>
        /// 调用webServices
        /// </summary>
        private void upLoad()
        {

            string serviceresult = "";
            try
            {
                Task task = new Task(() => {
                    serviceresult = WebHelper.ImportComplete(sourceFile);
                });

                task.Start();            
            }
            catch (System.Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 执行解析器
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="dataType"></param>
        /// <param name="value"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        private string scriptExec(string columnName, string dataType, string value, ref string err)
        {
            try
            {
                ScriptRuntime pyRuntime = Python.CreateRuntime();
                dynamic obj = pyRuntime.UseFile(string.Format("script/{0}.py", columnName));

                if (dataType == "LONG" || dataType == "NUMBER")
                {
                    value = Convert.ToString(obj.TransferScript(Convert.ToDouble(value)));
                }
                else
                {
                    value = Convert.ToString(obj.TransferScript(value));
                }
            }
            catch (System.Exception e)
            {
                err += string.Format("{0} 字段计算式内容有误，无法完成转换\t", columnName);
            }
            return value;
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
            dataLog.TestProjectID = TaskCenter.TaskID;// ProjectList.FID;
            dataLog.ImpFileName = impFileName;
            dataLog.ObjectTable = DataScriptRule.DesTable;
            dataLog.LastUpdatedBy = MainWindow.UserID;
            dataLog.CreatedBy = MainWindow.UserID;
            dataLog.LastUpdateIp = "127.0.0.1";
            dataLog.TypeName = TaskCenter.ScriptID;
            dataLog.Version = TaskCenter.TaskTimes;
            DataLogDAL.Insert(dataLog);
            return dataLog;
        }

        /// <summary>
        /// 读取文件，获取数据
        /// </summary>
        /// <param name="fileType">文件类型</param>
        /// <returns></returns>
        private DataTable getDataTable(string fileType)
        {
            fileType = DataScript.FileType;

            if (fileType == "xls/xlsx")
            {
                return ExcelImportHelper.GetDataTable(sourceFile);
            }
            else
            {
                return TextImportHelper.GetDataTable(sourceFile, DataScriptRule.getColSeperatorChar());
            }
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            ProjectList mapModify = new ProjectList();
            MainWindow window = App.Current.MainWindow as MainWindow;
            window.StartPage(mapModify);
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                TextRange tr = new TextRange(outLog.Document.ContentStart, outLog.Document.ContentEnd);
                sw.Write(tr.Text);
                sw.Close();
                fs.Close();
                MessageBox.Show("日志保存成功！");
            }
        }

        private void btTest_Click(object sender, RoutedEventArgs e)
        {
            Test();
        }

        private void prvButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;
            UIElement item = ImportStack.Pop();
            window.StartPage(item);
        }
    }

}
