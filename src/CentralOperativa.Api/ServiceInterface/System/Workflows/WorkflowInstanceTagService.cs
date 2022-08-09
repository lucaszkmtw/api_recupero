using ServiceStack.OrmLite;
using System;
using System.Linq;
using ServiceStack;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    [Authenticate]
    public class WorkflowInstanceTagService : ApplicationService
    {
        public object Get(ServiceModel.System.Workflows.WorkflowInstanceTag.GetWorkflowInstanceTag request)
        {
            return Db.SingleById<Domain.System.Workflows.WorkflowInstanceTag>(request.Id);
        }

        public object Post(ServiceModel.System.Workflows.WorkflowInstanceTag.Post request)
        {            
            request.Id = (int)Db.Insert((Domain.System.Workflows.WorkflowInstanceTag)request, true);           
            return request;
        }

        public object Delete(ServiceModel.System.Workflows.WorkflowInstanceTag.DeleteWorkflowInstanceTag request)
        {
            try
            {
                Db.DeleteById<Domain.System.Workflows.WorkflowInstanceTag>(request.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object Get(ServiceModel.System.Workflows.WorkflowInstanceTag.QueryWorkflowInstanceTag request)
        {
            var allItems = Db.Select(Db.From<Domain.System.Workflows.WorkflowInstanceTag>().Where(w => w.Id > 0));

            if (request.Id != 0)
                allItems = allItems.Where(w => w.Id == request.Id).ToList();
            if (request.WorkflowTagId !=0)
                allItems = allItems.Where(w => w.WorkflowTagId == request.WorkflowTagId).ToList();
            if (request.WorkflowInstanceId !=0)
                allItems = allItems.Where(w => w.WorkflowInstanceId == request.WorkflowInstanceId).ToList();


            return allItems;
        }


    }
}