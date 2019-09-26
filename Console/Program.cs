
using DataImport.BLL;
using DataImport.DataAccess;
using System;
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

            //SQLiteHelper sh = new SQLiteHelper(dbPath);
            //var r = SQLiteImportHelper.GetDataTable(dbPath);

            //string fileName = @"E:\work\wuxi\db.db";

            try
            {

            

            string fileName = @"c:\db.db";
            string userName = "shaozj";
            string userId = "4028b48152632a160152635092f7000e";
            string projectCode = "tdm2test";
            string taskCode = "1.1.1.2";
            string scriptCode = "trainGroup2019002";
            int times = 3;
            BetchLogic bl = new BetchLogic(
                   userId, userName,
                   projectCode, taskCode, scriptCode, times, fileName);

            if (bl.init())
            {
                bl.run();

                Console.WriteLine("完成了");
                
            }
            
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }

            Console.ReadLine();
        }

        
    }
}
