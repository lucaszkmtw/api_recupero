using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System;

namespace CentralOperativa.ServiceInterface.System
{
    public class TenantRepository
    {
        public async Task InitializeTenant(IDbConnection db, int id)
        {
            //Initialize workflows
            var workflowTypes = await db.SelectAsync(db.From<Domain.System.Workflows.WorkflowType>());
            foreach (var workflowType in workflowTypes)
            {
                var workflow = (await db.SelectAsync(db.From<Domain.System.Workflows.Workflow>().Where(w => w.TypeId == workflowType.Id))).FirstOrDefault();
                if (workflow != null)
                {
                    db.CopyWorkflow(workflow.Id, id);
                }
            }
        }

        public async Task<Api.GetTenantResponse> GetTenant(IDbConnection db, int id)
        {
            var model = (await db.SelectAsync<Api.GetTenantResponse>(db
                .From<Domain.System.Tenant>()
                .Join<Domain.System.Tenant, Domain.System.DocumentManagement.Folder>()
                .Join<Domain.System.Persons.Person>()
                .Where(x => x.Id == id))).SingleOrDefault();
            return model;
        }
    }
}