using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataImport.WebSite
{
    /// <summary>
    /// message 的摘要说明
    /// </summary>
    public class message : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain"; 

            string key = context.Request.QueryString["filename"];
            string message = HttpRuntime.Cache.Get(key) as string;

            if (message != null) {
                context.Response.Write(message);
            }
            
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