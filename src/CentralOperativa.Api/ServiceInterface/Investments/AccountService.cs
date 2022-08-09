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
    public class AccountService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        public AccountService(IAutoQueryDb autoQuery)
        {
            _autoQuery = autoQuery;
        }

        public object Any(Api.QueryAccounts request)
        {

            var query = Db.From<Account>()
                    .OrderByDescending(q => q.Id)
                    .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));
            return Db.Select(query);
        }
        public object Put(Api.PostAccount request)
        {
            {
                BusinessPartnerAccount bpaccount = Db.SingleById<BusinessPartnerAccount>(request.PartnerAccountId);
                bpaccount.CurrencyId = Convert.ToInt16(request.CurrencyId);
                bpaccount.Type = request.Type;
                bpaccount.Code = request.Prefix;
                bpaccount.Name = request.Prefix;
                Db.Update((BusinessPartnerAccount)bpaccount);
                Db.Update((Domain.Investments.Account)request);
                return request;
            }
        }

        public object Post(Api.PostAccount request)
        {

            BusinessPartnerAccount bpaccount = new BusinessPartnerAccount();
            var investor = Db.SingleById<Domain.Investments.Investor>(request.InvestorId);
            var model = investor.ConvertTo<Api.Investor>();

            bpaccount.BusinessPartnerId = model.BusinessPartnerId;
            bpaccount.Code = request.Prefix;
            bpaccount.Name = request.Prefix;
            bpaccount.CreateDate = DateTime.UtcNow;
            bpaccount.CreatedById = Session.UserId;
            bpaccount.CurrencyId = Convert.ToInt16(request.CurrencyId);
            bpaccount.Guid = Guid.NewGuid();
            bpaccount.Type = request.Type;

            int partnerAccountId = (int)Db.Insert((BusinessPartnerAccount)bpaccount,true );
            request.PartnerAccountId = partnerAccountId;
            request.CreatedById = Session.UserId;
            request.CreateDate = DateTime.UtcNow;
            request.Id = (int)Db.Insert((Account)request, true);

            return request;
            
        }

        
         public async Task<object> Get(Api.GetAccount request)
        {
            var account = (await Db.SingleByIdAsync<Account>(request.Id)).ConvertTo<Api.Account>();
            BusinessPartnerAccount bpAccount = (await Db.SingleByIdAsync<BusinessPartnerAccount>(account.PartnerAccountId));
            account.PartnerAccount = bpAccount;
            account.Currency = (await Db.SingleByIdAsync<Currency>(bpAccount.CurrencyId)).ConvertTo<Currency>();
            
            //var account = Db.SingleById<Account>(request.Id);
            //var model = account.ConvertTo<Api.Account>();

            //Code
            var query = $"SELECT SUM(Amount) Balance FROM BusinessPartnerAccountEntries WHERE AccountId = {account.PartnerAccountId} GROUP BY  AccountId";
            var balance = await Db.ScalarAsync<decimal>(query);
            account.Balance = balance;




            return account;

        }

        public QueryResponse<Api.QueryAccountsResult> Get(Api.QueryAccounts request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }
            var parameters = Request.GetRequestParams();

            var q = _autoQuery.CreateQuery(request, parameters)
                //investor
                .Join<Account, Investor>((acc, inv) => acc.InvestorId == inv.Id)
                .Join<Investor, Person>((inv, pinv) => inv.PersonId == pinv.Id, Db.JoinAlias("InvestorPerson"))
                //trader
                .Join<Account, Trader>((acc, tr) => acc.TraderId == tr.Id)
                .Join<Trader, Person>((tr, ptr) => tr.PersonId == ptr.Id, Db.JoinAlias("TraderPerson"))
                //custodian
                .Join<Account, Custodian>((acc, cs) => acc.CustodianId == cs.Id)
                .Join<Custodian, Person>((cs, pcs) => cs.PersonId == pcs.Id, Db.JoinAlias("CustodianPerson"))
                //manager
                .Join<Account, Manager>((acc, mg) => acc.ManagerId == mg.Id)
                .Join<Manager, Person>((mg, pmg) => mg.PersonId == pmg.Id, Db.JoinAlias("ManagerPerson"))
                //businesPartnerAccount
                .Join<Account, BusinessPartnerAccount>((acc, bpacc) => acc.PartnerAccountId == bpacc.Id)
                //currency
                .Join<BusinessPartnerAccount, Currency>((bpacc, cy)=> bpacc.CurrencyId == cy.Id);
                

            q.Select<Account,Person, Person, Person, Person, BusinessPartnerAccount, Currency>((acc, ptr, pinv, pcs, pmg, bpacc, cy) => new {
                acc.Id,
                //investor
                acc.InvestorId,
                acc.InvestorAssignment,
                InvestorPersonName = Sql.JoinAlias(pinv.Name, "InvestorPerson"),
                //trader
                acc.TraderId,
                acc.TraderAssignment,
                TraderPersonName = Sql.JoinAlias(ptr.Name, "TraderPerson"),
                //custodian
                acc.CustodianId,
                acc.CustodianAssignment,
                CustodianPersonName = Sql.JoinAlias(pcs.Name, "CustodianPerson"),
                //manager
                acc.ManagerId,
                acc.ManagerAssignment,
                ManagerPersonName = Sql.JoinAlias(pmg.Name, "ManagerPerson"),
                //businessPartnerAccount
                BusinessPartnerAccountName = bpacc.Name,
                BusinessPartnerAccountType = bpacc.Type,
                BusinessPartnerAccountCode = bpacc.Code,
                //currency
                CurrencySymbol = cy.Symbol,
                CurrencyName = cy.Name
            });
            q.OrderByDescending(o => o.Id);

            var result = _autoQuery.Execute(request, q);
            return result;

        }

        public object Get(Api.LookupAccounts request)
        {
            var query = Db.From<Domain.Investments.Account>();

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
                Data = Db.Select<Api.GetAccountResult>(query).Select(x => new LookupItem { Id = x.Id }),
                Total = (int)count
            };
            return result;
        }

        public object Delete(Api.DeleteAccount request)
        {

            var trader = Db.SingleById<Account>(request.Id);

            return request;
        }
    }
}
