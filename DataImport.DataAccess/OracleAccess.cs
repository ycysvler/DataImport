using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace DataImport.DataAccess
{
    public class OracleAccess
    {

        public static void BatInsert()
        {
            int recc = 100000;
            string[] p0 = new string[recc];
            float[] p1 = new float[recc];
            float[] p2 = new float[recc];

            for (int i = 0; i < recc; i++) {
                p0[i] = Guid.NewGuid().ToString().Replace("-", "");
                p1[i] = i;
                p2[i] = i; 
            }

            OracleParameter[] param = new OracleParameter[3];

            OracleParameter op0 = new OracleParameter("COLUMN0", OracleDbType.Varchar2);
            op0.Direction = ParameterDirection.Input;
            op0.Value = p0;
            param[0] = op0;

            OracleParameter op1 = new OracleParameter("COLUMN1", OracleDbType.Single);
            op1.Direction = ParameterDirection.Input;
            op1.Value = p1;
            param[1] = op1;

            OracleParameter op2 = new OracleParameter("COLUMN2", OracleDbType.Single);
            op2.Direction = ParameterDirection.Input;
            op2.Value = p2;
            param[2] = op2;

            string sql = "INSERT INTO TBL_KKX_010_DATA1(COLUMN0,COLUMN1,COLUMN2) VALUES(:COLUMN0,:COLUMN1,:COLUMN2)";
            ExecuteSqlBat(recc, sql, param);
        }

        public static bool ExecuteSqlBat(int recordCount, string sql, params OracleParameter[] param)
        {
            bool result = false;

            if (recordCount < 1)
                return false;
            string connectionString = ConfigurationManager.ConnectionStrings["oracle"].ConnectionString;
            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand command = new OracleCommand();
            command.Connection = conn;
            conn.Open();
            OracleTransaction tx = conn.BeginTransaction();
            try
            {
                //这个参数需要指定每次批插入的记录数  
                command.ArrayBindCount = recordCount;
                //用到的是数组,而不是单个的值,这就是它独特的地方  
                command.CommandText = sql;

                for (int i = 0; i < param.Length; i++)
                {
                    OracleParameter oparam = param[i];
                    command.Parameters.Add(oparam);
                }

                //这个调用将把参数数组传进SQL,同时写入数据库  
                command.ExecuteNonQuery();
                tx.Commit();

                result = true;
            }
            catch (Exception ex)
            {
                result = false;

                tx.Rollback();
                throw ex;

            }

            return result;
        
        }  


    }
}
