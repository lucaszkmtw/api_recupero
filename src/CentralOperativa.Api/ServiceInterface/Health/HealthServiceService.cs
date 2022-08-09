using System;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Health;
using CentralOperativa.ServiceInterface.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class HealthServiceService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;

        public HealthServiceService(
            IAutoQueryDb autoQuery,
            PersonRepository personRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
        }

        public async Task<HealthService.GetResult> Get(HealthService.GetHealthService request)
        {
            HealthService.GetResult model = null;
            var tenantHealthServiceIds = Db.From<Domain.Health.HealthServiceTenant>().Where(w => w.TenantId == Session.TenantId).Select(x => x.HealthServiceId);
            var healthService = (await Db.SelectAsync(Db.From<Domain.Health.HealthService>()
                .Where(w => w.Id == request.Id)
                .And(w => Sql.In(w.Id, tenantHealthServiceIds))))
                .SingleOrDefault();

            if (healthService != null)
            {
                model = healthService.ConvertTo<HealthService.GetResult>();
                var person = await _personRepository.GetPerson(Db, healthService.PersonId);
                model.Person = person;

            }
            return model;
        }

        public async Task<HealthService.GetResult> Get(HealthService.GetHealthServiceByCode request)
        {
            HealthService.GetResult model = null;

            var tenantHealthServiceIds = Db.From<Domain.Health.HealthServiceTenant>().Where(w => w.TenantId == Session.TenantId).Select(x => x.HealthServiceId);
            var healthService = (await Db.SelectAsync(Db.From<Domain.Health.HealthService>()
                .Where(w => w.Code == request.Code)
                .And(w => Sql.In(w.Id, tenantHealthServiceIds))))
                .SingleOrDefault();

            if (healthService != null)
            {
                model = healthService.ConvertTo<HealthService.GetResult>();
                var person = await _personRepository.GetPerson(Db, healthService.PersonId);
                model.Person = person;
            }

            return model;
        }

        public object Put(HealthService.PostHealthService request)
        {
            return Db.Update((Domain.Health.HealthService)request);
        }

        public async Task<HealthService.GetResult> Post(HealthService.PostHealthService request)
        {
            if (request.PersonId == 0 && request.Person != null)
            {
                request.Person = await _personRepository.CreatePerson(Db, request.Person);
                request.PersonId = request.Person.Id;
            }

            var tenantHealthServiceIds = Db.From<Domain.Health.HealthServiceTenant>().Where(w => w.TenantId == Session.TenantId).Select(x => x.HealthServiceId);
            var healthService = (await Db.SelectAsync(Db.From<Domain.Health.HealthService>()
                .Where(w => w.Id == request.Id)
                .And(w => Sql.In(w.Id, tenantHealthServiceIds))))
                .SingleOrDefault();
            if (healthService == null)
            {
                request.Id = (int) await Db.InsertAsync((Domain.Health.HealthService) request, true);
                Db.Insert(new Domain.Health.HealthServiceTenant
                {
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = Session.UserId,
                    HealthServiceId = request.Id,
                    TenantId = Session.TenantId
                });
            }
            else
            {
                request.Id = healthService.Id;
            }

            return await Get(new HealthService.GetHealthService { Id = request.Id });
        }

        public QueryResponse<HealthService.QueryHealthServicesResponse> Get(HealthService.QueryHealthServices request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var requestParams = Request.GetRequestParams();
            var q = _autoQuery.CreateQuery(request, requestParams);
            var tenantHealthServiceIds = Db.From<Domain.Health.HealthServiceTenant>().Where(w => w.TenantId == Session.TenantId).Select(x => x.HealthServiceId);
            q.Where(w => Sql.In(w.Id, tenantHealthServiceIds));
            return _autoQuery.Execute(request, q);
        }

        public LookupResult Get(HealthService.Lookup request)
        {
            var query = Db.From<Domain.Health.HealthService>()
                .Join<Domain.Health.HealthService, Domain.System.Persons.Person>();

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id);
            }
            else if (request.Ids != null)
            {
                query.Where(w => Sql.In(w.Id, request.Ids));
            }
            else if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where<Domain.System.Persons.Person>(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
            }

            var count = Db.Count(query);
            query = query.OrderBy(q => q.Code)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<HealthServiceLookupQueryResult>(query).Select(x => new LookupItem { Id = x.Id, Text = $"{x.PersonName}" }),
                Total = (int)count
            };
            return result;
        }

        public class HealthServiceLookupQueryResult
        {
            public int Id { get; set; }
            public string PersonCode { get; set; }
            public string PersonName { get; set; }
        }
    }
}