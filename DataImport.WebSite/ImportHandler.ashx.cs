using System;
using System.Collections.Generic;
using System.Web;

namespace DataImport.WebSite
{
    /// <summary>
    /// ImportHandler 的摘要说明
    /// </summary>
    public class ImportHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            _Default.TableName = context.Request["tablename"];
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