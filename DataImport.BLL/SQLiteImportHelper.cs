using DataImport.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataImport.BLL
{
    public class SQLiteImportHelper
    {
        public static string[] GetColumns(string filePath) {
            SQLiteHelper sh = new SQLiteHelper(filePath); 
            List<string> tableNames = sh.TableNames(); 
            DataTable dt = sh.GetReaderSchema(tableNames[0]);

            List<string> result = new List<string>();
            result.Add("时间");
            foreach(DataRow dr in dt.Rows){
                result.Add(dr["ColumnName"].ToString());
            } 
            return result.ToArray();
        }

        public static DataTable GetDataTable(string filePath) {
            SQLiteHelper sh = new SQLiteHelper(filePath);
            List<string> tableNames = sh.TableNames();  
            DataSet ds = sh.Query(string.Format("select * from [{0}] limit 3", tableNames[0]));
            DataTable dt = ds.Tables[0];
            dt.Columns.Add("时间");
            foreach (DataRow dr in dt.Rows) {
                dr["时间"] = dr["localTime"].ToString() + " " + dr["timeId"].ToString();
            }

            return dt;
        }
    }
}
