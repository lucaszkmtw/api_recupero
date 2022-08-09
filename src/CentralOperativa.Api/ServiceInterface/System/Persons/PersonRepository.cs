using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System.Persons;

namespace CentralOperativa.ServiceInterface.System.Persons
{
    [Authenticate]
    public class PersonRepository
    {
        public async Task<Api.Person> GetPerson(IDbConnection db, string code)
        {
            var person = db.Select<Api.Person>(db.From<Person>().Where(w => w.Code == code)).SingleOrDefault();
            if (person != null)
            {
                person.Emails = await db.SelectAsync(db.From<PersonEmail>().Where(w => w.PersonId == person.Id));
                person.Documents = await db.SelectAsync(db.From<PersonDocument>().Where(w => w.PersonId == person.Id));
                person.Phones = await db.SelectAsync(db.From<PersonPhone>().Where(w => w.PersonId == person.Id));
                person.Addresses = await db.SelectAsync<Api.PersonAddress>(db.From<PersonAddress>().Where(w => w.PersonId == person.Id));
                foreach (var personAddress in person.Addresses)
                {
                    personAddress.Address = (await db.SingleByIdAsync<Domain.System.Location.Address>(personAddress.AddressId)).ConvertTo<ServiceModel.System.Location.Address.GetAddressResult>();
                    personAddress.Address.Place = await db.SingleByIdAsync<Domain.System.Location.PlaceNode>(personAddress.Address.PlaceId);
                }

                person.References = await db.SingleAsync<Reference>(w => w.PersonId == person.Id);
            }
            return person;
        }

        public async Task<Api.Person> GetPerson(IDbConnection db, int id)
        {
            var person = (await db.SelectAsync<Api.Person>(db.From<Person>().Where(w => w.Id == id))).SingleOrDefault();
            if (person != null)
            {
                person.Emails = await db.SelectAsync(db.From<PersonEmail>().Where(w => w.PersonId == id));
                person.Documents = await db.SelectAsync(db.From<PersonDocument>().Where(w => w.PersonId == person.Id));
                person.Phones = await db.SelectAsync(db.From<PersonPhone>().Where(w => w.PersonId == id));
                person.Addresses = await db.SelectAsync<Api.PersonAddress>(db.From<PersonAddress>().Where(w => w.PersonId == id));
                foreach (var personAddress in person.Addresses)
                {
                    personAddress.Address = (await db.SingleByIdAsync<Domain.System.Location.Address>(personAddress.AddressId)).ConvertTo<ServiceModel.System.Location.Address.GetAddressResult>();
                    personAddress.Address.Place = await db.SingleByIdAsync<Domain.System.Location.PlaceNode>(personAddress.Address.PlaceId);
                }

                person.Fields = await db.SelectAsync<Api.PersonField>(
                    db.From<PersonFieldValue>()
                    .Join<PersonFieldValue, PersonField>()
                    .Where(w => w.PersonId == person.Id));
                person.References = await db.SingleAsync<Reference>(w => w.PersonId == person.Id);

                var employee = (await db.SelectAsync(
                                        db.From<Domain.CRM.Contacts.Employee>()
                                            .Where(w => w.PersonId == person.Id))
                                    ).SingleOrDefault();
                if (employee != null)
                {
                    person.EmployerId = employee.EmployerId;
                }
                return person;
            }

            throw new Exception($"Person with id {id} was not found.");
        }

        public async Task<Api.PostPerson> CreatePerson(IDbConnection db, Api.PostPerson person, int userId = 0)
        {
            var existing = !string.IsNullOrEmpty(person.Code) ? (await db.SelectAsync(db.From<Person>().Where(w => w.Code == person.Code))).SingleOrDefault() : null;
            if (existing != null)
            {
                person.Id = existing.Id;
                person.BirthDate = person.BirthDate ?? existing.BirthDate;
                person.DeathDate = person.DeathDate ?? existing.DeathDate;
                person.Gender = person.Gender ?? existing.Gender;
            }

            if (person.IsOrganization.HasValue && !person.IsOrganization.Value)
            {
                person.Name = Api.PersonExtensions.GetFullName(person);
            }
            else
            {
                person.FirstName = null;
                person.LastName = null;
            }

            if (existing == null)
            {
                person.Id = (int) await db.InsertAsync((Person)person, true);
            }
            else
            {
                await db.UpdateAsync((Person)person);
            }

            await SaveRelationships(db, null, person, userId);

            return person;
        }

        public async Task<Api.PostPerson> UpdatePerson(IDbConnection db, Api.PostPerson person, bool deleteRelationshipsIfNeeded = false, int userId = 0)
        {
            var existing = await GetPerson(db, person.Id);
            if (existing == null)
            {
                throw new ArgumentException("Supplied personId not found.");
            }

            person.BirthDate = person.BirthDate ?? existing.BirthDate;
            person.DeathDate = person.DeathDate ?? existing.DeathDate;
            person.Gender = person.Gender ?? existing.Gender;

            if (person.IsOrganization.HasValue && !person.IsOrganization.Value)
            {
                person.Name = Api.PersonExtensions.GetFullName(person);
            }
            else
            {
                person.FirstName = null;
                person.LastName = null;
            }

            await db.UpdateAsync((Person)person);
            await SaveRelationships(db, (deleteRelationshipsIfNeeded)?person: null, person, userId);
            return person;
        }

        private async Task SaveRelationships(IDbConnection db, Person person, Api.PostPerson request, int userId = 0)
        {
            //Documents
            if (person != null)
            {
                var documentIds = request.Documents.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                if (documentIds.Any())
                {
                    await db.DeleteAsync<PersonDocument>(x => x.PersonId == request.Id && !Sql.In(x.Id, documentIds));
                }
                else
                {
                    await db.DeleteAsync<PersonDocument>(x => x.PersonId == request.Id);
                }
            }

            foreach (var document in request.Documents)
            {
                if (document.Id == 0)
                {
                    if (!string.IsNullOrEmpty(document.Number))
                    {
                        await db.InsertAsync(new PersonDocument
                        {
                            PersonId = request.Id
                        });
                    }
                }
                else
                {
                    await db.UpdateAsync((PersonDocument)document);
                }
            }

            //Emails
            if (person != null)
            {
                var emailIds = request.Emails.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                if (emailIds.Any())
                {
                    await db.DeleteAsync<PersonEmail>(x => x.PersonId == request.Id && !Sql.In(x.Id, emailIds));
                }
                else
                {
                    await db.DeleteAsync<PersonEmail>(x => x.PersonId == request.Id);
                }
            }

            foreach (var email in request.Emails)
            {
                if (email.Id == 0)
                {
                    if (!string.IsNullOrEmpty(email.Address))
                    {
                        await db.InsertAsync(new PersonEmail
                        {
                            PersonId = request.Id,
                            Address = email.Address,
                            TypeId = email.TypeId,
                            TypeName = email.TypeName
                        });
                    }
                }
                else
                {
                    await db.UpdateAsync((PersonEmail)email);
                }
            }

            //Phones
            if (person != null)
            {
                var phoneIds = request.Phones.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                if (phoneIds.Any())
                {
                    await db.DeleteAsync<PersonPhone>(x => x.PersonId == request.Id && !Sql.In(x.Id, phoneIds));
                }
                else
                {
                    await db.DeleteAsync<PersonPhone>(x => x.PersonId == request.Id);
                }
            }

            foreach (var phone in request.Phones)
            {
                if (phone.Id == 0)
                {
                    if (!string.IsNullOrEmpty(phone.Number))
                    {
                        await db.InsertAsync(new PersonPhone
                        {
                            PersonId = request.Id,
                            IsDefault = phone.IsDefault,
                            Number = phone.Number,
                            TypeId = phone.TypeId,
                            TypeName = phone.TypeName
                        });
                    }
                }
                else
                {
                    await db.UpdateAsync((PersonPhone)phone);
                }
            }

            //Addresses
            if (person != null)
            {
                var addressIds = request.Addresses.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                if (addressIds.Any())
                {
                    await db.DeleteAsync<PersonAddress>(x => x.PersonId == request.Id && !Sql.In(x.Id, addressIds));
                }
                else
                {
                    await db.DeleteAsync<PersonAddress>(x => x.PersonId == request.Id);
                }
            }

            foreach (var personAddress in request.Addresses)
            {
                if (personAddress.Id == 0)
                {
                    if (personAddress.Address.Id == 0)
                    {
                        personAddress.AddressId = (int) await db.InsertAsync(new Domain.System.Location.Address
                        {
                            Appartment = personAddress.Address.Appartment,
                            Floor = personAddress.Address.Floor,
                            PlaceId = personAddress.Address.PlaceId == 0 ? 2 : personAddress.Address.PlaceId,
                            Street = personAddress.Address.Street,
                            StreetNumber = personAddress.Address.StreetNumber,
                            ZipCode = personAddress.Address.ZipCode
                        }, true);
                    }

                    await db.InsertAsync(new PersonAddress
                    {
                        PersonId = request.Id,
                        AddressId = personAddress.AddressId,
                        IsDefault = personAddress.IsDefault,
                        TypeId = personAddress.TypeId,
                        TypeName = personAddress.TypeName
                    });
                }
                else
                {
                    await db.UpdateAsync((PersonAddress)personAddress);
                    if (personAddress.AddressId != 0)
                    {
                        await db.UpdateAsync((Domain.System.Location.Address)personAddress.Address);
                    }
                }

            }

            //Employer
            if (request.EmployerId.HasValue && request.EmployerId.Value > 0)
            {
                var employee = (await db.SelectAsync(db.From<Domain.CRM.Contacts.Employee>().Where(w => w.PersonId == request.Id))).SingleOrDefault();
                if(employee == null)
                {
                    employee = new Domain.CRM.Contacts.Employee
                    {
                        PersonId = request.Id,
                        EmployerId = request.EmployerId.Value,
                        CreatedDate = DateTime.Now,
                        FromDate = DateTime.Now,
                        ToDate = DateTime.Now,
                        CreatedById = userId
                    };
                    await db.InsertAsync(employee);
                }else
                {
                    employee.EmployerId = request.EmployerId.Value;
                    await db.UpdateAsync(employee);
                }
            }
        }

        public async Task DeletePerson(IDbConnection db, int id)
        {
            var personAddressId = db.From<PersonAddress>().Select(c => c.Id).Where(c => c.PersonId == id);
            await db.DeleteByIdAsync<PersonAddress>(personAddressId);

            var personPhoneId = db.From<PersonPhone>().Select(c => c.Id).Where(c => c.PersonId == id);
            await db.DeleteByIdAsync<PersonPhone>(personPhoneId);

            var personEmailId = db.From<PersonEmail>().Select(c => c.Id).Where(c => c.PersonId == id);
            await db.DeleteByIdAsync<PersonEmail>(personEmailId);

            await db.DeleteByIdAsync<Person>(id);
        }
    }
}