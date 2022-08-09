using System;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System.Persons;

namespace CentralOperativa.ServiceInterface.System.Persons
{
    [Authenticate]
    public class PersonService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(PersonService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;

        public PersonService(IAutoQueryDb autoQuery, PersonRepository personRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
        }

        public async Task<LookupResult> Get(Api.LookupPerson request)
        {
            var query = Db.From<Person>();
            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }

            else
            {
                if (!string.IsNullOrEmpty(request.Filter))
                {
                    if (request.Filter == "with_no_linked_user")
                    {
                        var userPersonsIds = Db.Column<int>(Db.From<Domain.System.User>().Select(x => x.PersonId));
                        query.And(w => !Sql.In(w.Id, userPersonsIds));
                    }
                }

                if (!string.IsNullOrEmpty(request.Q))
                {
                    var tokens = request.Q.Split(' ');
                    foreach (var token in tokens)
                    {
                        int intToken;
                        if (int.TryParse(token, out intToken))
                        {
                            query.Where(x => x.Name.Contains(token) || x.Code.Contains(token));
                        }
                        else
                        {
                            query.Where(x => x.Name.Contains(token));
                        }
                    }
                }
            }

            var count = await Db.CountAsync(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = (await Db.SelectAsync(query)).Select(x => new LookupItem { Id = x.Id, Text = x.Name + (!string.IsNullOrEmpty(x.Code) ? " - (" + x.Code + ")" : string.Empty) }),
                Total = (int)count
            };
            return result;
        }

        public async Task<Api.Person> Get(Api.GetPerson request)
        {
            var person = await _personRepository.GetPerson(Db, request.Id);
            return person;
        }

        public async Task<Api.Person> Get(Api.GetPersonByCode request)
        {
            var person = (await Db.SelectAsync<Api.Person>(Db.From<Person>().Where(w => w.Code == request.Code))).SingleOrDefault();
            if (person != null)
            {
                person.Emails = await Db.SelectAsync(Db.From<PersonEmail>().Where(w => w.PersonId == person.Id));
                person.Phones = await Db.SelectAsync(Db.From<PersonPhone>().Where(w => w.PersonId == person.Id));
                person.Addresses = await Db.SelectAsync<Api.PersonAddress>(Db.From<PersonAddress>().Where(w => w.PersonId == person.Id));
                foreach (var personAddress in person.Addresses)
                {
                    personAddress.Address = (await Db.SingleByIdAsync<Domain.System.Location.Address>(personAddress.AddressId)).ConvertTo<ServiceModel.System.Location.Address.GetAddressResult>();
                    personAddress.Address.Place = await Db.SingleByIdAsync<Domain.System.Location.PlaceNode>(personAddress.Address.PlaceId);
                }

                person.References = await Db.SingleAsync<Reference>(w => w.PersonId == person.Id);
            }
            return person;
        }

        public async Task<Api.Person> Get(Api.GetPersonByEmail request)
        {
            var person = (await Db.SelectAsync<Api.Person>(Db.From<Person>()
                .Join<Person, PersonEmail>()
                .Where<PersonEmail>(w => w.Address == request.Email))).FirstOrDefault();
            if (person != null)
            {
                person.Emails = await Db.SelectAsync(Db.From<PersonEmail>().Where(w => w.PersonId == person.Id));
                person.Phones = await Db.SelectAsync(Db.From<PersonPhone>().Where(w => w.PersonId == person.Id));
                person.Addresses = await Db.SelectAsync<Api.PersonAddress>(Db.From<PersonAddress>().Where(w => w.PersonId == person.Id));
                foreach (var personAddress in person.Addresses)
                {
                    personAddress.Address = (await Db.SingleByIdAsync<Domain.System.Location.Address>(personAddress.AddressId)).ConvertTo<ServiceModel.System.Location.Address.GetAddressResult>();
                    personAddress.Address.Place = await Db.SingleByIdAsync<Domain.System.Location.PlaceNode>(personAddress.Address.PlaceId);
                }

                person.References = await Db.SingleAsync<Reference>(w => w.PersonId == person.Id);
            }
            return person;
        }

        public QueryResponse<Api.QueryResult> Get(Api.QueryPersons request)
        {
            //var p = Request.GetRequestParams();
            //var name = "";
            //if (p.ContainsKey("Name"))
            //{
            //    name = p["Name"];
            //}
            //p.RemoveKey("Name");


            var query = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            if (request.Codes != null)
            {
                query.Where(x => Sql.In(x.Code, request.Codes));
            }
            //if (name != "")
            //{
            //    query.Where(x => x.Name.Contains(name));
            //}
            return _autoQuery.Execute(request, query);
        }


        public QueryResponse<Api.QueryResultForvalidation> Get(Api.QueryPersonForValidation request)
        {
            var query = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            query.LeftJoin<Person, PersonPhone>((p, pf) => pf.PersonId == p.Id && pf.TypeId == 2);
            if (request.Codes != null)
            {
                query.Where(x => Sql.In(x.Code, request.Codes));
            }
            if (request.PersonPhoneNumber != null)
            {
                query.Where<PersonPhone>(x => Sql.In(x.Number, request.PersonPhoneNumber));
            }
            var result = _autoQuery.Execute(request, query);

            return result;
        }


        public async Task<Api.PostPerson> Put(Api.PostPerson request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    await _personRepository.UpdatePerson(Db, request, true, Session.UserId);
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

        public async Task<object> Post(Api.PostPersonBatch request)
        {
            if (Log.IsDebugEnabled) Log.DebugFormat("SY:PER:PostBatch begin ({0}).", request.Count);

            //var key = "sy:pe:" + this.Session.Tenant.Id;
            //Cache.Remove(key);
            //var vendors = this.Cache.GetBusinessDocumentMessage<List<Domain.Procurement.Vendor>>(key);
            //if (vendors == null)
            //{
            //    vendors =
            //        Db.Select(Db.From<Domain.Procurement.Vendor>().Where(w => w.TenantId == this.Session.Tenant.Id));
            //    Cache.Set(key, vendors, TimeSpan.FromMinutes(5));
            //}

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    foreach (var item in request)
                    {
                        item.Person = await _personRepository.CreatePerson(Db, item.Person);
                        item.ImportLog.TargetId = item.Person.Id;
                        item.ImportLog.LastActivity = DateTime.UtcNow;
                    }

                    //Insert import logs
                    await Db.InsertAllAsync(request.Select(x => x.ImportLog));

                    trx.Commit();
                    if (Log.IsDebugEnabled) Log.DebugFormat("SY:PER:PostBatch completed.", request.Count);
                    return true;
                }
                catch
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public async Task<Api.PostPerson> Post(Api.PostPerson request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    await _personRepository.CreatePerson(Db, request, Session.UserId);
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

        public async Task<Api.ValidatePerson> Post(Api.ValidatePerson request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    //var person = Db.SingleById<Domain.System.Persons.Person>(request.Id).ConvertTo<CentralOperativa.ServiceModel.System.Persons.Person.PostPerson>();
                    var person = await Db.SingleByIdAsync<Person>(request.Id);
                    person.IsValid = true;
                    //person.IsOrganization = true; //Review
                    //Db.UpdatePerson(person); //Review
                    await Db.UpdateAsync(person);
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

        public async Task Delete(Api.DeletePerson request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    await _personRepository.DeletePerson(Db, request.Id);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public async Task<object> Get(Api.GetPersonNosis request)
        {
            var person = _personRepository.GetPerson(Db, request.Id);
            if (person == null)
            {
                throw new ArgumentException("The person was not found");
            }

            var nosis = (await Db.SelectAsync(Db.From<Nosis>().Where(w => w.Id == person.Id))).SingleOrDefault();
            if (nosis == null)
            {
                //HttpClient client = new HttpClient();
                //await client.GetAsync("")
            }

            return nosis;
        }
        public object Get(Api.GetPersonsFormResponsesRequest request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "UserName";
            }

            var q = Db.From<Domain.Cms.Forms.FormResponse>();
            q.LeftJoin<Domain.Cms.Forms.FormResponse, Domain.System.User>((fr, u) => fr.CreatedById == u.Id);
            q.Join<Domain.Cms.Forms.FormResponse, Domain.Cms.Forms.Form>((fr, f) => fr.FormId == f.Id && f.TenantId == Session.TenantId && f.TypeId == 1);
            q.UnsafeSelect(@"FormResponses.{0} AS Id,
                            Forms.Name as FormName,
                            Users.Name as UserName,
                            FormResponses.StartDate as StartDate,
                            FormResponses.EndDate as EndDate,
                            FormResponses.StatusId as StatusId,
                            Forms.Id as FormId,
                            Forms.AllowUpdates as AllowUpdates
                        ".Fmt("Id".SqlColumn()));
            q.Where(fr => fr.PersonId == request.PersonId);
            return _autoQuery.Execute(request, q);
        }
    }
}
