using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataImport.DataAccess.Entitys
{
    public class Project
    {
        public string FID{get;set;}
        public string ProjectName{get;set;}
        public string ProjectCode{get;set;} 
        public string TaskNo { get; set; }
        public string TaskCode{get;set;}
        public string TaskName { get; set; }
        public string TestTimes { get; set; }
        public string UserCode { get; set; }
        public string ScriptID { get; set; }

        public static Project Parse(DataRow row)
        {
            Project result = new Project();

            result.FID = row["task_id"].ToString();
            result.ProjectName = row["project_name"].ToString();
            result.ProjectCode = row["project_code"].ToString();
            result.TaskNo = row["task_no"].ToString();
            result.TaskCode = row["task_code"].ToString();
            result.TaskName = row["task_name"].ToString();
            result.TestTimes = row["Test_times"].ToString();
            result.UserCode = row["user_code"].ToString();
            result.ScriptID = row["mds_imp_data_script_id"].ToString(); 
            
            return result;
        }

        public static Project Parse(XElement element)
        {
            Project result = new Project();

            result.FID = element.Element("id").Value;
            result.ProjectName = element.Element("project_name").Value;
            result.ProjectCode = element.Element("project_code").Value;

            return result;
        }
    }
}
