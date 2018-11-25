using DataImport.DataAccess.Entitys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess
{
    public class TableDAL
    {
        public static List<string> getTableNames()
        {

            List<string> result = new List<string>();

            string sql = "select TABLE_NAME from user_tables ";

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                result.Add(row[0].ToString());
            }

            return result;
        }

        public static int removeDataRow(string tableName, string projectId, int times) {

            string sql = string.Format("delete from {0} where PROJECTID ='{1}' and TASKTIMES={2}", tableName, projectId, times);
            return OracleHelper.Excuse(sql);
        }

        public static string getTablePK(string tableName)
        {
            string sql = string.Format(@"select m.column_name from user_constraints s, user_cons_columns m   
             where upper(m.table_name)='{0}' and m.table_name=s.table_name  
             and m.constraint_name=s.constraint_name and s.constraint_type='P'", tableName);

            DataSet ds = OracleHelper.Query(sql);

            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0][0].ToString();
            }
            return "";
        }

        public static List<Structure> getTableStructure(string tableName)
        {
            List<Structure> result = new List<Structure>();

            string sql = string.Format(@"select t.table_name,t.column_name,t.data_type,t.data_length,t.nullable,t.column_id,c.comments
       FROM user_tab_cols t, user_col_comments c
       WHERE upper(t.table_name)='{0}'   
             and c.table_name=t.table_name   
             and c.column_name=t.column_name   
             and t.hidden_column='NO'   
 order by t.column_id ", tableName);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                result.Add(Structure.Parse(row));
            }
            return result;
        }
        /// <summary>
        /// 设置主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        public static void SetPrimary(string tableName, string columnName)
        {
            //alter table TS modify(COLUMN1 not null)  
            string sql = string.Format("alter table {0} modify({1} not null)", tableName, columnName); 
            OracleHelper.Query(sql);

            //ALTER TABLE TS ADD CONSTRAINT tb_PRIMARY PRIMARY KEY  (COLUMN0)
            sql = string.Format("ALTER TABLE {0} ADD CONSTRAINT {0}_PK PRIMARY KEY ({1})", tableName, columnName);
            OracleHelper.Query(sql); 
        }
        public static void AddAttribute(string tableName, int count) {
            for (int index = 1; index <= count; index++)
            {
               string sql = string.Format("alter table {0} add ATTRIBUTE{1} number", tableName, index);
               OracleHelper.Query(sql); 
            }
        }

        public static void AddIndex(string tableName, string columnName) {
            string index = string.Format("i_{0}_{1}", columnName.Remove(0, columnName.Length - 3), tableName);
            if (index.Length > 30) {
                index = index.Remove(29);
            }
            string sql = string.Format("CREATE INDEX {2} ON {0} ({1}) ", tableName, columnName, index);
               OracleHelper.Query(sql);  
        }

        public static void DropTable(string tableName) {
            string sql = string.Format("drop table {0}", tableName);
            OracleHelper.Excuse(sql);
        }

        public static void AddColumn(string tableName, string columnName, string columnType) {
            string sql = "";
            switch (columnType)
            {
                case "number":
                    sql = string.Format("alter table {0} add {1} number", tableName, columnName);
                    break;
                default :
                    sql = string.Format("alter table {0} add {1} NVARCHAR2(48)", tableName, columnName);
                    break;

            }
             
            OracleHelper.Query(sql); 
        }
       
        public static void CreateTable(string tableName, DataTable source) {
            var structures = getTableStructure(tableName);
            float f;
            if (structures.Count == 0)
            {
                // new
                StringBuilder ct = new StringBuilder();
                
                ct.AppendFormat("create table {0} ", tableName);
                ct.AppendFormat("(");

                
                for (int i = 0; i < source.Columns.Count; i++) {

                    bool isnum = false;
                    if (float.TryParse(source.Rows[0][i].ToString(), out f))
                    {
                        isnum = true;
                    }
                    else
                    {
                        try
                        {
                            // 是个16进制的数字
                            Convert.ToInt32(source.Rows[0][i].ToString(), 16);
                            isnum = true;
                        }
                        catch { }
                    }
                    

                    if (isnum) {
                        ct.AppendFormat("COLUMN{0} NUMBER,", i);
                    }
                    else
                    {
                        ct.AppendFormat("COLUMN{0} NVARCHAR2(48),", i);
                    }
                }
                ct.AppendFormat("PROJECTID NVARCHAR2(48),"); 
                ct.AppendFormat("CREATED_BY NVARCHAR2(200),");
                ct.AppendFormat("CREATION_DATE NVARCHAR2(48),"); 
                ct.AppendFormat("LAST_UPDATED_BY NVARCHAR2(200),");
                ct.AppendFormat("LAST_UPDATE_DATE NVARCHAR2(48),"); 
                ct.AppendFormat("TASKTIMES NUMBER");
                ct.AppendFormat(")");
                
                OracleHelper.Query(ct.ToString());
                string comment;

                for (int i = 0; i < source.Columns.Count; i++)
                {
                     comment = string.Format("comment on column {0}.COLUMN{1} is '{2}'", tableName, i, source.Columns[i].ColumnName);
                    OracleHelper.Query(comment);

                    if (source.Columns[i].ColumnName == System.Configuration.ConfigurationManager.AppSettings["pk"]) {
                        AddIndex(tableName, string.Format("COLUMN{0},TASKTIMES,PROJECTID", i));
                    }
                }
                comment = string.Format("comment on column {0}.PROJECTID is '项目ID'", tableName);
                OracleHelper.Query(comment); 
                comment = string.Format("comment on column {0}.CREATED_BY is '创建人'", tableName);
                OracleHelper.Query(comment);
                comment = string.Format("comment on column {0}.CREATION_DATE is '创建时间'", tableName);
                OracleHelper.Query(comment);
                comment = string.Format("comment on column {0}.LAST_UPDATED_BY is '更新人'", tableName);
                OracleHelper.Query(comment);
                comment = string.Format("comment on column {0}.LAST_UPDATE_DATE is '更新时间'", tableName);
                OracleHelper.Query(comment);
                comment = string.Format("comment on column {0}.TASKTIMES is '实验次数'", tableName);
                OracleHelper.Query(comment);

               
               
            }
            else { 
                // alert
                Hashtable newCol = new Hashtable();
                foreach(DataColumn dc in source.Columns){
                    var st = structures.FirstOrDefault(it => it.Comments == dc.ColumnName);
                    if (st == null) {
                        newCol.Add(dc.ColumnName, source.Rows[0][dc].ToString());
                    }
                } 

                int i = 0;
                string newColString;

                int columnCount = structures.Count(it => it.ColumnName.IndexOf("COLUMN") > -1);

                foreach(string key in newCol.Keys){
                    int index = columnCount + i;
                    if (float.TryParse(newCol[key].ToString(), out f))
                    {
                        newColString = string.Format("alter table {0} add COLUMN{1} number", tableName, index);
                    }
                    else {
                        newColString = string.Format("alter table {0} add COLUMN{1} NVARCHAR2(48)", tableName, index);
                    }
                    OracleHelper.Query(newColString);

                    string comment = string.Format("comment on column {0}.COLUMN{1} is '{2}'", tableName, index, key);
                    OracleHelper.Query(comment);

                    i++;
                } 
            } 
        }

        public static int Comment(string tableName, string columnName, string comments) {

            string sql = string.Format("comment on column {0}.{1} is '{2}'", tableName, columnName, comments);
            return OracleHelper.Excuse(sql);
        }

        public static string getUpdateSql(string tablename, Dictionary<Structure, string> valueMap, string keys)
        {
            string sql;
            string set = "";
            string where = "";

            foreach (var key in valueMap.Keys)
            {
                string tempval = "";

                switch (key.DataType.ToUpper())
                {
                    case "DATE":
                        tempval = string.Format("to_date('{0}','yyyy/mm/dd hh24:mi:ss')", valueMap[key]);
                        break;
                    case "LONG":
                    case "NUMBER":
                        tempval = string.Format("{0}", valueMap[key]);
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case " NVARCHAR2":
                    default:
                        tempval = string.Format("'{0}'", valueMap[key]);
                        break;
                }

                if (keys.ToUpper().Contains(key.ColumnName.ToUpper()))
                {
                    where += string.Format(" {0}={1} and ", key.ColumnName, tempval);
                }
                else
                {
                    set += string.Format(" {0}={1},", key.ColumnName, tempval);
                }
            }
            set = set.Trim().TrimEnd(new char[] { ',' });

            if (where.Length > 0)
            {
                where = where.Remove(where.Length - 5);
            }

            sql = string.Format("UPDATE {0} SET {1} WHERE {2}", tablename, set, where);
            return sql;
        
        }

        public static int update(string tablename, Dictionary<Structure, string> valueMap, string keys)
        {
            string sql; 
            string set = "";
            string where = "";

            foreach (var key in valueMap.Keys)
            { 
                string tempval = "";

                switch (key.DataType.ToUpper())
                {
                    case "DATE":
                        tempval = string.Format("to_date('{0}','yyyy/mm/dd hh24:mi:ss')", valueMap[key]);
                        break;
                    case "LONG":
                    case "NUMBER":
                        tempval = string.Format("{0}", valueMap[key]);
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case " NVARCHAR2":
                    default:
                        tempval = string.Format("'{0}'", valueMap[key]);
                        break;
                }

                if (keys.ToUpper().Contains(key.ColumnName.ToUpper()))
                {
                    where += string.Format(" {0}={1} and ", key.ColumnName, tempval);
                }
                else
                {
                    set += string.Format(" {0}={1},", key.ColumnName, tempval); 
                }
            }
            set = set.Trim().TrimEnd(new char[] { ',' }); 

            if(where.Length > 0){
                where =  where.Remove(where.Length - 5);
            } 

            sql = string.Format("UPDATE {0} SET {1} WHERE {2}", tablename, set, where);

            return OracleHelper.Excuse(sql);
        }
        public static int Insert(string tablename, Dictionary<Structure, string> valueMap, string userid)
        {
            string sql;
            string col = "";
            string val = "";
            foreach (var key in valueMap.Keys)
            {
                col += key.ColumnName + ",";

                switch (key.DataType.ToUpper())
                {
                    case "DATE":
                        val += string.Format("to_date('{0}','yyyy/mm/dd hh24:mi:ss'),", valueMap[key]);
                        break;
                    case "LONG":
                    case "NUMBER":
                        val += string.Format("{0},", valueMap[key]);
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case " NVARCHAR2":
                    default:
                        val += string.Format("'{0}',", valueMap[key]);
                        break;
                }
            }

            col = col.TrimEnd(new char[] { ',' });
            val = val.TrimEnd(new char[] { ',' });

            sql = string.Format("INSERT INTO {0} ({1},created_by,last_updated_by,CREATION_DATE,LAST_UPDATE_DATE) VALUES({2},'{3}','{3}','{4}','{4}')", tablename, col, val, userid, DateTime.Now.ToString());

            return OracleHelper.Excuse(sql);
        }
        /// <summary>
        /// 用已有表结构，创建一个新表
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="tempTable"></param>
        /// <returns></returns>
        public static int createtempTable(string sourceTable, string tempTable) {

            string sql = string.Format("create table {1} as select * from {0} where 1<>1", sourceTable, tempTable);
            return OracleHelper.Excuse(sql);
        }

        public static string getInsertSql(string tablename, Dictionary<Structure, string> valueMap, string userid)
        {
            string sql;
            string col = "";
            string val = "";
            foreach (var key in valueMap.Keys)
            {
                col += key.ColumnName + ",";

                switch (key.DataType.ToUpper())
                {
                    case "DATE":
                        if (string.IsNullOrEmpty(valueMap[key]))
                        {
                            val += string.Format("null,", valueMap[key]);
                        }
                        else {
                            val += string.Format("to_date('{0}','yyyy/mm/dd hh24:mi:ss'),", valueMap[key]);
                        }
                       
                        
                        break;
                    case "LONG":
                    case "NUMBER":
                        if (string.IsNullOrEmpty(valueMap[key]))
                        {
                            val += string.Format("null,", valueMap[key]);
                        }
                        else
                        {
                            val += string.Format("{0},", valueMap[key]);
                        }
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case " NVARCHAR2":
                    default:
                        val += string.Format("'{0}',", valueMap[key]);
                        break;
                }
            }

            col = col.TrimEnd(new char[] { ',' });
            val = val.TrimEnd(new char[] { ',' });

            sql = string.Format("INSERT INTO {0} ({1},created_by,last_updated_by,CREATION_DATE,LAST_UPDATE_DATE) VALUES({2},'{3}','{3}','{4}','{4}')", tablename, col, val, userid, DateTime.Now.ToString());

            return sql;
        }


        public static int dropColumn(string tableName, string columnName) {
            string sql = string.Format("ALTER TABLE {0} DROP COLUMN {1}", tableName, columnName);
            return OracleHelper.Excuse(sql);
        }
    }
}
