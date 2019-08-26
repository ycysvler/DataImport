
using DataImport.BLL;
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
 

            var r = SQLiteImportHelper.GetDataTable(dbPath);
        }

        
    }
}
