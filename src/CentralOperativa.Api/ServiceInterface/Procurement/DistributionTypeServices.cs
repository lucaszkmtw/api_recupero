﻿using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Procurement;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Procurement
{
    [Authenticate]
    public class DistributionTypeServices : ApplicationService
    {
        public object Put(DistributionType.Post request)
        {
            return Db.Update((CentralOperativa.Domain.Procurement.DistributionType)request);
        }

        public object Post(DistributionType.Post request)
        {
            request.Id = (int)Db.Insert((CentralOperativa.Domain.Procurement.DistributionType)request, true);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(DistributionType.Find request)
        {
            var query = Db.From<CentralOperativa.Domain.Procurement.DistributionType>()
                .OrderByDescending(q => q.Id)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            if (!string.IsNullOrEmpty(request.Description))
                query.Where(q => q.Description.Contains(request.Description));

            return Db.Select(query);
        }

        public LookupResult Get(DistributionType.Lookup request)
        {
            var query = Db.From<CentralOperativa.Domain.Procurement.DistributionType>()
                .Select(x => new {x.Id, x.Description});

            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(q => q.Description.Contains(request.Q) || q.Description.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page.GetValueOrDefault(0), request.PageSize.GetValueOrDefault(10) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Description }),
                Total = (int)count
            };
            return result;
        }

        public object Get(DistributionType.Get request)
        {
            var model = Db.SingleById<CentralOperativa.Domain.Procurement.DistributionType>(request.Id);
            return model;
        }
    }
}