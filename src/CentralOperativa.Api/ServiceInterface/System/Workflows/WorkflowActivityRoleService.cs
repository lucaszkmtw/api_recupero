using System;
using System.Linq;
using ServiceStack;
using ServiceStack.OrmLite;
using WorkflowActivityRole = CentralOperativa.ServiceModel.System.Workflows.WorkflowActivityRole;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    [Authenticate]
    public class WorkflowActivityRoleService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(WorkflowActivityRole.PostWorkflowActivityRole request)
        {
            return Db.Update((Domain.System.Workflows.WorkflowActivityRole)request);
        }

        public object Post(WorkflowActivityRole.PostWorkflowActivityRole request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var currentWorkflowActivityRoles = Db.Select(Db.From<Domain.System.Workflows.WorkflowActivityRole>().Where(w => w.WorkflowActivityId == request.WorkflowActivityId));
                    if(currentWorkflowActivityRoles.Count == 0)
                    {
                        request.IsDefault = true;
                    }

                    request.Id = (int) Db.Insert((Domain.System.Workflows.WorkflowActivityRole) request, true);

                    //Permissions en 0 para todos los roles que estan en el workflow
                    var roleIds = Db.ColumnDistinct<int>(Db.From<Domain.System.Workflows.WorkflowActivityRole>()
                        .Join<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Workflows.WorkflowActivity>()
                        .Where<Domain.System.Workflows.WorkflowActivity>(w => w.WorkflowId == request.WorkflowId)
                        .Select(x => x.RoleId));
                    foreach (var roleId in roleIds)
                    {
                        var workflowActivityRolePermission = new Domain.System.Workflows.WorkflowActivityRolePermission
                        {
                            Permission = 0,
                            RoleId = roleId,
                            WorkflowActivityRoleId = request.Id
                        };
                        Db.Insert(workflowActivityRolePermission);
                    }

                    trx.Commit();
                }
                catch(Exception)
                {
                    trx.Rollback();
                }
            }

            return request;
        }

        public object Delete(WorkflowActivityRole.DeleteWorkflowActivityRole request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    //DeleteRole associated permissions
                    Db.Delete<Domain.System.Workflows.WorkflowActivityRolePermission>(w => w.WorkflowActivityRoleId == request.Id);
                    
                    Db.DeleteById<Domain.System.Workflows.WorkflowActivityRole>(request.Id);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                }
            }

            return request;
        }

        public WorkflowActivityRole.GetWorkflowActivityRoleResponse Get(WorkflowActivityRole.Get request)
        {
            var workflowActivityRole = Db.Select<ServiceModel.System.Workflows.WorkflowActivityRole.GetWorkflowActivityRoleResponse>(Db
                .From<Domain.System.Workflows.WorkflowActivityRole>()
                .Join<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Role>()
                .Where(x => x.Id == request.Id)).SingleOrDefault();
            if (workflowActivityRole != null)
            {
                workflowActivityRole.Permissions = Db
                    .Select<ServiceModel.System.Workflows.WorkflowActivityRolePermission>(
                        Db.From<Domain.System.Workflows.WorkflowActivityRolePermission>()
                            .Join<Domain.System.Workflows.WorkflowActivityRolePermission, Domain.System.Role>()
                            .Where(w => w.WorkflowActivityRoleId == workflowActivityRole.Id));
            }

            return workflowActivityRole;
        }

        public object Get(WorkflowActivityRole.QueryWorkflowActivityRoles request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }
    }
}