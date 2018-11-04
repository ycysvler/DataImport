using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess
{
    public class ProjectDAL
    {
        public static List<Project> getList()
        {
            List<Project> result = new List<Project>();

            string sql = "SELECT * FROM pm_task_info_view";

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                Project item = Project.Parse(dr);
                result.Add(item);
            }

            return result;
        }

        public static Project getInfo(String FID) {
            Project result = null;

            string sql = String.Format("SELECT * FROM pm_task_info_view WHERE TASK_ID='{0}'", FID);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                result = Project.Parse(dr); 
            }

            return result;
        }
    }
}
