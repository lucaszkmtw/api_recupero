using System;
using ServiceStack;
using ServiceStack.OrmLite;
using Batch = CentralOperativa.ServiceModel.System.Batch;

namespace CentralOperativa.ServiceInterface.System
{
    [Authenticate]
    public class BatchServices : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public Domain.System.Batch Get(Batch.GetBatch request)
        {
            var model = Db.SingleById<Domain.System.Batch>(request.Id);
            return model;
        }

        public QueryResponse<Batch.QueryBatchesResult> Get(Batch.QueryBatches request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Post(Batch.PostBatch request)
        {
            throw new NotImplementedException();
        }

        public object Post(Batch.ImportBatch request)
        {
            throw new NotImplementedException();
        }

        public object Post(Batch.DistributeBatch request)
        {
            throw new NotImplementedException();
        }
    }
}
