using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataImport.DataAccess.Entitys
{
    public class TasktimeInfo
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string TestTime { get; set; }
        public string TestPersion { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdateBy { get; set; }
        public string CreationDate { get; set; }
        public string LastUpdateDate { get; set; }
        public string LastUpdateIp { get; set; }
        public string Version { get; set; }

        public List<DataImport.DataAccess.Entitys.TaskInfo.resolver> resolvers = new List<DataImport.DataAccess.Entitys.TaskInfo.resolver>();
        /// <summary>
        /// 已经执行过的脚本
        /// </summary>
        public List<DataLog> scriptRunds = new List<DataLog>();

        public string resolver0 {
            get {
                var item = resolvers.FirstOrDefault(it => it.scriptType == "0");
                if (item != null) {

                    if (scriptRunds.Count(it => it.TypeName == item.resolverId && it.Version == TestTimeInt) > 0)
                        return "已执行";
                    else
                        return "未执行";
                    //return item.typeName;
                }
                return ""; 
            }
        }
        public string resolver0code {
            get
            {
                var item = resolvers.FirstOrDefault(it => it.scriptType == "0");
                if (item != null)
                {
                    return item.resolverId;
                }
                return "";
            }
        }

        public string resolver1
        {
            get
            {
                var item = resolvers.FirstOrDefault(it => it.scriptType == "1");
                if (item != null)
                {
                    if (scriptRunds.Count(it => it.TypeName == item.resolverId && it.Version == TestTimeInt) > 0)
                        return "已执行";
                    else
                        return "未执行";
                }
                return ""; 
            }
        }
        public string resolver1code
        {
            get
            {
                var item = resolvers.FirstOrDefault(it => it.scriptType == "1");
                if (item != null)
                {
                    return item.resolverId;
                }
                return "";
            }
        }

        public string resolver2
        {
            get
            {
                var item = resolvers.FirstOrDefault(it => it.scriptType == "2");
                if (item != null)
                {
                    if (scriptRunds.Count(it => it.TypeName == item.resolverId && it.Version == TestTimeInt) > 0)
                        return "已执行";
                    else
                        return "未执行";
                }
                return ""; 
            }
        }
        public string resolver2code
        {
            get
            {
                var item = resolvers.FirstOrDefault(it => it.scriptType == "2");
                if (item != null)
                {
                    return item.resolverId;
                }
                return "";
            }
        }
        public string resolver3
        {
            get
            {
                var item = resolvers.FirstOrDefault(it => it.scriptType == "3");
                if (item != null)
                {
                    if (scriptRunds.Count(it => it.TypeName == item.resolverId && it.Version == TestTimeInt) > 0)
                        return "已执行";
                    else
                        return "未执行";
                }
                return ""; 
            }
        }
        public string resolver3code
        {
            get
            {
                var item = resolvers.FirstOrDefault(it => it.scriptType == "3");
                if (item != null)
                {
                    return item.resolverId;
                }
                return "";
            }
        }
        public int TestTimeInt {
            get {
                int result = 1;
                int.TryParse(this.TestTime, out result);
                return result; 
            }
        }

        public static TasktimeInfo Parse(XElement element)
        {
            TasktimeInfo result = new TasktimeInfo();
            result.Id = element.Element("id").Value;
            result.TaskId = element.Element("task_id").Value;
            result.TestTime = element.Element("test_time").Value;
            result.TestPersion = element.Element("test_persion").Value;
            result.BeginDate = element.Element("begin_date").Value;
            result.EndDate = element.Element("end_date").Value;
            result.Remark = element.Element("remark").Value;
            result.CreatedBy = element.Element("created_by").Value;
            result.LastUpdateBy = element.Element("last_update_by").Value;
            result.LastUpdateIp = element.Element("last_update_ip").Value;
            result.Version = element.Element("version").Value;  

            return result;
        }

        public string ToXml() {
            StringBuilder xml = new StringBuilder();
            xml.AppendFormat("<tasktimeinfos><tasktimeinfo>");
            xml.AppendFormat("<id>{0}</id>", this.Id);
            xml.AppendFormat("<task_id>{0}</task_id>", this.TaskId);
            xml.AppendFormat("<test_time>{0}</test_time>", this.TestTime);
            xml.AppendFormat("<test_persion>{0}</test_persion>", this.TestPersion);
            xml.AppendFormat("<begin_date>{0}</begin_date>", this.BeginDate);
            xml.AppendFormat("<end_date>{0}</end_date>", this.EndDate);
            xml.AppendFormat("<remark>{0}</remark>", this.Remark);
            xml.AppendFormat("<created_by>{0}</created_by>", this.CreatedBy);
            xml.AppendFormat("<last_update_by>{0}</last_update_by>", this.LastUpdateBy);
            xml.AppendFormat("<creation_date>{0}</creation_date>", this.CreationDate);
            xml.AppendFormat("<last_update_date>{0}</last_update_date>", this.LastUpdateDate);
            xml.AppendFormat("<last_update_ip>{0}</last_update_ip>", this.LastUpdateIp);
            xml.AppendFormat("<version>{0}</version>", this.Version); 
            xml.AppendFormat("</tasktimeinfo></tasktimeinfos>");
            
            return xml.ToString();
        }
    }

    //<tasktimeinfo>
    //<id>任务次数主键</id>
    //<task_id>工步id</task_id>
    //<test_time>试验次数</test_time>
    //<test_persion>试验人员</test_persion>
    //<begin_date>开始时间</begin_date>
    //<end_date>结束时间</end_date>
    //<remark>备注</remark>
    //<created_by>创建人</created_by>
    //<last_update_by>更新人员</last_update_by>
    //<creation_date>创建日期</creation_date>
    //<last_update_date>最后更新日期</last_update_date>
    //<last_update_ip>最后更新ip</last_update_ip>
    //<version>版本</version>
    //</tasktimeinfo>

}
