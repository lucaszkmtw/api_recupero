using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.Health;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.BusinessPartners;
using CentralOperativa.ServiceInterface.System.Persons;
using HealthService = CentralOperativa.ServiceModel.Health.HealthService;
using TreatmentRequest = CentralOperativa.ServiceModel.Health.TreatmentRequest;

namespace CentralOperativa.ServiceInterface.Health
{
    using Api = ServiceModel.Health;

    [Authenticate]
    public class PatientService : ApplicationService
    {
        private IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;
        private readonly BusinessPartnerRepository _businessPartnerRepository;

        public PatientService(
            IAutoQueryDb autoQuery,
            PersonRepository personRepository,
            BusinessPartnerRepository businessPartnerRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _businessPartnerRepository = businessPartnerRepository;
        }

        public object Put(Api.PostPatient request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = Db.SingleById<Patient>(request.Id);
                    if (existing != null)
                    {
                        Db.Update((Patient)request);

                        //TODO: Manejar si se esta modificando una persona que ya tiene otro patient asignado...
                        //return HttpError.Conflict("There is already a patient linked to person " + request.PersonId);
                    }

                    SaveHealthServicePatient(request.Id, request.GetHealthServicePatient);
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

        public object Post(Api.PostPatient request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = Db.Select(Db.From<Patient>().Where(x => x.PersonId == request.PersonId)).SingleOrDefault();
                    if (existing != null)
                    {
                        return HttpError.Conflict("There is already a patient linked to person " + request.PersonId);
                    }

                    request.Id = (int)Db.Insert((Patient)request, true);
                    SaveHealthServicePatient(request.Id, request.GetHealthServicePatient);
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

        public async Task<Api.PostPatientBatch> Post(Api.PostPatientBatch request)
        {
            var healthServiceService = ResolveService<HealthServiceService>();

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

                        // Health Service
                        if (item.GetHealthServicePatient.HealthServiceId == 0)
                        {
                            var existingHealthService = await healthServiceService.Get(new HealthService.GetHealthServiceByCode
                            {
                                Code = item.GetHealthServicePatient.HealthService.Code
                            });
                            item.GetHealthServicePatient.HealthService = existingHealthService ??
                                                                      await healthServiceService.Post(item.GetHealthServicePatient.HealthService.ConvertTo<HealthService.PostHealthService>());
                            item.GetHealthServicePatient.HealthServiceId = item.GetHealthServicePatient.HealthService.Id;
                        }
                       
                        // Patient
                        var existingPatient = Db.Select<Patient>(w => w.PersonId == item.PersonId).SingleOrDefault();
                        if (existingPatient == null)
                        {
                            // BusinessPartner
                            var businessPartner = new ServiceModel.BusinessPartners.PostBusinessPartner { CreateDate = DateTime.UtcNow, CreatedById = Session.UserId, Guid = Guid.NewGuid(), PersonId = item.PersonId, Status = Domain.BusinessPartners.BusinessPartnerStatus.Active, TenantId = Session.TenantId, TypeId = 1 };
                            await _businessPartnerRepository.InsertBusinessPartner(Db, Session, businessPartner);
                            item.Id = (int) await Db.InsertAsync((Patient)item, true);
                        }
                        else
                        {
                            item.Id = existingPatient.Id;
                        }

                        item.GetHealthServicePatient.PatientId = item.Id;

                        // HealthServicePatient
                        SaveHealthServicePatient(item.Id, item.GetHealthServicePatient);
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

        public async Task<Api.GetPatientResponse> Get(Api.GetPatient request)
        {
            var patient = Db.SingleById<Patient>(request.Id).ConvertTo<Api.GetPatientResponse>();
            if (patient.Id == 0)
            {
                throw HttpError.NotFound($"The patient with id {request.Id} was not found.");
            }

            var person = await _personRepository.GetPerson(Db, patient.PersonId);
            patient.Person = person;
            var healthServicePatient = (await Db.SelectAsync<Api.GetHealthServicePatientResult>(Db.From<HealthServicePatient>().Where(w => w.PatientId == patient.Id && w.ToDate == null))).SingleOrDefault();
            if (healthServicePatient != null)
            {
                patient.GetHealthServicePatient = healthServicePatient;
                var healthService = await Db.SingleByIdAsync<Domain.Health.HealthService>(healthServicePatient.HealthServiceId);
                patient.GetHealthServicePatient.HealthService =
                    healthService.ConvertTo<HealthService.GetResult>();
                var healthServicePerson = await _personRepository.GetPerson(Db, healthService.PersonId);
                patient.GetHealthServicePatient.HealthService.Person = healthServicePerson;
            }

            return patient;
        }

        public QueryResponse<Api.QueryPatientsResult> Get(Api.QueryPatients request)
        {
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams())
                .LeftJoin<Patient, HealthServicePatient>((p, hspa) => p.Id == hspa.PatientId && hspa.ToDate == null)
                .LeftJoin<HealthServicePatient, Domain.Health.HealthService>()
                .LeftJoin<HealthServicePatient, HealthServiceTenant>((hsp, hst) => hsp.HealthServiceId == hst.HealthServiceId && hst.TenantId == Session.TenantId);

            q.LeftJoin<Domain.System.Persons.Person, Domain.System.Persons.PersonAddress>((p, pa) => p.Id == pa.PersonId && pa.IsDefault)
                .LeftJoin<Domain.System.Persons.PersonAddress, Domain.System.Location.Address>()
                .LeftJoin<Domain.System.Location.Address, Domain.System.Location.Place>()
                .LeftJoin<Domain.System.Persons.Person, Domain.System.Persons.PersonPhone>((p, pp) => p.Id == pp.PersonId && pp.IsDefault)
                .CustomJoin("LEFT JOIN Persons HealthServicePersons ON (HealthServices.{0} = HealthServicePersons.Id)".Fmt("PersonId".SqlColumn()))
                .UnsafeSelect(@"Patients.Id AS Id
                        , Persons.Id AS PersonId
                        , Persons.Code AS PersonCode
                        , Persons.Name AS PersonName
                        , Addresses.Name AS Address
                        , PersonPhones.Number AS Phone
                        , HealthServicePersons.{0} AS HealthService
                        , HealthServicePatients.{1} AS ServicePlan
                        , HealthServicePatients.{2} AS CardNumber
                        , CASE Places.[TypeId] WHEN 3 THEN Places.[Name] ELSE NULL END City
                        , CASE Places.[TypeId] WHEN 2 THEN Places.[Name] WHEN 3 THEN(SELECT states.Name FROM Places states WHERE Id = Places.ParentId) ELSE NULL END [State]
                        ".Fmt("Name".SqlColumn(), "ServicePlan".SqlColumn(), "CardNumber".SqlColumn()));
            return _autoQuery.Execute(request, q);
        }

        public object Get(Api.LookupPatient request)
        {
            var query = Db.From<Patient>().Join<Patient, Domain.System.Persons.Person>();

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
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.QueryPatientsResult>(query)
                .Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }

        public async Task<List<TreatmentRequest.GetResponse>> Get(Api.GetPatientClinicalHistory request)
        {
            var treatmentRequestService = TryResolve<TreatmentRequestService>();
            var ids = await Db.ColumnAsync<int>(Db.From<Domain.Health.TreatmentRequest>().Where(w => w.PatientId == request.PatientId).OrderByDescending(x => x.Id).Select(x => x.Id));
            var model = new List<TreatmentRequest.GetResponse>();
            ids.ForEach(async x => model.Add(await treatmentRequestService.Get(new TreatmentRequest.Get { Id = x })));

            return model;
        }

        public object Get(Api.GetPatientDocuments request)
        {
            var folderIds =
                Db.Column<int>(
                    Db.From<Domain.Health.TreatmentRequest>()
                        .Where(w => w.PatientId == request.PatientId && w.FolderId != null)
                        .Select(x => x.FolderId));

            var folders = Db.Select(Db.From<Domain.System.DocumentManagement.Folder>()
                .Join<Domain.Health.TreatmentRequest>()
                .Where<Domain.Health.TreatmentRequest>(w => w.PatientId == request.PatientId && w.FolderId != null));

            var files = Db.Select(Db.From<Domain.System.DocumentManagement.File>()
                .Join<Domain.System.DocumentManagement.File, Domain.System.DocumentManagement.FolderFile>()
                .Join<Domain.System.DocumentManagement.FolderFile, Domain.System.DocumentManagement.Folder>()
                .Join<Domain.System.DocumentManagement.Folder, Domain.Health.TreatmentRequest>()
                .Where<Domain.Health.TreatmentRequest>(w => w.PatientId == request.PatientId && w.FolderId != null));

            var model = new ServiceModel.System.DocumentManagement.GetFolderResult();
            //model.Children.AddRange(folders);
            model.Files.AddRange(files);

            return model;
        }

        private void SaveHealthServicePatient(int patientId, Api.GetHealthServicePatientResult getHealthServicePatient)
        {
            var existingHealthServices = Db.Select<HealthServicePatient>(w => w.PatientId == patientId);
            if (existingHealthServices.Any())
            {
                var activeItem = existingHealthServices.Where(x => !x.ToDate.HasValue).SingleOrDefault();
                if (activeItem != null)
                {
                    // Si hay uno activo y el nuevo valor es null lo desactivo.
                    if (getHealthServicePatient == null || getHealthServicePatient.HealthServiceId == 0)
                    {
                        activeItem.ToDate = DateTime.UtcNow;
                        Db.Update(activeItem);
                    }

                    // Si el nuevo valor es distinto al activo lo desactivo e inserto el valor nuevo.
                    // TODO: Implementar esta comparacion con .Equals y GetHashCode
                    if (activeItem.HealthServiceId != getHealthServicePatient.HealthServiceId
                        || activeItem.ServicePlan != getHealthServicePatient.ServicePlan
                        || activeItem.CardNumber != getHealthServicePatient.CardNumber
                        || activeItem.Data1 != getHealthServicePatient.Data1
                        || activeItem.Data2 != getHealthServicePatient.Data2)
                    {
                        activeItem.ToDate = DateTime.UtcNow;
                        Db.Update(activeItem);

                        Db.Insert(new HealthServicePatient
                        {
                            PatientId = patientId,
                            HealthServiceId = getHealthServicePatient.HealthServiceId,
                            CardNumber = getHealthServicePatient.CardNumber,
                            ServicePlan = getHealthServicePatient.ServicePlan,
                            Data1 = getHealthServicePatient.Data1,
                            Data2 = getHealthServicePatient.Data2,
                            FromDate = DateTime.UtcNow,
                        });
                    }
                }
            }
            else
            {
                if (getHealthServicePatient?.HealthServiceId != null)
                {
                    Db.Insert(new HealthServicePatient
                    {
                        PatientId = patientId,
                        HealthServiceId = getHealthServicePatient.HealthServiceId,
                        CardNumber = getHealthServicePatient.CardNumber,
                        ServicePlan = getHealthServicePatient.ServicePlan,
                        Data1 = getHealthServicePatient.Data1,
                        Data2 = getHealthServicePatient.Data2,
                        FromDate = DateTime.UtcNow
                    });
                }
            }
        }
    }
}
