using DataImport.BLL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;

namespace DataImport.WebSite
{
    /// <summary>
    /// import 的摘要说明
    /// </summary>
    public class import : IHttpHandler
    {
        string key = "";

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain"; 

            string path = context.Server.MapPath("upload");  
            string fileName = string.Format(@"{0}\{1}\{2}", path, "groupTrailDate", context.Request.QueryString["filename"]);

            key = System.IO.Path.GetFileName(fileName);

            HttpRuntime.Cache[this.key] = "";


            string projectCode = "";
            string taskCode = "";
            string scriptCode = "";
            string userId = context.Request.QueryString["userid"];// "4028b48152632a160152635092f7000e";
            string userName = context.Request.QueryString["username"]; //"shaozj";
            int times = 0;
               
            string[] t1 = System.IO.Path.GetFileNameWithoutExtension(fileName).Split('@');

            // 能够解开
            if (t1.Length > 1)
            {
                string[] t2 = t1[1].Split(',');
                if (t2.Length > 3)
                {
                    projectCode = t2[0];
                    taskCode = t2[1];
                    scriptCode = t2[2];
                    int.TryParse(t2[3], out times);
                }

                BetchLogic bl = new BetchLogic(
                   userId, userName,
                   projectCode, taskCode, scriptCode, times, fileName);

                bl.CompleteEvent += Bl_CompleteEvent;
                bl.MessageEvent += Bl_MessageEvent;

                //if (bl.init())
                //{
                //    bl.run();

                //}


                Thread thread = new Thread(new ThreadStart(() =>
                {
                    if (bl.init())
                    {
                        bl.run();

                    }


                }));

                thread.Start();

                context.Response.Write("run");

            }
        }

        private void Bl_MessageEvent(object sender, MessageArgs e)
        { 
            HttpRuntime.Cache[this.key] = e.Message["message"].ToString();
        }

        private void Bl_CompleteEvent(object sender, CompleteArgs e)
        {
            HttpRuntime.Cache[this.key] = e.Message.ToString();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}