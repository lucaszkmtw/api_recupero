using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.CRM.Contacts;
using CentralOperativa.Domain.Financials;
using CentralOperativa.ServiceInterface.System.Persons;
using Api = CentralOperativa.ServiceModel.CRM.Contacts;
using CentralOperativa.ServiceModel.System.Location;

namespace CentralOperativa.ServiceInterface.CRM.Contacts
{
    [Authenticate]
    public class ContactService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(ContactService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;
        private readonly LeadService _leadService;

        public ContactService(
            IAutoQueryDb autoQuery,
            PersonRepository personRepository,
            LeadService leadService)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _leadService = leadService;
        }

        public object Get(Api.QueryContactByGroup request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Join<Contact, ContactGroupMember>((p, pp) => p.Id == pp.ContactId && pp.ContactGroupId == request.ContactGroupId);
            q.Join<ContactGroupMember, ContactGroup>((cgm, cg) => cg.Id == cgm.ContactGroupId);
            q.CustomJoin("LEFT JOIN (select PersonId, max(Address) Address from PersonEmails group by PersonId) PersonEmails ON (PersonEmails.PersonId = Contacts.PersonId)");
            q.CustomJoin("LEFT JOIN (select PersonId, max(Number) Number from PersonPhones group by PersonId) PersonPhones ON (PersonPhones.PersonId = Contacts.PersonId)");
            q.LeftJoin<Contact, Employee>((c, e) => c.PersonId == e.PersonId);
            q.CustomJoin("LEFT JOIN Persons Employer ON (Employees.EmployerId = Employer.Id)")
                .Where<ContactGroup>(w => w.TenantId == Session.TenantId)
            .UnsafeSelect(@"Employer.{0} AS EmployerName,
                            Contacts.Id as Id,
                            Contacts.Id as ContactId,
                            Contacts.PersonId as PersonId,
                            Persons.Code as Code,
                            Persons.Name as Name,
                            PersonEmails.Address as Address,
                            PersonPhones.Number as Number
                        ".Fmt("Name".SqlColumn()));

            if (!string.IsNullOrEmpty(request.Q))
            {
                q.UnsafeWhere("Persons.Name LIKE {0}", Utils.SqlLike(request.Q));
            }

            return _autoQuery.Execute(request, q);
        }

        public object Get(Api.QueryContactByGroupAll request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Join<Contact, UserContact>((p, pp) => p.Id == pp.ContactId && pp.UserId == Session.UserId);
            q.CustomJoin("LEFT JOIN (select PersonId, max(Address) Address from PersonEmails group by PersonId) PersonEmails ON (PersonEmails.PersonId = Contacts.PersonId)");
            q.CustomJoin("LEFT JOIN (select PersonId, max(Number) Number from PersonPhones group by PersonId) PersonPhones ON (PersonPhones.PersonId = Contacts.PersonId)");
            q.LeftJoin<Contact, Employee>((c, e) => c.PersonId == e.PersonId);
            q.CustomJoin("LEFT JOIN Persons Employer ON (Employees.EmployerId = Employer.Id)")
            .UnsafeSelect(@"Employer.{0} AS EmployerName,
                            Contacts.Id as Id,
                            Contacts.Id as ContactId,
                            Contacts.PersonId as PersonId,
                            Persons.Code as Code,
                            Persons.Name as Name,
                            PersonEmails.Address as Address,
                            PersonPhones.Number as Number
                        ".Fmt("Name".SqlColumn()));

            if (!string.IsNullOrEmpty(request.Q))
            {
                q.UnsafeWhere("Persons.Name LIKE {0}", Utils.SqlLike(request.Q));
            }
            return _autoQuery.Execute(request, q);

        }

        //
        public object Get(Api.QueryContactByGroupIds request)
        {
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Join<Contact, ContactGroupMember>((p, pp) => p.Id == pp.ContactId && pp.ContactGroupId == request.ContactGroupId);
            q.Join<ContactGroupMember, ContactGroup>((cgm, cg) => cg.Id == cgm.ContactGroupId);
            q.CustomJoin("LEFT JOIN (select PersonId, max(Address) Address from PersonEmails group by PersonId) PersonEmails ON (PersonEmails.PersonId = Contacts.PersonId)");
            q.CustomJoin("LEFT JOIN (select PersonId, max(Number) Number from PersonPhones group by PersonId) PersonPhones ON (PersonPhones.PersonId = Contacts.PersonId)");
            q.LeftJoin<Contact, Employee>((c, e) => c.PersonId == e.PersonId);
            q.CustomJoin("LEFT JOIN Persons Employer ON (Employees.EmployerId = Employer.Id)")
                .Where<ContactGroup>(w => w.TenantId == Session.TenantId)
            .UnsafeSelect(@"Contacts.Id as Id".Fmt("Name".SqlColumn()));
            if (!string.IsNullOrEmpty(request.Q))
            {
                q.UnsafeWhere("Persons.Name LIKE {0}", Utils.SqlLike(request.Q));
            }

            return _autoQuery.Execute(request, q);
        }

        public object Get(Api.QueryContactByGroupAllIds request)
        {
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Join<Contact, UserContact>((p, pp) => p.Id == pp.ContactId && pp.UserId == Session.UserId);
            q.CustomJoin("LEFT JOIN (select PersonId, max(Address) Address from PersonEmails group by PersonId) PersonEmails ON (PersonEmails.PersonId = Contacts.PersonId)");
            q.CustomJoin("LEFT JOIN (select PersonId, max(Number) Number from PersonPhones group by PersonId) PersonPhones ON (PersonPhones.PersonId = Contacts.PersonId)");
            q.LeftJoin<Contact, Employee>((c, e) => c.PersonId == e.PersonId);
            q.CustomJoin("LEFT JOIN Persons Employer ON (Employees.EmployerId = Employer.Id)")
            .UnsafeSelect(@"Contacts.Id as Id".Fmt("Name".SqlColumn()));

            if (!string.IsNullOrEmpty(request.Q))
            {
                q.UnsafeWhere("Persons.Name LIKE {0}", Utils.SqlLike(request.Q));
            }
            return _autoQuery.Execute(request, q);

        }

        //
        public object Get(Api.QueryEmployeeBankBranch request)
        {
            return Db.Select<Api.QueryEmployeeBankBranch>(
                    Db
                    .From<BankBranch>()
                    .Join<BankBranch, Bank>()
                    .Where(c => c.Id == request.Id)
                )
                .SingleOrDefault();
        }
        //

        public async Task<object> Get(Api.QueryContact request)
        {
            var contact = (await Db.SelectAsync(Db.From<Contact>().Where(c => c.Id == request.Id))).SingleOrDefault();
            var queryContact = new Api.QueryContact
            {
                Person = await _personRepository.GetPerson(Db, contact.PersonId),
                Employee = (await Db.SelectAsync(Db.From<Employee>().Where(e => e.PersonId == contact.PersonId))).SingleOrDefault()
            };
            if (queryContact.Employee != null)
            {
                queryContact.Employer = await _personRepository.GetPerson(Db, queryContact.Employee.EmployerId);
                queryContact.BankAccount = (await Db.SelectAsync(Db.From<BankAccount>().Where(ba => ba.Id == queryContact.Employee.BankAccountId))).SingleOrDefault();
                if (queryContact.BankAccount != null)
                {
                    var bankBranch = (await Db.SelectAsync(Db.From<BankBranch>().Where(bb => bb.Id == queryContact.BankAccount.BankBranchId))).SingleOrDefault();
                    if (bankBranch != null)
                    {
                        queryContact.Bank = (await Db.SelectAsync(Db.From<Bank>().Where(b => b.Id == bankBranch.BankId))).SingleOrDefault();
                    }
                }

                queryContact.BankAccounts = await Db.SelectAsync<Api.QueryContact.ContactBankAccount>(Db
                    .From<EmployeeBankAccount>()
                    .Join<EmployeeBankAccount, BankAccount>()
                    .Join<BankAccount, BankBranch>()
                    .Join<BankBranch, Bank>()
                    .Where<EmployeeBankAccount>(ba => ba.EmployeeId == queryContact.Employee.Id)
                    );

            }
            queryContact.ProcurementsCount = (int) await Db.CountAsync(Db.From<Domain.BusinessDocuments.BusinessDocument>().Where(b => b.IssuerId == contact.PersonId));
            queryContact.SalesCount = (int) await Db.CountAsync(Db.From<Domain.BusinessDocuments.BusinessDocument>().Where(b => b.ReceiverId == contact.PersonId));
            queryContact.pollsCount = (int) await Db.CountAsync(Db.From<Domain.Cms.Forms.FormResponse>()
                .Join<Domain.Cms.Forms.FormResponse, Domain.Cms.Forms.Form>((fr, f) => fr.FormId == f.Id && f.TenantId == Session.TenantId)
                .Where(fr => fr.PersonId == queryContact.Person.Id));

            queryContact.AccountsCount = (int) await Db.CountAsync(Db.From<Domain.BusinessPartners.BusinessPartnerAccount>()
                .Join<Domain.BusinessPartners.BusinessPartnerAccount, Domain.BusinessPartners.BusinessPartner>((bpa, bp) => bpa.BusinessPartnerId == bp.Id && bp.TenantId == Session.TenantId)
                .Where<Domain.BusinessPartners.BusinessPartner>(bp => bp.PersonId == queryContact.Person.Id));

            return queryContact;
        }

        public async Task<object> Get(Api.QueryContactPatient request)
        {
            var contact = await Db.SingleByIdAsync<Contact>(request.ContactId);
            var patient = (await Db.SelectAsync(Db.From<Domain.Health.Patient>().Where(p => p.PersonId == contact.PersonId))).SingleOrDefault();
            return patient;
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Post(Api.PostContact request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    CreateContact(request);
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

        public object Put(Api.PostContact request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    UpdateContact(request);
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

        public object Delete(Api.DeleteContact request)
        {
            object model;
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var userContact = Db.From<UserContact>()
                        .Select(c => c.Id)
                        .Where(c => c.UserId == Session.UserId && c.ContactId == request.Id);
                    model = Db.Delete(userContact);
                    var contactGroupMemberIds = Db.Select(
                                        Db
                                        .From<ContactGroupMember>()
                                        .Join<ContactGroupMember, ContactGroup>((cgm, cg) => cgm.ContactGroupId == cg.Id)
                                        .Join<ContactGroup, ContactGroupPermission>((cg, cgp) => cg.Id == cgp.ContactGroupId && cgp.UserId == Session.UserId)
                                        .Where(c => c.ContactId == request.Id)
                                      )
                                    .Select(x => x.Id)
                                    .ToList();
                    if (contactGroupMemberIds.Any())
                    {
                        Db.Delete<ContactGroupMember>(x => Sql.In(x.Id, contactGroupMemberIds));
                    }

                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return model;
        }

        public async Task<object> Post(Api.PostContactsStartCampaign request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    foreach (var id in request.ContactIds)
                    {
                        var contact = (await Db.SelectAsync(Db.From<Contact>().Where(c => c.Id == id))).SingleOrDefault();
                        var lead = new Domain.CRM.Lead
                        {
                            PersonId = contact.PersonId,
                            Status = 0,
                            CampaignId = request.CampaignId
                        };
                        lead.Id = (int) await Db.InsertAsync(lead, true);
                        await _leadService.SaveRelationships(Db, lead, Session.UserId);

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

        public object Post(Api.PostContactFile request)
        {
            var path = Path.GetTempPath();
            path += "\\CentralOperativa\\";
            Directory.CreateDirectory(path);

            var files = Request.Files;
            var httpFile = files[0];

            var strDate = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            var fileName = strDate + httpFile.FileName;

            using (var fileStream = httpFile.InputStream)
            {
                var buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                FileStream wFile = new FileStream(path + fileName, FileMode.Create);
                wFile.Write(buffer, 0, buffer.Length);
                wFile.Close();
                request.FileName = fileName;
            }

            request.Columns = new List<string>();
            var lines = File.ReadAllLines(path + fileName);
            var line = lines[0];
            var columns = line.Split(';');
            foreach (var column in columns)
            {
                request.Columns.Add(column);
            }

            return request;
        }

        public async Task<object> Post(Api.ImportContacts request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var path = Path.GetTempPath();
                    path += "\\CentralOperativa\\";
                    var fileName = request.FileName;
                    var lines = File.ReadAllLines(path + fileName);
                    for (var i = 1; i < lines.Count(); i++)
                    {
                        var post = new Api.PostContact();
                        var line = lines[i];
                        var columns = line.Split(';');
                        var person = new ServiceModel.System.Persons.PostPerson();
                        Domain.System.Persons.Person personaux = null;
                        var code = "";
                        var hasAddress = false;
                        var address = new Address.GetAddressResult();
                        var stateName = "";
                        var cityName = "";
                        var hasPhone = false;
                        var phone = new ServiceModel.System.Persons.PostPersonPhone();
                        var hasEmail = false;
                        var email = new ServiceModel.System.Persons.PostPersonEmail();
                        var hasEmployer = false;
                        var employer = new ServiceModel.System.Persons.PostPerson();
                        var hasEmployee = false;
                        var employee = new Employee();
                        DateTime fromDate = DateTime.Now;
                        var bankAccountNumber = "";
                        var bankName = "";
                        var bankBranchName = "";
                        var currencyName = "";
                        var groupNames = new List<string>();
                        for (var j = 0; j < columns.Length; j++)
                        {
                            if (!columns[j].IsNullOrEmpty())
                            {
                                var colName = request.Columns[j];
                                switch (colName)
                                {
                                    case "cuit":
                                        person.Code = columns[j];
                                        code = columns[j];
                                        break;
                                    case "firstName":
                                        person.FirstName = columns[j];
                                        break;
                                    case "lastName":
                                        person.LastName = columns[j];
                                        break;
                                    case "personStreet":
                                        hasAddress = true;
                                        address.Street = columns[j];
                                        break;
                                    case "personStreetNumber":
                                        hasAddress = true;
                                        address.StreetNumber = columns[j];
                                        break;
                                    case "personFloor":
                                        hasAddress = true;
                                        address.Floor = columns[j];
                                        break;
                                    case "personAppartment":
                                        hasAddress = true;
                                        address.Appartment = columns[j];
                                        break;
                                    case "personLocation":
                                        hasAddress = true;
                                        cityName = columns[j];
                                        break;
                                    case "personProvince":
                                        hasAddress = true;
                                        stateName = columns[j];
                                        break;
                                    case "personPhone":
                                        hasPhone = true;
                                        phone.Number = columns[j];
                                        phone.TypeId = 2;
                                        break;
                                    case "personEmail":
                                        hasEmail = true;
                                        email.Address = columns[j];
                                        email.TypeId = 2;
                                        break;
                                    case "personWebUrl":
                                        person.WebUrl = columns[j];
                                        break;
                                    case "personEmployerFirstName":
                                        hasEmployer = true;
                                        hasEmployee = true;
                                        employer.FirstName = columns[j];
                                        break;
                                    case "personEmployerLastName":
                                        hasEmployer = true;
                                        hasEmployee = true;
                                        employer.LastName = columns[j];
                                        break;
                                    case "personEmployerCode":
                                        hasEmployer = true;
                                        hasEmployee = true;
                                        employer.Code = columns[j];
                                        break;
                                    case "personEmployerPhone":
                                        hasEmployer = true;
                                        employer.Phones.Add(new ServiceModel.System.Persons.PostPersonPhone
                                        {
                                            Number = columns[j],
                                            TypeId = 2
                                        });
                                        break;
                                    case "personSalary":
                                        if (decimal.TryParse(columns[j], out var salary))
                                        {
                                            hasEmployee = true;
                                            employee.Salary = salary;
                                        }
                                        break;
                                    case "personEmployeeStartDate":
                                        if (DateTime.TryParse(columns[j], out fromDate))
                                        {
                                            hasEmployee = true;
                                        }
                                        break;
                                    case "bank":
                                        bankName = columns[j];
                                        break;
                                    case "bankBranch":
                                        bankBranchName = columns[j];
                                        break;
                                    case "cbu":
                                        bankAccountNumber = columns[j];
                                        break;
                                    case "currency":
                                        currencyName = columns[j];
                                        break;
                                    case "groups":
                                        groupNames = columns[j].Split(',').ToList();
                                        break;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(code))
                        {
                            personaux = await _personRepository.GetPerson(Db, code);
                        }

                        person.IsOrganization = false;

                        if (personaux == null)
                        {
                            person.IsValid = true;
                            if (hasAddress)
                            {

                                //Busco provincia y si no existe la inserto
                                if (!string.IsNullOrEmpty(stateName))
                                {
                                    var state = (await Db.SelectAsync(Db.From<Domain.System.Location.Place>()
                                    .Where(b => b.Name == stateName && b.TypeId == 2)))
                                    .SingleOrDefault();
                                    if (state == null)
                                    {
                                        state = new Domain.System.Location.Place
                                        {
                                            Name = stateName,
                                            TypeId = 2
                                        };
                                        state.Id = (int) await Db.InsertAsync(state, true);
                                    }

                                    address.PlaceId = state.Id;
                                }

                                if (!string.IsNullOrEmpty(cityName))
                                {

                                    //Busco localidad y si no existe la inserto
                                    var city = (await Db.SelectAsync(Db.From<Domain.System.Location.Place>()
                                        .Where(b => b.Name == cityName && b.TypeId == 3)))
                                        .SingleOrDefault();
                                    if (city == null)
                                    {
                                        city = new Domain.System.Location.Place
                                        {
                                            Name = cityName,
                                            TypeId = 3
                                        };

                                        if (address.PlaceId != 0)
                                        {
                                            city.ParentId = address.PlaceId;
                                        }

                                        city.Id = (int)Db.Insert(city, true);

                                    }
                                    address.PlaceId = city.Id;
                                }

                                person.Addresses.Add(new ServiceModel.System.Persons.PostPersonAddress
                                {
                                    Address = address,
                                    TypeId = 2
                                });
                            }

                            if (hasPhone)
                            {
                                person.Phones.Add(phone);
                            }

                            if (hasEmail)
                            {
                                person.Emails.Add(email);
                            }
                            personaux = await _personRepository.CreatePerson(Db, person);
                        }
                        else
                        {
                            person.Id = personaux.Id;
                            await _personRepository.UpdatePerson(Db, person);
                        }

                        if (hasEmployer)
                        {
                            employer.IsOrganization = false;
                            employer = await _personRepository.CreatePerson(Db, employer);
                            if (hasEmployee)
                            {
                                var bankAccount = new BankAccount();
                                if (!string.IsNullOrEmpty(bankName) && !string.IsNullOrEmpty(bankAccountNumber))
                                {
                                    var bank = Db.Select(Db.From<Bank>().Where(b => b.Name == bankName)).SingleOrDefault();
                                    if (bank == null)
                                    {
                                        bank = new Bank
                                        {
                                            Name = bankName
                                        };
                                        bank.Id = (int)Db.Insert(bank, true);

                                    }

                                    var bankBranch = !string.IsNullOrEmpty(bankBranchName) ?
                                        Db.Select(Db.From<BankBranch>().Where(bb => bb.BankId == bank.Id && bb.Name == bankBranchName)).FirstOrDefault() : 
                                        Db.Select(Db.From<BankBranch>().Where(bb => bb.BankId == bank.Id)).FirstOrDefault();

                                    if (bankBranch == null)
                                    {
                                        bankBranch = new BankBranch
                                        {
                                            Name = bankBranchName,
                                            BankId = bank.Id
                                        };
                                        bankBranch.Id = (int)Db.Insert(bankBranch, true);
                                    }

                                    bankAccount = Db.Select(Db.From<BankAccount>().Where(ba => ba.Number == bankAccountNumber)).SingleOrDefault();
                                    if (bankAccount == null)
                                    {

                                        if (!string.IsNullOrEmpty(currencyName))
                                        {
                                            var currency = Db.Select(Db.From<Currency>().Where(ba => ba.Name.Contains(currencyName) || ba.Symbol.Contains(currencyName))).FirstOrDefault();
                                            if (currency == null)
                                            {
                                                currency = new Currency
                                                {
                                                    Name = currencyName,
                                                    Symbol = string.Empty
                                                };
                                                currency.Id = (int)Db.Insert(currency, true);
                                            }

                                            bankAccount = new BankAccount
                                            {
                                                BankBranchId = bankBranch.Id,
                                                Number = bankAccountNumber,
                                                Code = string.Empty,
                                                Description = string.Empty,
                                                PersonId = person.Id,
                                                CurrencyId = currency.Id
                                            };
                                        }

                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(currencyName))
                                        {
                                            var currency = Db.Select(Db.From<Currency>().Where(ba => ba.Name.Contains(currencyName) || ba.Symbol.Contains(currencyName))).SingleOrDefault();
                                            if (currency != null)
                                            {
                                                bankAccount.CurrencyId = currency.Id;
                                            }

                                        }
                                        bankAccount.BankBranchId = bankBranch.Id;
                                        bankAccount.Number = bankAccountNumber;
                                        Db.Update(bankAccount);
                                    }
                                }

                                var existing = Db.Select<Employee>(x => x.PersonId == person.Id).SingleOrDefault();
                                employee.EmployerId = employer.Id;
                                employee.FromDate = fromDate;
                                if (bankAccount != null)
                                {
                                    employee.BankAccountId = bankAccount.Id;
                                }

                                if (existing == null)
                                {
                                    employee.PersonId = person.Id;
                                    employee.CreatedById = Session.UserId;
                                    employee.CreatedDate = DateTime.Now;
                                    employee.EmployerId = employer.Id;
                                    employee.ToDate = DateTime.Now;
                                    employee.Id = (int)Db.Insert(employee, true);
                                }
                                else
                                {
                                    employee.PersonId = existing.PersonId;
                                    employee.CreatedById = existing.CreatedById;
                                    employee.CreatedDate = existing.CreatedDate;
                                    employee.EmployerId = existing.EmployerId;
                                    employee.ToDate = existing.ToDate;
                                    Db.Update(employee);
                                }

                                if (bankAccount != null)
                                {
                                    post.BankAccounts = new List<Api.QueryContactsResult.ContactBankAccount>
                                    {
                                        new Api.QueryContactsResult.ContactBankAccount
                                        {
                                            BankAccount = bankAccount
                                        }
                                    };
                                }

                            }
                        }


                        post.PersonId = personaux.Id;
                        post.GroupNames = groupNames;
                        CreateContact(post);

                    }

                    trx.Commit();
                    request.InsertedItemsCount = lines.Length - 1;
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }
        public object CreateContact(Api.PostContact request)
        {
            if (request.ContactId == 0)
            {
                var contact = Db
                                .Select(Db.From<Contact>()
                                .Where(w => w.PersonId == request.PersonId))
                                .SingleOrDefault();
                if (contact == null)
                {
                    contact = new Contact
                    {
                        PersonId = request.PersonId
                    };
                    request.ContactId = (int)Db.Insert(contact, true);
                }
                else
                {
                    request.ContactId = contact.Id;
                }

            }

            var usercontact = Db
                            .Select(Db.From<UserContact>()
                            .Where(w => w.ContactId == request.ContactId && w.UserId == Session.UserId))
                            .SingleOrDefault();
            if (usercontact == null)
            {
                usercontact = new UserContact
                {
                    ContactId = request.ContactId,
                    UserId = Session.UserId
                };
                Db.Insert(usercontact);
            }

            if (request.ContactGroupId != 0)
            {
                var contactGroupMember = new ContactGroupMember
                {
                    ContactGroupId = request.ContactGroupId,
                    ContactId = request.ContactId
                };
                request.Id = (int)Db.Insert(contactGroupMember, true);
            }

            if (request.GroupNames != null)
            {

                foreach (var groupName in request.GroupNames)
                {

                    var group = Db.Select(
                                        Db
                                        .From<ContactGroup>()
                                        .Join<ContactGroup, ContactGroupPermission>((cg, cgp) => cg.Id == cgp.ContactGroupId && cgp.UserId == Session.UserId)
                                        .Where(c => c.Name.ToUpper() == groupName.ToUpper() && c.TenantId == Session.TenantId)
                                      ).FirstOrDefault();
                    if (group == null)
                    {
                        group = new ContactGroup
                        {
                            Name = groupName,
                            TenantId = Session.TenantId
                        };
                        group.Id = (int)Db.Insert(group, true);

                        var permission = new ContactGroupPermission
                        {
                            ContactGroupId = group.Id,
                            UserId = Session.UserId
                        };
                        Db.Insert(permission);

                    }

                    var groupmember = Db.Select(
                                    Db.From<ContactGroupMember>()
                                    .Where(c => c.ContactGroupId == group.Id && c.ContactId == request.ContactId)
                                    )
                                    .FirstOrDefault();
                    if (groupmember == null)
                    {
                        groupmember = new ContactGroupMember
                        {
                            ContactGroupId = group.Id,
                            ContactId = request.ContactId
                        };
                        Db.Insert(groupmember);
                    }


                }

            }

            //Employer
            if (request.EmployerId.HasValue && request.EmployerId.Value > 0)
            {
                var employee = Db.Select(
                                        Db.From<Employee>()
                                            .Where(w => w.PersonId == request.PersonId))
                                    .SingleOrDefault();
                if (employee == null)
                {
                    employee = new Employee
                    {
                        PersonId = request.PersonId,
                        EmployerId = request.EmployerId.Value,
                        CreatedDate = DateTime.Now,
                        FromDate = DateTime.Now,
                        ToDate = DateTime.Now,
                        CreatedById = Session.UserId
                    };
                    if (request.BankAccountId.HasValue)
                    {
                        employee.BankAccountId = request.BankAccountId.Value;
                    }

                    if (request.Salary.HasValue)
                    {
                        employee.Salary = request.Salary.Value;
                    }
                    Db.Insert(employee);
                }
                else
                {

                    employee.EmployerId = request.EmployerId.Value;

                    if (request.BankAccountId.HasValue)
                    {
                        employee.BankAccountId = request.BankAccountId.Value;
                    }
                    if (request.Salary.HasValue)
                    {
                        employee.Salary = request.Salary.Value;
                    }
                    Db.Update(employee);
                }
            }

            if (!request.WebUrl.IsNullOrEmpty())
            {
                var person = Db.Select(Db.From<Domain.System.Persons.Person>().Where(w => w.Id == request.PersonId)).SingleOrDefault();
                if (person != null)
                {
                    person.WebUrl = request.WebUrl;
                    Db.Update(person);
                }
            }

            Save(request, Session.TenantId);
            return null;
        }

        public object UpdateContact(Api.PostContact request)
        {
            if (request.ContactId == 0)
            {
                var contact = Db
                                .Select(Db.From<Contact>()
                                .Where(w => w.Id == request.Id))
                                .SingleOrDefault();
                if (contact == null)
                {

                }
                else
                {
                    request.ContactId = contact.Id;
                    contact.PersonId = request.PersonId;
                    Db.Update(contact);
                }

            }

            var usercontact = Db
                            .Select(Db.From<UserContact>()
                            .Where(w => w.ContactId == request.ContactId && w.UserId == Session.UserId))
                            .SingleOrDefault();
            if (usercontact == null)
            {
                usercontact = new UserContact
                {
                    ContactId = request.ContactId,
                    UserId = Session.UserId
                };
                Db.Insert(usercontact);
            }

            if (request.ContactGroupId != 0)
            {
                var contactGroupMember = new ContactGroupMember
                {
                    ContactGroupId = request.ContactGroupId,
                    ContactId = request.ContactId
                };
                request.Id = (int)Db.Insert(contactGroupMember, true);
            }

            //Employer
            if (request.EmployerId.HasValue && request.EmployerId.Value > 0)
            {
                var employee = Db.Select(
                                        Db.From<Employee>()
                                            .Where(w => w.PersonId == request.PersonId))
                                    .SingleOrDefault();
                if (employee == null)
                {
                    employee = new Employee
                    {
                        PersonId = request.PersonId,
                        EmployerId = request.EmployerId.Value,
                        CreatedDate = DateTime.Now,
                        FromDate = DateTime.Now,
                        ToDate = DateTime.Now,
                        CreatedById = Session.UserId
                    };
                    if (request.BankAccountId.HasValue)
                    {
                        employee.BankAccountId = request.BankAccountId.Value;
                    }

                    if (request.Salary.HasValue)
                    {
                        employee.Salary = request.Salary.Value;
                    }
                    Db.Insert(employee);
                }
                else
                {

                    employee.EmployerId = request.EmployerId.Value;

                    if (request.BankAccountId.HasValue)
                    {
                        employee.BankAccountId = request.BankAccountId.Value;
                    }
                    if (request.Salary.HasValue)
                    {
                        employee.Salary = request.Salary.Value;
                    }
                    Db.Update(employee);
                }
            }

            if (!request.WebUrl.IsNullOrEmpty())
            {
                var person = Db.Select(Db.From<Domain.System.Persons.Person>().Where(w => w.Id == request.PersonId)).SingleOrDefault();
                person.WebUrl = request.WebUrl;
                Db.Update(person);
            }

            Save(request, Session.TenantId);

            return null;
        }
        private void Save(Api.PostContact request, int tenantId)
        {
            var employee = Db.Select(Db.From<Employee>().Where(w => w.PersonId == request.PersonId)).SingleOrDefault();
            if (employee != null && request.BankAccounts != null)
            {
                var bankAccountIds = request.BankAccounts.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
                if (bankAccountIds.Any())
                {
                    var idsToDelete = Db.Select<int>(Db.From<EmployeeBankAccount>().Where(x => x.EmployeeId == employee.Id && !Sql.In(x.Id, bankAccountIds)).Select(x => x.BankAccountId)).ToList();
                    Db.Delete<EmployeeBankAccount>(x => x.EmployeeId == employee.Id && !Sql.In(x.Id, bankAccountIds));
                    Db.Delete<BankAccount>(x => Sql.In(x.Id, idsToDelete));

                }
                else
                {
                    var idsToDelete = Db.Select<int>(Db.From<EmployeeBankAccount>().Where(x => x.EmployeeId == employee.Id).Select(x => x.BankAccountId)).ToList();
                    Db.Delete<EmployeeBankAccount>(x => x.EmployeeId == employee.Id);
                    Db.Delete<BankAccount>(x => Sql.In(x.Id, idsToDelete));

                }

                foreach (var bankAccount in request.BankAccounts)
                {
                    if (bankAccount.Id.HasValue)
                    {
                        if (bankAccount.BankAccount.Id != 0)
                        {
                            Db.Update(bankAccount.BankAccount);
                        }

                        Db.Update(new EmployeeBankAccount
                        {
                            Id = bankAccount.Id.Value,
                            EmployeeId = employee.Id,
                            BankAccountId = bankAccount.BankAccountId
                        });
                    }
                    else
                    {
                        bankAccount.BankAccount.PersonId = request.PersonId;
                        bankAccount.BankAccountId = (int)Db.Insert(bankAccount.BankAccount, true);
                        bankAccount.BankAccount.Id = bankAccount.BankAccountId;

                        Db.Insert(new EmployeeBankAccount
                        {
                            EmployeeId = employee.Id,
                            BankAccountId = bankAccount.BankAccountId
                        });
                    }
                }
            }
        }
    }
}

