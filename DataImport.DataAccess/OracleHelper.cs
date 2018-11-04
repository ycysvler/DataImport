using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Text.RegularExpressions;
using log4net;
using System.Data.OracleClient;


namespace DataImport.DataAccess
{
    public class OracleHelper
    {
        static ILog log = log4net.LogManager.GetLogger("Logger");

        public static DataSet Query(string sql)
        {
            try
            {
                log.Debug(string.Format("\t{0}", sql));
                DataSet result = new DataSet();
                OracleDataAdapter adapter = new OracleDataAdapter(sql, ConfigurationManager.ConnectionStrings["oracle"].ConnectionString);

                adapter.Fill(result);
                return result;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("exec:{0}", sql));
                throw ex;
            }
        }

        public static int Excuse(string sql)
        {

            log.Debug(string.Format("\t{0}", sql));
            sql = Regex.Replace(sql, @"[\n\r]", " ");

            int result = 0;
            OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["oracle"].ConnectionString);
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(sql, conn);
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

        public static int Excuse(List<string> sqlList)
        {
            using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["oracle"].ConnectionString))
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                OracleTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    foreach (string sql in sqlList)
                    {

                        cmd.CommandText = sql;
                        int result = cmd.ExecuteNonQuery();
                        Console.WriteLine(string.Format(string.Format("{0} : {1}", result, sql.Remove(18))));
                    }
                    tx.Commit();

                    return sqlList.Count;
                }
                catch (System.Data.OracleClient.OracleException e)
                {
                    tx.Rollback();
                    return 0;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static object Scalar(string sql)
        {
            log.Debug(string.Format("\t{0}", sql));
            sql = Regex.Replace(sql, @"[\n\r]", " ");

            object result = null;
            OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["oracle"].ConnectionString);
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(sql, conn);
                result = cmd.ExecuteOracleScalar();
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

        public static int Excuse(string connstring, string sql)
        {

            log.Debug(string.Format("\t{0}", sql));

            sql = Regex.Replace(sql, @"[\n\r]", " ");

            int result = 0;
            OracleConnection conn = new OracleConnection(connstring);
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(sql, conn);
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

        public static object[] ExcuteProc(string strProcName, int ResultCount, params OracleParameter[] paras)
        {
            using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["oracle"].ConnectionString))
            {
                OracleCommand cmd = new OracleCommand(strProcName, conn);
                if (paras != null && paras.Length > 0)
                {
                    for (int j = 0; j < paras.Length; j++)
                    {
                        if (paras[j].Value == null)
                        {
                            paras[j].Value = DBNull.Value;
                        }
                    }
                }
                cmd.Parameters.AddRange(paras);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                cmd.ExecuteNonQuery();
                int i = 0; 
                object[] objResult = new object[ResultCount];
                foreach (OracleParameter p in cmd.Parameters)
                {
                    if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.InputOutput)
                    { 
                        objResult[i++] = p.Value;
                    }
                }
                return objResult;
            }
        }

    }
}
