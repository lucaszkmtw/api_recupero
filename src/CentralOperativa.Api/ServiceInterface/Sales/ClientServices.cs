using System;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Sales;
using CentralOperativa.ServiceInterface.BusinessPartners;
using CentralOperativa.ServiceInterface.System.Persons;
using CentralOperativa.Infraestructure;
using Api = CentralOperativa.ServiceModel.Sales;

namespace CentralOperativa.ServiceInterface.Sales
{
    [Authenticate]
    public class ClientService : ApplicationService
    {
        private readonly PersonRepository _personRepository;
        private readonly BusinessPartnerRepository _businessPartnerRepository;

        public ClientService(PersonRepository personRepository, BusinessPartnerRepository businessPartnerRepository)
        {
            _personRepository = personRepository;
            _businessPartnerRepository = businessPartnerRepository;
        }

        public async Task<Api.GetClientResult> Get(Api.GetClient request)
        {
            var model = new Api.GetClientResult
            {
                BusinessPartner = await _businessPartnerRepository.GetBusinessPartner(Db, request.Id, true),
                Id = request.Id
            };

            return model;
        }

        public async Task<Api.PostClient> Put(Api.PostClient request)
        {
            request = await SaveClient(request);
            return request;
        }

        public async Task<Api.PostClient> Post(Api.PostClient request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.BusinessPartner.Status = BusinessPartnerStatus.Active;
                    request = await SaveClient(request);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return request;
        }

        private async Task<Api.PostClient> SaveClient(Api.PostClient request)
        {
            request.BusinessPartner.TenantId = Session.TenantId;
            if (request.BusinessPartner.PersonId == default(int) && request.BusinessPartner.Person != null)
            {
                //request.BusinessPartner.Person = await _personRepository.CreatePerson(Db, request.BusinessPartner.Person);
                request.BusinessPartner.PersonId = request.BusinessPartner.Person.Id;
            }

            if (request.BusinessPartner.Id != default(int))
            {
                await Db.UpdateAsync((BusinessPartner)request.BusinessPartner);
            }
            else
            {
                var businessPartnerResult = await _businessPartnerRepository.InsertBusinessPartner(Db, Session, request.BusinessPartner);
                request.BusinessPartner = businessPartnerResult.Item1;
                if (businessPartnerResult.Item2)
                {
                    Db.Insert(new Client { Id = request.BusinessPartner.Id });
                }
            }

            request.Id = request.BusinessPartner.Id;

            return request;
        }

        public async Task Delete(Api.DeleteClient request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var businessPartner = await _businessPartnerRepository.GetBusinessPartner(Db, request.Id);
                    if (businessPartner != null && businessPartner.TypeId == 1)
                    {
                        //Borrado lódico
                        businessPartner.Status = BusinessPartnerStatus.Deleted;
                        await Db.UpdateAsync((BusinessPartner)businessPartner);
                        //Borrado fisico
                        //Db.DeleteById<Domain.Sales.Client>(request.Id);
                        //Db.DeleteById<Domain.BusinessPartners.BusinessPartner>(request.Id);
                    }
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }


        /// <summary>
        /// Returns PersonId and PersonName
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Get(Api.LookupClient request)
        {
            var query = Db.From<BusinessPartner>()
                .Join<BusinessPartner, Domain.System.Persons.Person>()
                .Where(w => w.TypeId == 1 && w.TenantId == Session.TenantId);

            if (request.Id.HasValue)
            {
                if (request.ReturnPersonId.HasValue && request.ReturnPersonId.Value)
                {
                    query = query.Where<Domain.System.Persons.Person>(w => w.Id == request.Id.Value);
                }
                else
                {
                    query = query.Where<Client>(w => w.Id == request.Id.Value);
                }
            }
            else if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where<Domain.System.Persons.Person>(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.QueryClientsResult>(query).Select(x => new LookupItem { Id = (request.ReturnPersonId.HasValue && request.ReturnPersonId.Value) ? x.PersonId : x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }
    }
}