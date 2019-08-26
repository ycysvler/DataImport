using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace DataImport.DataAccess
{
    public class SQLiteHelper
    {
        ILog log = null; 
        string dbPath = "";

        public SQLiteHelper(string dbPath) {
            this.dbPath = dbPath;
            this.log = log4net.LogManager.GetLogger("Logger");
        }

        public List<string> TableNames() {
            DataSet ds = this.Query("select name from sqlite_master where type='table' order by name");

            List<string> result = new List<string>();

            foreach (DataRow dr in ds.Tables[0].Rows) {
                result.Add(dr["name"].ToString());
            }

            return result;
        }

        public DataTable GetReaderSchema(string tableName)
        {
            DataTable schemaTable = null;
            IDbCommand cmd = new SQLiteCommand();
            cmd.CommandText = string.Format("select * from [11时19分17秒088_A]", tableName);
             
            using (SQLiteConnection conn = new SQLiteConnection("data source=" + dbPath)) {
                conn.Open();
                cmd.Connection = conn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
                {
                    schemaTable = reader.GetSchemaTable();
                }
            }
             
            return schemaTable;
        }

        public DataSet Query(string sql) {
            try
            {
                log.Debug(string.Format("\t{0}", sql));
                DataSet result = new DataSet();
                SQLiteConnection conn = new SQLiteConnection("data source=" + dbPath); 
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, conn); 
                adapter.Fill(result);
                return result;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("exec:{0}", sql));
                throw ex;
            }
        }

        public int Excuse(string sql)
        { 
            log.Debug(string.Format("\t{0}", sql));
            sql = Regex.Replace(sql, @"[\n\r]", " ");

            int result = 0;
            SQLiteConnection conn = new SQLiteConnection("data source=" + dbPath);

            try
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                log.Debug(string.Format("\t{0}", ex.ToString()));
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return result; 
        }

        public object Scalar(string sql)
        {
            log.Debug(string.Format("\t{0}", sql));
            sql = Regex.Replace(sql, @"[\n\r]", " ");

            object result = null;
            SQLiteConnection conn = new SQLiteConnection("data source=" + dbPath);
            try
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                result = cmd.ExecuteScalar();
            }
            catch (System.Exception ex)
            {
                log.Debug(string.Format("\t{0}", ex.ToString()));
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }
    }
}
