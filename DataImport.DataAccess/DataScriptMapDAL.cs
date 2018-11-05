using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess
{
    public class DataScriptMapDAL
    {
        public static List<DataScriptMap> getList()
        {
            List<DataScriptMap> result = new List<DataScriptMap>();

            string sql = "SELECT * FROM MDS_IMP_DATA_SCRIPT_MAP";

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataScriptMap item = DataScriptMap.Parse(dr);

                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 保存数据更新值转换解析器
        /// </summary>
        private static void SaveScriptFile(DataScriptMap entity)
        {
            if (!Directory.Exists("script"))
            {
                Directory.CreateDirectory("script");
            }
            if (File.Exists(string.Format("script/{0}.py", entity.FID))) { 
                File.Delete(string.Format("script/{0}.py", entity.FID));
            }
            FileStream fs = new FileStream(string.Format("script/{0}.py", entity.FID), FileMode.OpenOrCreate);

            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(entity.TransferScript);
            sw.Flush();
            sw.Close();
        }

        public static void SaveScriptFile(string fid, string script) {
            if (!Directory.Exists("script"))
            {
                Directory.CreateDirectory("script");
            }

            if (File.Exists(string.Format("script/{0}.py", fid)))
            {
                File.Delete(string.Format("script/{0}.py", fid));
            }

            FileStream fs = new FileStream(string.Format("script/{0}.py", fid), FileMode.OpenOrCreate);

            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(script);
            sw.Flush();
            sw.Close();
        }

        private static string LoadScriptFile(string fid)
        {
            if (File.Exists(string.Format("script/{0}.py", fid)))
            {
                FileStream fs = new FileStream(string.Format("script/{0}.py", fid), FileMode.Open);

                StreamReader reader = new StreamReader(fs);
                string result = reader.ReadToEnd();

                reader.Close();
                fs.Close();

                return result;
            }
            else
            {
                return "";
            }
        }

        public static List<DataScriptMap> getList(string mdsID)
        {
            List<DataScriptMap> result = new List<DataScriptMap>();

            string sql = string.Format("SELECT * FROM MDS_IMP_DATA_SCRIPT_MAP WHERE MDS_IMP_DATA_SCRIPT_RULE_ID='{0}'", mdsID);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataScriptMap item = DataScriptMap.Parse(dr);
                result.Add(item);
            }

            return result;
        }

        public static int delAll(string mdsID)
        {
            string sql = string.Format("DELETE  FROM MDS_IMP_DATA_SCRIPT_MAP WHERE MDS_IMP_DATA_SCRIPT_RULE_ID='{0}'", mdsID);
            return OracleHelper.Excuse(sql);
        }

        public static int Insert(DataScriptMap item)
        {
            string sql = string.Format("INSERT INTO MDS_IMP_DATA_SCRIPT_MAP(FID,MDS_IMP_DATA_SCRIPT_RULE_ID,TABLE_COL_NAME,FILE_COL_NAME,TRANSFER_TYPE,INSERT_VALUE_METHOD,TRANSFER_SCRIPT,REMARK,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,LAST_UPDATE_IP,VERSION) ");
            sql += string.Format("VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',to_date('{9}','yyyy/mm/dd hh24:mi:ss'),'{10}',to_date('{11}','yyyy/mm/dd hh24:mi:ss'),'{12}',{13})",
                item.FID, item.MdsImpDataScriptRuleID, item.TableColName, item.FileColName, item.TransferType, item.InsertValueMethod,
                item.TransferScript.Replace("'", "''").Replace("\r\n", "'||chr(13)||chr(10)||'"), 
                item.Remark, item.CreatedBy, item.CreationDate.ToString("yyyy/MM/dd HH:mm:ss"), item.LastUpdatedBy, 
                item.LastUpdateDate.ToString("yyyy/MM/dd HH:mm:ss"), item.LastUpdateIp, item.Version);

            //SaveScriptFile(item);

            return OracleHelper.Excuse(sql);
        }

        public static void AutoScriptMap(string scriptid, DataTable source, string tablename, string userid) {

            delAll(scriptid);

            var structures = TableDAL.getTableStructure(tablename);

            foreach (DataColumn column in source.Columns) {
                var st = structures.FirstOrDefault(it => it.Comments == column.ColumnName);
                if (st != null)
                {
                    DataScriptMap map = new DataScriptMap();
                    map.FID = Guid.NewGuid().ToString().Replace("-", "");
                    map.MdsImpDataScriptRuleID = scriptid;
                    map.TableColName = st.ColumnName;
                    map.FileColName = column.ColumnName;
                    map.TransferType = "02";
                    map.CreatedBy = userid;
                    map.LastUpdatedBy = userid;
                    map.LastUpdateIp = "127.0.0.1";
                    Insert(map);
                }
            }
        }
    }
}
