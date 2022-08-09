using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.Domain.Health;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Procurement;
using CentralOperativa.ServiceInterface.System.Workflows;
using ServiceStack;
using ServiceStack.OrmLite;
using TreatmentRequest = CentralOperativa.ServiceModel.Health.TreatmentRequest;
using WorkflowInstance = CentralOperativa.ServiceModel.System.Workflows.WorkflowInstance;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class TreatmentRequestService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly WorkflowInstanceService _workflowInstanceService;
        private readonly Procurement.VendorService _vendorService;
        private readonly DiseaseService _diseaseService;
        private readonly WorkflowActivityRepository _workflowActivityRepository;

        public TreatmentRequestService(
            AutoQuery autoQuery,
            WorkflowInstanceService workflowInstanceService,
            Procurement.VendorService vendorService,
            DiseaseService diseaseService,
            WorkflowActivityRepository workflowActivityRepository)
        {
            _autoQuery = autoQuery;
            _workflowInstanceService = workflowInstanceService;
            _vendorService = vendorService;
            _diseaseService = diseaseService;
            _workflowActivityRepository = workflowActivityRepository;
        }

        public async Task<TreatmentRequest.GetResponse> Get(TreatmentRequest.Get request)
        {
            var treatmentRequest = (await Db.SingleByIdAsync<Domain.Health.TreatmentRequest>(request.Id)).ConvertTo<TreatmentRequest.GetResponse>();
            var workflowInstance = await _workflowInstanceService.Get(new WorkflowInstance.GetWorkflowInstance { Id = treatmentRequest.WorkflowInstanceId });
            treatmentRequest.WorkflowInstance = workflowInstance;

            // PatientDiagnostics
            var patientDiagnostics = await Db.SelectAsync(Db.From<PatientDiagnostic>().Where(w => w.TreatmentRequestId == treatmentRequest.Id));
            foreach (var patientDiagnostic in patientDiagnostics)
            {
                var disease = await _diseaseService.Get(new ServiceModel.Health.Disease.Get { Id = patientDiagnostic.DiseaseId });
                var doctor = await Db.SingleByIdAsync<Doctor>(patientDiagnostic.DoctorId);
                var doctorPerson = await Db.SingleByIdAsync<Domain.System.Persons.Person>(doctor.PersonId);
                var diagnostic = new TreatmentRequest.GetResponse.PatientDiagnostic
                {
                    Id = patientDiagnostic.Id,
                    Comments = patientDiagnostic.Comments,
                    Date = patientDiagnostic.Date,
                    DiseaseId = disease.Id,
                    Disease = disease,
                    DoctorId = patientDiagnostic.DoctorId,
                    DoctorName = doctorPerson.Name
                };

                treatmentRequest.Diagnostics.Add(diagnostic);
            }

            // TreatmentRequestDrugs
            var treatmentRequestDrugs = await Db.SelectAsync(Db.From<TreatmentRequestDrug>().Where(w => w.TreatmentRequestId == treatmentRequest.Id));
            foreach (var treatmentRequestDrug in treatmentRequestDrugs)
            {
                var drug = Db.SingleById<Drug>(treatmentRequestDrug.DrugId);
                var item = treatmentRequestDrug.ConvertTo<TreatmentRequest.GetResponse.TreatmentRequestDrug>();
                item.Drug = drug;
                if (treatmentRequestDrug.CommercialDrugId.HasValue)
                {
                    var commercialDrug = (await Db.SelectAsync<ServiceModel.Health.CommercialDrug.QueryResult>(Db
                        .From<CommercialDrug>()
                        .Join<CommercialDrug, Drug>()
                        .LeftJoin<CommercialDrug, DrugPresentation>()
                        .LeftJoin<CommercialDrug, Domain.System.Persons.Person>()
                        .Where(w => w.Id == treatmentRequestDrug.CommercialDrugId.Value)))
                        .Single();
                    item.CommercialDrug = commercialDrug;
                }

                treatmentRequest.Drugs.Add(item);
            }

            // TreatmentRequestArticles
            var treatmentRequestProducts = await Db.SelectAsync(Db.From<TreatmentRequestProduct>().Where(w => w.TreatmentRequestId == treatmentRequest.Id));
            foreach (var treatmentRequestProduct in treatmentRequestProducts)
            {
                var product = Db.SingleById<Product>(treatmentRequestProduct.ProductId);
                var item = treatmentRequestProduct.ConvertTo<TreatmentRequest.GetResponse.TreatmentRequestProduct>();
                item.Product = product;
                if (treatmentRequestProduct.VendorId.HasValue)
                {
                    var vendor = await _vendorService.Get(new Vendor.GetVendor { Id = treatmentRequestProduct.VendorId.Value });
                    item.Vendor = vendor;
                }

                treatmentRequest.Products.Add(item);
            }

            // TreatmentRequestPractices
            var treatmentRequestPractices = await Db.SelectAsync(Db.From<TreatmentRequestPractice>().Where(w => w.TreatmentRequestId == treatmentRequest.Id));
            foreach (var treatmentRequestPractice in treatmentRequestPractices)
            {
                var item = treatmentRequestPractice.ConvertTo<TreatmentRequest.GetResponse.TreatmentRequestPractice>();
                {
                    var medicalPractice = await Db.SingleByIdAsync<MedicalPractice>(item.MedicalPracticeId);
                    item.MedicalPractice = medicalPractice;

                    if (treatmentRequestPractice.VendorId.HasValue)
                    {
                        var vendor = await _vendorService.Get(new Vendor.GetVendor { Id = treatmentRequestPractice.VendorId.Value });
                        item.Vendor = vendor;
                    }
                }
                treatmentRequest.Practices.Add(item);
            }

            return treatmentRequest;
        }

        public object Get(TreatmentRequest.Query request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var parameters = Request.GetRequestParams();
            var q = _autoQuery.CreateQuery(request, parameters)
                .Join<Domain.Health.TreatmentRequest, Patient>()
                .Join<Patient, Domain.System.Persons.Person>()
                .Join<Domain.Health.TreatmentRequest, Domain.System.Workflows.WorkflowInstance>()
                .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivity>(
                    (wi, wa) => wi.CurrentWorkflowActivityId == wa.Id)
                .Join<Domain.System.Workflows.WorkflowInstance, Workflow>();

            List<int> roleIds;
            if (Session.Roles.Contains("admin"))
            {
                roleIds = Db.Column<int>(Db.From<Domain.System.Role>().Where(w => w.Id != 1).Select(x => x.Id));
            }
            else
            {
                roleIds = Db.Column<int>(Db.From<Domain.System.Role>().Where(w => Sql.In(w.Name, Session.Roles)));
            }

            SqlExpression<Domain.Health.TreatmentRequest> wiIdsQ = null;
            switch (request.View)
            {
                case 0: //Own
                    wiIdsQ = Db.From<Domain.Health.TreatmentRequest>()
                        .Join<Domain.Health.TreatmentRequest, Patient>()
                        .Join<Patient, Domain.System.Persons.Person>()
                        .Join<Domain.Health.TreatmentRequest, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && Sql.In(wir.RoleId, roleIds))
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 1: //Supervised
                    wiIdsQ = Db.From<Domain.Health.TreatmentRequest>()
                        .Join<Domain.Health.TreatmentRequest, Patient>()
                        .Join<Patient, Domain.System.Persons.Person>()
                        .Join<Domain.Health.TreatmentRequest, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission == 2 && Sql.In(warp.RoleId, roleIds))
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 2: //Others
                    wiIdsQ = Db.From<Domain.Health.TreatmentRequest>()
                        .Join<Domain.Health.TreatmentRequest, Patient>()
                        .Join<Patient, Domain.System.Persons.Person>()
                        .Join<Domain.Health.TreatmentRequest, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission < 2 && Sql.In(warp.RoleId, roleIds))
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 3: //Terminated
                    wiIdsQ = Db.From<Domain.Health.TreatmentRequest>()
                        .Join<Domain.Health.TreatmentRequest, Patient>()
                        .Join<Patient, Domain.System.Persons.Person>()
                        .Join<Domain.Health.TreatmentRequest, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && Sql.In(warp.RoleId, roleIds))
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 4: //Finished
                    wiIdsQ = Db.From<Domain.Health.TreatmentRequest>()
                        .Join<Domain.Health.TreatmentRequest, Patient>()
                        .Join<Patient, Domain.System.Persons.Person>()
                        .Join<Domain.Health.TreatmentRequest, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal)
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 5: //All
                    break;
            }

            wiIdsQ.FromExpression = wiIdsQ.FromExpression.Replace(
                "(\"WorkflowActivityRoles\".\"WorkflowActivityId\" = \"WorkflowInstances\".\"CurrentWorkflowActivityId\")",
                "(\"WorkflowActivityRoles\".\"WorkflowActivityId\" = \"WorkflowInstances\".\"CurrentWorkflowActivityId\" AND \"WorkflowActivityRoles\".\"RoleId\" = \"WorkflowInstanceAssignments\".\"RoleId\")");

            if (parameters.ContainsKey("personNameContains"))
            {
                wiIdsQ.And<Domain.System.Persons.Person>(w => w.Name.Contains(parameters["personNameContains"]));
            }

            if (!string.IsNullOrEmpty(request.Q))
            {
                wiIdsQ.WhereExpression += " (";
                wiIdsQ.And<Domain.System.Persons.Person>(w => w.Name.Contains(request.Q) || w.Code.Contains(request.Q));
                wiIdsQ.Or<WorkflowActivity>(w => w.Name.Contains(request.Q));
                wiIdsQ.UnsafeOr("TreatmentRequests.MessageThreadId IN (SELECT DISTINCT m.MessageThreadId FROM Messages m WHERE CONTAINS(m.Body, {0}))", "\"" + request.Q + "*\"");

                if(request.View < 4)
                {
                    wiIdsQ.UnsafeOr($"WorkflowInstanceAssignments.RoleId IN (SELECT Id FROM Roles WHERE Name LIKE '%{request.Q}%')");
                }

                if (int.TryParse(request.Q, out var intValue))
                {
                    wiIdsQ.Or(x => x.Id == intValue);
                }
                wiIdsQ.WhereExpression += " )";

                if (wiIdsQ.WhereExpression == null)
                {
                    wiIdsQ.WhereExpression += "WHERE";
                }
                else
                {
                    wiIdsQ.WhereExpression = wiIdsQ.WhereExpression.Replace("( AND", "AND (");
                }
            }

            q.Where<Domain.System.Workflows.WorkflowInstance>(w => Sql.In(w.Id, wiIdsQ));

            // WORKAROUND bug size de 2ndo parámetro función CONTAINS
            foreach (var parameter in q.Params)
            {
                var sqlParam = parameter as SqlParameter;
                if (sqlParam?.SqlDbType == SqlDbType.NVarChar && sqlParam.Size == 8000)
                {
                    sqlParam.Size = 4000;
                }
            }

            var result = _autoQuery.Execute(request, q);

            var roleMap = Db.Select<WorkflowInstanceRoleMap>(Db
                .From<WorkflowInstanceAssignments>()
                .Join<Domain.System.Role>()
                .Where(w => Sql.In(w.WorkflowInstanceId, wiIdsQ))
                .And(w => w.IsActive));
            result.Results.ForEach(x => x.Roles = string.Join(",", roleMap.Where(w => w.WorkflowInstanceId == x.WorkflowInstanceId).Select(w => w.RoleName)));
            return result;
        }

        private class WorkflowInstanceRoleMap
        {
            public int WorkflowInstanceId { get; set; }
            public string RoleName { get; set; }
        }

        public LookupResult Get(TreatmentRequest.Lookup request)
        {
            var query = Db.From<Domain.Health.TreatmentRequest>();

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Date.ToString().Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Id.ToString() }),
                Total = (int)count
            };
            return result;
        }

        public object Put(TreatmentRequest.Post request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    Db.Update((Domain.Health.TreatmentRequest)request);
                    request.WorkflowInstance = (WorkflowInstance.PostWorkflowInstance)HostContext.ServiceController.Execute(request.WorkflowInstance, Request);
                    Save(request);
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

        public async Task<object> Post(TreatmentRequest.Post request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    //WorkflowInstance
                    var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 1, (short)WellKnownWorkflowTypes.TreatmentRequest);
                    request.WorkflowInstance.WorkflowId = currentActivity.WorkflowId;
                    request.WorkflowInstance.CreatedByUserId = Session.UserId;
                    request.WorkflowInstance.CreateDate = DateTime.UtcNow;
                    request.WorkflowInstance.Guid = Guid.NewGuid();
                    request.WorkflowInstance.CurrentWorkflowActivityId = currentActivity.Id;
                    request.WorkflowInstance = (WorkflowInstance.PostWorkflowInstance) HostContext.ServiceController.Execute(request.WorkflowInstance, Request);
                    request.WorkflowInstanceId = request.WorkflowInstance.Id;

                    // Message Thread
                    if (!string.IsNullOrEmpty(request.Comments))
                    {
                        var messageThread = new Domain.System.Messages.MessageThread { CreateDate = DateTime.UtcNow };
                        messageThread.Id = (int)Db.Insert(messageThread, true);
                        request.MessageThreadId = messageThread.Id;

                        var message = new Domain.System.Messages.Message
                        {
                            MessageThreadId = messageThread.Id,
                            CreateDate = DateTime.UtcNow,
                            SenderId = Session.UserId,
                            Body = request.Comments
                        };
                        message.Id = (int)Db.Insert(message, true);
                    }

                    request.Id = (int)Db.Insert((Domain.Health.TreatmentRequest)request, true);

                    Save(request);
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

        private void Save(TreatmentRequest.Post request)
        {
            //TreatmentRequestArticles
            var treatmentRequestArticleIds = request.Products.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
            if (treatmentRequestArticleIds.Any())
            {
                Db.Delete<TreatmentRequestProduct>(x => x.TreatmentRequestId == request.Id && !Sql.In(x.Id, treatmentRequestArticleIds));
            }
            else
            {
                Db.Delete<TreatmentRequestProduct>(x => x.TreatmentRequestId == request.Id);
            }

            foreach (var product in request.Products)
            {
                if (product.Id.HasValue)
                {
                    Db.Update(new TreatmentRequestProduct
                    {
                        Id = product.Id.Value,
                        TreatmentRequestId = request.Id,
                        ProductId = product.ProductId,
                        Quantity = product.Quantity,
                        Comments = product.Comments,
                        VendorId = product.VendorId,
                        Price = product.Price
                    });
                }
                else
                {
                    Db.Insert(new TreatmentRequestProduct
                    {
                        TreatmentRequestId = request.Id,
                        ProductId = product.ProductId,
                        Quantity = product.Quantity,
                        Comments = product.Comments,
                        VendorId = product.VendorId,
                        Price = product.Price
                    });
                }
            }

            //Drugs
            var treatmentRequestDrugIds = request.Drugs.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
            if (treatmentRequestDrugIds.Any())
            {
                Db.Delete<TreatmentRequestDrug>(x => x.TreatmentRequestId == request.Id && !Sql.In(x.Id, treatmentRequestDrugIds));
            }
            else
            {
                Db.Delete<TreatmentRequestDrug>(x => x.TreatmentRequestId == request.Id);
            }

            foreach (var drug in request.Drugs)
            {
                if (drug.Id.HasValue)
                {
                    Db.Update(new TreatmentRequestDrug
                    {
                        Id = drug.Id.Value,
                        TreatmentRequestId = request.Id,
                        DrugId = drug.DrugId,
                        CommercialDrugId = drug.CommercialDrugId,
                        Quantity = drug.Quantity,
                        Frequency = drug.Frequency,
                        Comments = drug.Comments,
                        VendorId = drug.VendorId,
                        Price = drug.Price
                    });
                }
                else
                {
                    Db.Insert(new TreatmentRequestDrug
                    {
                        TreatmentRequestId = request.Id,
                        DrugId = drug.DrugId,
                        CommercialDrugId = drug.CommercialDrugId,
                        Quantity = drug.Quantity,
                        Frequency = drug.Frequency,
                        Comments = drug.Comments,
                        VendorId = drug.VendorId,
                        Price = drug.Price
                    });
                }
            }

            //PatientDiagnostics
            var patientDiagnosticIds = request.Diagnostics.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
            if (patientDiagnosticIds.Any())
            {
                Db.Delete<PatientDiagnostic>(x => x.TreatmentRequestId == request.Id && !Sql.In(x.Id, patientDiagnosticIds));
            }
            else
            {
                Db.Delete<PatientDiagnostic>(x => x.TreatmentRequestId == request.Id);
            }

            foreach (var diagnostic in request.Diagnostics)
            {
                if (diagnostic.Id.HasValue)
                {
                    Db.Update(new PatientDiagnostic
                    {
                        Id = diagnostic.Id.Value,
                        TreatmentRequestId = request.Id,
                        PatientId = request.PatientId,
                        DoctorId = request.DoctorId,
                        DiseaseId = diagnostic.DiseaseId,
                        Comments = diagnostic.Comments,
                        Date = DateTime.UtcNow
                    });
                }
                else
                {
                    Db.Insert(new PatientDiagnostic
                    {
                        TreatmentRequestId = request.Id,
                        PatientId = request.PatientId,
                        DoctorId = request.DoctorId,
                        DiseaseId = diagnostic.DiseaseId,
                        Comments = diagnostic.Comments,
                        Date = DateTime.UtcNow
                    });
                }
            }

            //TreatmentRequestPractices
            var treatmentRequestPracticeIds = request.Practices.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
            if (treatmentRequestPracticeIds.Any())
            {
                Db.Delete<TreatmentRequestPractice>(x => x.TreatmentRequestId == request.Id && !Sql.In(x.Id, treatmentRequestPracticeIds));
            }
            else
            {
                Db.Delete<TreatmentRequestPractice>(x => x.TreatmentRequestId == request.Id);
            }

            foreach (var practice in request.Practices)
            {
                if (practice.Id.HasValue)
                {
                    Db.Update(new TreatmentRequestPractice
                    {
                        Id = practice.Id.Value,
                        TreatmentRequestId = request.Id,
                        MedicalPracticeId = practice.MedicalPracticeId,
                        Quantity = practice.Quantity,
                        Frequency = practice.Frequency,
                        Comments = practice.Comments,
                        VendorId = practice.VendorId,
                        FromDate = practice.FromDate,
                        ToDate = practice.ToDate
                    });
                }
                else
                {
                    Db.Insert(new TreatmentRequestPractice
                    {
                        TreatmentRequestId = request.Id,
                        MedicalPracticeId = practice.MedicalPracticeId,
                        Quantity = practice.Quantity,
                        Frequency = practice.Frequency,
                        Comments = practice.Comments,
                        VendorId = practice.VendorId,
                        FromDate = practice.FromDate,
                        ToDate = practice.ToDate
                    });
                }
            }
        }
    }
}