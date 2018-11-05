using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess
{
    public class DataScriptRuleDAL
    {
        public static List<DataScriptRule> getList()
        {
            List<DataScriptRule> result = new List<DataScriptRule>();

            string sql = "SELECT * FROM MDS_IMP_DATA_SCRIPT_RULE";

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataScriptRule item = DataScriptRule.Parse(dr);
                result.Add(item);
            }

            return result;
        }

        public static DataScriptRule getInfo(string fid) {
            DataScriptRule result = null;

            string sql = string.Format("SELECT * FROM MDS_IMP_DATA_SCRIPT_RULE WHERE FID='{0}'", fid);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataScriptRule item = DataScriptRule.Parse(dr);
                result = item;
            } 
            return result;
        }

        public static int Insert(DataScriptRule item)
        {
            string sql = string.Format("INSERT INTO MDS_IMP_DATA_SCRIPT_RULE(FID,MDS_IMP_DATA_SCRIPT_ID,LINE_SEPERATOR,COL_SEPERATOR,COLNAME_LINES,DATA_LINES,DES_TABLE,REMARK,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,LAST_UPDATE_IP,VERSION,DES_FILE) ");
            sql += string.Format("VALUES('{0}','{1}','{2}','{3}',{4},{5},'{6}','{7}','{8}',to_date('{9}','yyyy/mm/dd hh24:mi:ss'),'{10}',to_date('{11}','yyyy/mm/dd hh24:mi:ss'),'{12}',{13},'{14}')",
                item.FID, item.MdsImpDataScriptID, item.LineSeperator, item.ColSperator, item.ColnameLines, item.DataLines, 
                item.DesTable, item.Remark, item.CreatedBy, 
                item.CreationDate.ToString("yyyy/MM/dd HH:mm:ss"), item.LastUpdatedBy, item.LastUpdateDate.ToString("yyyy/MM/dd HH:mm:ss"), item.LastUpdateIp, item.Version, item.DesFile);

            return OracleHelper.Excuse(sql); 
        }

        public static int Delete(string FID)
        {
            string sql = string.Format("DELETE FROM MDS_IMP_DATA_SCRIPT_RULE WHERE FID='{0}'", FID);
            return OracleHelper.Excuse(sql);
        }

        public static int update(DataScriptRule item)
        {
            string sql = string.Format("UPDATE MDS_IMP_DATA_SCRIPT_RULE SET ");
            sql += string.Format("LINE_SEPERATOR='{0}',", item.LineSeperator);
            sql += string.Format("COL_SEPERATOR='{0}',", item.ColSperator);
            sql += string.Format("COLNAME_LINES={0},", item.ColnameLines);
            sql += string.Format("DATA_LINES={0},", item.DataLines);
            sql += string.Format("DES_TABLE='{0}',", item.DesTable);
            sql += string.Format("DES_FILE='{0}',", item.DesFile); 
            sql += string.Format("REMARK='{0}',", item.Remark);
            sql += string.Format("LAST_UPDATE_DATE=to_date('{0}','yyyy/mm/dd hh24:mi:ss') ", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            sql += string.Format("WHERE FID='{0}' ", item.FID);
            return OracleHelper.Excuse(sql);
        }

        public static int updateBusinessPK(string fid, string businessPk) {
            string sql = string.Format("UPDATE MDS_IMP_DATA_SCRIPT_RULE SET ");
            sql += string.Format("DES_BUSINESS_PK='{0}'", businessPk);
            sql += string.Format("WHERE FID='{0}' ", fid);
            return OracleHelper.Excuse(sql);
        }
    }
}
