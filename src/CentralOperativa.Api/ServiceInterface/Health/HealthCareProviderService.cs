using System;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.Persons;

namespace CentralOperativa.ServiceInterface.Health
{
    using Api = ServiceModel.Health;
    [Authenticate]
    public class HealthCareProviderService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;

        public HealthCareProviderService(
            IAutoQueryDb autoQuery,
            PersonRepository personRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
        }

        public object Put(Api.PostHealthCareProvider request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = Db.SingleById<Domain.Health.HealthCareProvider>(request.Id);
                    if (existing != null)
                    {
                        Db.Update((Domain.Health.HealthCareProvider)request);

                        //TODO: Manejar si se esta modificando una persona que ya tiene otro patient asignado...
                        //return HttpError.Conflict("There is already a patient linked to person " + request.PersonId);
                    }
                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public object Post(Api.PostHealthCareProvider request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = Db.Select(Db.From<Domain.Health.HealthCareProvider>().Where(x => x.PersonId == request.PersonId && x.TenantId == Session.TenantId)).SingleOrDefault();
                    if (existing != null)
                    {
                        return HttpError.Conflict("There is already a health service provider linked to person " + request.PersonId);
                    }

                    request.TenantId = Session.TenantId;
                    request.Id = (int)Db.Insert((Domain.Health.HealthCareProvider)request, true);
                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public async Task<Api.PostHealthCareProviderBatch> Post(Api.PostHealthCareProviderBatch request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    foreach (var item in request.Items)
                    {
                        // Person
                        if (item.PersonId == 0)
                        {
                            var existingPerson = await _personRepository.GetPerson(Db, item.Person.Code);
                            if (existingPerson == null)
                            {
                                item.Person = await _personRepository.CreatePerson(Db, item.Person);

                            }
                            else
                            {
                                item.Person.Id = existingPerson.Id;
                                item.Person = await _personRepository.UpdatePerson(Db, item.Person);
                            }

                            item.PersonId = item.Person.Id;
                        }

                        // HealthCareProvider
                        var existingHealthServiceProvider = (await Db.SelectAsync<Domain.Health.HealthCareProvider>(w => w.PersonId == item.PersonId)).SingleOrDefault();
                        if (existingHealthServiceProvider == null)
                        {
                            item.Id = (int) await Db.InsertAsync((Domain.Health.HealthCareProvider)item, true);
                        }
                        else
                        {
                            item.Id = existingHealthServiceProvider.Id;
                        }
                    }

                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public async Task<Api.GetHealthCareProviderResponse> Get(Api.GetHealthCareProvider request)
        {
            var healthServiceProvider = Db.SingleById<Domain.Health.HealthCareProvider>(request.Id).ConvertTo<Api.GetHealthCareProviderResponse>();
            healthServiceProvider.Person = await _personRepository.GetPerson(Db, healthServiceProvider.PersonId);
            return healthServiceProvider;
        }

        public QueryResponse<Api.QueryHealthCareProvidersResponse> Get(Api.QueryHealthCareProviders request)
        {
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            return _autoQuery.Execute(request, q);
        }

        public object Get(Api.LookupHealthCareProvider request)
        {
            var query = Db.From<Domain.Health.HealthCareProvider>()
                .Join<Domain.Health.HealthCareProvider, Domain.System.Persons.Person>();

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where<Domain.System.Persons.Person>(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
            }

            var count = Db.Count(query);
            query = query.OrderByDescending(q => q.Id)
              .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));


            var result = new LookupResult
            {
                Data = Db.Select<Api.QueryHealthCareProvidersResponse>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }
    }
}
