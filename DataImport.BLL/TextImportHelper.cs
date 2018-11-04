using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.BLL
{
    public class TextImportHelper
    {
        public static string[] GetColumns(string filePath, char separator) {
            StreamReader sr = new StreamReader(filePath, Encoding.Default);
            string headLine = sr.ReadLine();
            sr.Close();

            //List<string> columns = headLine.Trim().Split(new char[] { separator }).ToList();
            List<string> columns = SplitFuckString(headLine.Trim(), separator);
            columns[0] = "时间";
           for (int i = columns.Count - 1; i >= 0; i--) { 
            if(string.IsNullOrEmpty(columns[i])){
                columns.RemoveAt(i);
            }
           }

           return columns.ToArray();
        }

        /// <summary>
        /// 收到一个fsw10数据模板，这个模板简直日了狗了；
        /// 列头逗号分隔的，时而1个逗号，时而2个逗号，没有规律
        /// 数据更日，一共就两行，第二行正常数据后面有2W多个空格；
        /// 想起秋天飞去南方的大雁，一会排成S型，一会排成B型； 
        /// </summary>
        /// <param name="rowString"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<string> SplitFuckString(string rowString, char separator)
        {
            List<string> result = new List<string>();
            if (rowString[0] == separator)
            {
                return result;
            }


            StringBuilder sbder = new StringBuilder();

            for (int i = 0; i < rowString.Length; i++)
            {
                if (rowString[i] == '\0') {
                    break;
                }
                if (rowString[i] != separator)
                {
                    sbder.Append(rowString[i]);
                }
                else
                {
                    if (sbder.Length > 0)
                    {
                        result.Add(sbder.ToString());
                        sbder.Clear();
                    }
                }
            }

            if (sbder.Length > 0)
            {
                result.Add(sbder.ToString());
                sbder.Clear();
            }

            return result;
        }

        public static DataTable GetDataTable(string filePath, char separator)
        {
            DataTable dt = new DataTable();
            // 获取列头，创建表结构
            string[] columnNames = GetColumns(filePath, separator);

            for (int i = 0; i < columnNames.Length; i++) {
                DataColumn col = new DataColumn(columnNames[i].TrimEnd('\n').TrimEnd('\r'));
                dt.Columns.Add(col);
            }

            StreamReader sr = new StreamReader(filePath, Encoding.Default);
            sr.ReadLine();

            int count = 0;

            while (true) {
                // 读一行数据
                string row = sr.ReadLine(); 
                // 空数据，结束读取
                if (string.IsNullOrEmpty(row)) { break; }

                row = row.Trim();
                // 根据分隔符取数据
                //string[] columnDatas = row.Split(separator);
                string[] columnDatas = SplitFuckString(row,separator).ToArray();
                
                // 创建一个新行
                DataRow dr = dt.NewRow();
                for (int i = 0; i < columnDatas.Length; i++) {
                    dr[i] = columnDatas[i];
                }
                dt.Rows.Add(dr);

                count++;

                if (count >= 3)
                    break;
            }

            return dt;
        }
    }
}
