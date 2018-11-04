using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
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

                if (type == "deliver")
                {
                    path = string.Format(@"{0}\{1}\file{2}", path, "groupDeliver", id); 
                }
                else {
                    path = string.Format(@"{0}\{1}\file{2}", path, "groupTrailDate", id);
                }

                // if path is not exist, create directory;
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

                HttpPostedFile file = Request.Files[f];
                
                string filename = string.Format("{0}{1}", Request["name"], Path.GetExtension(file.FileName));
                file.SaveAs(path + "\\" + Path.GetFileName(file.FileName));
            }
        }
    }
}