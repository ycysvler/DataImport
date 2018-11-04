using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess.Entitys
{
    public class Structure
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string DataLength { get; set; }
        public bool NullAble { get; set; }
        public string ColumnID { get; set; }
        public string Comments { get; set; }
        public bool IsKey { get; set; }

        public override int GetHashCode()
        {
            return this.ColumnName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Structure target = obj as Structure;

            if (target == null)
                return false;
            else
                return this.ColumnName.Equals(target.ColumnName);
        }

        public static Structure Parse(DataRow row)
        {
            Structure result = new Structure();

            result.TableName = row["TABLE_NAME"].ToString();
            result.ColumnName = row["COLUMN_NAME"].ToString();
            result.DataType = row["DATA_TYPE"].ToString();
            result.DataLength = row["DATA_LENGTH"].ToString();
            result.NullAble = row["NULLABLE"].ToString() == "Y" ? true : false;
            result.ColumnID = row["COLUMN_ID"].ToString();
            result.Comments = row["COMMENTS"].ToString();
            //result.IsKey = row["COMMENTS"].ToString() == "1" ? true : false;

            return result;
        }
    }
}
