using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess
{
    public class DataLogItemsDAL
    {
        public static int Insert(DataLogItems item)
        {
            string sql = string.Format("INSERT INTO MDS_IMP_DATA_LOG_ITEMS(FID,LOGID,IMP_STATUS,ERROR_MSG,ROW_INDEX,CONTENT) ");
            sql += string.Format("VALUES('{0}','{1}',{2},'{3}',{4},'{5}')",
                item.FID, item.LogID, item.ImpStatus, item.ErrorMsg, item.RowIndex, item.Content);

            return OracleHelper.Excuse(sql);
        }
    }
}
