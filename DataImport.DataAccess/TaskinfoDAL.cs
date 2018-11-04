using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using static DataImport.DataAccess.Entitys.TaskInfo;

namespace DataImport.DataAccess
{
    public class TaskinfoDAL
    {
        public static List<TaskInfo> getList(string loginName)
        {
            List<TaskInfo> result = new List<TaskInfo>();

            string sql = string.Format(@"with a as(select t.parent_id parent_id, min(t.plan_code_gantt) min_task_code
  from mpm.pm_task_info t
 where t.actual_e_date is null
   and (t.accept_state <> '3' or t.accept_state is null)
   and t.task_type = '1'
   and t.id in (select pti.parent_id
                  from mpm.pm_task_info pti left join pt6.sys_user su
                       on pti.user_id = su.id
                 where pti.pm_task_type = '1' and (su.login_name='{0}' or exists(
                 select 1 from tdm.tdm_task_rsrc_user ttru left join pt6.sys_user su1 on ttru.user_id = su1.id
                 where ttru.task_id = pti.id and su1.login_name = '{0}'
                 ))) --包含下达给试验部公布的工序
 group by t.parent_id
),
--查询所有任务下的按顺序应当执行的工序code对应的ID
b as (
  select a.parent_id,a.min_task_code,pti1.id from mpm.pm_task_info pti1,a
  where pti1.parent_id = a.parent_id 
  and pti1.task_name not like '%_A' AND  pti1.task_name NOT LIKE '%_B'
  and pti1.task_name not like '%_C' AND  pti1.task_name NOT LIKE '%_D'
),
--查询带ABCD的科研工序，只能查询上一未做的阶段工序
b1 as(
 select a.parent_id,a.min_task_code,pti1.id from mpm.pm_task_info pti1,a
  where pti1.parent_id = a.parent_id 
    and ((pti1.TASK_NAME  LIKE '%_A' AND nvl(pti1.ACTUAL_PROGRESS,0)<100)
OR(pti1.TASK_NAME LIKE '%_B' AND nvl(pti1.ACTUAL_PROGRESS,0)<100 AND 
 NOT EXISTS(SELECT 1 FROM mpm.PM_TASK_INFO WHERE PARENT_ID=pti1.PARENT_ID AND TASK_NAME LIKE '%_A' 
 AND nvl(ACTUAL_PROGRESS,0)<100))
 OR(pti1.TASK_NAME LIKE '%_C' AND nvl(pti1.ACTUAL_PROGRESS,0)<100 AND 
 NOT EXISTS(SELECT 1 FROM mpm.PM_TASK_INFO WHERE PARENT_ID=pti1.PARENT_ID AND TASK_NAME LIKE '%_B' 
 AND nvl(ACTUAL_PROGRESS,0)<100))
  OR(pti1.TASK_NAME LIKE '%_D' AND nvl(pti1.ACTUAL_PROGRESS,0)<100 AND 
 NOT EXISTS(SELECT 1 FROM mpm.PM_TASK_INFO WHERE PARENT_ID=pti1.PARENT_ID AND TASK_NAME LIKE '%_C' 
 AND nvl(ACTUAL_PROGRESS,0)<100))
 ) 
),
b2 as (
select parent_id,min_task_code,id from b
union all
select parent_id,min_task_code,id from b1
),
--查询该工序节点父节点和子节点的信息
d as(
  select distinct pti2.* from mpm.pm_task_info pti2,b2
  where pti2.parent_id = b2.id or b2.parent_id = pti2.id or pti2.id=b2.id
  and pti2.actual_e_date is null
  and pti2.accept_state <> '3'
),
--过滤工步不为登陆人的工步
c as (
  select d.* from d
  left join pt6.sys_user su
  on d.user_id=su.id
  where (su.login_name='{0}' or exists(
                 select 1 from tdm.tdm_task_rsrc_user ttru left join pt6.sys_user su1 on ttru.user_id = su1.id
                 where ttru.task_id = d.id and su1.login_name = '{0}')) or d.task_type<>0
)
select c.id, --WBS/任务的唯一标识
       decode(c.task_type, 0, '工步', 1, '工序', 2, '里程碑', 3,'任务','') task_type, --类型
       pp.project_code, --项目code
       pp.project_name, --项目名称
       c.plan_s_date, --计划开始时间
       c.plan_e_date, --计划结束时间
       c.actual_s_date, --实际开始时间
       c.actual_e_date, --实际结束时间
       (select attribute_07 from mpm.pm_task_info where instr(c.task_path,id)>0 and        task_type='3')attribute_07, --任务项目编号
       c.plan_code_gantt, --任务名称
       c.task_name, --任务名称
       c.task_code, --任务code
       su.login_name, --登入名
       sdv.dept_name, --部门名称
       syu.login_name as PLAN_MANAGER_name, --主管名称
       c.INTERFACE_STATE , --任务状态
       c.parent_id --父节点ID
       from c
left join pt6.sys_user su
    on c.user_id = su.id --通过user_id左连接 系统用户表 拿到 用户登入名
  left join pt6.sys_user syu
    on c.plan_manager_id = syu.id --通过plan_manager_id左连接 系统用户表 拿到 主管名称
  left join pt6.sys_dept_v sdv
    on c.dept_id = sdv.id --通过dept_id左连接 部门表视图 拿到 部门名称
  left join pt6.pm_project pp
    on c.pm_project_id = pp.id --通过pm_project_id左连接 项目结构信息表 拿到 项目code 项目名称
 where c.task_status = '30' --状态 等于30(已下达)
   and nvl(c.ACTUAL_PROGRESS, 0) < 100 --并且 计划实际完成百分比%小于100 order by c.plan_code_gantt


", loginName);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                TaskInfo item = TaskInfo.Parse(dr);
                result.Add(item);
            }

            foreach (var task in result)
            {
                task.parentName = "";
                // 工步，工序，任务
                var p1 = result.FirstOrDefault(it => it.id == task.parentId);
                TaskInfo p2 = null;
                //if (p1 != null)
                //{
                //    p2 = result.FirstOrDefault(it => it.id == p1.parentId);
                //}

                //if (p1 != null && p2 != null)
                //{
                //    task.parentName = string.Format("{0}", p2.taskName, p1.taskName);
                //    // task.parentName = string.Format("{0}/{1}", p2.taskName, p1.taskName);
                //}
                //else
                    if (p1 != null)
                {
                    task.parentName = string.Format("{0}", p1.taskName);
                }

                task.parentName = task.taskName;
            }

            return result;
        }

        public static void loadEx(TaskInfo info) {
            if (!info.loaded) {

                info.resources = getResources(info.id);
                info.delivers = getDelivers(info.id);
                info.users = getUsers(info.id);
                info.scripts = getScripts(info.id);
                info.techFiles = getTechFiles(info.id);
                info.arithmetics = getArithmetics(info.id);
                info.resolvers = getResolvers(info.id);

                info.loaded = true;
            }
        }

        /// <summary>
        /// 3.试验设备sql: （参数id为点击主表的任务id）
        /// </summary>
        private static List<resource> getResources(string id) {
            List<resource> result = new List<resource>();

            string sql = string.Format(@"SELECT TT.ID,PD.RESOURCE_CODE,PD.RESOURCE_NAME,
                       V.LOOKUP_NAME FROM pt6.PM_RESOURCE_DEFINE PD INNER JOIN TDM.TDM_TASK_RSRC_USER TT
                    ON TT.DEVICE_ID = PD.ID LEFT JOIN pt6.SYS_LOOKUP_V V ON V.LOOKUP_CODE = PD.RESOURCE_TYPE
                 WHERE TT.TASK_ID = '{0}' AND V.LOOKUP_TYPE = 'TDM_ RESOURCE_TYPE'  AND TT.TYPE = 'R'",id);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                resource item = resource.Parse(dr);
                result.Add(item);
            } 
            return result;
        }

        private static List<deliver> getDelivers(string id) {
            List<deliver> result = new List<deliver>();

            string sql = string.Format(@"SELECT PTD.ID,PTD.DELIVERABLE_NAME, PTD.DELIVER_TYPE,
TO_CHAR(PTD.DELIVER_ACUTAL_DATE, 'YYYY-MM-DD') DELIVER_ACUTAL_DATE,
CASE
  WHEN PTD.DELIVER_STAUS = '20' THEN 
   '已提交'
  WHEN PTD.DELIVER_STAUS = '30' THEN
   '后补充' 
  ELSE 
   '未提交' 
END AS DELIVER_STAUS_EN,
(SELECT wm_concat(ATTACH_NAME) FROM pt6.sys_attachment where parent_table_id='groupTrailDate123321'
 and parent_register_id=PTD.ID)ATTACH_NAMES 
    FROM pt6.PM_TASK_DELIVER PTD
   WHERE PTD.TASK_ID = '{0}'", id);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                deliver item = deliver.Parse(dr);
                result.Add(item);
            }
            return result;
        }

        private static List<user> getUsers(string id) {
            List<user> result = new List<user>();

            string sql = string.Format(@"SELECT TT.ID,  SU.NAME   AS USER_NAME,SU.NO AS USER_NO, 
                     SL.ROLE_NAME USER_ROSE FROM TDM.TDM_TASK_RSRC_USER TT  
               INNER JOIN pt6.SYS_USER SU   ON SU.ID = TT.USER_ID  
               INNER JOIN pt6.SYS_USER_ROLE SR   ON SR.SYS_USER_ID = SU.ID  
               INNER JOIN pt6.SYS_ROLE SL   ON SL.ID = SR.SYS_ROLE_ID  
               WHERE TT.TASK_ID = '{0}'  
                 AND TT.TYPE = 'U'", id);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                user item = user.Parse(dr);
                result.Add(item);
            }
            return result;
        }

        private static List<script> getScripts(string id) {
            List<script> result = new List<script>();

            string sql = string.Format(@" with a as 
               (select t.id as script_id, t.script_code   
                  from pt6.pm_task_info pt  
                  left join tdm.tdm_automatic_script t  
                    on pt.task_script_code = t.script_code  
                 where pt.id = '{0}')  
              select sa.id,  
              a.script_code,   
              sa.attach_name   
                from a  
               left join pt6.sys_attachment sa  
                  on sa.parent_register_id = a.script_id",id);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                script item = script.Parse(dr);
                result.Add(item);
            }

            return result;
        }

        private static List<techFile> getTechFiles(string id) {
            List<techFile> result = new List<techFile>();

            string sql = string.Format(@"select ptd.id, (select 
                         sa.attach_name  from pt6.sys_attachment sa  
                   where sa.parent_register_id = ptd.id) attach_name, 
                       ptd.deliverable_name, su.login_name,sd.dept_name, ptd.deliver_type   
                  from pt6.tdm_task_techfile ptd  
                  left join pt6.sys_user su  
                  on ptd.deliver_user_id=su.id  
                  left join pt6.sys_dept_v sd  
                  on   ptd.deliver_dept=sd.id  
                 where ptd.task_id = '{0}' ", id);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                techFile item = techFile.Parse(dr);
                result.Add(item);
            }

            return result;
        }

        private static List<arithmetic> getArithmetics(string id) {
            List<arithmetic> result = new List<arithmetic>();

            string sql = string.Format(@"with a as (SELECT T.ARITHMETIC_CODE, T.ID AS METIC_ID  
							    FROM pt6.PM_TASK_INFO PT  
							    LEFT JOIN TDM.TDM_ARITHMETIC_MANAGEMENT T  
							      ON PT.TASK_METIC_CODE = T.ARITHMETIC_CODE  
							 WHERE PT.ID = '{0}')  
							select sa.id, a.ARITHMETIC_CODE,   
							sa.attach_name   
							  from a  
							 left join pt6.sys_attachment sa  
							    on sa.parent_register_id = a.METIC_ID", id);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                arithmetic item = arithmetic.Parse(dr);
                if (!string.IsNullOrEmpty(item.arithmeticCode)){
                    result.Add(item);
                } 
            }

            return result;
        }

        private static List<resolver> getResolvers(string id) {
            List<resolver> result = new List<resolver>();

            string sql = string.Format(@"with a as
							 (select t.fid as resolver_id, t.mids_script_code resolver_code,t.script_type script_type,t.mids_script_name type_name 
							    from pt6.pm_task_info pt left join import.mds_imp_data_script t 
							      on instr(pt.resolver_task_id,t.fid)>0 where pt.id = '{0}') 
							select a.resolver_id id,a.resolver_id,a.resolver_code,a.type_name,a.script_type,sa.attach_name  from  a 
							 left join pt6.sys_attachment sa  on sa.parent_register_id = a.resolver_id", id);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                resolver item = resolver.Parse(dr);
                 
                    result.Add(item);
                 
            }

            return result;
        }
    }
}
