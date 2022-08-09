using System.Linq;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.Investments;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Financials;
using System.Threading.Tasks;

using Api = CentralOperativa.ServiceModel.Investments;
using ApiSession = CentralOperativa.ServiceModel.System;
using System;

namespace CentralOperativa.ServiceInterface.Investments
{
    [Authenticate]
    public class AssetService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        public AssetService(IAutoQueryDb autoQuery)
        {
            _autoQuery = autoQuery;
        }

        public object Any(Api.QueryAssets request)
        {

            var query = Db.From<Asset>()
                    .OrderByDescending(q => q.Id)
                    .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));
            return Db.Select(query);
        }
        public object Put(Api.PostAsset request)
        {
            {
                Db.Update((Domain.Investments.Asset)request);
                return request;
            }
        }

        public object Post(Api.PostAsset request)
        {
            request.AccountId = request.Id;
            request.Id = 0;
            Account account = Db.SingleById<Account>(request.AccountId);

            var query = $"SELECT SUM(Amount) Balance FROM BusinessPartnerAccountEntries WHERE AccountId = {account.PartnerAccountId} GROUP BY  AccountId";
            var balance = Db.ScalarAsync<decimal>(query);

            if (balance.Result >= request.Amount)
            {
                var createDate = DateTime.UtcNow;
                BusinessPartnerAccountEntry accountEntry = new BusinessPartnerAccountEntry();
                accountEntry.AccountId = account.PartnerAccountId;
                accountEntry.Amount = request.Amount * -1;
                accountEntry.CreateDate = createDate;
                accountEntry.Description = "Plazo Fijo";
                accountEntry.PostingDate = createDate;
                int accountEntryId = (int)Db.Insert((BusinessPartnerAccountEntry)accountEntry, true);
                

                request.CreatedById = Session.UserId;
                request.CreateDate = createDate;
                request.Id = (int)Db.Insert((Asset)request, true);
                return request;

            }
            else
            {
                return HttpError.Conflict("ERR_Insufficient_Balance"); ;
            }

        }


        public async Task<object> Get(Api.GetAsset request)
        {
            var asset = (await Db.SingleByIdAsync<Asset>(request.Id)).ConvertTo<Api.Asset>();
            return asset;

        }

        //public QueryResponse<Api.QueryAssetsResult> Get(Api.QueryAssets request)
        //{

        //}

        public object Get(Api.LookupAssets request)
        {
            var query = Db.From<Domain.Investments.Asset>();

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
                Data = Db.Select<Api.GetAssetResult>(query).Select(x => new LookupItem { Id = x.Id }),
                Total = (int)count
            };
            return result;
        }

        public object Delete(Api.DeleteAsset request)
        {

            var trader = Db.SingleById<Asset>(request.Id);

            return request;
        }
    }
}
