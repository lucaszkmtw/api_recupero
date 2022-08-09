using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Financials;
using CentralOperativa.Domain.Inv;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.Persons;
using CentralOperativa.ServiceModel.BusinessPartners;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.BusinessPartners
{
    [Authenticate]
    public class BusinessPartnerService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;
        private readonly BusinessPartnerRepository _businessPartnerRepository;

        public BusinessPartnerService(IAutoQueryDb autoQuery, PersonRepository personRepository, BusinessPartnerRepository businessPartnerRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _businessPartnerRepository = businessPartnerRepository;
        }

        public async Task<GetBusinessPartnerResult> Get(GetBusinessPartner request)
        {
            var model = (await Db.SingleByIdAsync<BusinessPartner>(request.Id)).ConvertTo<GetBusinessPartnerResult>();
            model.Person = await _personRepository.GetPerson(Db, model.PersonId);
            return model;
        }

        public async Task<GetBusinessPartnerPersonResult> Get(GetBusinessPartnerByPerson request)
        {
            var data = await Db.SingleAsync<BusinessPartner>(w => w.PersonId == request.PersonId
            && w.TypeId == request.TypeId
            && w.TenantId == Session.TenantId);
            var model = data.ConvertTo<GetBusinessPartnerPersonResult>();
            model.InventorySites = await Db.SelectAsync(Db.From<InventorySite>().Where(w => w.PersonId == request.PersonId));
            return model;
        }

        public async Task<GetBusinessPartnerResult> Put(PostBusinessPartner request)
        {
            await Db.UpdateAsync((BusinessPartner)request);
            return await _businessPartnerRepository.GetBusinessPartner(Db, request.Id);
        }

        public async Task<PostBusinessPartner> Post(PostBusinessPartner request)
        {
            var client = (BusinessPartner)request;
            client.TenantId = Session.TenantId;
            request.Id = (int)await Db.InsertAsync(client, true);
            return request;
        }

        public object Get(QueryBusinessPartners request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where(w => w.TenantId == Session.TenantId && w.Status == BusinessPartnerStatus.Active);
            var model = _autoQuery.Execute(request, q);
            return Request.ToOptimizedResult(model);
        }

        public async Task<object> Get(LookupBusinessPartner request)
        {
            var query = Db.From<BusinessPartner>().Join<BusinessPartner, Person>();
            if (request.Id.HasValue)
            {
                query = query.Where(w => w.Id == request.Id.Value);
            }
            else if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where<Person>(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
            }
            query.Where(w => w.TenantId == Session.TenantId);
            var count = await Db.CountAsync(query);

            query = query.OrderByDescending(q => q.Id)
               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));

            var result = new LookupResult
            {
                Data = (await Db.SelectAsync<QueryBusinessPartnersResult>(query)).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }

        public async Task<AccountBusinessPartner> Get(AccountBusinessPartner request)
        {
            var model = (await Db.SingleByIdAsync<BusinessPartnerAccount>(request.Id)).ConvertTo<AccountBusinessPartner>();
            return model;
        }

        public object Get(AccountBusinessPartnerEntries request)
        {
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.UnsafeOrderBy("CreateDate desc");
            var model = _autoQuery.Execute(request, q);

            var balance = (decimal)0.00;
            for (var i = model.Results.Count - 1; i >= 0; i--)
            {
                balance += model.Results[i].Amount;
                model.Results[i].Balance = balance;
            }

            return model;
        }

        public async Task<object> Get(GetBusinessPartnerPerson request)
        {
            var model = request;
            var businessPartners = await Db.SelectAsync(Db.From<BusinessPartner>().Where(w => w.PersonId == request.Id && w.TenantId == Session.TenantId));
            var businessPartnerIds = new List<int>();
            foreach (var businessPartner in businessPartners)
            {
                businessPartnerIds.Add(businessPartner.Id);
            }

            model.Accounts.Currencies = (await Db.SelectAsync(Db.From<Currency>()
                .Join<Currency, BusinessPartnerAccount>()
                .Where<BusinessPartnerAccount>(w => businessPartnerIds.Contains(w.BusinessPartnerId))
                )).Distinct().ToList();
            if (businessPartnerIds.Count > 0)
            {
                var query = $"SELECT bpa.*, (SELECT SUM(bpae.Amount) Amount FROM BusinessPartnerAccountEntries bpae WHERE bpae.AccountId = bpa.Id) Balance FROM BusinessPartnerAccounts bpa WHERE bpa.BusinessPartnerId IN (" + string.Join(",", businessPartnerIds) + ")"; //= {businessPartner.Id}";
                model.Accounts.Items = Db.Select<GetBusinessPartnerPersonResult.BusinessPartnerAccounts.Account>(query);
            }
            return model;
        }
    }
}









//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using ServiceStack;
//using ServiceStack.OrmLite;

//using CentralOperativa.Domain.BusinessPartners;
//using CentralOperativa.Infraestructure;
//using CentralOperativa.ServiceInterface.System.Persons;

//namespace CentralOperativa.ServiceInterface.BusinessPartners
//{
//    [Authenticate]
//    public class BusinessPartnerService : ApplicationService
//    {
//        private readonly IAutoQueryDb _autoQuery;
//        private readonly PersonRepository _personRepository;
//        private readonly BusinessPartnerRepository _businessPartnerRepository;

//        public BusinessPartnerService(IAutoQueryDb autoQuery, PersonRepository personRepository, BusinessPartnerRepository businessPartnerRepository)
//        {
//            _autoQuery = autoQuery;
//            _personRepository = personRepository;
//            _businessPartnerRepository = businessPartnerRepository;
//        }

//        public async Task<ServiceModel.BusinessPartners.GetBusinessPartnerResult> Get(ServiceModel.BusinessPartners.GetBusinessPartner request)
//        {
//            var model = (await Db.SingleByIdAsync<BusinessPartner>(request.Id)).ConvertTo<ServiceModel.BusinessPartners.GetBusinessPartnerResult>();
//            model.Person = await _personRepository.GetPerson(Db, model.PersonId);
//            return model;
//        }

//        public async Task<ServiceModel.BusinessPartners.GetBusinessPartnerPersonResult> Get(ServiceModel.BusinessPartners.GetBusinessPartnerByPerson request)
//        {
//            var data = await Db.SingleAsync<BusinessPartner>(w => w.PersonId == request.PersonId
//            && w.TypeId == request.TypeId
//            && w.TenantId == Session.TenantId);
//            var model = data.ConvertTo<ServiceModel.BusinessPartners.GetBusinessPartnerPersonResult>();
//            model.InventorySites = await Db.SelectAsync(Db.From<Domain.Inv.InventorySite>().Where(w => w.PersonId == request.PersonId));
//            return model;
//        }

//        public async Task<ServiceModel.BusinessPartners.GetBusinessPartnerResult> Put(ServiceModel.BusinessPartners.PostBusinessPartner request)
//        {
//            await Db.UpdateAsync((BusinessPartner)request);
//            return await _businessPartnerRepository.GetBusinessPartner(Db, request.Id);
//        }

//        public async Task<ServiceModel.BusinessPartners.PostBusinessPartner> Post(ServiceModel.BusinessPartners.PostBusinessPartner request)
//        {
//            var client = (BusinessPartner) request;
//            client.TenantId = Session.TenantId;
//            request.Id = (int) await Db.InsertAsync(client, true);
//            return request;
//        }

//        public object Get(ServiceModel.BusinessPartners.QueryBusinessPartners request)
//        {
//            if (request.OrderBy == null)
//            {
//                request.OrderBy = "Name";
//            }

//            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
//            q.Where(w => w.TenantId == Session.TenantId && w.Status == BusinessPartnerStatus.Active);
//            var model = _autoQuery.Execute(request, q);
//            return Request.ToOptimizedResult(model);
//        }

//        public async Task<object> Get(ServiceModel.BusinessPartners.LookupBusinessPartner request)
//        {
//            var query = Db.From<BusinessPartner>().Join<BusinessPartner, Domain.System.Persons.Person>();
//            if (request.Id.HasValue)
//            {
//                query = query.Where(w => w.Id == request.Id.Value);
//            }
//            else if (!string.IsNullOrEmpty(request.Q))
//            {
//                query = query.Where<Domain.System.Persons.Person>(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
//            }
//            query.Where(w => w.TenantId == Session.TenantId);
//            var count = await Db.CountAsync(query);

//            query = query.OrderByDescending(q => q.Id)
//               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));

//            var result = new LookupResult
//            {
//                Data = (await Db.SelectAsync<ServiceModel.BusinessPartners.QueryBusinessPartnersResult>(query)).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
//                Total = (int)count
//            };
//            return result;
//        }

//        public async Task<ServiceModel.BusinessPartners.AccountBusinessPartner> Get(ServiceModel.BusinessPartners.AccountBusinessPartner request)
//        {
//            var model = (await Db.SingleByIdAsync<BusinessPartnerAccount>(request.Id)).ConvertTo<ServiceModel.BusinessPartners.AccountBusinessPartner>();
//            return model;
//        }

//        public object Get(ServiceModel.BusinessPartners.AccountBusinessPartnerEntries request)
//        {
//            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
//            q.UnsafeOrderBy("CreateDate desc");
//            var model = _autoQuery.Execute(request, q);

//            var balance = (decimal) 0.00;
//            for(var i = model.Results.Count - 1; i >= 0; i--)
//            {
//                balance += model.Results[i].Amount;
//                model.Results[i].Balance = balance;
//            }

//            return model;
//        }

//        public async Task<object> Get(ServiceModel.BusinessPartners.GetBusinessPartnerPerson request)
//        {
//            var model = request;
//            var businessPartners = await Db.SelectAsync(Db.From<BusinessPartner>().Where(w => w.PersonId == request.Id && w.TenantId == Session.TenantId));
//            var businessPartnerIds = new List<int>();
//            foreach (var businessPartner in businessPartners)
//            {
//                businessPartnerIds.Add(businessPartner.Id);
//            }

//            model.Accounts.Currencies = (await Db.SelectAsync(Db.From<Domain.Financials.Currency>()
//                .Join<Domain.Financials.Currency, BusinessPartnerAccount>()
//                .Where<BusinessPartnerAccount>(w => businessPartnerIds.Contains(w.BusinessPartnerId))
//                )).Distinct().ToList();
//            if(businessPartnerIds.Count > 0)
//            {
//                var query = $"SELECT bpa.*, (SELECT SUM(bpae.Amount) Amount FROM BusinessPartnerAccountEntries bpae WHERE bpae.AccountId = bpa.Id) Balance FROM BusinessPartnerAccounts bpa WHERE bpa.BusinessPartnerId IN (" + string.Join(",", businessPartnerIds) + ")"; //= {businessPartner.Id}";
//                model.Accounts.Items = Db.Select<ServiceModel.BusinessPartners.GetBusinessPartnerPersonResult.BusinessPartnerAccounts.Account>(query);
//            }
//            return model;
//        }
//    }
//}