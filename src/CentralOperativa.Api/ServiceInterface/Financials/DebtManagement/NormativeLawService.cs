using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using System.Collections.Generic;
using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class NormativeLawService : ApplicationService
    {
        /*
        public object Put(Api.PostNormativeLaw request)
        {
            Db.Update((NormativeLaw)request);
            return request;
        }
    
        public object Post(Api.PostNormativeLaw request)
        {
            {
                request.Id = (int)Db.Insert((NormativeLaw)request, true);
                return request;
            }
        }*/
        public object Put(Api.PostNormativeLaw request)
        {
            var q = Db.From<NormativeLaw>()
                 .Where(x => x.NormativeId == request.NormativeId)
                 .Update(p => p.Status);

            Db.UpdateOnly(new NormativeLaw { Status = 1 }, onlyFields: q);

            for (var i = 0; i < request.Laws.Count; i++) // por cada tipo de ley que seleccione
            {
                NormativeLaw vnormativeLaw;
                var query = Db.From<NormativeLaw>()
                           .Where(w => w.NormativeId == request.NormativeId
                                   && w.LawId == request.Laws[i].Id);

                vnormativeLaw = Db.Select(query).SingleOrDefault();

                if (vnormativeLaw == null)  // si no existe la relacion => inserta NormativeLaw
                {
                    NormativeLaw vnormativelaw = new NormativeLaw();
                    vnormativelaw.NormativeId = request.NormativeId;
                    vnormativelaw.LawId = request.Laws[i].Id;
                    vnormativelaw.Status = (int)BusinessPartnerStatus.Active;

                    vnormativelaw.Id = (int)Db.Insert((NormativeLaw)vnormativelaw, true);
                }
                else
                {
                    if (vnormativeLaw.Status == (int)BusinessPartnerStatus.Deleted)
                    {
                        vnormativeLaw.Status = (int)BusinessPartnerStatus.Active;
                        Db.Update((NormativeLaw)vnormativeLaw);
                    }
                }

            }

            return request;
        }

        public object Post(Api.PostNormativeLaw request)
        {
            for (var i = 0; i < request.Laws.Count; i++) // por cada ley que seleccione
            {
                NormativeLaw vnormativeLaw;
                var query = Db.From<NormativeLaw>()
                            .Where(w => w.LawId == request.LawId // aca hay que poner laws
                                    && w.LawId == request.Laws[i].Id);

                vnormativeLaw = Db.Select(query).SingleOrDefault();

                if (vnormativeLaw == null)  // si no existe la relacion => inserta normativelaws
                {
                    vnormativeLaw = new NormativeLaw();
                    vnormativeLaw.NormativeId = request.NormativeId;
                    vnormativeLaw.LawId = request.Laws[i].Id;
                    vnormativeLaw.Status = (int)BusinessPartnerStatus.Active;

                    vnormativeLaw.Id = (int)Db.Insert((NormativeLaw)vnormativeLaw, true);
                }
                else
                {
                    if (vnormativeLaw.Status == (int)BusinessPartnerStatus.Deleted)
                    {
                        vnormativeLaw.Status = (int)BusinessPartnerStatus.Active;
                        Db.Update((NormativeLaw)vnormativeLaw);
                    }
                }
            }
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(Api.QueryNormativeLaws request)
        {

            var query = Db.From<NormativeLaw>()
                    .Join<NormativeLaw, Normative>()
                    .Join<NormativeLaw, Law>()
                    .OrderByDescending(q => q.Id)
                    .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            return Db.Select(query);
        }
        /*
        public object Get(Api.GetNormativeLaw request)
        {
            var organism = Db.SingleById<NormativeLaw>(request.Id);
            var model = organism.ConvertTo<Api.NormativeLaw>();
            return model;
        }
        */

        public object Get(Api.GetNormativeLaw request)
        {
            var query = Db.From<NormativeLaw>()
                        .Join<NormativeLaw, Law>()
                        .Where<NormativeLaw>(o => o.NormativeId == request.Id)
                        .OrderByDescending(q => q.Id);

            var results = Db.Select(query);

            var model = results.ConvertTo<Api.PostNormativeLaw>();

            //model.Id = request.Id;
            model.NormativeId = request.Id;

            model.Laws = new List<Law>();

            foreach (var result in results)
            {
                var law = Db.SingleById<Law>(result.LawId);

                model.Laws.Add(law);

            }

            return model;
        }

        public QueryResponse<Api.QueryNormativeLawResult> Get(Api.QueryNormativeLaws request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupNormativeLaw request)
        {
            var query = Db.From<NormativeLaw>()
                        .Join<NormativeLaw,Normative>()
                        .Join<NormativeLaw, Law>();


            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }


            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.GetNormativeLawResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.NormativeName }),
                Total = (int)count
            };
            return result;
        }


    }
}