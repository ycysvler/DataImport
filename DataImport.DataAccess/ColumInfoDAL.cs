using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataImport.DataAccess
{
    public class ColumInfoDAL
    {
        public static int Insert(ColumInfo item)
        {
            string sql = string.Format("INSERT INTO MDS_IMP_COLUM_INFO(FID,PM_TASK_TIME_ID,TABLE_NAME,COL_NAME,REMARK,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,LAST_UPDATE_IP,VERSION) ");
            sql += string.Format("VALUES('{0}','{1}','{2}','{3}','{4}','{5}',to_date('{6}','yyyy/mm/dd hh24:mi:ss'),'{7}',to_date('{8}','yyyy/mm/dd hh24:mi:ss'),'{9}',{10})",
                item.FID, item.TaskTimeID, item.TableName, item.ColName,  item.Remark, item.CreatedBy, item.CreationDate, item.LastUpdatedBy, item.LastUpdateDate, item.LastUpdateIp, item.Version);

            return OracleHelper.Excuse(sql);
        }

        public static void Insert(String tableName,string taskID,string userid, List<Structure> structures) {

            foreach (var item in structures) {
                ColumInfo info = new ColumInfo();
                info.FID = Guid.NewGuid().ToString().Replace("-", "");
                info.TaskTimeID = taskID;
                info.TableName = tableName;
                info.ColName = item.ColumnName;
                info.Remark = item.Comments;
                info.CreatedBy = userid;
                info.CreationDate = DateTime.Now.ToString();
                info.LastUpdateDate = DateTime.Now.ToString();
                info.LastUpdatedBy = userid;
                info.LastUpdateIp = "";

                Insert(info);
            }
        } 
    }
}
 
