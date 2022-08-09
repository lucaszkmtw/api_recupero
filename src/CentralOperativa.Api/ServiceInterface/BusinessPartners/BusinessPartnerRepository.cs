using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Financials;
using CentralOperativa.Domain.Inv;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.BusinessPartners;

namespace CentralOperativa.ServiceInterface.BusinessPartners
{
    public class BusinessPartnerRepository
    {
        private readonly PersonRepository _personRepository;
        private readonly BusinessPartnerAccountRepository _businessPartnerAccountRepository;

        public BusinessPartnerRepository(
            PersonRepository personRepository,
            BusinessPartnerAccountRepository businessPartnerAccountRepository)
        {
            _personRepository = personRepository;
            _businessPartnerAccountRepository = businessPartnerAccountRepository;
        }

        public async Task<Api.GetBusinessPartnerResult> GetBusinessPartner(IDbConnection db, int id, bool? includeItems = false)
        {
            var model = (await db.SelectAsync<Api.GetBusinessPartnerResult>(db.From<BusinessPartner>().Where(w => w.Id == id))).SingleOrDefault();
            if (model == null)
            {
                throw HttpError.NotFound($"The business partner with id {id} was not found.");
            }

            if (includeItems.HasValue && includeItems.Value)
            {
                model.Person = await _personRepository.GetPerson(db, model.PersonId);
                model.Accounts.Currencies = (await db.SelectAsync(db.From<Currency>().Join<Currency, BusinessPartnerAccount>().Where<BusinessPartnerAccount>(w => w.BusinessPartnerId == model.Id))).Distinct().ToList();

                var query = $"SELECT bpa.*, (SELECT SUM(bpae.Amount) Amount FROM BusinessPartnerAccountEntries bpae WHERE bpae.AccountId = bpa.Id) Balance FROM BusinessPartnerAccounts bpa WHERE bpa.BusinessPartnerId = {model.Id}";
                model.Accounts.Items = await db.SelectAsync<Api.GetBusinessPartnerResult.BusinessPartnerAccounts.Account>(query);
            }
            return model;
        }

        public async Task<Tuple<Api.PostBusinessPartner, bool>> InsertBusinessPartner(IDbConnection db, Session session, Api.PostBusinessPartner businessPartner)
        {
            bool createNew;

            //Check deleted
            var existing = (await db.SelectAsync(db.From<BusinessPartner>().Where(w => w.TenantId == businessPartner.TenantId && w.PersonId == businessPartner.PersonId && w.TypeId == businessPartner.TypeId))).SingleOrDefault();
            if (existing != null)
            {
                createNew = false;

                existing.Status = BusinessPartnerStatus.Active;
                await db.UpdateAsync(existing);
                businessPartner = (await GetBusinessPartner(db, existing.Id)).ConvertTo<Api.PostBusinessPartner>();
            }
            else
            {
                createNew = true;

                //Code
                var query = $"SELECT MAX(CAST(Code AS NUMERIC)) Number FROM BusinessPartners WHERE TypeId = {businessPartner.TypeId} AND TenantId = {businessPartner.TenantId} AND ISNUMERIC(Code) = 1";
                var currentNumber = await db.ScalarAsync<int>(query);
                var code = (currentNumber + 1).ToString();

                businessPartner.Code = code;
                businessPartner.CreateDate = DateTime.UtcNow;
                businessPartner.CreatedById = session.UserId;
                businessPartner.Guid = Guid.NewGuid();
                businessPartner.Code = code;
                businessPartner.Id = (int)await db.InsertAsync((BusinessPartner)businessPartner, true);

                //Default Account
                var businessPartnerAccount = new BusinessPartnerAccount
                {
                    BusinessPartnerId = businessPartner.Id,
                    CurrencyId = 1,
                    Name = "Cuenta corriente " + (businessPartner.TypeId == 1 ? "cliente" : "proveedor")
                };
                await _businessPartnerAccountRepository.InsertBusinessPartnerAccount(db, session, businessPartnerAccount);

                //Default Inventory Site
                var inventorySite = (await db.SelectAsync(db.From<InventorySite>().Where(w => w.PersonId == businessPartner.PersonId))).FirstOrDefault();
                if (inventorySite == null)
                {
                    inventorySite = new InventorySite
                    {
                        PersonId = businessPartner.PersonId,
                        Name = "Predeterminado"
                    };
                    await db.InsertAsync(inventorySite);
                }
            }

            return new Tuple<Api.PostBusinessPartner, bool>(businessPartner, createNew);
        }

        public async Task<Api.PostBusinessPartner> UpdateBusinessPartner(IDbConnection db, Api.PostBusinessPartner businessPartner)
        {
            await db.UpdateAsync((BusinessPartner)businessPartner);
            return businessPartner;
        }
    }
}




//using System;
//using System.Data;
//using System.Linq;
//using System.Threading.Tasks;
//using CentralOperativa.Domain.BusinessPartners;
//using CentralOperativa.Infraestructure;
//using CentralOperativa.ServiceInterface.System.Persons;
//using ServiceStack;
//using ServiceStack.OrmLite;
//using Api = CentralOperativa.ServiceModel.BusinessPartners;

//namespace CentralOperativa.ServiceInterface.BusinessPartners
//{
//    public class BusinessPartnerRepository
//    {
//        private readonly PersonRepository _personRepository;
//        private readonly BusinessPartnerAccountRepository _businessPartnerAccountRepository;

//        public BusinessPartnerRepository(
//            PersonRepository personRepository,
//            BusinessPartnerAccountRepository businessPartnerAccountRepository)
//        {
//            _personRepository = personRepository;
//            _businessPartnerAccountRepository = businessPartnerAccountRepository;
//        }

//        public async Task<Api.GetBusinessPartnerResult> GetBusinessPartner(IDbConnection db, int id, bool? includeItems = false)
//        {
//            var model = (await db.SelectAsync<Api.GetBusinessPartnerResult>(db.From<BusinessPartner>().Where(w => w.Id == id))).SingleOrDefault();
//            if (model == null)
//            {
//                throw HttpError.NotFound($"The business partner with id {id} was not found.");
//            }

//            if (includeItems.HasValue && includeItems.Value)
//            {
//                model.Person = await _personRepository.GetPerson(db, model.PersonId);
//                model.Accounts.Currencies = (await db.SelectAsync(db.From<Domain.Financials.Currency>().Join<Domain.Financials.Currency, BusinessPartnerAccount>().Where<BusinessPartnerAccount>(w => w.BusinessPartnerId == model.Id))).Distinct().ToList();

//                var query = $"SELECT bpa.*, (SELECT SUM(bpae.Amount) Amount FROM BusinessPartnerAccountEntries bpae WHERE bpae.AccountId = bpa.Id) Balance FROM BusinessPartnerAccounts bpa WHERE bpa.BusinessPartnerId = {model.Id}";
//                model.Accounts.Items = await db.SelectAsync<Api.GetBusinessPartnerResult.BusinessPartnerAccounts.Account>(query);
//            }
//            return model;
//        }

//        public async Task<Tuple<Api.PostBusinessPartner, bool>> InsertBusinessPartner(IDbConnection db, Session session, Api.PostBusinessPartner businessPartner)
//        {
//            bool createNew;

//            //Check deleted
//            var existing = (await db.SelectAsync(db.From<BusinessPartner>().Where(w => w.TenantId == businessPartner.TenantId && w.PersonId == businessPartner.PersonId && w.TypeId == businessPartner.TypeId))).SingleOrDefault();
//            if (existing != null)
//            {
//                createNew = false;

//                existing.Status = BusinessPartnerStatus.Active;
//                await db.UpdateAsync(existing);
//                businessPartner = (await GetBusinessPartner(db, existing.Id)).ConvertTo<Api.PostBusinessPartner>();
//            }
//            else
//            {
//                createNew = true;

//                //Code
//                var query = $"SELECT MAX(CAST(Code AS NUMERIC)) Number FROM BusinessPartners WHERE TypeId = {businessPartner.TypeId} AND TenantId = {businessPartner.TenantId} AND ISNUMERIC(Code) = 1";
//                var currentNumber = await  db.ScalarAsync<int>(query);
//                var code = (currentNumber + 1).ToString();

//                businessPartner.Code = code;
//                businessPartner.CreateDate = DateTime.UtcNow;
//                businessPartner.CreatedById = session.UserId;
//                businessPartner.Guid = Guid.NewGuid();
//                businessPartner.Code = code;
//                businessPartner.Id = (int) await db.InsertAsync((BusinessPartner) businessPartner, true);

//                //Default Account
//                var businessPartnerAccount = new BusinessPartnerAccount
//                {
//                    BusinessPartnerId = businessPartner.Id,
//                    CurrencyId = 1,
//                    Name = "Cuenta corriente " + (businessPartner.TypeId == 1 ? "cliente" : "proveedor")
//                };
//                await _businessPartnerAccountRepository.InsertBusinessPartnerAccount(db, session, businessPartnerAccount);

//                //Default Inventory Site
//                var inventorySite = (await db.SelectAsync(db.From<Domain.Inv.InventorySite>().Where(w => w.PersonId == businessPartner.PersonId))).FirstOrDefault();
//                if (inventorySite == null)
//                {
//                    inventorySite = new Domain.Inv.InventorySite
//                    {
//                        PersonId = businessPartner.PersonId,
//                        Name = "Predeterminado"
//                    };
//                    await db.InsertAsync(inventorySite);
//                }
//            }

//            return new Tuple<Api.PostBusinessPartner, bool>(businessPartner, createNew);
//        }

//        public async Task<Api.PostBusinessPartner> UpdateBusinessPartner(IDbConnection db, Api.PostBusinessPartner businessPartner)
//        {
//            await db.UpdateAsync((BusinessPartner)businessPartner);
//            return businessPartner;
//        }
//    }
//}