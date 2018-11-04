using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.BLL
{
    public class AccessImportHelper
    {
        string filePath;
        string connString;

        public AccessImportHelper(string filePath)
        {
            this.filePath = filePath;
            this.connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath;
        }

        public DataTable getDataTable()
        {
            List<string> names = getTableNames();

            DataTable dt = new DataTable();

            int minRowCount = 3;

            List<DataTable> dts = new List<DataTable>();

            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];

                DataTable d = getDataTable(name, 3);

                dts.Add(d);

                foreach (DataColumn dc in d.Columns)
                {
                    if (i > 0 && dc.ColumnName == "时间")
                    {
                        continue;
                    }
                    dt.Columns.Add(dc.ColumnName, dc.DataType);
                }

                minRowCount = minRowCount < d.Rows.Count ? minRowCount : d.Rows.Count;
            }


            for (int r = 0; r < minRowCount; r++)
            {
                DataRow dr = dt.NewRow();

                foreach (var t in dts)
                {
                    foreach (DataColumn cd in t.Columns)
                    {
                        dr[cd.ColumnName] = t.Rows[r][cd.ColumnName];
                    }
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public DataTable getAllDataTable() {
            List<string> names = getTableNames();

            DataTable dt = new DataTable();

            int minRowCount = 99999999;

            List<DataTable> dts = new List<DataTable>();

            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];

                DataTable d = getDataTable(name);

                dts.Add(d);

                foreach (DataColumn dc in d.Columns)
                {
                    if (i > 0 && dc.ColumnName == "时间")
                    {
                        continue;
                    }
                    dt.Columns.Add(dc.ColumnName, dc.DataType);
                }

                minRowCount = minRowCount < d.Rows.Count ? minRowCount : d.Rows.Count;
            }


            for (int r = 0; r < minRowCount; r++)
            {
                DataRow dr = dt.NewRow();

                foreach (var t in dts)
                {
                    foreach (DataColumn cd in t.Columns)
                    {
                        dr[cd.ColumnName] = t.Rows[r][cd.ColumnName];
                    }
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public List<string> getColumnNames() {
            List<string> result = new List<string>();

            DataTable dt = getDataTable();
            foreach (DataColumn dc in dt.Columns) {
                result.Add(dc.ColumnName);
            }

            return result;
        }

        private DataTable getDataTable(string name, int rows)
        {
            OleDbConnection conn = new OleDbConnection(this.connString);
            DataTable dt = new DataTable();

            try
            {
                OleDbCommand cmd = conn.CreateCommand();

                cmd.CommandText = string.Format("select top {1} * from {0}", name, rows);
                conn.Open();
                OleDbDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        string columnName = dr.GetName(i);
                        if(columnName.ToLower() != "id")
                            dt.Columns.Add(columnName);
                    }
                    dt.Rows.Clear();
                }
                while (dr.Read())
                {
                    DataRow row = dt.NewRow();
                    foreach (DataColumn dc in dt.Columns) {
                        row[dc.ColumnName] = dr[dc.ColumnName];
                    } 
                    dt.Rows.Add(row);
                }

                dr.Close();
                cmd.Dispose();
            }
            catch (System.Exception ex)
            {
                conn.Close();
            }

            foreach (DataColumn column in dt.Columns)
            {
                column.ColumnName = column.ColumnName == "时间" ? column.ColumnName : name + "." + column.ColumnName.Replace("#", ".");
            }

            return dt;

        }

        
        public DataTable getDataTable(string name)
        {
            OleDbConnection conn = new OleDbConnection(this.connString);
            DataTable dt = new DataTable();

            try
            {
                OleDbCommand cmd = conn.CreateCommand();

                cmd.CommandText = "select * from " + name;
                conn.Open();
                OleDbDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        dt.Columns.Add(dr.GetName(i));
                    }
                    dt.Rows.Clear();
                }
                while (dr.Read())
                {
                    DataRow row = dt.NewRow();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        row[i] = dr[i];
                    }
                    dt.Rows.Add(row);
                }

                dr.Close();
                cmd.Dispose();
            }
            catch (System.Exception ex)
            {
                conn.Close();
            }

            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.ToLower() != "id") 
                    column.ColumnName = column.ColumnName == "时间" ? column.ColumnName : name + "." + column.ColumnName.Replace("#", ".");
            }

            dt.Columns.Remove("ID");

            return dt;
        }

        public List<string> getTableNames()
        {
            List<string> result = new List<string>();
            OleDbConnection conn = new OleDbConnection(this.connString);
            conn.Open();
            DataTable cnSch = conn.GetSchema("tables");
            conn.Close();

            foreach (DataRow dr in cnSch.Rows)
            {
                foreach (DataColumn column in cnSch.Columns)
                {
                    if (dr[column].ToString() == "TABLE")
                    {
                        result.Add(dr["TABLE_NAME"].ToString());
                    }
                }
            }

            return result;
        }
    }
}
