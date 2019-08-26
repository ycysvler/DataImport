
using DataImport.DataAccess;
using System.Data;
using System.Data.SQLite;

namespace Consoles
{
    class Program
    {
        static Program instance = new Program();

        static void Main(string[] args)
        {
            //WriteLog100W.Write(); 
            string dbPath = @"C:\sql\db格式数据示例.db";

            SQLiteHelper sh = new SQLiteHelper(dbPath);
              
            DataSet ds = sh.Query("select name from sqlite_master where type='table' order by name");


        }
    }
}
