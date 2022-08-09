using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Procurement;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.BusinessPartners;
using CentralOperativa.ServiceInterface.System.Persons;
using Api = CentralOperativa.ServiceModel.Procurement.Vendor;

namespace CentralOperativa.ServiceInterface.Procurement
{
    [Authenticate]
    public class VendorService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(VendorService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;
        private readonly BusinessPartnerRepository _businessPartnerRepository;

        public VendorService(IAutoQueryDb autoQuery, PersonRepository personRepository,
            BusinessPartnerRepository businessPartnerRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _businessPartnerRepository = businessPartnerRepository;
        }

        public async Task<Api.PostVendor> Put(Api.PostVendor request)
        {
            request = await SaveVendor(request);
            return request;
        }

        public async Task<object> Post(Api.PostVendorBatch request)
        {
            if (Log.IsDebugEnabled) Log.DebugFormat("PR: VND:PostBatch begin ({0}).", request.Count);

            var key = "pr:ve:" + Session.TenantId;
            Cache.Remove(key);

            var vendors = Cache.Get<List<Vendor>>(key);
            if (vendors == null)
            {
                vendors = await Db.SelectAsync(Db
                    .From<Vendor>()
                    .Join<Vendor, BusinessPartner>((v, bp) => v.Id == bp.Id)
                    .Where<BusinessPartner>(w => w.TenantId == Session.TenantId));
                Cache.Set(key, vendors, TimeSpan.FromMinutes(5));
            }

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    foreach (var item in request)
                    {
                        item.Vendor = await SaveVendor(item.Vendor);
                        item.ImportLog.TargetId = item.Vendor.Id;
                        item.ImportLog.LastActivity = DateTime.UtcNow;
                    }

                    //Insert import logs
                    await Db.InsertAllAsync(request.Select(x => x.ImportLog));

                    trx.Commit();
                    if (Log.IsDebugEnabled) Log.DebugFormat("PR:VND:PostBatch completed.", request.Count);
                    return true;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public async Task<Api.PostVendor> Post(Api.PostVendor request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.BusinessPartner.Status = BusinessPartnerStatus.Active;
                    request = await SaveVendor(request);
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

        private async Task<Api.PostVendor> SaveVendor(Api.PostVendor request)
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
                    Db.Insert(new Vendor {Id = request.BusinessPartner.Id});
                }
            }

            request.Id = request.BusinessPartner.Id;

            return request;
        }

        public object Get(Api.QueryVendors request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where<BusinessPartner>(w => w.TypeId == 2 && w.TenantId == Session.TenantId && w.Status == BusinessPartnerStatus.Active);
            return _autoQuery.Execute(request, q);
        }

        /// <summary>
        /// Returns PersonId and PersonName
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Get(Api.Lookup request)
        {
            var query = Db.From<BusinessPartner>()
                .Join<BusinessPartner, Domain.System.Persons.Person>()
                .Where(w => w.TypeId == 2 && w.TenantId == Session.TenantId);

            if (request.Id.HasValue)
            {
                if (request.ReturnPersonId.HasValue && request.ReturnPersonId.Value)
                {
                    query = query.Where<Domain.System.Persons.Person>(w => w.Id == request.Id.Value);
                }
                else
                {
                    query = query.Where<Vendor>(w => w.Id == request.Id.Value);
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
                Data = Db.Select<Api.QueryVendorsResult>(query).Select(x => new LookupItem { Id = (request.ReturnPersonId.HasValue && request.ReturnPersonId.Value) ? x.PersonId : x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }

        public async Task<Api.GetVendorResult> Get(Api.GetVendor request)
        {
            var model = new Api.GetVendorResult
            {
                BusinessPartner = await _businessPartnerRepository.GetBusinessPartner(Db, request.Id, true),
                Id = request.Id
            };
            return model;
        }

        public async Task Delete(Api.DeleteVendor request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var businessPartner = await _businessPartnerRepository.GetBusinessPartner(Db, request.Id);
                    if (businessPartner != null && businessPartner.TypeId == 2)
                    {
                        //Borrado lódico
                        businessPartner.Status = BusinessPartnerStatus.Deleted;
                        await Db.UpdateAsync((BusinessPartner)businessPartner);
                        //Borrado fisico
                        //Db.DeleteById<Domain.Procurement.Vendor>(request.Id);
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
    }
}