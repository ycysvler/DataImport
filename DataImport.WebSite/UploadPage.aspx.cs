using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DataImport.WebSite
{
    public partial class UploadPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            foreach (string f in Request.Files.AllKeys)
            {
                string path = this.Server.MapPath("upload");
                string id = Request.QueryString["id"];
                string type = "data";

                type = Request.QueryString["type"];

                //if (type == "deliver")
                //{
                //    path = string.Format(@"{0}\{1}\file{2}", path, "groupDeliver", id); 
                //}
                //else {
                //    path = string.Format(@"{0}\{1}\file", path, "groupTrailDate", id);
                //}

                path = string.Format(@"{0}\{1}", path, "groupTrailDate");

                // if path is not exist, create directory;
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

                HttpPostedFile file = Request.Files[f];

                string key = System.IO.Path.GetFileName(file.FileName);
                HttpRuntime.Cache["key"]="";

                string name = Path.GetFileName(file.FileName);

                if (this.Request.QueryString["name"] != null)
                    name = this.Request.QueryString["name"];


                file.SaveAs(path + "\\" + name);
            }
        }
    }
}