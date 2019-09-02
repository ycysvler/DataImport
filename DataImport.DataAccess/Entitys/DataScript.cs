using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess.Entitys
{
    public class DataScript
    {
        public string FID { get; set; }
        public string MidsScriptCode{get;set;}
        public string MidsScriptName{get;set;}
        public string MidsScriptVesion{get;set;}
        public string FileType{get;set;}
        public string IndexKey{get;set;}
        public string ValidFlag{get;set;}
        /// <summary>
        /// 适用试验类型
        /// </summary>
        public string ApplyTestProject{get;set;}
        public string Remark{get;set;}
        public string CreatedBy{get;set;}
        public DateTime CreationDate{get;set;}
        public string LastUpdatedBy{get;set;}
        public DateTime LastUpdateDate{get;set;}
        public string LastUpdateIp{get;set;}
        public int Version{get;set;}
        public int ScriptType { get; set; }
        public string ScriptTypeName { get {
            switch (ScriptType) {
                case 0: return "上位机";
                case 1: return "模型机";
                case 2: return "设备机";
                default: return "其它";
            }
        } }
        /// <summary>
        /// 项目编码：PROJECT_CODE
        /// </summary>
        public string ProjectCode { get; set; }
        /// <summary>
        /// 试验名称：TASK_NAME
        /// </summary>
        public string TaskName{get;set;}
        /// <summary>
        /// TABLE_NAME：目标表
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// TABLE_NAME：如果是sqlite, 那么这里有3个表名字
        /// </summary>
        public string TableNameExt { get; set; }


        public string Release { get; set; }
        public string ReleaseText {
            get {
                if (this.Release == "01")
                    return "未发布";
                else
                    return "已发布";
            }
        }

        public int Invalid { get; set; }

        public string InvalidText { get {
                if (this.Invalid == 0)
                    return "正常";
                else
                    return "失效";

            } }

        public string DisplayName {
            get { return string.Format("{0}(code:{1})(version:{2})", MidsScriptName,MidsScriptCode, Version ); }
        }

        public static DataScript Parse(DataRow row)
        {
            DataScript result = new DataScript();

            result.FID = row["FID"].ToString();
            result.MidsScriptCode = row["MIDS_SCRIPT_CODE"].ToString();
            result.MidsScriptName = row["MIDS_SCRIPT_NAME"].ToString();
            result.MidsScriptVesion = row["MIDS_SCRIPT_VERSION"].ToString();
            result.FileType = row["FILE_TYPE"].ToString() ;
            result.IndexKey = row["INDEX_KEY"].ToString();
            result.ValidFlag = row["VALID_FLAG"].ToString();
            result.ApplyTestProject = row["APPLY_TEST_PROJECT"].ToString();
            result.Remark = row["REMARK"].ToString();
            result.CreatedBy = row["CREATED_BY"].ToString();
            result.CreationDate = Convert.ToDateTime( row["CREATION_DATE"]);
            result.LastUpdatedBy = row["LAST_UPDATED_BY"].ToString();
            result.LastUpdateDate = Convert.ToDateTime( row["LAST_UPDATE_DATE"]);
            result.LastUpdateIp = row["LAST_UPDATE_IP"].ToString();
            result.Version = Convert.ToInt32( row["VERSION"] );
            if(row["SCRIPT_TYPE"] != DBNull.Value)
            result.ScriptType = Convert.ToInt32(row["SCRIPT_TYPE"]);
            result.ProjectCode = row["PROJECT_CODE"].ToString();
            result.TaskName = row["TASK_NAME"].ToString();
            result.TableName = row["TABLE_NAME"].ToString();
            result.Release = row["RELEASE"].ToString();

            if (row["INVALID"] != DBNull.Value) {
                result.Invalid = Convert.ToInt32( row["INVALID"].ToString());
            }
            if (row["TABLE_NAMEEXT"] != DBNull.Value)
            {
                result.TableNameExt = row["TABLE_NAMEEXT"].ToString();
            }

            return result;
        }
    }
}
