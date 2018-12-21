using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess
{
    public class DataScriptDAL
    {
        /// <summary>
        /// 任务下是否有配置script
        /// </summary>
        /// <param name="taskid"></param>
        /// <param name="scriptCode"></param>
        /// <returns></returns>
        public static int getTaskScriptCount(string taskid,string scriptCode) {
            string sql = string.Format("select count(1) from mpm.pm_task_info pti left join import.mds_imp_data_script s on instr(pti.resolver_task_id, s.fid) > 0 where pti.id='{0}' and s.mids_script_code='{1}'", taskid, scriptCode);

            int count = Convert.ToInt32( OracleHelper.Scalar(sql).ToString());
            return count;
        }


        public static List<DataScript> getList() {

            List<DataScript> result = new List<DataScript>();

            string sql = "SELECT * FROM MDS_IMP_DATA_SCRIPT";

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows) {
                DataScript item = DataScript.Parse(dr);
                result.Add(item);
            }

            return result;
        }

        public static List<string> getMapColName(string tableName)
        {
            List<string> result = new List<string>();
            string sql = string.Format("select TABLE_COL_NAME from MDS_IMP_DATA_SCRIPT_MAP WHERE MDS_IMP_DATA_SCRIPT_RULE_ID IN(select FID from MDS_IMP_DATA_SCRIPT t where t.table_name = '{0}')", tableName);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            { 
                result.Add(dr[0].ToString());
            }

            return result;
        }

        public static int updateLevel(string fid, string level) {
            string sql = string.Format("UPDATE  MDS_IMP_DATA_SCRIPT SET MIDS_SCRIPT_VERSION='{1}'  WHERE FID='{0}'",fid,level);
            return OracleHelper.Excuse(sql);
        }


        public static int updateInvalid(string fid, int Invalid )
        {
            string sql = string.Format("UPDATE  MDS_IMP_DATA_SCRIPT SET INVALID={1}  WHERE FID='{0}'", fid, Invalid);
            return OracleHelper.Excuse(sql);
        }

        public static DataScript getInfo(string fid) {
            DataScript result = new DataScript();

            string sql = string.Format("SELECT * FROM MDS_IMP_DATA_SCRIPT WHERE FID='{0}'",  fid);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataScript item = DataScript.Parse(dr);
                result = item;
            }

            return result;
        }

        public static int CheckCode(string code) {
            string sql = string.Format("SELECT COUNT(MIDS_SCRIPT_CODE) FROM MDS_IMP_DATA_SCRIPT WHERE MIDS_SCRIPT_CODE='{0}'", code);
            return Convert.ToInt32( OracleHelper.Scalar(sql).ToString());

        }

        public static int Release(string fid) {
            string sql = string.Format("UPDATE  MDS_IMP_DATA_SCRIPT SET RELEASE='02'  WHERE FID='{0}'", fid);
            return OracleHelper.Excuse(sql);
        }

        public static int Insert(DataScript item) {
            string sql = string.Format("INSERT INTO MDS_IMP_DATA_SCRIPT(FID,MIDS_SCRIPT_CODE,MIDS_SCRIPT_NAME,MIDS_SCRIPT_VERSION,FILE_TYPE,INDEX_KEY,VALID_FLAG,APPLY_TEST_PROJECT,REMARK,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,LAST_UPDATE_IP,VERSION,SCRIPT_TYPE,PROJECT_CODE,TASK_NAME,TABLE_NAME,RELEASE) ");
            sql += string.Format("VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',to_date('{10}','yyyy/mm/dd hh24:mi:ss'),'{11}',to_date('{12}','yyyy/mm/dd hh24:mi:ss'),'{13}',{14},{15},'{16}','{17}','{18}','01')",
                item.FID, item.MidsScriptCode, item.MidsScriptName, item.MidsScriptVesion, item.FileType, 
                item.IndexKey, item.ValidFlag, item.ApplyTestProject, item.Remark, item.CreatedBy, 
                item.CreationDate.ToString("yyyy/MM/dd HH:mm:ss"), item.LastUpdatedBy, 
                item.LastUpdateDate.ToString("yyyy/MM/dd HH:mm:ss"), item.LastUpdateIp, item.Version, item.ScriptType,item.ProjectCode, 
                item.TaskName, item.TableName);

            return OracleHelper.Excuse(sql);
        }

        public static int Delete(string FID) {
            string sql = string.Format("DELETE FROM MDS_IMP_DATA_SCRIPT WHERE FID='{0}'", FID);
            return OracleHelper.Excuse(sql);
        }

        public static int update(DataScript item) {
            string sql = string.Format("UPDATE MDS_IMP_DATA_SCRIPT SET ");
            sql += string.Format("MIDS_SCRIPT_CODE='{0}',", item.MidsScriptCode);
            sql += string.Format("MIDS_SCRIPT_NAME='{0}',", item.MidsScriptName);
            sql += string.Format("MIDS_SCRIPT_VERSION='{0}',", item.MidsScriptVesion);
            sql += string.Format("FILE_TYPE='{0}',", item.FileType);
            sql += string.Format("INDEX_KEY='{0}',", item.IndexKey);
            sql += string.Format("VALID_FLAG='{0}',", item.ValidFlag);
            sql += string.Format("APPLY_TEST_PROJECT='{0}',", item.ApplyTestProject);
            sql += string.Format("REMARK='{0}',", item.Remark);
            sql += string.Format("SCRIPT_TYPE={0},", item.ScriptType);
            sql += string.Format("PROJECT_CODE='{0}',", item.ProjectCode);
            sql += string.Format("TASK_NAME='{0}',", item.TaskName);
            sql += string.Format("TABLE_NAME='{0}',", item.TableName);
            sql += string.Format("INVALID={0},", item.Invalid);
            sql += string.Format("LAST_UPDATE_DATE=to_date('{0}','yyyy/mm/dd hh24:mi:ss')  ", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            sql += string.Format("WHERE FID='{0}' ", item.FID);
            return OracleHelper.Excuse(sql); 
        }
    }
}
