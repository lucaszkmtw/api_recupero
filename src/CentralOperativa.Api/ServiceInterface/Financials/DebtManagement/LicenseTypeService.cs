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
    public class LicenseTypeService : ApplicationService
    {
        public object Put(Api.PostLicenseType request)
        {
            Db.Update((Domain.Financials.DebtManagement.LicenseType)request);
            return request;
        }

        public object Post(Api.PostLicenseType request)
        {
            var licensetype = Db.Single<Domain.Financials.DebtManagement.LicenseType>(w => w.Name == request.Name);
            if (licensetype != null)
            {
                licensetype.Status = (int)BusinessPartnerStatus.Active;
                Db.Update((Domain.Financials.DebtManagement.LicenseType)licensetype);
                request.Id = licensetype.Id;
            }
            else
            {
                request.Id = (int)Db.Insert((Domain.Financials.DebtManagement.LicenseType)request, true);
            }
            return request;
        }

        public object Delete(Api.DeleteLicenseType request)
        {

            var licensetype = Db.SingleById<Domain.Financials.DebtManagement.LicenseType>(request.Id);
            licensetype.Status = (int)BusinessPartnerStatus.Deleted;
            Db.Update((Domain.Financials.DebtManagement.LicenseType)licensetype);

            return request;
        }


        public async Task<Api.GetLicenseTypeResponse> Get(Api.GetLicenseType request)
        {
            var licensetype = (await Db.SelectAsync<Api.GetLicenseTypeResponse>(Db.From<Domain.Financials.DebtManagement.LicenseType>()
                .Where(w => w.Id == request.Id))).SingleOrDefault();
            return licensetype;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object GetLicenseType(Api.GetLicenseType request)
        {
            var model = Db.SingleById<Domain.Financials.DebtManagement.LicenseType>(request.Id);
            return model;
        }

        public LookupResult Get(Api.LookupLicenseType request)
        {
            var query = Db.From<Domain.Financials.DebtManagement.LicenseType>()
                .Select(x => new { x.Id, x.Name });

            /*
            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(q => q.Name.Contains(request.Q) || q.Name.Contains(request.Q));
            }
            */

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
               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }



    }
}