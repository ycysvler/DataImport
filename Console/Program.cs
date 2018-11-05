using DataImport.BLL;
using DataImport.DataAccess;
using DataImport.DataAccess.Entitys;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading; 

namespace Consoles
{
    class Program
    {
        static Program instance = new Program();

        static void Main(string[] args)
        {
            WriteLog100W.Write(); 


            return;

            StreamWriter sw = new StreamWriter("D:\\1.txt");
            for (int i = 1; i <= 50000; i++) {
                string s = string.Format("{0},12.5,5.57,13.7,0.7932,S50，S37,391,389,5515,77.9,5515,77.9,5515,77.9,5515,77.9,5515,77.9", i.ToString("00000"));
                sw.WriteLine(s);
            } 
            sw.Close();

            DateTime begin = DateTime.Now;
            System.Console.WriteLine(begin.ToString());

            
           

            List<string> sqls = new List<string>();

            sqls.Add("update TBL_TRAINGROUP_DATA1 set COLUMN1=10 WHERE COLUMN0='xx.34X5311'");

            OracleHelper.Excuse(sqls);

            //OracleAccess.BatInsert();

            //for (int i = 0; i < 10; i++) {

            //    List<string> sqlList = new List<string>();

            //    for (int j = 0; j < 10000; j++) {
            //        string sql = string.Format("insert into TBL_TRAINGROUP_DATA4(ID,COLUMN0,COLUMN1,COLUMN2,ATTRIBUTE1,ATTRIBUTE2) VALUES('{0}',{1},{2},{3},{4},{5})", Guid.NewGuid().ToString(), i * 1000 + j, i * 1000 + j, i * 1000 + j, j, j);
            //        sqlList.Add(sql);

            //        //Console.WriteLine(i * 1000 + j);

            //    }
            //    Console.WriteLine((i + 1) * 10000);

            //    OracleHelper.Excuse(sqlList);
            //}

            DateTime end = DateTime.Now;
            System.Console.WriteLine(end.ToString());

            TimeSpan span = end - begin;
            System.Console.WriteLine(span.TotalSeconds);

            System.Console.ReadLine();
            return;
            // 读取输入参数
            //if (instance.initArgs(args))
            //{
            //    instance.Run();
            //}
             
        }
        string projectfid, scriptid, filepath, userid;
        Dictionary<string, string> ColumnMap = new Dictionary<string, string>();
        Dictionary<string, string> ScriptMap = new Dictionary<string, string>();


        private bool initArgs(string[] args)
        {
            bool result = true;

            if (args.Length != 4)
            {
                System.Console.WriteLine("缺少参数------------------------------------------");
                System.Console.WriteLine("用法：console.exe projectid datascriptid filepath");
                System.Console.WriteLine("参数：");
                System.Console.WriteLine("    projectid\t\t项目编号");
                System.Console.WriteLine("    datascriptid\t解析器编号");
                System.Console.WriteLine("    filepath\t\t数据文件路径");
                System.Console.WriteLine("    userid\t\t本次操作的用户ID，用于写日志");
                return false;
            }

            projectfid = args[0];
            var project = ProjectDAL.getInfo(projectfid);
            if (project == null)
            {
                System.Console.WriteLine("项目'{0}'不存在，请检查参数", projectfid);
                result = false;
            }

            scriptid = args[1];
            var datascript = DataScriptDAL.getInfo(scriptid);
            if (datascript.FID == null)
            {
                System.Console.WriteLine("解析器'{0}'不存在，请检查参数", scriptid);
                result = false;
            }

            filepath = args[2];
            if (!File.Exists(filepath))
            {
                System.Console.WriteLine("数据文件'{0}'不存在，请检查参数", filepath);
                result = false;
            }
            userid = args[3];

            return result;
        }

        private void Run()
        {
            DataScriptRule rule = DataScriptRuleDAL.getInfo(scriptid);
            DataScript srcipt = DataScriptDAL.getInfo(scriptid);

            List<DataScriptMap> maps = DataScriptMapDAL.getList(scriptid);
            foreach (var map in maps)
            {
                ColumnMap[map.TableColName] = map.FileColName;
                ScriptMap[map.TableColName] = map.TransferScript;
            }

            // 拿到主键
            String pk = TableDAL.getTablePK(rule.DesTable);
            // 拿到表结构
            List<Structure> structList = TableDAL.getTableStructure(rule.DesTable);
            // 读取数据
            DataTable dataTable = getDataTable(srcipt, rule, filepath);
            int allcount = dataTable.Rows.Count;
            int successCount = 0;

            Thread thread = new Thread(new ThreadStart(() =>
           {
               // 写入导入数据日志
               DataLog dataLog = createLog(dataTable.Rows.Count, System.IO.Path.GetFileName(filepath),
                   projectfid, rule.DesTable);
               // 上传文件
               WebHelper.uploadFile(filepath, dataLog.FID, "aa");

               // 循环插入数据
               for (int i = 0; i < allcount; i++)
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
                       string value = dr[sourcecolumname].ToString();
                       // 通过算式计算value的值
                       if (ScriptMap.ContainsKey(structure.ColumnName) && !string.IsNullOrEmpty(ScriptMap[structure.ColumnName]))
                       {
                           value = scriptExec(structure.ColumnName, structure.DataType, value, ref err);
                       }
                       // 构造每一列数据
                       insertValue.Add(structure, value);

                       // 拼接了一条页面提示字符串
                       stringBuilder.Append(dr[sourcecolumname].ToString());
                       stringBuilder.Append("\t");
                   }
                   // 显示进度条
                   System.Console.WriteLine("-------------------------------------------------------------");
                   System.Console.WriteLine("第{0}条数据", i + 1);
                   System.Console.WriteLine(stringBuilder); 
                   

                   int result = 0;

                   DataLogItems dLogItem = new DataLogItems();
                   try
                   {
                       dLogItem.FID = Guid.NewGuid().ToString().Replace("-", "");
                       dLogItem.LogID = dataLog.FID;
                       dLogItem.Content = stringBuilder.ToString();
                       dLogItem.RowIndex = i;
                       dLogItem.ImpStatus = 0;

                       // 如果转换数据时候已经出错，就不操作了
                       if (string.IsNullOrEmpty(err))
                       {
                           // projectid
                           Structure projectid = new Structure() { ColumnName = System.Configuration.ConfigurationManager.AppSettings["projectid_columnname"], DataType = "VARCHAR2" };
                           insertValue[projectid] = projectfid;

                           int count = 0;

                           if (!string.IsNullOrEmpty(rule.DesBusinessPk))
                           {
                               // 更新
                               count = TableDAL.update(rule.DesTable, insertValue, rule.DesBusinessPk + "," + projectid.ColumnName);
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
                               result = TableDAL.Insert(rule.DesTable, insertValue, userid);
                               // 记录一条添加成功
                               successCount++;
                           }


                       }
                       else
                       {
                           dLogItem.ImpStatus = -1;
                           dLogItem.ErrorMsg = err;
                       }

                   }
                   catch (System.Exception e)
                   {
                       err = e.Message;

                       dLogItem.ImpStatus = -1;
                       dLogItem.ErrorMsg = e.Message;

                       System.Console.WriteLine("异常：{0}", err);
                   }

                   DataLogItemsDAL.Insert(dLogItem);
               }

               // 算一下，一共插入了多少列
               List<Structure> strucs = new List<Structure>();
               foreach (string tablecolumnname in ColumnMap.Keys)
               {
                   Structure structure = structList.Single(it => it.ColumnName == tablecolumnname);
                   strucs.Add(structure);
               }
               // 把列数据记录到数据库
               ColumInfoDAL.Insert(rule.DesTable, rule.FID, userid, strucs);

               System.Console.WriteLine("数据导入完成，原始文件总记录：（{0}条），导入成功：（{1}条）", allcount, successCount);

           }));

            thread.Start();
        }

        /// <summary>
        /// 读取文件，获取数据
        /// </summary>
        /// <param name="fileType">文件类型</param>
        /// <returns></returns>
        private DataTable getDataTable(DataScript srcipt, DataScriptRule rule, string sourceFile)
        {
            DataTable dt = new DataTable();
            try
            {
                string fileType = srcipt.FileType;

                if (fileType == "xls/xlsx")
                {
                    return ExcelImportHelper.GetDataTable(sourceFile);
                }
                else
                {
                    return TextImportHelper.GetDataTable(sourceFile, rule.getColSeperatorChar());
                }
            }
            catch(System.Exception e) {
                System.Console.WriteLine("解析数据文件异常，请检查数据文件是否符合格式要求---------------------");
                System.Console.WriteLine(e.Message);
            }
            return dt;
        }

        private DataLog createLog(int count, string impFileName, string projectid, string objecttable)
        {
            DataLog dataLog = new DataLog();
            dataLog.FID = Guid.NewGuid().ToString().Replace("-", "");
            dataLog.ImpDateTime = DateTime.Now;
            dataLog.FullFloderName = string.Format("{0}{1}", dataLog.FID, System.IO.Path.GetExtension(filepath));
            dataLog.CreationDate = DateTime.Now;
            dataLog.LastUpdateDate = DateTime.Now;
            dataLog.ImpRowsCount = count;
            dataLog.TestProjectID = projectid;
            dataLog.ImpFileName = impFileName;
            dataLog.ObjectTable = objecttable;
            dataLog.LastUpdatedBy = userid;
            dataLog.CreatedBy = userid;
            dataLog.LastUpdateIp = "127.0.0.1";
            DataLogDAL.Insert(dataLog);
            return dataLog;
        }
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
    }
}
