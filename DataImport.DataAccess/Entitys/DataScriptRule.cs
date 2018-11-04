using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess.Entitys
{
    public class DataScriptRule
    {
        public string FID { get; set; }
        public string MdsImpDataScriptID { get; set; }
        public string LineSeperator { get; set; }
        public string ColSperator { get; set; }
        public int ColnameLines { get; set; }
        public int DataLines { get; set; }
        public string DesTable { get; set; }
        public string DesFile { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string LastUpdateIp { get; set; }
        public int Version { get; set; }
        public string DesBusinessPk { get; set; }

        public static DataScriptRule Parse(DataRow row)
        {
            DataScriptRule result = new DataScriptRule();

            result.FID = row["FID"].ToString();
            result.MdsImpDataScriptID = row["MDS_IMP_DATA_SCRIPT_ID"].ToString();
            result.LineSeperator = row["LINE_SEPERATOR"].ToString();
            result.ColSperator = row["COL_SEPERATOR"].ToString();
            result.ColnameLines = Convert.ToInt32(row["COLNAME_LINES"]);
            result.DataLines = Convert.ToInt32(row["DATA_LINES"]);
            result.DesTable = row["DES_TABLE"].ToString();
            result.DesFile = row["DES_FILE"].ToString();

            result.Remark = row["REMARK"].ToString();
            result.CreatedBy = row["CREATED_BY"].ToString();
            result.CreationDate = Convert.ToDateTime(row["CREATION_DATE"]);
            result.LastUpdatedBy = row["LAST_UPDATED_BY"].ToString();
            result.LastUpdateDate = Convert.ToDateTime(row["LAST_UPDATE_DATE"]);
            result.LastUpdateIp = row["LAST_UPDATE_IP"].ToString();
            result.DesBusinessPk = row["DES_BUSINESS_PK"].ToString();
            result.Version = Convert.ToInt32(row["VERSION"]);

            return result;
        }

        public char getColSeperatorChar()
        {
            switch (this.ColSperator)
            {
                case "tab":
                    return '\t';
                case "comma":
                    return ',';
                case "semicolon":
                    return ';';
                case "space":
                    return ' ';
                default:
                    return ',';
            }
        }

        public static char getColSeperatorChar(String sperator)
        {
            switch (sperator)
            {
                case "tab":
                    return '\t';
                case "comma":
                    return ',';
                case "semicolon":
                    return ';';
                case "space":
                    return ' ';
                default:
                    return ',';
            }
        }
    }
}
