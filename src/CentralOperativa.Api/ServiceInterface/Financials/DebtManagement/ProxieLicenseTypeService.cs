using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.ServiceModel.System.Persons;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class ProxieLicenseTypeService : ApplicationService
    {
        public object Put(Api.PostProxieLicenseType request)
        {
            var q = Db.From<ProxieLicenseType>()
                 .Where(x => x.ProxieId == request.ProxieId)
                 .Update(p => p.Status);

            Db.UpdateOnly(new ProxieLicenseType { Status = 1 }, onlyFields: q);

            for (var i = 0; i < request.LicenseTypes.Count; i++) // por cada tipo de licencia que seleccione
            {
                ProxieLicenseType proxieLicenseType;
                var query = Db.From<ProxieLicenseType>()
                           .Where(w => w.ProxieId == request.ProxieId
                                   && w.LicenseTypeId == request.LicenseTypes[i].Id);

                proxieLicenseType = Db.Select(query).SingleOrDefault();

                if (proxieLicenseType == null)  // si no existe la relacion => inserta ProxieLicenseType
                {
                    ProxieLicenseType vproxieLicenseType = new ProxieLicenseType();
                    vproxieLicenseType.ProxieId = request.ProxieId;
                    vproxieLicenseType.LicenseTypeId = request.LicenseTypes[i].Id;
                    vproxieLicenseType.Status = (int)BusinessPartnerStatus.Active;

                    vproxieLicenseType.Id = (int)Db.Insert((ProxieLicenseType)vproxieLicenseType, true);
                }
                else
                {
                    if (proxieLicenseType.Status == (int)BusinessPartnerStatus.Deleted)
                    {
                        proxieLicenseType.Status = (int)BusinessPartnerStatus.Active;
                        Db.Update((ProxieLicenseType)proxieLicenseType);
                    }
                }

            }

            return request;
        }

        public object Post(Api.PostProxieLicenseType request)
        {
            for (var i = 0; i < request.LicenseTypes.Count; i++) // por cada tipo de licencia que seleccione
            {
                ProxieLicenseType vproxieLicenseType;
                var query = Db.From<ProxieLicenseType>()
                            .Where(w => w.ProxieId == request.ProxieId // aca hay que poner proxietype
                                    && w.LicenseTypeId == request.LicenseTypes[i].Id);

                vproxieLicenseType = Db.Select(query).SingleOrDefault();

                if (vproxieLicenseType == null)  // si no existe la relacion => inserta ProxieLicenseType
                {
                    vproxieLicenseType = new ProxieLicenseType();
                    vproxieLicenseType.ProxieId = request.ProxieId;
                    vproxieLicenseType.LicenseTypeId = request.LicenseTypes[i].Id;
                    vproxieLicenseType.Status = (int)BusinessPartnerStatus.Active;

                    vproxieLicenseType.Id = (int)Db.Insert((ProxieLicenseType)vproxieLicenseType, true);
                }
                else
                {
                    if (vproxieLicenseType.Status == (int)BusinessPartnerStatus.Deleted)
                    {
                        vproxieLicenseType.Status = (int)BusinessPartnerStatus.Active;
                        Db.Update((ProxieLicenseType)vproxieLicenseType);
                    }
                }
            }
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(Api.QueryProxieLicenseTypes request)
        {

            var query = Db.From<ProxieLicenseType>()
                    .Join<ProxieLicenseType, Domain.Financials.DebtManagement.LicenseType>()
                    .Join<ProxieLicenseType, Domain.System.Persons.Person>()
                    .OrderByDescending(q => q.Id)
                    .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            return Db.Select(query);
        }


        public object Get(Api.GetProxieLicenseType request)
        {
            var query = Db.From<ProxieLicenseType>()
                        .Join<ProxieLicenseType, Domain.Financials.DebtManagement.LicenseType>()
                        .Where<ProxieLicenseType>(o => o.ProxieId == request.Id && o.Status == 0)
                        .OrderByDescending(q => q.Id);

            var results = Db.Select(query);

            var model = results.ConvertTo<Api.PostProxieLicenseType>();

            model.Id = request.Id;
            model.ProxieId = request.Id;

            model.LicenseTypes = new List<Domain.Financials.DebtManagement.LicenseType>();

            foreach (var result in results)
            {
                var licenseType = Db.SingleById <Domain.Financials.DebtManagement.LicenseType >(result.LicenseTypeId);

                model.LicenseTypes.Add(licenseType);

            }

            return model;


        }

        public QueryResponse<Api.QueryProxieLicenseTypeResult> Get(Api.QueryProxieLicenseTypes request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupProxieLicenseType request)
        {
            var query = Db.From<ProxieLicenseType>()
                        .Join<ProxieLicenseType, Domain.System.Persons.Person>();


            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }

            //query.Where(x => x.Status == (int)BusinessPartnerStatus.Active);

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.GetProxieLicenseTypeResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }

    }
}