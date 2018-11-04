using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataImport.DataAccess
{
    public class ObjtableInfoDAL
    {
        public static int Insert(ObjtableInfo item)
        {
            string sql = string.Format("INSERT INTO MDS_IMP_OBJTABLE_INFO(FID,OBJECT_TABLE_CODE,OBJECT_TABLE_NAME,STATUS,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,LAST_UPDATE_IP,VERSION) ");
            sql += string.Format("VALUES('{0}','{1}','{2}','{3}','{4}',to_date('{5}','yyyy/mm/dd hh24:mi:ss'),'{6}',to_date('{7}','yyyy/mm/dd hh24:mi:ss'),'{8}',{9})",
                item.FID, item.ObjectTableCode, item.ObjectTableName, item.Status, item.CreatedBy, item.CreationDate, item.LastUpdatedBy, item.LastUpdateDate, item.LastUpdateIp, item.Version);

            return OracleHelper.Excuse(sql);
        }

        public static int Count(string tableName) {
            string sql = string.Format("SELECT COUNT(1) FROM MDS_IMP_OBJTABLE_INFO WHERE OBJECT_TABLE_CODE='{0}'", tableName);
            object count = OracleHelper.Scalar(sql);
            return Convert.ToInt32(count.ToString());
        }

        public static List<ObjtableInfo> getList()
        {
            List<ObjtableInfo> result = new List<ObjtableInfo>();

            string sql = "SELECT * FROM MDS_IMP_OBJTABLE_INFO";

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                ObjtableInfo item = ObjtableInfo.Parse(dr);
                result.Add(item);
            }

            return result;
        }
    }
}
