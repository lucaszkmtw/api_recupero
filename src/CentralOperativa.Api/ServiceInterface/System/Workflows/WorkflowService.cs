using System;
using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Workflow = CentralOperativa.ServiceModel.System.Workflows.Workflow;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    [Authenticate]
    public class WorkflowService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public Workflow.PostWorkflow Put(Workflow.PostWorkflow request)
        {
            Db.Update((Domain.System.Workflows.Workflow)request);
            return request;
        }

        public Workflow.PostWorkflow Post(Workflow.PostWorkflow request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.Id = (int) Db.Insert((Domain.System.Workflows.Workflow) request, true);
                    trx.Commit();
                }
                catch(Exception)
                {
                    trx.Rollback();
                }
            }

            return request;
        }

        public Workflow.GetWorkflowResponse Get(Workflow.GetWorkflow request)
        {
            var workflow = Db.SingleById<Domain.System.Workflows.Workflow>(request.Id).ConvertTo<ServiceModel.System.Workflows.Workflow.GetWorkflowResponse>();
            workflow.Activities = Db.Select(Db.From<Domain.System.Workflows.WorkflowActivity>().Where(w => w.WorkflowId == workflow.Id).OrderBy(o => o.ListIndex));
            return workflow;
        }

        public QueryResponse<Domain.System.Workflows.Workflow> Get(Workflow.QueryWorkflows request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where(x => x.TenantId == Session.TenantId);
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Workflow.LookupWorkflow request)
        {
            var query = Db.From<Domain.System.Workflows.Workflow>();
            query.Where(x => x.TenantId == Session.TenantId);
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name + " (" + x.Code + ")" }),
                Total = (int)count
            };
            return result;
        }
    }
}