﻿using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataImport.DataAccess
{
    public class ImportLogDAL
    {
        public static int Insert(ImportLog item)
        {
            string sql = string.Format("INSERT INTO MDS_IMP_IMPORT_LOG(CONTENT,FILE_NAME,PROJECT_CODE,TIMES, CREATED_BY,CREATION_DATE,TASK_CODE,FILE_SIZE,SCRIPT_CODE ) ");
            sql += string.Format("VALUES('{0}','{1}','{2}','{3}','{4}',to_date('{5}','yyyy/mm/dd hh24:mi:ss'),'{6}','{7}','{8}')",
                item.Content,
                item.FileName,
                item.ProjectCode,
                item.Times,
                item.CreatedBy, 
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                item.TaskCode,
                item.FileSize,
                item.ScriptCode
                );

            return OracleHelper.Excuse(sql);
        }
    }
}
