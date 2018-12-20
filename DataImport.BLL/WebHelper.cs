using DataImport.BLL.TdmToMpmWebservice;
using DataImport.BLL.TdmToZkWebservice;
using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace DataImport.BLL
{
    public class WebHelper
    {
        public static string ImportComplete(string filepath)
        {
            TdmWsService.TdmWsServiceClient client = new TdmWsService.TdmWsServiceClient();
            return client.getPyParseInfo("", filepath);
        }
        public static string verificationReportState(string id)
        {
            TdmToZkWebserviceClient client = new TdmToZkWebserviceClient();
            string result = client.verificationReportState(id);
            return result;// result.IndexOf("Y") > -1;
        }

        public static bool taskReportByClient(string id)
        {
            TdmToZkWebserviceClient client = new TdmToZkWebserviceClient();
            string result = client.taskReportByClient(id);
            return result.IndexOf("Y") > -1;
        }

        public static bool modifyActualDate(string id, string actualSDate, string actualEDate, string actualWorkHours)
        {
            TdmToZkWebserviceClient client = new TdmToZkWebserviceClient();
            string result = client.modifyActualDate(id, actualSDate, actualEDate, actualWorkHours);
            return result.IndexOf("Y") > -1;
        }
        
        public static bool modifyTdmDeliveryListState(string id, string state) {
            TdmToZkWebserviceClient client = new TdmToZkWebserviceClient();
            string result = client.modifyTdmDeliveryListState(id, state);
            return result.IndexOf("Y") > -1;
        }
        public static bool addAtachFileInfo2Tdm(string id, string attName, string size, string path) {
            TdmToZkWebserviceClient client = new TdmToZkWebserviceClient();
            string result = client.addAtachFileInfo2Tdm(id, attName, size, path);
            return result.IndexOf("Y") > -1;
        }

        public static void ImportComplete(string tablename, string ApplyTestProject)
        {
            try
            {
                WebClient client = new WebClient();
                Encoding utf8 = Encoding.UTF8;

                string encode = HttpUtility.UrlEncode(ApplyTestProject, utf8).ToUpper();

                Stream stream = client.OpenRead(
                    string.Format("{0}?tablename={1}&applytestproject={2}",
                    ConfigurationManager.AppSettings["importuri"],
                    encode));
                StreamReader reader = new StreamReader(stream);
                string result = reader.ReadToEnd();
            }
            catch (System.Exception e) { }
        }

        public static bool uploadFile(string file, string name,string taskid)
        {
            return uploadFile(file, name, taskid, "data");            
        }

        public static bool uploadFile(string file, string name, string taskid, string type)
        {
            bool result = false;
            
                WebClient client = new WebClient();
                string uri = string.Format("{0}?name={1}&id={2}&type={3}", ConfigurationManager.AppSettings["uploaduri"], name, taskid,type);
                client.UploadFile(new Uri(uri), file);
                result = true;
             
            return result;
        }

        public static string GetHttp(string url)
        { 
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
             
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 20000;

            //byte[] btBodys = Encoding.UTF8.GetBytes(body);
            //httpWebRequest.ContentLength = btBodys.Length;
            //httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();

            httpWebResponse.Close();
            streamReader.Close();

            return responseContent;
        }

        public static string Login(string loginname, string password)
        {

            DataImport.BLL.TdmToZkWebservice.TdmToZkWebserviceClient loginService = new DataImport.BLL.TdmToZkWebservice.TdmToZkWebserviceClient();
            string result = loginService.validateUserInfo(loginname, password);


            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(result);
            XmlNode node = xmlDoc.SelectSingleNode("Result");
            XmlNode code = node.SelectSingleNode("returnValue");
            XmlNode desc = node.SelectSingleNode("infoMsg");

            if (code.InnerText == "1")
            {
                string descString = desc.InnerText;
                int index = descString.IndexOf(';');
                if (index > 0)
                {
                    string uid = descString.Remove(0, 3);
                    return uid;
                }
                else
                    return "";
            }

            return "";
        }

        public static List<TaskInfo> listTdmTasks(string loginName) {

            TdmToZkWebserviceClient cl = new TdmToZkWebserviceClient();
            string xml = cl.listTdmTasks(loginName);

            XElement xe = XElement.Parse(xml);

            IEnumerable<XElement> elements = from ele in xe.Elements("taskinfo") select ele;

            List<TaskInfo> result = new List<TaskInfo>();

            foreach (var element in elements) {
                result.Add(TaskInfo.Parse(element));
            }

            foreach (var task in result)
            {
                task.parentName = "";
                // 工步，工序，任务
                var p1 = result.FirstOrDefault(it => it.id == task.parentId);
                TaskInfo p2 = null;
                if (p1 != null)
                {
                    p2 = result.FirstOrDefault(it => it.id == p1.parentId);
                }

                if (p1 != null && p2 != null)
                {
                    task.parentName = string.Format("{0}", p2.taskName, p1.taskName);
                   // task.parentName = string.Format("{0}/{1}", p2.taskName, p1.taskName);
                }
                else
                    if (p1 != null)
                    {
                        task.parentName = string.Format("{0}", p1.taskName);
                    }
            }

            return result.OrderBy(it=>it.planCodeGantt).ToList();
        }

        

        public static void DownFile(string file_url, string path)
        {
            //获取文件路径 
            if (file_url == null)
            {
                return;
            } 
            //获取远程文件的数据流
            FileStream fs = new FileStream(path, FileMode.Create);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(file_url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();

            int bufferSize = 2048;
            byte[] bytes = new byte[bufferSize];

            try
            {
                int length = stream.Read(bytes, 0, bufferSize);

                while (length > 0)
                {
                    fs.Write(bytes, 0, length);
                    length = stream.Read(bytes, 0, bufferSize);
                }
                stream.Close();
                fs.Close();
                response.Close();
            }
            catch
            {
                return;
            }
        }

        public static List<Project> listProject(string loginName) {
            List<Project> result = new List<Project>();

            TdmToZkWebserviceClient cl = new TdmToZkWebserviceClient();
            string xml = cl.listTdmProjectInfo(loginName);
            XElement xe = XElement.Parse(xml);

            IEnumerable<XElement> elements = from ele in xe.Elements("projectinfo") select ele;
              
            foreach (var element in elements)
            {
                result.Add(Project.Parse(element));
            }
            return result;
        }
        public static bool updateTdmTaskState(string taskid, string userName)
        {
            return updateTdmTaskState(taskid, "1", userName);
        }
        public static bool updateTdmTaskState(string taskid, string state, string userName)
        {

            TdmToZkWebserviceClient cl = new TdmToZkWebserviceClient();

            string xml = cl.updateTdmTaskState(taskid, state, userName);
            XElement xe = XElement.Parse(xml);
            string returnValue = xe.Element("returnValue").Value;

            return returnValue.IndexOf('0') > -1;
        }

        public static List<TasktimeInfo> listTdmTaskTimesInfo(string id) {
            TdmToMpmWebserviceClient cl = new TdmToMpmWebserviceClient();
            string xml = cl.listTdmTaskTimesInfo(id);

            List<TasktimeInfo> result = new List<TasktimeInfo>();
            try
            {
                XElement xe = XElement.Parse(xml);

                IEnumerable<XElement> elements = from ele in xe.Elements("tasktimeinfo") select ele;

                foreach (var element in elements)
                {
                    result.Add(TasktimeInfo.Parse(element));
                }
            }
            catch { }
            return result.OrderBy(it=>it.TestTime).ToList(); 
        }

        public static bool updateTdmTaskTimesInfo(TasktimeInfo info, TdmTaskTimeMethod method)
        {
            TdmToMpmWebserviceClient cl = new TdmToMpmWebserviceClient();
            string infoxml= info.ToXml();
            string xml = cl.updateTdmTaskTimesInfo(infoxml, method.ToString());

            XElement xe = XElement.Parse(xml);

            string returnValue = xe.Element("returnValue").Value;

            return returnValue == "0";
        } 
    }

    public enum TdmTaskTimeMethod {
        add, modify, delete
    }
}
