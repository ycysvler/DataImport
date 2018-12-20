using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataImport.DataAccess.Entitys
{
    public class TaskInfo
    {
        /// <summary>
        /// 用于记录是否已经加载扩展信息
        /// </summary>
        public bool loaded { get; set; }

        public string id { get; set; }
        public string taskType { get; set; }
        public string planCodeGantt { get; set; }
        public string projectCode { get; set; }
        public string projectName { get; set; }
        public string planSdate { get; set; }
        public string planEdate { get; set; }
        public string actualSdate { get; set; }
        public string actualEdate { get; set; } 
        public string taskName { get; set; }
        public string taskCode { get; set; }
        public string loginName { get; set; }
        public string deptName { get; set; }
        public string planManagerName { get; set; }
        public string parentId { get; set; }
        public string parentName { get; set; }
        public string interfaceState { get; set; }
        public string interfaceStateText
        {
            get
            {
                if (string.IsNullOrEmpty(interfaceState))
                    return "未接收";
                if (interfaceState == "1")
                    return "已接收";
                if (interfaceState == "3")
                    return "已上报";
                return "未接收";
            }
        }
        public List<resource> resources = new List<resource>();
        public List<deliver> delivers = new List<deliver>();
        public List<user> users = new List<user>();
        public List<script> scripts = new List<script>();
        public List<arithmetic> arithmetics = new List<arithmetic>();
        public List<techFile> techFiles = new List<techFile>();
        public List<resolver> resolvers = new List<resolver>();

        public static TaskInfo Parse(DataRow dr) {
            TaskInfo result = new TaskInfo();

            result.id = dr["ID"].ToString();
            result.taskType = dr["TASK_TYPE"].ToString(); 
            result.projectCode = dr["PROJECT_CODE"].ToString();
            result.projectName = dr["PROJECT_NAME"].ToString();

            result.planSdate = dr["PLAN_S_DATE"].ToString();
            result.planEdate = dr["PLAN_E_DATE"].ToString();

            result.actualSdate = dr["ACTUAL_S_DATE"].ToString();
            result.actualEdate = dr["ACTUAL_E_DATE"].ToString();

            result.taskName = dr["TASK_NAME"].ToString();
            result.planCodeGantt = dr["PLAN_CODE_GANTT"].ToString();

            
            result.taskCode = dr["TASK_CODE"].ToString();
            result.loginName = dr["LOGIN_NAME"].ToString();
            result.deptName = dr["DEPT_NAME"].ToString();
            result.planManagerName = dr["PLAN_MANAGER_NAME"].ToString();
            result.parentId = dr["PARENT_ID"].ToString();
            result.interfaceState = dr["INTERFACE_STATE"].ToString();

            return result;
        }

        public static TaskInfo Parse(XElement element)
        {
            TaskInfo result = new TaskInfo();

            result.id = element.Element("id").Value;
            result.taskType = element.Element("taskType").Value;
            
            result.projectCode = element.Element("projectCode").Value;
            result.projectName = element.Element("projectName").Value;
            result.planSdate = element.Element("planSdate").Value;
            result.planEdate = element.Element("planEdate").Value;
            result.actualSdate = element.Element("actualSdate").Value;
            result.actualEdate = element.Element("actualEdate").Value;
            result.taskName = element.Element("taskName").Value;
            result.planCodeGantt = element.Element("planCodeGantt").Value;
            result.taskCode = element.Element("taskCode").Value;
            result.loginName = element.Element("loginName").Value;
            result.deptName = element.Element("deptName").Value;
            result.planManagerName = element.Element("planManagerName").Value;
            result.parentId = element.Element("parentId").Value;
            result.interfaceState = element.Element("interfaceState").Value;

            foreach (var el in element.Element("resources").Elements("resource"))
            {
                result.resources.Add(resource.Parse(el));
            }
            foreach (var el in element.Element("delivers").Elements("deliver"))
            {
                result.delivers.Add(deliver.Parse(el));
            }
            foreach (var el in element.Element("users").Elements("user"))
            {
                result.users.Add(user.Parse(el));
            }
            foreach (var el in element.Element("techFiles").Elements("techFile"))
            {
                result.techFiles.Add(techFile.Parse(el));
            }
            foreach (var el in element.Element("resolvers").Elements("resolver"))
            {
                result.resolvers.Add(resolver.Parse(el));
            }

            var s = script.Parse(element.Element("script"));
            if (s != null)
                result.scripts.Add(s);

            var a = arithmetic.Parse(element.Element("arithmetic"));
            if (a != null)
                result.arithmetics.Add(a);

            return result;

        }

        public class resource
        {
            public string resourceCode { get; set; }
            public string resourceName { get; set; }
            public string resourceType { get; set; }

            public static resource Parse(XElement element)
            {
                resource result = new resource();
                result.resourceCode = element.Element("resourceCode").Value;
                result.resourceName = element.Element("resourceName").Value;
                result.resourceType = element.Element("resourceType").Value;
                return result;
            }
            public static resource Parse(DataRow element)
            {
                resource result = new resource();
                result.resourceCode = element["RESOURCE_CODE"].ToString();
                result.resourceName = element["RESOURCE_NAME"].ToString();
                result.resourceType = element["LOOKUP_NAME"].ToString();
                return result;
            }
        }
        public class deliver
        {
            public string deliverName { get; set; }
            public string attachNames { get; set; }
            public string deliverType { get; set; }
            public string deliverPlanDate { get; set; }
            public string deliverState { get; set; }
            public string deliverId { get; set; }
            

            public System.Windows.Visibility Visibility
            {
                get
                {
                    if (deliverType == "半物理试验数据")
                    {
                        return System.Windows.Visibility.Collapsed;
                    }
                    else {
                        return System.Windows.Visibility.Visible;
                    }
                }
            }
            public static deliver Parse(XElement element)
            {
                deliver result = new deliver();
                result.deliverId = element.Element("deliverId").Value;
                result.deliverName = element.Element("deliverName").Value;
                result.attachNames = element.Element("attachNames").Value; 
                result.deliverType = element.Element("deliverType").Value;
                result.deliverPlanDate = element.Element("deliverPlanDate").Value;
                result.deliverState = element.Element("deliverState").Value;
                return result;
            }

            public static deliver Parse(DataRow element) {
                deliver result = new deliver();
                result.deliverId   = element["ID"].ToString();
                result.deliverName = element["DELIVERABLE_NAME"].ToString();
                result.attachNames = element["ATTACH_NAMES"].ToString();
                result.deliverType = element["DELIVER_TYPE"].ToString();
                result.deliverPlanDate = element["DELIVER_ACUTAL_DATE"].ToString();
                result.deliverState = element["DELIVER_STAUS_EN"].ToString();
                return result;
            }
        }
        public class user
        {
            public string userName { get; set; }
            public string userCode { get; set; }
            public string userRole { get; set; }
            public static user Parse(XElement element)
            {
                user result = new user();
                result.userName = element.Element("userName").Value;
                result.userCode = element.Element("userCode").Value;
                result.userRole = element.Element("userRole").Value;
                return result;
            }
            public static user Parse(DataRow element)
            {
                user result = new user();
                result.userName = element["USER_NAME"].ToString();
                result.userCode = element["USER_NO"].ToString();
                result.userRole = element["USER_ROSE"].ToString();
                return result;
            }
        }
        public class script
        {
            public string scriptCode { get; set; }
            public string attachName { get; set; }
            public string url { get; set; }

            public static script Parse(XElement element)
            {
                if (element.Elements().Count() == 0)
                    return null;

                script result = new script();
                result.scriptCode = element.Element("scriptCode").Value;
                result.attachName = element.Element("attachName").Value;
                result.url = element.Element("url").Value;
                return result;
            }

            public static script Parse(DataRow element)
            { 
                script result = new script();
                result.scriptCode = element["SCRIPT_CODE"].ToString();
                result.attachName = element["ATTACH_NAME"].ToString();
                result.url = System.Configuration.ConfigurationManager.AppSettings["scriptdownuri"].ToString() + element["ID"].ToString();
                return result;
            }

        }
        public class techFile
        {
            public string techName { get; set; }
            public string userLoginName { get; set; }
            public string userDept { get; set; }
            public string techType { get; set; }
            public string attachName { get; set; }
            public string url { get; set; }

            public static techFile Parse(XElement element)
            {
                if (element.Elements().Count() == 0)
                    return null;

                techFile result = new techFile();
                result.techName = element.Element("techName").Value;
                result.userLoginName = element.Element("userLoginName").Value;
                result.userDept = element.Element("userDept").Value;
                result.techType = element.Element("techType").Value;

                XElement attachments = element.Element("attachments");
                if (attachments.Elements().Count() > 0)
                {
                    XElement attachment = attachments.Element("attachment");
                    result.attachName = attachment.Element("attachName").Value;
                    result.url = attachment.Element("url").Value;
                }

                return result;
            }

            public static techFile Parse(DataRow element)
            { 
                techFile result = new techFile();  
                result.techName = element["DELIVERABLE_NAME"].ToString();
                result.userLoginName = element["LOGIN_NAME"].ToString();
                result.userDept = element["DEPT_NAME"].ToString();
                result.techType = element["DELIVER_TYPE"].ToString() ;
                result.attachName = element["ATTACH_NAME"].ToString();
                result.url = System.Configuration.ConfigurationManager.AppSettings["scriptdownuri"].ToString() + element["ID"].ToString();

                
                return result;
            }
        }

        public class arithmetic
        {
            public string arithmeticCode { get; set; }
            public string attachName { get; set; }
            public string url { get; set; }

            public static arithmetic Parse(XElement element)
            {
                if (element.Elements().Count() == 0)
                    return null;

                arithmetic result = new arithmetic();
                result.arithmeticCode = element.Element("arithmeticCode").Value;
                result.attachName = element.Element("attachName").Value;
                result.url = element.Element("url").Value;

                return result;
            }

            public static arithmetic Parse(DataRow element)
            { 
                arithmetic result = new arithmetic();
                result.arithmeticCode = element["ARITHMETIC_CODE"].ToString();
                result.attachName = element["ATTACH_NAME"].ToString();
                result.url = System.Configuration.ConfigurationManager.AppSettings["scriptdownuri"].ToString() + element["ID"].ToString();

                return result;
            }
        }

        public class resolver
        {
            public string resolverId { get; set; }
            public string typeName { get; set; }
            public string resolverCode { get; set; }
            public string attachName { get; set; }
            public string url { get; set; }
            public string scriptType { get; set; }
            public string displayName { get; set; }

            public static resolver Parse(XElement element)
            {
                if (element.Elements().Count() == 0)
                    return null;

                resolver result = new resolver();
                result.resolverId = element.Element("resolverId").Value;
                result.typeName = element.Element("typeName").Value;
                result.resolverCode = element.Element("resolverCode").Value;
                result.attachName = element.Element("attachName").Value;
                result.url = element.Element("url").Value;
                result.scriptType = element.Element("scriptType").Value;

                return result;
            }

            public static resolver Parse(DataRow element)
            { 
                resolver result = new resolver();
                result.resolverId = element["ID"].ToString();
                result.typeName = element["TYPE_NAME"].ToString();
                result.resolverCode = element["RESOLVER_CODE"].ToString();
                result.attachName = element["ATTACH_NAME"].ToString();
                // result.url = element["url"].ToString();
                result.scriptType = element["SCRIPT_TYPE"].ToString();

                return result;
            }
        }
    }
}
