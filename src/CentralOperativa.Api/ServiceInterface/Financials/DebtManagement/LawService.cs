using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.Domain.BusinessPartners;
using System;
using System.Threading.Tasks;

namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class LawService : ApplicationService
    {
        public object Put(Api.PostLaw request)
        {
            Db.Update((Law)request);
            return request;
        }

        public object Post(Api.PostLaw request)
        {
            var normative = Db.Single<Law>(w => w.Code == request.Code);
            if (normative != null)
            {
                normative.Status = (int)BusinessPartnerStatus.Active;
                normative.Name = request.Name;
                normative.Prescription = request.Prescription;
                normative.MaxPrescriptionDate = request.MaxPrescriptionDate;
                Db.Update((Law)normative);
            }
            else
            {
                request.Id = (int)Db.Insert((Law)request, true);
            }
            return request;
        }

        public object Delete(Api.DeleteLaw request)
        {
            var law = Db.SingleById<Law>(request.Id);
            law.Status = (int)BusinessPartnerStatus.Deleted;
            Db.Update((Law)law);

            return request;
        }
      
        public async Task<Api.GetResponseLaw> Get(Api.GetLaw request)
        {            
            var law = (await Db.SelectAsync<Api.GetResponseLaw>(Db.From<Law>()
                .Where(w => w.Id == request.Id))).SingleOrDefault();
            return law;
        }
        
        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(Api.QueryLaws request)
        {
            var query = Db.From<Domain.Financials.DebtManagement.Law>()
                .Where<Law>(w => w.Status == (int)BusinessPartnerStatus.Active)
                .OrderByDescending(q => q.Name)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            if (!string.IsNullOrEmpty(request.Name))
                query.Where(q => q.Name.Contains(request.Name));

            if (!string.IsNullOrEmpty(request.Code))
                query.Where(q => q.Code.Contains(request.Code));                

            return Db.Select(query);
        }

        public LookupResult Get(Api.LookupLaw request)
        {
            var query = Db.From<Law>();

            if (request.CategoryId != default(int) && request.ProductId != default(int))
            {
                query.Join<Law, Domain.Catalog.ProductCategoryLaw>((l, pcl) => l.Id == pcl.LawId)
                     .Join<Domain.Catalog.ProductCategoryLaw, Domain.Catalog.ProductCategory>((pcl, pc) => pcl.ProductCategoryId == pc.Id
                                && pc.CategoryId == request.CategoryId && pc.ProductId == request.ProductId);
            }

            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }

            if (request.OrganismId != default(int))
            {
                var categoriesIds = Db.Select(Db.From<Domain.Financials.DebtManagement.OrganismLaw>().Where(oc => oc.OrganismId == request.OrganismId)).Select(x => x.LawId).ToList();
                if (categoriesIds != null)
                {
                    query.Where(x => Sql.In(x.Id, categoriesIds));
                }
            }



            query.Select(x => new { x.Id, x.Name });

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }
        /*
        public object Get(Api.GetLaw request)
        {
            var model = Db.SingleById<Law>(request.Id);
            return model;
        }*/
    }
}