using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.BusinessDocuments;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.Domain.System;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.BusinessPartners;
using CentralOperativa.ServiceInterface.System.Workflows;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Inv;
using CentralOperativa.ServiceInterface.System;
using CentralOperativa.ServiceInterface.System.Persons;
using Api = CentralOperativa.ServiceModel.BusinessDocuments;
using System.IO;
using System.Net.Http;
using System.Text;
using CentralOperativa.ServiceInterface.System.DocumentManagement;
using Globalization = System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Data;
//using Api = CentralOperativa.ServiceModel.Loans;

namespace CentralOperativa.ServiceInterface.BusinessDocuments
{
    [Authenticate]
    public class BusinessDocumentService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(BusinessDocumentService));
        private readonly IAutoQueryDb _autoQuery;
        private readonly TenantRepository _tenantRepository;
        private readonly PersonRepository _personRepository;
        private readonly WorkflowActivityRepository _workflowActivityRepository;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;
        private readonly BusinessPartnerRepository _businessPartnerRepository;
        private readonly FileRepository _fileRepository;

        public BusinessDocumentService(
            IAutoQueryDb autoQuery,
            TenantRepository tenantRepository,
            PersonRepository personRepository,
            WorkflowActivityRepository workflowActivityRepository,
            WorkflowInstanceRepository workflowInstanceRepository,
            BusinessPartnerRepository businessPartnerRepository,
            FileRepository fileRepository)
        {
            _autoQuery = autoQuery;
            _tenantRepository = tenantRepository;
            _personRepository = personRepository;
            _workflowActivityRepository = workflowActivityRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
            _businessPartnerRepository = businessPartnerRepository;
            _fileRepository = fileRepository;
        }

        public async Task<bool> Post(Api.PostBusinessDocumentSubmitForApproval request)
        {
            var businessDocument = await Db.SingleAsync<BusinessDocument>(w => w.Guid == request.BusinessDocumentGuid);
            if (businessDocument.ApprovalWorkflowInstanceId == null)
            {
                using (var trx = Db.OpenTransaction())
                {
                    try
                    {
                        var businessDocumentType = await Db.SingleByIdAsync<BusinessDocumentType>(businessDocument.TypeId);
                        // Tomo de workflowTypeId el que está asociado al businessdocumentType, en caso de que no lo tenga defaulteo al de factura de compra.
                        var workflowTypeId = businessDocumentType.ApprovalWorkflowTypeId ?? (short)WellKnownWorkflowTypes.InvoiceApproval;
                        var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 1, workflowTypeId);
                        var workflowInstance = new WorkflowInstance
                        {
                            CreateDate = DateTime.UtcNow,
                            CreatedByUserId = Session.UserId,
                            WorkflowId = currentActivity.WorkflowId,
                            CurrentWorkflowActivityId = currentActivity.Id,
                            Guid = Guid.NewGuid()
                        };
                        workflowInstance.Id = (int)await Db.InsertAsync(workflowInstance, true);

                        foreach (var approvalRule in currentActivity.ApprovalRules)
                        {
                            var workflowInstanceApproval = new WorkflowInstanceApproval
                            {
                                WorkflowInstanceId = workflowInstance.Id,
                                RoleId = approvalRule.RoleId,
                                UserId = approvalRule.UserId,
                                WorkflowActivityId = approvalRule.WorkflowActivityId,
                                Status = WorkflowInstanceApprovalStatus.Pending,
                                CreateDate = DateTime.UtcNow
                            };
                            await Db.InsertAsync(workflowInstanceApproval);
                        }

                        //Current activity roles
                        var activityRoles = await Db.SelectAsync<WorkflowActivityRole>(w => w.WorkflowActivityId == currentActivity.Id);

                        //Assign workflowinstance roles
                        foreach (var activityRole in activityRoles.Where(w => w.IsDefault))
                        {
                            var workflowInstanceRole = new WorkflowInstanceAssignments
                            {
                                WorkflowInstanceId = workflowInstance.Id,
                                WorkflowActivityId = workflowInstance.CurrentWorkflowActivityId,
                                RoleId = activityRole.RoleId,
                                UserId = Session.UserId,
                                CreateDate = DateTime.UtcNow,
                                IsActive = true
                            };
                            await Db.InsertAsync(workflowInstanceRole);
                        }

                        businessDocument.ApprovalWorkflowInstanceId = workflowInstance.Id;

                        switch (workflowTypeId)
                        {
                            case (short)WellKnownWorkflowTypes.InventoryDispatch:
                                businessDocument.Status = BusinessDocumentStatus.InProcess;
                                break;
                            case (short)WellKnownWorkflowTypes.InventoryReceipt:
                                businessDocument.Status = BusinessDocumentStatus.Control;
                                break;
                            default:
                                businessDocument.Status = BusinessDocumentStatus.PendingApproval;
                                break;
                        }

                        await Db.UpdateAsync(businessDocument);
                        trx.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        trx.Rollback();
                        throw;
                    }
                }
            }

            return false;
        }

        public async Task<object> Get(Api.QueryBusinessDocumentApprovals request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);
            var parameters = Request.GetRequestParams();

            var roleIdsQ = Session.Roles.Contains("admin") ?
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId).Select(x => x.Id) :
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId && Sql.In(w.Name, Session.Roles)).Select(x => x.Id);

            var q = _autoQuery.CreateQuery(request, parameters)
                .Join<BusinessDocument, BusinessDocumentType>((bd, bdt) => bd.TypeId == bdt.Id)
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id, Db.JoinAlias("Issuer"))
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.ReceiverId == p.Id, Db.JoinAlias("Receiver"))
                .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id)
                .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id)
                .Join<WorkflowInstance, Workflow>();
            q.Select<BusinessDocument, BusinessDocumentType, Domain.System.Persons.Person, WorkflowInstance, WorkflowActivity, Workflow>((bd, bdt, p, wi, wa, w) => new {
                bd.Id,
                bd.Number,
                bd.CreateDate,
                bd.DocumentDate,
                BusinessDocumentTypeName = bdt.Name,
                BusinessDocumentTypeShortName = bdt.ShortName,
                WorkflowCode = w.Code,
                bd.IssuerId,
                IssuerName = Sql.JoinAlias(p.Name, "Issuer"),
                bd.ReceiverId,
                ReceiverName = Sql.JoinAlias(p.Name, "Receiver"),
                WorkflowActivityId = wa.Id,
                WorkflowActivityIsFinal = wa.IsFinal,
                WorkflowActivityName = wa.Name,
                WorkflowInstanceId = wi.Id,
                WorkflowInstanceGuid = wi.Guid,
                WorkflowInstanceIsTerminated = wi.IsTerminated,
                WorkflowInstanceProgress = wi.Progress
            });

            SqlExpression<BusinessDocument> wiIdsQ = null;
            switch (request.View)
            {
                case 0: //Own
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wia) => wi.Id == wia.WorkflowInstanceId && wia.IsActive && Sql.In(wia.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 1: //Supervised
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIdsQ))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission == 2 && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 2: //Others
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIdsQ))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission < 2 && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 3: //Terminated
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive)
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 4: //Finished
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal)
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 5: //All
                    break;
            }

            if (wiIdsQ != null)
            {
                if (!string.IsNullOrEmpty(request.Q))
                {
                    wiIdsQ.WhereExpression += " (";
                    wiIdsQ.And<WorkflowActivity>(w => w.Name.Contains(request.Q));
                    wiIdsQ.UnsafeOr(
                        "BusinessDocuments.MessageThreadId IN (SELECT DISTINCT m.MessageThreadId FROM Messages m WHERE CONTAINS(m.Body, {0}))",
                        "\"" + request.Q + "*\"");
                    wiIdsQ.Or<Domain.System.Persons.Person>(w => w.Name.Contains(request.Q));
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

                switch (request.Module)
                {
                    case Api.BusinessDocumentModule.AccountsPayables:
                        // Filtrar los receivers entre el personId del Tenant y las personas habilitadas a ser facturadas por cuenta y orden

                        //Por cuenta y orden???
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        var receiverIds = new List<int>();
                        receiverIds.Add(tenant.PersonId);
                        wiIdsQ.And(w => Sql.In(w.ReceiverId, receiverIds));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;

                    case Api.BusinessDocumentModule.AccountsReceivables:
                        // Filtrar los issuers entre el personId del Tenant y las personas habilitadas a emitir facturas por cuenta y orden
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        var issuerIds = new List<int> { tenant.PersonId };
                        wiIdsQ.And(w => Sql.In(w.IssuerId, issuerIds));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                    case Api.BusinessDocumentModule.Inventory:
                        // Filtrar los issuers entre el personId del Tenant y las personas habilitadas a emitir facturas por cuenta y orden
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        wiIdsQ.And(w => Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                    default:
                        //Por cuenta y orden???
                        //var clients1 = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds1 = clients1.Select(x => x.PersonId).ToList();

                        var receiverIds1 = new List<int> { tenant.PersonId };
                        var issuerIds1 = new List<int> { tenant.PersonId };
                        wiIdsQ.And(w => Sql.In(w.ReceiverId, receiverIds1) || Sql.In(w.IssuerId, issuerIds1));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                }

                q.Where<WorkflowInstance>(w => Sql.In(w.Id, wiIdsQ));
            }

            if (request.Module == Api.BusinessDocumentModule.Inventory)
            {
                q.OrderByDescending(o => o.DocumentDate);
                q.ThenByDescending(o => o.Id);
            }
            var result = _autoQuery.Execute(request, q);

            var roleMap = await Db.SelectAsync<ServiceModel.System.Workflows.WorkflowInstanceRoleMap>(Db
                .From<WorkflowInstanceAssignments>()
                .Join<Role>()
                .Where(w => Sql.In(w.WorkflowInstanceId, wiIdsQ))
                .And(w => w.IsActive));
            result.Results.ForEach(x => x.Roles = string.Join(",", roleMap.Where(w => w.WorkflowInstanceId == x.WorkflowInstanceId).Select(w => w.RoleName)));
            return result;
        }

        public async Task<bool> Post(Api.PostBusinessDocumentSubmitForCollect request)
        {
            var businessDocument = await Db.SingleAsync<BusinessDocument>(w => w.Guid == request.BusinessDocumentGuid);
            if (businessDocument.ApprovalWorkflowInstanceId == null)
            {
                try
                {
                    var businessDocumentType = await Db.SingleByIdAsync<BusinessDocumentType>(businessDocument.TypeId);
                    // Tomo de workflowTypeId el que está asociado al businessdocumentType, en caso de que no lo tenga defaulteo al de Cobranza expedientes.
                    var workflowTypeId = businessDocumentType.ApprovalWorkflowTypeId ?? (short)WellKnownWorkflowTypes.Collection;
                    var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 0, workflowTypeId);
                    var workflowInstance = new WorkflowInstance
                    {
                        CreateDate = DateTime.UtcNow,
                        CreatedByUserId = Session.UserId,
                        WorkflowId = currentActivity.WorkflowId,
                        CurrentWorkflowActivityId = currentActivity.Id,
                        Guid = Guid.NewGuid()
                    };
                    workflowInstance.Id = (int)await Db.InsertAsync(workflowInstance, true);

                    foreach (var approvalRule in currentActivity.ApprovalRules)
                    {
                        var workflowInstanceApproval = new WorkflowInstanceApproval
                        {
                            WorkflowInstanceId = workflowInstance.Id,
                            RoleId = approvalRule.RoleId,
                            UserId = approvalRule.UserId,
                            WorkflowActivityId = approvalRule.WorkflowActivityId,
                            Status = WorkflowInstanceApprovalStatus.Pending,
                            CreateDate = DateTime.UtcNow
                        };
                        await Db.InsertAsync(workflowInstanceApproval);
                    }

                    //Current activity roles
                    var activityRoles = await Db.SelectAsync<WorkflowActivityRole>(w => w.WorkflowActivityId == currentActivity.Id);

                    //Assign workflowinstance roles
                    foreach (var activityRole in activityRoles.Where(w => w.IsDefault))
                    {
                        var workflowInstanceRole = new WorkflowInstanceAssignments
                        {
                            WorkflowInstanceId = workflowInstance.Id,
                            WorkflowActivityId = workflowInstance.CurrentWorkflowActivityId,
                            RoleId = activityRole.RoleId,
                            UserId = Session.UserId,
                            CreateDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        await Db.InsertAsync(workflowInstanceRole);
                    }

                    businessDocument.ApprovalWorkflowInstanceId = workflowInstance.Id;

                    switch (workflowTypeId)
                    {
                        case (short)WellKnownWorkflowTypes.InventoryDispatch:
                            businessDocument.Status = BusinessDocumentStatus.InProcess;
                            break;
                        case (short)WellKnownWorkflowTypes.InventoryReceipt:
                            businessDocument.Status = BusinessDocumentStatus.Control;
                            break;
                        default:
                            businessDocument.Status = BusinessDocumentStatus.PendingApproval;
                            break;
                    }

                    await Db.UpdateAsync(businessDocument);
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return false;
        }

        public async Task<object> Get(Api.QueryBusinessDocumentCollects request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);
            var parameters = Request.GetRequestParams();

            var roleIdsQ = Db.Column<int>(Session.Roles.Contains("admin") ?
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId).Select(x => x.Id) :
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId && Sql.In(w.Name, Session.Roles)));


            var q = _autoQuery.CreateQuery(request, parameters)
                .Join<BusinessDocument, BusinessDocumentType>((bd, bdt) => bd.TypeId == bdt.Id)
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id, Db.JoinAlias("Issuer"))
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.ReceiverId == p.Id, Db.JoinAlias("Receiver"))
                .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id)
                .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id)
                .Join<WorkflowInstance, Workflow>();
            q.Select<BusinessDocument, BusinessDocumentType, Domain.System.Persons.Person, WorkflowInstance, WorkflowActivity, Workflow>((bd, bdt, p, wi, wa, w) => new {
                bd.Id,
                bd.Number,
                bd.CreateDate,
                bd.DocumentDate,
                bd.Total,
                BusinessDocumentTypeName = bdt.Name,
                BusinessDocumentTypeShortName = bdt.ShortName,
                WorkflowCode = w.Code,
                bd.IssuerId,
                IssuerName = Sql.JoinAlias(p.Name, "Issuer"),
                bd.ReceiverId,
                ReceiverName = Sql.JoinAlias(p.Name, "Receiver"),
                WorkflowActivityId = wa.Id,
                WorkflowActivityIsFinal = wa.IsFinal,
                WorkflowActivityName = wa.Name,
                WorkflowInstanceId = wi.Id,
                WorkflowInstanceGuid = wi.Guid,
                WorkflowInstanceIsTerminated = wi.IsTerminated,
                WorkflowInstanceProgress = wi.Progress
            });

            SqlExpression<BusinessDocument> wiIdsQ = null;
            switch (request.View)
            {
                case 0: //Own
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wia) => wi.Id == wia.WorkflowInstanceId && wia.IsActive && Sql.In(wia.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 1: //Supervised
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIdsQ))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission == 2 && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 2: //Others
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIdsQ))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission < 2 && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 3: //Terminated
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive)
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 4: //Finished
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal)
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 5: //All
                    break;
            }

            if (wiIdsQ != null)
            {
                if (!string.IsNullOrEmpty(request.Q))
                {
                    wiIdsQ.WhereExpression += " (";
                    wiIdsQ.And<WorkflowActivity>(w => w.Name.Contains(request.Q));
                    wiIdsQ.UnsafeOr(
                        "BusinessDocuments.MessageThreadId IN (SELECT DISTINCT m.MessageThreadId FROM Messages m WHERE CONTAINS(m.Body, {0}))",
                        "\"" + request.Q + "*\"");
                    wiIdsQ.Or<Domain.System.Persons.Person>(w => w.Name.Contains(request.Q));
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

                switch (request.Module)
                {
                    case Api.BusinessDocumentModule.AccountsPayables:
                        // Filtrar los receivers entre el personId del Tenant y las personas habilitadas a ser facturadas por cuenta y orden

                        //Por cuenta y orden???
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        var receiverIds = new List<int>();
                        receiverIds.Add(tenant.PersonId);
                        wiIdsQ.And(w => Sql.In(w.ReceiverId, receiverIds));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;

                    case Api.BusinessDocumentModule.AccountsReceivables:
                        // Filtrar los issuers entre el personId del Tenant y las personas habilitadas a emitir facturas por cuenta y orden
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        var issuerIds = new List<int> { tenant.PersonId };
                        wiIdsQ.And(w => Sql.In(w.IssuerId, issuerIds));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                    case Api.BusinessDocumentModule.Inventory:
                        // Filtrar los issuers entre el personId del Tenant y las personas habilitadas a emitir facturas por cuenta y orden
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        wiIdsQ.And(w => Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                    case Api.BusinessDocumentModule.Collector:
                        wiIdsQ.And(w => Sql.In(w.TypeId, new List<int> { 22, 25 }));

                        break;
                    default:
                        //Por cuenta y orden???
                        //var clients1 = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds1 = clients1.Select(x => x.PersonId).ToList();

                        var receiverIds1 = new List<int> { tenant.PersonId };
                        var issuerIds1 = new List<int> { tenant.PersonId };
                        wiIdsQ.And(w => Sql.In(w.ReceiverId, receiverIds1) || Sql.In(w.IssuerId, issuerIds1));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                }

                q.Where<WorkflowInstance>(w => Sql.In(w.Id, wiIdsQ));
            }

            if (request.Module == Api.BusinessDocumentModule.Collector)
            {
                q.Where<BusinessDocument>(bd => Sql.In(bd.TypeId, new List<int> { 22, 25 }));
            }
            if (request.Module == Api.BusinessDocumentModule.Inventory)
            {
                q.OrderByDescending(o => o.DocumentDate);
                q.ThenByDescending(o => o.Id);
            }
            var result = _autoQuery.Execute(request, q);

            var roleMap = await Db.SelectAsync<ServiceModel.System.Workflows.WorkflowInstanceRoleMap>(Db
                .From<WorkflowInstanceAssignments>()
                .Join<Role>()
                .Where(w => Sql.In(w.WorkflowInstanceId, wiIdsQ))
                .And(w => w.IsActive));
            result.Results.ForEach(x => x.Roles = string.Join(",", roleMap.Where(w => w.WorkflowInstanceId == x.WorkflowInstanceId).Select(w => w.RoleName)));

            var businessDocumentIds = result.Results.ToList().Select(x => x.Id);
            

            return result;
        }

        public async Task<bool> Post(Api.PostBusinessDocumentSubmitForDebtCollect request)
        {
            var businessDocument = await Db.SingleAsync<BusinessDocument>(w => w.Guid == request.BusinessDocumentGuid);
            if (businessDocument.ApprovalWorkflowInstanceId == null)
            {
                try
                {
                    var businessDocumentType = await Db.SingleByIdAsync<BusinessDocumentType>(businessDocument.TypeId);
                    // Tomo de workflowTypeId el que está asociado al businessdocumentType, en caso de que no lo tenga defaulteo al de Cobranza expedientes.
                    var workflowTypeId = businessDocumentType.ApprovalWorkflowTypeId ?? (short)WellKnownWorkflowTypes.DebtCollection;
                    var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 0, workflowTypeId);
                    var workflowInstance = new WorkflowInstance
                    {
                        CreateDate = DateTime.UtcNow,
                        CreatedByUserId = Session.UserId,
                        WorkflowId = currentActivity.WorkflowId,
                        CurrentWorkflowActivityId = currentActivity.Id,
                        Guid = Guid.NewGuid()
                    };
                    workflowInstance.Id = (int)await Db.InsertAsync(workflowInstance, true);

                    foreach (var approvalRule in currentActivity.ApprovalRules)
                    {
                        var workflowInstanceApproval = new WorkflowInstanceApproval
                        {
                            WorkflowInstanceId = workflowInstance.Id,
                            RoleId = approvalRule.RoleId,
                            UserId = approvalRule.UserId,
                            WorkflowActivityId = approvalRule.WorkflowActivityId,
                            Status = WorkflowInstanceApprovalStatus.Pending,
                            CreateDate = DateTime.UtcNow
                        };
                        await Db.InsertAsync(workflowInstanceApproval);
                    }

                    //Current activity roles
                    var activityRoles = await Db.SelectAsync<WorkflowActivityRole>(w => w.WorkflowActivityId == currentActivity.Id);

                    //Assign workflowinstance roles
                    foreach (var activityRole in activityRoles.Where(w => w.IsDefault))
                    {
                        var workflowInstanceRole = new WorkflowInstanceAssignments
                        {
                            WorkflowInstanceId = workflowInstance.Id,
                            WorkflowActivityId = workflowInstance.CurrentWorkflowActivityId,
                            RoleId = activityRole.RoleId,
                            UserId = Session.UserId,
                            CreateDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        await Db.InsertAsync(workflowInstanceRole);
                    }

                    businessDocument.ApprovalWorkflowInstanceId = workflowInstance.Id;

                    switch (workflowTypeId)
                    {
                        case (short)WellKnownWorkflowTypes.InventoryDispatch:
                            businessDocument.Status = BusinessDocumentStatus.InProcess;
                            break;
                        case (short)WellKnownWorkflowTypes.InventoryReceipt:
                            businessDocument.Status = BusinessDocumentStatus.Control;
                            break;
                        default:
                            businessDocument.Status = BusinessDocumentStatus.PendingApproval;
                            break;
                    }

                    await Db.UpdateAsync(businessDocument);
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return false;
        }

        //public bool Get(Api.GetBusinessDocumentTesterPDF request)
        //{
        //    try
        //    {
        //        //var template = Db.SingleById<Domain.System.Notifications.EmailTemplate>(2);
        //        PdfDocument pdf = PdfGenerator.GeneratePdf("<p><h1>Hello World</h1>This is html rendered text</p>", PageSize.A4, 60);
        //        pdf.Save("documentoDemo.pdf");

        //        //var str = "<!DOCTYPE html><head><title>Page Title</title></head><h1>This is a heading</h1><p>This is a paragraph.</p>";
        //        //var response = HTMLToPDF(str);
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public Document HTMLToPDF(string HtmlData) //, string fullDestinyFilePath)
        //{

        //    //// step 1
        //    //Document document = new Document();
        //    //// step 2
        //    //var stream = new MemoryStream();
        //    //PdfWriter writer = PdfWriter.GetInstance(document, stream);
        //    //// step 3
        //    //document.Open();
        //    //// step 4
        //    //String str = "<html><head></head><body style=\"font-size:12.0pt; font-family:Times New Roman\">" +
        //    //        "<a href='http://www.rgagnon.com/howto.html'><b>Real's HowTo</b></a>" +
        //    //        "<h1>Show your support</h1>" +
        //    //        "<p>It DOES cost a lot to produce this site - in ISP storage and transfer fees</p>" +
        //    //        "<p>TEST POLSKICH ZNAKÓW: \u0104\u0105\u0106\u0107\u00d3\u00f3\u0141\u0142\u0179\u017a\u017b\u017c\u017d\u017e\u0118\u0119</p>" +
        //    //        "<hr/>" +
        //    //        "<p>the huge amounts of time it takes for one person to design and write the actual content.</p>" +
        //    //        "<p>If you feel that effort has been useful to you, perhaps you will consider giving something back?</p>" +
        //    //        "<p>Donate using PayPal\u017d</p>" +
        //    //        "<p>Contributions via PayPal are accepted in any amount</p>" +
        //    //        "<p><br/><table border='1'><tr><td>Java HowTo</td></tr><tr>" +
        //    //        "<td style='background-color:red;'>Javascript HowTo</td></tr>" +
        //    //        "<tr><td>Powerbuilder HowTo</td></tr></table></p>" +
        //    //        "</body></html>";

        //    //XMLWorkerHelper worker = XMLWorkerHelper.GetInstance();
        //    //var html = new StringReader(HtmlData);
        //    ////InputStream is = new ByteArrayInputStream(str.getBytes(StandardCharsets.UTF_8));
        //    //worker.ParseXHtml(writer, document, html);
        //    //// step 5
        //    //document.Close();

        //    //return document;





        //    //// do some additional cleansing to handle some scenarios that are out of control with the html data  
        //    //HtmlData = HtmlData.Replace("<br>", "<br />");

        //    //// create a stream that we can write to, in this case a MemoryStream  
        //    //using (var stream = new MemoryStream())
        //    //{
        //    //    // create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF  
        //    //    using (var document = new Document())
        //    //    {
        //    //        // create a writer that's bound to our PDF abstraction and our stream  
        //    //        using (var writer = PdfWriter.GetInstance(document, stream))
        //    //        {
        //    //            // open the document for writing  
        //    //            document.Open();

        //    //            // read html data to StringReader  
        //    //            using (var html = new StringReader(HtmlData))
        //    //            {
        //    //                XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, html);
        //    //            }

        //    //            // close document  
        //    //            document.Close();
        //    //            return document;
        //    //        }
        //    //    }
        //    //}



        //    //using (var stringReader = new StringReader(HtmlData))
        //    //{
        //    //    using (Document document = new Document())
        //    //    {
        //    //        var stream = new MemoryStream();
        //    //        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        //    //        document.Open();
        //    //        XMLWorkerHelper.GetInstance().ParseXHtml(
        //    //            writer, document, stringReader
        //    //        );
        //    //        return document;
        //    //    }
        //    //}


        //    //Document document = new Document();

        //    //PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(fullDestinyFilePath, FileMode.Create));

        //    //document.Open();
        //    //XMLWorkerHelper worker = XMLWorkerHelper.GetInstance();
        //    //InputStream is = new ByteArrayInputStream(html.getBytes(StandardCharsets.UTF_8));
        //    //worker.parseXHtml(writer, document, is, Charset.forName("UTF-8"));
        //    //// step 5
        //    //document.close();



        //    //StringWriter sw = new StringWriter();
        //    //sw.WriteLine(HtmlData);
        //    //StringReader sr = new StringReader(sw.ToString());
        //    //Document pdfDoc = new Document(PageSize.A4);

        //    //XMLWorkerHelper xMLWorkerHelper = XMLWorkerHelper.GetInstance();
        //    //var stream = new MemoryStream();
        //    //var fullDestinyFilePath = "C:/Users/Javi/Desktop/proyectos/Arvent/demo.pdf";

        //    //var writer = PdfWriter.GetInstance(pdfDoc, new FileStream(fullDestinyFilePath, FileMode.Create));
        //    //var stringReader = new StringReader(HtmlData);
        //    //xMLWorkerHelper.ParseXHtml(writer, pdfDoc, stringReader);
        //    //return pdfDoc;


        //    //byte[] pdf; // result will be here

        //    ////var cssText = File.ReadAllText(MapPath("~/css/test.css"));
        //    ////var html = File.ReadAllText(MapPath("~/css/test.html"));

        //    //using (var memoryStream = new MemoryStream())
        //    //{
        //    //    var document = new Document(PageSize.A4, 50, 50, 60, 60);
        //    //    var writer = PdfWriter.GetInstance(document, memoryStream);
        //    //    document.Open();

        //    //    using (var cssMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes("")))
        //    //    {
        //    //        using (var htmlMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(HtmlData)))
        //    //        {
        //    //            try
        //    //            {
        //    //                var stringReader = new StringReader(HtmlData);
        //    //                XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, stringReader);
        //    //                XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, htmlMemoryStream, cssMemoryStream);
        //    //            }
        //    //            catch(Exception ex)
        //    //            {
        //    //                throw ex;
        //    //            }

        //    //            XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, htmlMemoryStream, cssMemoryStream);
        //    //        }
        //    //    }

        //    //    document.Close();

        //    //    pdf = memoryStream.ToArray();
        //    //    return document;
        //    //}


        //    ////--------------------MEJOR FUNCIONAMIENTO
        //    //var template = Db.SingleById<Domain.System.Notifications.EmailTemplate>(2);
        //    //StringWriter sw = new StringWriter();
        //    //sw.WriteLine(template.Body.ToString());
        //    //StringReader sr = new StringReader(sw.ToString());
        //    //Document pdfDoc = new Document();
        //    //HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
        //    //var stream = new MemoryStream();
        //    //var fullDestinyFilePath = "C:/Users/Javi/Desktop/proyectos/Arvent/demo.pdf";
        //    //PdfWriter.GetInstance(pdfDoc, new FileStream(fullDestinyFilePath, FileMode.Create));
        //    //pdfDoc.Open();
        //    //htmlparser.Parse(sr);
        //    //pdfDoc.Close();
        //    //return pdfDoc;
        //    ////--------------------MEJOR FUNCIONAMIENTO

        //    //var template = Db.SingleById<Domain.System.Notifications.EmailTemplate>(2);
        //    ////using (var stringReader = new StringReader(template.Body))
        //    //using (var stringReader = new StringReader(HtmlData))
        //    //{
        //    //    using (Document document = new Document())
        //    //    {
        //    //        var memoryStream = new MemoryStream();
        //    //        PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
        //    //        document.Open();

        //    //        XMLWorkerHelper.GetInstance().ParseXHtml(
        //    //            writer, document, stringReader
        //    //        );
        //    //        return document;
        //    //    }
        //    //}


        //        Byte[] res = null;
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            var pdf = PdfGenerator.GeneratePdf(HtmlData, PdfSharp.PageSize.A4);
        //            pdf.Save(ms);
        //            res = ms.ToArray();
        //        }
        //        return new Document(); 
            
        //}


        public async Task<object> Get(Api.QueryBusinessDocumentDebtCollects request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }


            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);
            var parameters = Request.GetRequestParams();

            var roleIdsQ = Db.Column<int>(Session.Roles.Contains("admin") ?
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId).Select(x => x.Id) :
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId && Sql.In(w.Name, Session.Roles)));


            var q = _autoQuery.CreateQuery(request, parameters)
                .Join<BusinessDocument, BusinessDocumentType>((bd, bdt) => bd.TypeId == bdt.Id)
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id, Db.JoinAlias("Issuer"))
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.ReceiverId == p.Id, Db.JoinAlias("Receiver"))
                .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id)
                .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id)
                .Join<WorkflowInstance, Workflow>();
            q.Select<BusinessDocument, BusinessDocumentType, Domain.System.Persons.Person, WorkflowInstance, WorkflowActivity, Workflow>((bd, bdt, p, wi, wa, w) => new {
                bd.Id,
                bd.Number,
                bd.CreateDate,
                bd.DocumentDate,
                bd.VoidDate,
                bd.Total,
                BusinessDocumentTypeName = bdt.Name,
                BusinessDocumentTypeShortName = bdt.ShortName,
                WorkflowCode = w.Code,
                bd.IssuerId,
                IssuerName = Sql.JoinAlias(p.Name, "Issuer"),
                bd.ReceiverId,
                ReceiverName = Sql.JoinAlias(p.Name, "Receiver"),
                WorkflowActivityId = wa.Id,
                WorkflowActivityIsFinal = wa.IsFinal,
                WorkflowActivityName = wa.Name,
                WorkflowInstanceId = wi.Id,
                WorkflowInstanceGuid = wi.Guid,
                WorkflowInstanceIsTerminated = wi.IsTerminated,
                WorkflowInstanceProgress = wi.Progress
            });

            SqlExpression<BusinessDocument> wiIdsQ = null;
            switch (request.View)
            {
                case 0: //Own
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wia) => wi.Id == wia.WorkflowInstanceId && wia.IsActive && Sql.In(wia.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 1: //Supervised
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIdsQ))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission == 2 && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 2: //Others
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIdsQ))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission < 2 && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 3: //Terminated
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive)
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && Sql.In(warp.RoleId, roleIdsQ))
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 4: //Finished
                    wiIdsQ = Db.From<BusinessDocument>()
                        .Join<BusinessDocument, Domain.System.Persons.Person>((bd, p) => bd.IssuerId == p.Id)
                        .Join<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal)
                        .SelectDistinct(x => x.ApprovalWorkflowInstanceId);
                    break;
                case 5: //All
                    break;
            }

            if (wiIdsQ != null)
            {
                if (!string.IsNullOrEmpty(request.Q))
                {
                    wiIdsQ.WhereExpression += " (";
                    wiIdsQ.And<WorkflowActivity>(w => w.Name.Contains(request.Q));
                    wiIdsQ.UnsafeOr(
                        "BusinessDocuments.MessageThreadId IN (SELECT DISTINCT m.MessageThreadId FROM Messages m WHERE CONTAINS(m.Body, {0}))",
                        "\"" + request.Q + "*\"");
                    wiIdsQ.Or<Domain.System.Persons.Person>(w => w.Name.Contains(request.Q));
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

                switch (request.Module)
                {
                    case Api.BusinessDocumentModule.AccountsPayables:
                        // Filtrar los receivers entre el personId del Tenant y las personas habilitadas a ser facturadas por cuenta y orden

                        //Por cuenta y orden???
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        var receiverIds = new List<int>();
                        receiverIds.Add(tenant.PersonId);
                        wiIdsQ.And(w => Sql.In(w.ReceiverId, receiverIds));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;

                    case Api.BusinessDocumentModule.AccountsReceivables:
                        // Filtrar los issuers entre el personId del Tenant y las personas habilitadas a emitir facturas por cuenta y orden
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        var issuerIds = new List<int> { tenant.PersonId };
                        wiIdsQ.And(w => Sql.In(w.IssuerId, issuerIds));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                    case Api.BusinessDocumentModule.Inventory:
                        // Filtrar los issuers entre el personId del Tenant y las personas habilitadas a emitir facturas por cuenta y orden
                        //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds = clients.Select(x => x.PersonId).ToList();
                        wiIdsQ.And(w => Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                    case Api.BusinessDocumentModule.Collector:
                        wiIdsQ.And(w => Sql.In(w.TypeId, new List<int> { 22, 25 }));
                        break;
                    case Api.BusinessDocumentModule.DebtCollector:
                        wiIdsQ.And(w => Sql.In(w.TypeId, new List<int> { 23, 28, 30, 31 }));
                        q.Where(w => Sql.In(w.TypeId, new List<int> { 23, 28, 30, 31 }));
                        break;

                    default:
                        //Por cuenta y orden???
                        //var clients1 = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                        //var receiverIds1 = clients1.Select(x => x.PersonId).ToList();

                        var receiverIds1 = new List<int> { tenant.PersonId };
                        var issuerIds1 = new List<int> { tenant.PersonId };
                        wiIdsQ.And(w => Sql.In(w.ReceiverId, receiverIds1) || Sql.In(w.IssuerId, issuerIds1));
                        wiIdsQ.And(w => !Sql.In(w.TypeId, new List<int> { 20, 21 }));
                        break;
                }

                q.Where<WorkflowInstance>(w => Sql.In(w.Id, wiIdsQ));
            }

            if (request.Module == Api.BusinessDocumentModule.Inventory)
            {
                q.OrderByDescending(o => o.DocumentDate);
                q.ThenByDescending(o => o.Id);
            }
            var result = _autoQuery.Execute(request, q);

            var roleMap = await Db.SelectAsync<ServiceModel.System.Workflows.WorkflowInstanceRoleMap>(Db
                .From<WorkflowInstanceAssignments>()
                .Join<Role>()
                .Where(w => Sql.In(w.WorkflowInstanceId, wiIdsQ))
                .And(w => w.IsActive));
            result.Results.ForEach(x => x.Roles = string.Join(",", roleMap.Where(w => w.WorkflowInstanceId == x.WorkflowInstanceId).Select(w => w.RoleName)));

            foreach (var res in result.Results)
            {
                var businessDocumentItemsIds = Db.Select(Db.From<BusinessDocumentItem>().Where(x => x.BusinessDocumentId == res.Id)).ToList().Select(x => x.Id);
                var debtors = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(x => Sql.In(x.BusinessDocumentItemId, businessDocumentItemsIds))).ToList().Select(x => x.DebtorId).Distinct();
                if (debtors.Count() > 0)
                {
                    var debtor = Db.SingleById<Domain.Financials.DebtManagement.Debtor>(debtors.First());
                    var debtorPerson = Db.SingleById<Domain.System.Persons.Person>(debtor.PersonId);
                    res.DebtorName = debtors.Count() > 1 ? debtorPerson.Name + " y otros" : debtorPerson.Name;
                }


            }

            return result;
        }

        public async Task<object> Put(Api.PostBusinessDocument request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.Total = request.Items.Sum(x => (x.Quantity * x.UnitPrice) + (x.Quantity * x.UnitPrice * x.VatRate / 100));
                    await Db.UpdateAsync((BusinessDocument)request);
                    await Save(request);
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

        public object Post(Api.PostBusinessDocumentBatch request)
        {
            if (Log.IsDebugEnabled) Log.DebugFormat("BD:PostBatch begin ({0}).", request.Count);

            /*
            var key = "pr:ve:" + this.Session.Tenant.Id;
            Cache.Remove(key);

            var vendors = this.Cache.GetBusinessDocumentMessage<List<Domain.Procurement.Vendor>>(key);
            if (vendors == null)
            {
                vendors =
                    Db.Select(Db.From<Domain.Procurement.Vendor>().Where(w => w.TenantId == this.Session.Tenant.Id));
                Cache.Set(key, vendors, TimeSpan.FromMinutes(5));
            }

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    foreach (var item in request)
                    {
                        item.Vendor = this.SaveVendor(item.Vendor);
                        item.ImportLog.TargetId = item.Vendor.Id;
                        item.ImportLog.LastActivity = DateTime.UtcNow;
                    }

                    //Insert import logs
                    Db.InsertAll(request.Select(x => x.ImportLog));

                    trx.Commit();
                    if (Log.IsDebugEnabled) Log.DebugFormat("BD:PostBatch completed.", request.Count);
                    return true;
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw ex;
                }
            }
            */

            return true;
        }

        public async Task<object> Post(Api.PostBusinessDocument request)
        {
            request.CreateDate = DateTime.UtcNow;
            request.CreatedBy = Session.UserId;

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = await Db.SelectAsync(Db.From<BusinessDocument>().Where(w => w.IssuerId == request.IssuerId && w.ReceiverId == request.ReceiverId && w.Number == request.Number));
                    if (existing.Count > 0)
                    {
                        trx.Rollback();
                        return HttpError.Conflict("ERR_BusinessDocument_AlreadyExists");
                    }
                    request.Guid = Guid.NewGuid();
                    request.Total = request.Items.Sum(x => (x.Quantity * x.UnitPrice) + (x.Quantity * x.UnitPrice * x.VatRate / 100));
                    request.Id = (int)await Db.InsertAsync((BusinessDocument)request, true);
                    await Save(request);
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

        private async Task Save(Api.PostBusinessDocument request)
        {
            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);
            var currentItems = await Db.SelectAsync<BusinessDocumentItem>(w => w.BusinessDocumentId == request.Id);
            var idsToDelete = currentItems.Select(x => x.Id).Except(request.Items.Where(w => w.Id != 0).Select(s => s.Id));
            var businessDocumentType = await Db.SingleByIdAsync<BusinessDocumentType>(request.TypeId);
            Db.DeleteByIds<BusinessDocumentItem>(idsToDelete);

            foreach (var item in request.Items)
            {
                if (item.Id == 0)
                {
                    item.BusinessDocumentId = request.Id;
                    item.Id = (int)await Db.InsertAsync(item, true);
                }
                else
                {
                    await Db.UpdateAsync(item);
                }

            }

            // Cuenta corriente - La actualizo sólo para facturas. Remitos no mueve la cuenta, asi que por ahora deshabilito el movimiento de cuenta con este if, hay que pasar esto a parametros del BusinessDocumentType
            if (businessDocumentType.ShortName != "RME" && businessDocumentType.ShortName != "RMS")
            {
                // Cuenta corriente - La actualizo sólo para facturas. Remitos no mueve la cuenta, asi que por ahora deshabilito el movimiento de cuenta con este if, hay que pasar esto a parametros del BusinessDocumentType
                if (businessDocumentType.ShortName == "EXP" || businessDocumentType.ShortName == "EXP GDEBA"
                    || businessDocumentType.ShortName == "NAD" || businessDocumentType.ShortName == "TE")
                {
                    BusinessPartner businessPartnerReceiver = null;
                    BusinessPartner businessPartnerIssuer = null;

                    var receiver = Db.SingleById<Domain.Financials.DebtManagement.Debtor>(request.ReceiverId);
                    var issuer = Db.SingleById<Domain.Financials.DebtManagement.Organism>(request.IssuerId);

                    businessPartnerReceiver = (await Db.SelectAsync(Db.From<BusinessPartner>()
                        .Where(w => (w.TypeId == 8 || w.TypeId == 10) && w.TenantId == tenant.Id && w.PersonId == receiver.PersonId)))
                        .SingleOrDefault();


                    businessPartnerIssuer = (await Db.SelectAsync(Db.From<BusinessPartner>()
                        .Where(w => (w.TypeId == 9 || w.TypeId == 11) && w.TenantId == Session.TenantId && w.PersonId == issuer.PersonId)))
                        .SingleOrDefault();

                    if (businessPartnerReceiver != null)
                    {
                        var accountReceiver = (await _businessPartnerRepository.GetBusinessPartner(Db, businessPartnerReceiver.Id, true))
                                                .Accounts.Items.OrderBy(o => o.Id)
                                                .FirstOrDefault();

                        if (accountReceiver != null)
                        {
                            var accountEntry = (await Db.SelectAsync(Db.From<BusinessPartnerAccountEntry>().
                                                       Where(w => w.LinkedDocumentTypeId == request.TypeId && w.LinkedDocumentId == request.Id &&
                                                       w.AccountId == accountReceiver.Id))).SingleOrDefault();

                            //partner type 1 = sales
                            //partner type 2 = procurement
                            decimal amount = 0;
                            var description = "";
                            switch (businessPartnerReceiver.TypeId)
                            {
                                case 1: //sales
                                    amount = request.Total * -1;
                                    description = "Factura de venta nro. " + request.Number;
                                    break;
                                case 2: //procurement
                                    amount = request.Total;
                                    description = "Factura de compra nro. " + request.Number;
                                    break;
                            }

                            if (accountEntry == null)
                            {
                                accountEntry = new BusinessPartnerAccountEntry
                                {
                                    AccountId = accountReceiver.Id,
                                    Amount = amount,
                                    Description = description,
                                    CreateDate = DateTime.UtcNow,
                                    LinkedDocumentId = request.Id,
                                    LinkedDocumentTypeId = request.TypeId,
                                    PostingDate = DateTime.UtcNow
                                };
                                Db.Insert(accountEntry);
                            }
                            else
                            {
                                accountEntry.Amount = amount;
                                accountEntry.Description = description;
                                await Db.UpdateAsync(accountEntry);
                            }
                        }
                    }

                    if (businessPartnerIssuer != null)
                    {
                        var accountIssuer = (await _businessPartnerRepository.GetBusinessPartner(Db, businessPartnerIssuer.Id, true))
                                            .Accounts.Items.OrderBy(o => o.Id)
                                            .FirstOrDefault();

                        if (accountIssuer != null)
                        {
                            var accountEntry = (await Db.SelectAsync(Db.From<BusinessPartnerAccountEntry>().
                                                       Where(w => w.LinkedDocumentTypeId == request.TypeId && w.LinkedDocumentId == request.Id &&
                                                       w.AccountId == accountIssuer.Id))).SingleOrDefault();

                            //partner type 1 = sales
                            //partner type 2 = procurement
                            decimal amount = 0;
                            var description = "";
                            switch (businessPartnerIssuer.TypeId)
                            {
                                case 1: //sales
                                    amount = request.Total * -1;
                                    description = "Factura de venta nro. " + request.Number;
                                    break;
                                case 2: //procurement
                                    amount = request.Total;
                                    description = "Factura de compra nro. " + request.Number;
                                    break;
                            }

                            if (accountEntry == null)
                            {
                                accountEntry = new BusinessPartnerAccountEntry
                                {
                                    AccountId = accountIssuer.Id,
                                    Amount = amount,
                                    Description = description,
                                    CreateDate = DateTime.UtcNow,
                                    LinkedDocumentId = request.Id,
                                    LinkedDocumentTypeId = request.TypeId,
                                    PostingDate = DateTime.UtcNow
                                };
                                Db.Insert(accountEntry);
                            }
                            else
                            {
                                accountEntry.Amount = amount;
                                accountEntry.Description = description;
                                await Db.UpdateAsync(accountEntry);
                            }
                        }
                    }
                }
                else
                {
                    BusinessPartner businessPartner = null;
                    if (request.ReceiverId != tenant.PersonId)
                    {
                        businessPartner = (await Db.SelectAsync(Db.From<BusinessPartner>()
                            .Where(w => w.TypeId == 1 && w.TenantId == tenant.Id && w.PersonId == request.ReceiverId)))
                            .SingleOrDefault();
                    }
                    else if (request.IssuerId != tenant.PersonId)
                    {
                        businessPartner = (await Db.SelectAsync(Db.From<BusinessPartner>()
                            .Where(w => w.TypeId == 2 && w.TenantId == Session.TenantId && w.PersonId == request.IssuerId)))
                            .SingleOrDefault();
                    }

                    var account = (await _businessPartnerRepository.GetBusinessPartner(Db, businessPartner.Id, true))
                                            .Accounts.Items.OrderBy(o => o.Id)
                                            .FirstOrDefault();
                    if (account != null)
                    {
                        var accountEntry = (await Db.SelectAsync(Db.From<BusinessPartnerAccountEntry>().Where(w => w.LinkedDocumentTypeId == request.TypeId && w.LinkedDocumentId == request.Id && w.AccountId == account.Id))).SingleOrDefault();
                        //partner type 1 = sales
                        //partner type 2 = procurement
                        decimal amount = 0;
                        var description = "";
                        switch (businessPartner.TypeId)
                        {
                            case 1: //sales
                                amount = request.Total * -1;
                                description = "Factura de venta nro. " + request.Number;
                                break;
                            case 2: //procurement
                                amount = request.Total;
                                description = "Factura de compra nro. " + request.Number;
                                break;
                        }

                        if (accountEntry == null)
                        {
                            accountEntry = new BusinessPartnerAccountEntry
                            {
                                AccountId = account.Id,
                                Amount = amount,
                                Description = description,
                                CreateDate = DateTime.UtcNow,
                                LinkedDocumentId = request.Id,
                                LinkedDocumentTypeId = request.TypeId,
                                PostingDate = DateTime.UtcNow
                            };
                            Db.Insert(accountEntry);
                        }
                        else
                        {
                            accountEntry.Amount = amount;
                            accountEntry.Description = description;
                            await Db.UpdateAsync(accountEntry);
                        }
                    }
                }
            }
        }

        public async Task<object> Get(Api.GetBusinessDocument request)
        {
            var document = (await Db.SingleByIdAsync<BusinessDocument>(request.Id)).ConvertTo<Api.BusinessDocument>();
            var items = await Db.SelectAsync<BusinessDocumentItem>(w => w.BusinessDocumentId == document.Id);

            List<Product> products = null;
            var siteIds = new List<int>();
            if (items.Count > 0)
            {
                var productIds = items.Select(x => x.ProductId);
                products = await Db.SelectAsync<Product>(w => Sql.In(w.Id, productIds));

                siteIds.AddRange(items.Where(w => w.InventorySiteId.HasValue).Select(x => x.InventorySiteId.Value).Distinct());
            }
            if (document.InventorySiteId.HasValue)
            {
                siteIds.AddIfNotExists(document.InventorySiteId.Value);
            }

            var sites = await Db.SelectAsync<InventorySite>(w => Sql.In(w.Id, siteIds));
            foreach (var item in items)
            {
                var itemModel = item.ConvertTo<Api.BusinessDocumentItem>();
                if (!request.Edit && products != null)
                {
                    itemModel.Product = products.Single(x => x.Id == itemModel.ProductId);
                }
                if (!request.Edit && sites != null && itemModel.InventorySiteId != null)
                {
                    itemModel.Site = sites.Single(x => x.Id == itemModel.InventorySiteId);
                }
                document.Items.Add(itemModel);
            }

            if (!request.Edit)
            {
                // Issuer
                document.Issuer = await _personRepository.GetPerson(Db, document.IssuerId);

                // Receiver
                document.Receiver = await _personRepository.GetPerson(Db, document.ReceiverId);

                if (document.DispatcherId.HasValue)
                {
                    document.Dispatcher = await _personRepository.GetPerson(Db, document.DispatcherId.Value);
                }

                if (document.InventorySiteId != null)
                {
                    document.Site = sites.Single(x => x.Id == document.InventorySiteId);
                }

            }

            // ApprovalWorkflowInstance
            if (document.ApprovalWorkflowInstanceId.HasValue)
            {
                var workflowInstance = await _workflowInstanceRepository.GetWorkflowInstance(Db, Session, document.ApprovalWorkflowInstanceId.Value);
                document.ApprovalWorkflowInstance = workflowInstance;
            }

            return document;
        }


        public async Task<object> Get(Api.GetBusinessDocumentCollect request)
        {
            return  await GetBusinessDocumentCollect(request);
        }

        public async Task<object> Get(Api.GetBusinessDocumentCollectExecute request)
        {
            return await GetBusinessDocumentCollectExecute(request);
        }
        public async Task<object> Get(Api.QueryBusinessDocuments request)
        {
            try
            {
                var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);
                string orderBy = null;
                if (request.OrderBy == null && request.OrderByDesc == null)
                {
                    request.OrderByDesc = "Id";
                }
                else
                {
                    request.OrderBy = request.OrderBy?.Replace("Contains", string.Empty).Replace("issuerName", "issuer.name").Replace("receiverName", "receiver.name");
                    request.OrderByDesc = request.OrderByDesc?.Replace("Contains", string.Empty).Replace("issuerName", "issuer.name").Replace("receiverName", "receiver.name");
                }

                if (request.OrderBy == "issuer.name" || request.OrderBy == "receiver.name")
                {
                    orderBy = request.OrderBy;
                    request.OrderBy = null;
                }

                if (request.OrderByDesc == "issuer.name" || request.OrderByDesc == "receiver.name")
                {
                    orderBy = request.OrderByDesc + " DESC";
                    request.OrderByDesc = null;
                }

                var p = Request.GetRequestParams();
                var q = _autoQuery.CreateQuery(request, p);
                q.Join<BusinessDocument, BusinessDocumentType>((bd, i) => bd.TypeId == i.Id)
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, i) => bd.IssuerId == i.Id, Db.JoinAlias("issuer"))
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, r) => bd.ReceiverId == r.Id, Db.JoinAlias("receiver"))
                .Join<BusinessDocument, Domain.Catalog.Category>((bd, s) => bd.CategoryId == s.Id, Db.JoinAlias("category"))
                .LeftJoin<BusinessDocument, WorkflowInstance>((bd, wi) => bd.ApprovalWorkflowInstanceId == wi.Id)
                .LeftJoin<WorkflowInstance, WorkflowInstanceAssignments>((wi, wa) => wi.Id == wa.WorkflowInstanceId && wa.IsActive == true)
                .LeftJoin<WorkflowInstance, WorkflowActivity>((wi,wact) => wi.CurrentWorkflowActivityId == wact.Id, Db.JoinAlias("activity"))
                .Select(@"BusinessDocuments.*
                , BusinessDocumentTypes.ShortName AS TypeName
                , BusinessDocumentTypes.Code AS TypeCode
                , issuer.Name AS IssuerName
                , receiver.Name AS ReceiverName
                , category.Name AS CategoryName
                , activity.Name AS WorkflowActivityName");

                var invTypes = Db.Select(Db.From<BusinessDocumentType>().Where(w => w.ShortName == "RME" || w.ShortName == "RMS"));
                var invTypeIds = invTypes.Select(x => x.Id).ToList();

                var collTypes = Db.Select(Db.From<BusinessDocumentType>().Where(w => w.CollectionDocument == 1));
                var collTypeIds = collTypes.Select(x => x.Id).ToList();

                switch (request.Module)
                {
                    case Api.BusinessDocumentModule.Collector:
                        q.And(w => Sql.In(w.TypeId, collTypeIds));
                        if (request.RoleId != default(int))
                        {
                            q.And<WorkflowInstanceAssignments>(wa => wa.RoleId == request.RoleId);
                        }
                        break;

                    case Api.BusinessDocumentModule.AccountsPayables:
                        // Filtrar los receivers entre el personId del Tenant y las personas habilitadas a ser facturadas por cuenta y orden
                        //Por cuenta y orden???
                        var receiverIds = new List<int>();
                        receiverIds.Add(tenant.PersonId);
                        q.And(w => Sql.In(w.ReceiverId, receiverIds));
                        q.And(w => !Sql.In(w.TypeId, invTypeIds));
                        break;

                    case Api.BusinessDocumentModule.AccountsReceivables:
                        q.And(w => w.IssuerId == tenant.PersonId);
                        q.And(w => !Sql.In(w.TypeId, invTypeIds));
                        break;
                    case Api.BusinessDocumentModule.Inventory:
                        q.And(w => Sql.In(w.TypeId, invTypeIds));
                        q.And(w => w.IssuerId == tenant.PersonId || w.ReceiverId == tenant.PersonId);
                        break;
                    default:
                        //Por cuenta y orden???
                        var receiverIds1 = new List<int>();
                        receiverIds1.Add(tenant.PersonId);
                        var issuerIds1 = new List<int>();
                        issuerIds1.Add(tenant.PersonId);
                        q.And(w => Sql.In(w.ReceiverId, receiverIds1) || Sql.In(w.IssuerId, issuerIds1));
                        q.And(w => !Sql.In(w.TypeId, invTypeIds));
                        break;
                }

                if (p.ContainsKey("typeNameContains"))
                {
                    q.And<BusinessDocumentType>(w => w.ShortName.Contains(p["typeNameContains"]));
                }

                if (p.ContainsKey("issuerNameContains"))
                {
                    q.UnsafeWhere("issuer.Name LIKE {0}", Utils.SqlLike(p["issuerNameContains"]));
                }

                if (p.ContainsKey("receiverNameContains"))
                {
                    q.UnsafeWhere("receiver.Name LIKE {0}", Utils.SqlLike(p["receiverNameContains"]));
                }

                //ESTO ES LO QUE NO ME ANDA, CUANDO AGREGA ESTE WHERE SE ROMPE EL EXECUTE, PASA SI PASA POR EL PARAMETRO DE TYPENAME DE ARRIBA TAMBIEN.
                if (p.ContainsKey("typeCode"))
                {
                    q.UnsafeWhere("BusinessDocumentTypes.ShortName LIKE {0}", Utils.SqlLike(p["typeCode"]));
                }

                var count = Db.Count(q);
                if (count > 0)
                {
                    request.Include = "Sum(Total) total";
                }

                if(orderBy != null)
                {
                    q.OrderByExpression = orderBy;
                }

                if (request.Status != null)
                {
                    switch (request.Status)
                    {
                        case 20:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.Secretary);
                            break;
                        case 30:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.DDDR);
                            break;
                        case 40:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.DCEO);
                            break;
                        case 50:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.DGJ);
                            break;
                        case 60:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.Prosecution);
                            break;
                    }


                }
                //q.Where<BusinessDocument>(bd => bd.Status == request.Status);
                 
                var model = _autoQuery.Execute(request, q);
                var roleMap = Db.Select<ServiceModel.System.Workflows.WorkflowInstanceRoleMap>(Db
                    .From<WorkflowInstanceAssignments>()
                    .Join<Role>()
                    .Where(w => Sql.In(w.WorkflowInstanceId, q.Where(sw => sw.ApprovalWorkflowInstanceId.HasValue).SelectDistinct(x => x.ApprovalWorkflowInstanceId)))
                    .And(w => w.IsActive));
                model.Results.ForEach(x =>
                {
                    if (x.ApprovalWorkflowInstanceId.HasValue)
                    {
                        x.Roles = string.Join(",",
                            roleMap.Where(w => w.WorkflowInstanceId == x.ApprovalWorkflowInstanceId.Value)
                            .Select(w => w.RoleName));
                    }
                });
                return model;
            }
            catch(Exception ex)
            {
                throw new ApplicationException(Db.GetLastSql(), ex);
            }
        }

        public async Task<object> Get(Api.QueryBusinessDocumentNads request)
        {
            try
            {
                
                var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);

            /*    string orderBy = null;
                if (request.OrderBy == null && request.OrderByDesc == null)
                {
                    request.OrderByDesc = "Id";
                }
                else
                {
                    request.OrderBy = request.OrderBy?.Replace("Contains", string.Empty).Replace("issuerName", "issuer.name").Replace("receiverName", "receiver.name");
                    request.OrderByDesc = request.OrderByDesc?.Replace("Contains", string.Empty).Replace("issuerName", "issuer.name").Replace("receiverName", "receiver.name");
                }
           
                if (request.OrderBy == "issuer.name" || request.OrderBy == "receiver.name")
                {
                    orderBy = request.OrderBy;
                    request.OrderBy = null;
                }
               
                if (request.OrderByDesc == "issuer.name" || request.OrderByDesc == "receiver.name")
                {
                    orderBy = request.OrderByDesc + " DESC";
                    request.OrderByDesc = null;
                }
                 */
                var p = Request.GetRequestParams();
                var q = _autoQuery.CreateQuery(request, p);
                q.Join<BusinessDocument, BusinessDocumentType>((bd, i) => bd.TypeId == i.Id)
                //.Join<BusinessDocument, Domain.System.Persons.Person>((bd, i) => bd.IssuerId == i.Id, Db.JoinAlias("issuer"))
                .Join<BusinessDocument, Domain.System.Persons.Person>((bd, r) => bd.ReceiverId == r.Id, Db.JoinAlias("receiver"))
                //.Join<BusinessDocument, Domain.Catalog.Category>((bd, s) => bd.CategoryId == s.Id, Db.JoinAlias("category"))
                .Select(@"BusinessDocuments.*
                , BusinessDocumentTypes.ShortName AS TypeName
                , BusinessDocumentTypes.Code AS TypeCode                
                , receiver.Name AS ReceiverName");

                var invTypes = Db.Select(Db.From<BusinessDocumentType>().Where(w => w.ShortName == "RME" || w.ShortName == "RMS"));
                var invTypeIds = invTypes.Select(x => x.Id).ToList();

                var collTypes = Db.Select(Db.From<BusinessDocumentType>().Where(w => w.CollectionDocument == 1));
                var collTypeIds = collTypes.Select(x => x.Id).ToList();

                switch (request.Module)
                {
                    case Api.BusinessDocumentModule.Collector:
                        q.And(w => Sql.In(w.TypeId, collTypeIds));
                        break;

                    case Api.BusinessDocumentModule.AccountsPayables:
                        // Filtrar los receivers entre el personId del Tenant y las personas habilitadas a ser facturadas por cuenta y orden
                        //Por cuenta y orden???
                        var receiverIds = new List<int>();
                        receiverIds.Add(tenant.PersonId);
                        q.And(w => Sql.In(w.ReceiverId, receiverIds));
                        q.And(w => !Sql.In(w.TypeId, invTypeIds));
                        break;

                    case Api.BusinessDocumentModule.AccountsReceivables:
                        q.And(w => w.IssuerId == tenant.PersonId);
                        q.And(w => !Sql.In(w.TypeId, invTypeIds));
                        break;
                    case Api.BusinessDocumentModule.Inventory:
                        q.And(w => Sql.In(w.TypeId, invTypeIds));
                        q.And(w => w.IssuerId == tenant.PersonId || w.ReceiverId == tenant.PersonId);
                        break;
                    default:
                        //Por cuenta y orden???
                        var receiverIds1 = new List<int>();
                        receiverIds1.Add(tenant.PersonId);
                        var issuerIds1 = new List<int>();
                        issuerIds1.Add(tenant.PersonId);
                        q.And(w => Sql.In(w.ReceiverId, receiverIds1) || Sql.In(w.IssuerId, issuerIds1));
                        q.And(w => !Sql.In(w.TypeId, invTypeIds));
                        break;
                }

                if (p.ContainsKey("typeNameContains"))
                {
                    q.And<BusinessDocumentType>(w => w.ShortName.Contains(p["typeNameContains"]));
                }
                /*
                if (p.ContainsKey("issuerNameContains"))
                {
                    q.UnsafeWhere("issuer.Name LIKE {0}", Utils.SqlLike(p["issuerNameContains"]));
                }
                */
                if (p.ContainsKey("receiverNameContains"))
                {
                    q.UnsafeWhere("receiver.Name LIKE {0}", Utils.SqlLike(p["receiverNameContains"]));
                }

                //ESTO ES LO QUE NO ME ANDA, CUANDO AGREGA ESTE WHERE SE ROMPE EL EXECUTE, PASA SI PASA POR EL PARAMETRO DE TYPENAME DE ARRIBA TAMBIEN.
                if (p.ContainsKey("typeCode"))
                {
                    q.UnsafeWhere("BusinessDocumentTypes.ShortName LIKE {0}", Utils.SqlLike(p["typeCode"]));
                }

                var count = Db.Count(q);
                if (count > 0)
                {
                    request.Include = "Sum(Total) total";
                }
                /*
                if (orderBy != null)
                {
                    q.OrderByExpression = orderBy;
                }
                */
                if (request.Status != null)
                {
                    switch (request.Status)
                    {
                        case 20:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.Secretary);
                            break;
                        case 30:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.DDDR);
                            break;
                        case 40:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.DCEO);
                            break;
                        case 50:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.DGJ);
                            break;
                        case 60:
                            q.Where<BusinessDocument>(bd => bd.Status == BusinessDocumentStatus.Prosecution);
                            break;
                    }


                }
                //q.Where<BusinessDocument>(bd => bd.Status == request.Status);

                var model = _autoQuery.Execute(request, q);
                var roleMap = Db.Select<ServiceModel.System.Workflows.WorkflowInstanceRoleMap>(Db
                    .From<WorkflowInstanceAssignments>()
                    .Join<Role>()
                    .Where(w => Sql.In(w.WorkflowInstanceId, q.Where(sw => sw.ApprovalWorkflowInstanceId.HasValue).SelectDistinct(x => x.ApprovalWorkflowInstanceId)))
                    .And(w => w.IsActive));
                model.Results.ForEach(x =>
                {
                    if (x.ApprovalWorkflowInstanceId.HasValue)
                    {
                        x.Roles = string.Join(",",
                            roleMap.Where(w => w.WorkflowInstanceId == x.ApprovalWorkflowInstanceId.Value)
                            .Select(w => w.RoleName));
                    }
                });
                return model;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(Db.GetLastSql(), ex);
            }
        }

        //provisorio hasta terminar la tarea. Javier.enero 2017
        public async Task<object> Get(Api.QueryBusinessDocument1 request)
        {
            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);

            request.Include = "Sum(Total) total";
            //request.Include = "COUNT(*)";
            if (request.OrderByDesc == null)
            { 
                request.OrderByDesc = "Id";
            }

            var p = Request.GetRequestParams();
            if (p.ContainsKey("number"))
            {
                p.Add("numberContains", p["number"]);
                p.Remove("number");
            }

            var q = _autoQuery.CreateQuery(request, p);
            q.Join<BusinessDocument, BusinessDocumentType>((bd, i) => bd.TypeId == i.Id, Db.JoinAlias("bdt"))
            .Join<BusinessDocument, Domain.System.Persons.Person>((bd, i) => bd.IssuerId == i.Id, Db.JoinAlias("issuer"))
            .Join<BusinessDocument, Domain.System.Persons.Person>((bd, r) => bd.ReceiverId == r.Id, Db.JoinAlias("receiver"))            
            .Select(@"BusinessDocuments.*
                , bdt.ShortName AS TypeName
                , bdt.Code AS TypeCode
                , issuer.Name AS IssuerName
                , receiver.Name AS ReceiverName");

            switch (request.Module)
            {
                case Api.BusinessDocumentModule.AccountsPayables:
                    // Filtrar los receivers entre el personId del Tenant y las personas habilitadas a ser facturadas por cuenta y orden

                    //Por cuenta y orden???
                    //var clients = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                    //var receiverIds = clients.Select(x => x.PersonId).ToList();

                    var receiverIds = new List<int>();
                    receiverIds.Add(tenant.PersonId);
                    q.And(w => Sql.In(w.ReceiverId, receiverIds));
                    break;

                case Api.BusinessDocumentModule.AccountsReceivables:
                    // Filtrar los issuers entre el personId del Tenant y las personas habilitadas a emitir facturas por cuenta y orden
                    var clients = Db.Select(Db.From<BusinessPartner>().Where(w => w.TypeId == 1 && w.TenantId == tenant.Id));
                    //var receiverIds = clients.Select(x => x.PersonId).ToList();
                    //var issuerIds = new List<int>();
                    var issuerIds = clients.Select(x => x.PersonId).ToList();
                    issuerIds.Add(tenant.PersonId);
                    q.And(w => Sql.In(w.IssuerId, issuerIds));
                    break;
                default:
                    //Por cuenta y orden???
                    //var clients1 = Db.Select(Db.From<Domain.Sales.Client>().Where(w => w.TenantId == Session.Tenant.Id));
                    //var receiverIds1 = clients1.Select(x => x.PersonId).ToList();
                    var receiverIds1 = new List<int>();
                    receiverIds1.Add(tenant.PersonId);
                    var issuerIds1 = new List<int>();
                    issuerIds1.Add(tenant.PersonId);
                    q.And(w => Sql.In(w.ReceiverId, receiverIds1) || Sql.In(w.IssuerId, issuerIds1));
                    break;
            }

            if (p.ContainsKey("typeName"))
            {
                q.Where<BusinessDocumentType>(w => w.ShortName == p["typeName"]);
            }

            if (p.ContainsKey("issuerName"))
            {
                q.UnsafeWhere("issuer.Name LIKE {0}", Utils.SqlLike(p["issuerName"]));
            }

            if (p.ContainsKey("receiverName"))
            {
                q.UnsafeWhere("receiver.Name LIKE {0}", Utils.SqlLike(p["receiverName"]));
            }

            //ESTO ES LO QUE NO ME ANDA, CUANDO AGREGA ESTE WHERE SE ROMPE EL EXECUTE, PASA SI PASA POR EL PARAMETRO DE TYPENAME DE ARRIBA TAMBIEN.
            if (!string.IsNullOrEmpty(request.Type))
            {
                switch (request.Type)
                {
                    case "bills":
                        break;
                        case "invoices":
                        break;
                        case "checks":
                        break;
                        case "cashtransactions":
                        break;
                        case "banktransfers":
                        break;
                }
            }
            
            var count = Db.Count(q);
            if(count > 0)
            {
                request.Include = "Sum(Total) total";
            }

            var model = _autoQuery.Execute(request, q);
            var roleMap = Db.Select<ServiceModel.System.Workflows.WorkflowInstanceRoleMap>(Db
                .From<WorkflowInstanceAssignments>()
                .Join<Role>()
                .Where(w => Sql.In(w.WorkflowInstanceId, q.Where(sw => sw.ApprovalWorkflowInstanceId.HasValue).SelectDistinct(x => x.ApprovalWorkflowInstanceId)))
                .And(w => w.IsActive));
            model.Results.ForEach(x =>
            {
                if (x.ApprovalWorkflowInstanceId.HasValue)
                {
                    x.Roles = string.Join(",",
                        roleMap.Where(w => w.WorkflowInstanceId == x.ApprovalWorkflowInstanceId.Value)
                        .Select(w => w.RoleName));
                }
            });
            return model;
        }

        public object Get(Api.LookupBusinessDocumentRequest request)
        {
            var query = Db.From<BusinessDocument>();
            if (request.TypesId != null)
            {
                query.Where(x => Sql.In(x.TypeId, request.TypesId));
            }
            if (request.DebtorId != null)
            {
                query.Join<BusinessDocument, BusinessDocumentItem>((bd, bdi) => bd.Id == bdi.BusinessDocumentId)
                    .Join<BusinessDocumentItem, BusinessDocumentItemDebtor>((bdi, bdid) => bdi.Id == bdid.BusinessDocumentItemId)
                    .Join<BusinessDocumentItemDebtor, Domain.Financials.DebtManagement.Debtor>((bdid, d) => bdid.DebtorId == d.Id && d.PersonId == request.DebtorId);
                query.Where(x => x.Status != BusinessDocumentStatus.Paid);
                
            }
            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(q => Sql.In(q.Id, request.Ids));
            }
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Number.Contains(request.Q));
            }
            query.SelectDistinct();

            var count = Db.Count(query);
             
            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

   
            var result = new LookupResult
            {

                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Number + " - $" + x.Total.ToString("0.00") + " - " + x.DocumentDate.ToString("dd/MM/yyyy") }),
                Total = (int)count
            };
            return result;
        }

        public object Get(Api.GetBusinessDocumentResults request)
        {
            return ExportDocuments(request);
        }

        private class ExportResult
        {
            public int ItemTypesId { get; set; }
            public DateTime CreateDate { get; set; }
            public string TypeName { get; set; }
            public string TypeShortName { get; set; }
            public string Name { get; set; }
            public string Number { get; set; }
            public decimal Total { get; set; }
            public decimal SubTotal { get; set; }
            public decimal NoGrav { get; set; }
            public int IssuerId { get; set; }
            public int ReceiverId { get; set; }
        }

        private ExcelFileResult ExportDocuments(Api.GetBusinessDocumentResults request)
        {
            throw new NotImplementedException("Implement report service");

            /*
            var p = Request.GetRequestParams();
            var result = new DataTable();

            var tenant = Db.GetTenant(Session.TenantId);

            result.Columns.Add(new DataColumn { ColumnName = "Tipo" });
            result.Columns.Add(new DataColumn { ColumnName = "Punto Venta" });
            result.Columns.Add(new DataColumn { ColumnName = "Fecha Comprobante" });
            result.Columns.Add(new DataColumn { ColumnName = "Tipo Comprobante" });
            result.Columns.Add(new DataColumn { ColumnName = "Letra" });
            result.Columns.Add(new DataColumn { ColumnName = "Nro Comprobante" });
            result.Columns.Add(new DataColumn { ColumnName = "CUIT" });
            result.Columns.Add(new DataColumn { ColumnName = "Apellido y Nombre o Denominacion" });
            result.Columns.Add(new DataColumn { ColumnName = "Total Factura", DataType=typeof(double) });
            result.Columns.Add(new DataColumn { ColumnName = "Importe Gravado", DataType = typeof(double) });
            result.Columns.Add(new DataColumn { ColumnName = "Alicuota IVA", DataType = typeof(double) });
            result.Columns.Add(new DataColumn { ColumnName = "Impuesto Liquidado", DataType = typeof(double) });
            result.Columns.Add(new DataColumn { ColumnName = "Perc IB CABA", DataType = typeof(double) });
            result.Columns.Add(new DataColumn { ColumnName = "NO GRAV", DataType = typeof(double) });


            var query = "SELECT bd.*, t.Name as TypeName, t.ShortName as TypeShortName, i.SubTotal as SubTotal, nograv.SubTotal as NoGrav ";
            query += "FROM BusinessDocuments bd ";
            //query += "LEFT OUTER JOIN Persons p ON p.Id = bd.IssuerId ";
            query += "LEFT OUTER JOIN BusinessDocumentTypes t ON t.Id = bd.TypeId ";
            query += "LEFT OUTER JOIN (select sum(Quantity)*sum(UnitPrice) as SubTotal, BusinessDocumentId from BusinessDocumentItems where VatRate > 0 group by BusinessDocumentId) i ON i.BusinessDocumentId = bd.Id ";
            query += "LEFT OUTER JOIN (select sum(Quantity)*sum(UnitPrice) as SubTotal, BusinessDocumentId from BusinessDocumentItems where VatRate = 0 group by BusinessDocumentId) nograv ON nograv.BusinessDocumentId = bd.Id ";
            if (request.TypeId == 2)
            {
                var clients = Db.Select(Db.From<Domain.BusinessPartners.BusinessPartner>().Where(w => w.TypeId == 1 && w.TenantId == tenant.Id));
                var issuerIds = clients.Select(x => x.PersonId).ToList();
                issuerIds.Add(tenant.PersonId);
                query += "WHERE bd.IssuerId in (" + issuerIds.Join(",") + ") ";
            }
            else
            {
                var receivers = Db.Select(Db.From<Domain.BusinessPartners.BusinessPartner>().Where(w => w.TypeId == 2 && w.TenantId == tenant.Id));
                var receiverIds = receivers.Select(x => x.PersonId).ToList();
                receiverIds.Add(tenant.PersonId);
                //wiIdsQ.And(w => Sql.In(w.ReceiverId, receiverIds));
                query += "WHERE bd.ReceiverId in (" + receiverIds.Join(",") + ") ";
            }

            if (p.ContainsKey("fromDate"))
            {
                query += " and bd.CreateDate>=" + p["fromDate"].ConvertTo<DateTime>().Date.SqlValue();
            }
            if (p.ContainsKey("toDate"))
            {
                query += " and bd.CreateDate<=" + p["toDate"].ConvertTo<DateTime>().Date.SqlValue();
            }
            
            var documents = Db.Query<ExportResult>(query).ToList();
            
            foreach (var document in documents)
            {
                var colIndex = 0;
                var row = result.NewRow();
                //Viene hardcodeado en el TS
                //[{ id: 1, name: 'Productos' }, { id: 2, name: 'Servicios' }, { id: 3, name: 'Productos y Servicios' }];
                
                // Tipo
                switch (document.ItemTypesId)
                {
                    case 1:
                        row[colIndex] = "Productos";
                        break;
                    case 2:
                        row[colIndex] = "Servicios";
                        break;
                    case 3:
                        row[colIndex] = "Productos y Servicios";
                        break;
                    default:
                        row[colIndex] = "";
                        break;

                }

                row[++colIndex] = tenant.Name; // Punto de venta
                row[++colIndex] = document.CreateDate.ToShortDateString(); //Fecha comprobante
                row[++colIndex] = document.TypeName.Substring(0, document.TypeName.Length - 1); // Tipo de comprobante
                row[++colIndex] = document.TypeShortName.Last(); //Letra de comprobante
                row[++colIndex] = document.Number; //Numero de comprobante

                ServiceModel.System.Persons.Person.Person person;
                if (request.TypeId == 1) //Compras
                {
                    person = Db.GetPerson(document.IssuerId);
                }
                else
                {
                    person = Db.GetPerson(document.ReceiverId);
                }

                row[++colIndex] = person.Code;
                row[++colIndex] = person.Name;
                row[++colIndex] = document.Total; 
                row[++colIndex] = document.SubTotal;
                decimal iva = 0;
                if(document.SubTotal > 0)
                {
                    iva = (document.Total - document.SubTotal) * 100 / document.SubTotal;
                }
                row[++colIndex] = iva;
                row[++colIndex] = document.Total - document.SubTotal;//Impuesto Liquidado	
                row[++colIndex] = 0;//Perc IB CABA	
                row[++colIndex] = document.NoGrav; //NO GRAV

                result.Rows.Add(row);
            }

            var license = new License();
            license.SetLicense(Infraestructure.License.LStream);
            
            var wb = new Workbook();
            wb.Worksheets.Clear();
            var ws = wb.Worksheets.Add("Exportación " + request.TypeId);
            
            ws.Cells.ImportDataTable(result, true, 0, 0);

            ws.Cells.SetColumnWidth(0, 17.5);
            ws.Cells.SetColumnWidth(1, 17.5);
            ws.Cells.SetColumnWidth(2, 17.5);
            ws.Cells.SetColumnWidth(3, 20);
            ws.Cells.SetColumnWidth(4, 20);
            ws.Cells.SetColumnWidth(5, 18);
            ws.Cells.SetColumnWidth(6, 16);
            ws.Cells.SetColumnWidth(7, 30);
            ws.Cells.SetColumnWidth(8, 16);
            ws.Cells.SetColumnWidth(9, 16);
            ws.Cells.SetColumnWidth(10, 16);
            ws.Cells.SetColumnWidth(11, 16);
            ws.Cells.SetColumnWidth(12, 16);

            ws.Cells.InsertRow(0);

            Cell cell = ws.Cells["A1"];
            if(request.TypeId == 2)
            {
                cell.PutValue("Reporte Ventas");
            }else
            {
                cell.PutValue("Reporte Compras");
            }

            if (p.ContainsKey("fromDate"))
            {
                cell = ws.Cells["B1"];
                cell.PutValue("Desde: " + p["fromDate"].ConvertTo<DateTime>().ToShortDateString());
            }
            if (p.ContainsKey("toDate"))
            {
                cell = ws.Cells["C1"];
                cell.PutValue("Hasta: " + p["toDate"].ConvertTo<DateTime>().ToShortDateString());
            }

            //Styling for numeric columns
            Style stylen = wb.CreateStyle();
            stylen.Number = 39;
            stylen.HorizontalAlignment = TextAlignmentType.Right;

            StyleFlag styleFlagn = new StyleFlag();
            styleFlagn.NumberFormat = true;
            styleFlagn.HorizontalAlignment = true;

            for (var i = 8; i < 14; i++)
            {
                Column column = ws.Cells.Columns[i];
                column.ApplyStyle(stylen, styleFlagn);
            }
            
            //Header styling
            // Adding a new Style to the styles
            Style style = wb.CreateStyle();

            // Setting the vertical alignment of the text in the "A1" cell
            style.VerticalAlignment = TextAlignmentType.Center;

            // Setting the horizontal alignment of the text in the "A1" cell
            style.HorizontalAlignment = TextAlignmentType.Center;            

            // Shrinking the text to fit in the cell
            style.ShrinkToFit = true;
            
            // Setting the bottom border type of the cell to medium
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Medium;

            style.Font.IsBold = true;

            // Creating StyleFlag
            StyleFlag styleFlag = new StyleFlag();
            styleFlag.HorizontalAlignment = true;
            styleFlag.VerticalAlignment = true;
            styleFlag.ShrinkToFit = true;
            styleFlag.Borders = true;
            styleFlag.FontColor = true;
            styleFlag.FontBold = true;

            Row headerrow = ws.Cells.Rows[0];
            headerrow.ApplyStyle(style, styleFlag);
            Row subheaderrow = ws.Cells.Rows[1];
            subheaderrow.ApplyStyle(style, styleFlag);
            
            var ms = wb.SaveToStream();
            var fileContentsResult = new ExcelFileResult(ms, string.Format("CITI {0}", request.TypeId));
            return fileContentsResult;
            */
        }

        public async Task<object> Put(Api.PostBusinessDocumentCollection request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.Total = request.Items.Sum(x => x.UnitPrice);

                    var organism = Db.SingleById<Domain.Financials.DebtManagement.Organism>(request.IssuerId);

                    request.IssuerId = organism.PersonId;
                    request.ReceiverId = organism.PersonId;

                    await Db.UpdateAsync((BusinessDocument)request);
                    await Save(request);
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

        public async Task<object> Post(Api.PostBusinessDocumentCollection request)
        {
            request.CreateDate = DateTime.UtcNow;
            request.CreatedBy = Session.UserId;

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = await Db.SelectAsync(Db.From<BusinessDocument>().Where(w => w.IssuerId == request.IssuerId && w.ReceiverId == request.ReceiverId && w.Number == request.Number));
                    if (existing.Count > 0)
                    {
                        trx.Rollback();
                        return HttpError.Conflict("ERR_BusinessDocument_AlreadyExists");
                    }

                    //temp
                    var organism = Db.SingleById<Domain.Financials.DebtManagement.Organism>(request.IssuerId);
                    request.IssuerId = organism.PersonId;
                    request.ReceiverId = organism.PersonId;

                    request.Guid = Guid.NewGuid();
                    request.Total = request.Items.Sum(x => x.UnitPrice);
                    request.Status = BusinessDocumentStatus.Secretary;
                    request.CAEVoidDate = DateTime.Now;
                    request.NotificationDate = DateTime.Now;
                    request.CreateDate = DateTime.Now;
                    request.DocumentDate = DateTime.Now;
                    request.Id = (int)await Db.InsertAsync((BusinessDocument)request, true);
                    
                    await Save(request);
                    trx.Commit();
                }
                catch (Exception e)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return request;
        }

        private async Task Save(Api.PostBusinessDocumentCollection request)
        {
            var tenant = await _tenantRepository.GetTenant(Db, Session.TenantId);

            var currentItems = await Db.SelectAsync<BusinessDocumentItem>(w => w.BusinessDocumentId == request.Id);
            var idsToDelete = currentItems.Select(x => x.Id).Except(request.Items.Where(w => w.Id != 0).Select(s => s.Id));


            //Borro los creditors y debtors de los items a actualizar, que ya no pertenecen.
            List<int> itemCreditorsToUpdate = new List<int>();
            List<int> itemDebtorsToUpdate = new List<int>();
            foreach (var item in request.Items){
                if (item.Id > 0)
                {
                    //ojo controlar que no pueda tener dos veces mismo creditor
                    var creditorsToDelete = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => bdic.BusinessDocumentItemId == item.Id && !Sql.In(bdic.CreditorId, item.Creditors)));
                    var creditorIdsToDelete = creditorsToDelete.Select(x => x.Id);
                    Db.DeleteByIds<BusinessDocumentItemCreditor>(creditorIdsToDelete);

                    //ojo controlar que no pueda tener dos veces mismo debtor
                    var debtorsToDelete = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdic => bdic.BusinessDocumentItemId == item.Id && !Sql.In(bdic.DebtorId, item.Debtors)));
                    var debtorIdsToDelete = debtorsToDelete.Select(x => x.Id);
                    Db.DeleteByIds<BusinessDocumentItemDebtor>(debtorIdsToDelete);

                    //borrar los BusinessDocumentItemLaw
                    var lawIds = item.LawTexts.Select(x => x.LawId).ToList();
                    var itemLawsToDelete = Db.Select(Db.From<BusinessDocumentItemLaw>().Where(bdil => bdil.BusinessDocumentItemId == item.Id && !Sql.In(bdil.LawId, lawIds)));
                    var itemLawsIdsToDelete = itemLawsToDelete.Select(x => x.Id);
                    Db.DeleteByIds<BusinessDocumentItemLaw>(itemLawsIdsToDelete);
                }
            }

            //borro los creditors de todos los items que ya no pertenecen
            var creditorsToDeleteComplete = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => Sql.In(bdic.BusinessDocumentItemId, idsToDelete )));
            var creditorIdsToDeleteComplete = creditorsToDeleteComplete.Select(x => x.Id);
            Db.DeleteByIds<BusinessDocumentItemCreditor>(creditorIdsToDeleteComplete);

            //borro los debtors de todos los items que ya no pertenecen
            var debtorsToDeleteComplete = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdic => Sql.In(bdic.BusinessDocumentItemId, idsToDelete)));
            var debtorIdsToDeleteComplete = debtorsToDeleteComplete.Select(x => x.Id);
            Db.DeleteByIds<BusinessDocumentItemDebtor>(debtorIdsToDeleteComplete);

            //borro los BusinessDocumentItemLaw de todos los items que ya no pertenecen
            var itemLawsToDeleteComplete = Db.Select(Db.From<BusinessDocumentItemLaw>().Where(bdil => Sql.In(bdil.BusinessDocumentItemId, idsToDelete)));
            var itemLawsIdsToDeleteComplete = itemLawsToDeleteComplete.Select(x => x.Id);
            Db.DeleteByIds<BusinessDocumentItemLaw>(itemLawsIdsToDeleteComplete);

            //borro todos los items que ya no pertenecen.
            Db.DeleteByIds<BusinessDocumentItem>(idsToDelete);

            var businessDocumentType = await Db.SingleByIdAsync<BusinessDocumentType>(request.TypeId);

            foreach (var item in request.Items)
            {
                if (item.Id == 0)
                {
                    item.UnitTypeId = 1;
                    item.Bonus = 0;
                    item.VatRate = 0;
                    item.BusinessDocumentId = request.Id;

                    BusinessDocumentItem businessDocumentItem = new BusinessDocumentItem();
                    businessDocumentItem.PopulateWith(item);
                    if (businessDocumentItem.VoidDate == null){
                        if (businessDocumentItem.ItemDate != null)
                        {
                            businessDocumentItem.VoidDate = businessDocumentItem.ItemDate.Value.AddDays(30);
                            businessDocumentItem.OriginalVoidDate = businessDocumentItem.VoidDate;
                        }
                    }
                    else
                    {
                        businessDocumentItem.OriginalVoidDate = businessDocumentItem.VoidDate;
                    }

                    businessDocumentItem.OriginalAmount = item.OriginalAmount ?? 0;
                    businessDocumentItem.AppliedAmount = item.AppliedAmount ?? 0;
                    businessDocumentItem.AppliedInterest = item.AppliedInterest ?? 0;
                    businessDocumentItem.PendingInterest = item.PendingInterest ?? 0;

                    businessDocumentItem.Id = (int)await Db.InsertAsync(businessDocumentItem, true);
                    
                    if (item.Creditors != null)
                    {
                        foreach (var itemCreditor in item.Creditors)
                        {
                            BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor();
                            businessDocumentItemCreditor.BusinessDocumentItemId = businessDocumentItem.Id;
                            businessDocumentItemCreditor.CreditorId = itemCreditor;
                            businessDocumentItemCreditor.Id = (int)await Db.InsertAsync(businessDocumentItemCreditor, true);
                        }
                    }

                    if (item.Debtors != null)
                    {
                        foreach (var itemDebtor in item.Debtors)
                        {
                            BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor();
                            businessDocumentItemDebtor.BusinessDocumentItemId = businessDocumentItem.Id;
                            businessDocumentItemDebtor.DebtorId = itemDebtor;
                            businessDocumentItemDebtor.Id = (int)await Db.InsertAsync(businessDocumentItemDebtor, true);
                        }
                    }

                    if (item.LawTexts != null)
                    {
                        foreach (var itemLaw in item.LawTexts)
                        {
                            BusinessDocumentItemLaw businessDocumentItemLaw = new BusinessDocumentItemLaw();
                            businessDocumentItemLaw.BusinessDocumentItemId = businessDocumentItem.Id;
                            businessDocumentItemLaw.LawId = itemLaw.LawId;
                            businessDocumentItemLaw.Observation = itemLaw.Text;
                            businessDocumentItemLaw.Id = (int)await Db.InsertAsync(businessDocumentItemLaw, true);
                        }
                    }

                }
                else
                {
                    BusinessDocumentItem itemToUpdate = new BusinessDocumentItem();
                    itemToUpdate.PopulateWith(item);
                    itemToUpdate.UnitTypeId = 1;
                    itemToUpdate.Bonus = 0;
                    itemToUpdate.VatRate = 0;
                    itemToUpdate.BusinessDocumentId = request.Id;

                    await Db.UpdateAsync(itemToUpdate);

                    if (item.Creditors != null)
                    {
                        foreach (var creditor in item.Creditors)
                        {
                            //como ya borre antes es un update o existe.
                            var creditorItem = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => bdic.BusinessDocumentItemId == item.Id && bdic.CreditorId == creditor)).SingleOrDefault();
                            if (creditorItem == null)
                            {
                                BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor();
                                businessDocumentItemCreditor.BusinessDocumentItemId = item.Id;
                                businessDocumentItemCreditor.CreditorId = creditor;
                                businessDocumentItemCreditor.Id = (int)await Db.InsertAsync(businessDocumentItemCreditor, true);
                            }
                        }
                    }
                    if (item.Debtors != null)
                    {
                        foreach (var debtor in item.Debtors)
                        {
                            //como ya borre antes es un update o existe.
                            var debtorItem = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdic => bdic.BusinessDocumentItemId == item.Id && bdic.DebtorId == debtor)).SingleOrDefault();
                            if (debtorItem == null)
                            {
                                BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor();
                                businessDocumentItemDebtor.BusinessDocumentItemId = item.Id;
                                businessDocumentItemDebtor.DebtorId = debtor;
                                businessDocumentItemDebtor.Id = (int)await Db.InsertAsync(businessDocumentItemDebtor, true);
                            }
                        }
                    }

                    if (item.LawTexts != null)
                    {
                        foreach (var law in item.LawTexts)
                        {
                            //como ya borre antes es un update o existe.
                            var lawItem = Db.Select(Db.From<BusinessDocumentItemLaw>().Where(bdil => bdil.BusinessDocumentItemId == item.Id && bdil.LawId == law.LawId)).SingleOrDefault();
                            if (lawItem == null)
                            {
                                BusinessDocumentItemLaw businessDocumentItemLaw = new BusinessDocumentItemLaw();
                                businessDocumentItemLaw.BusinessDocumentItemId = item.Id;
                                businessDocumentItemLaw.LawId = law.LawId;
                                businessDocumentItemLaw.Observation = law.Text;
                                businessDocumentItemLaw.Id = (int)await Db.InsertAsync(businessDocumentItemLaw, true);
                            }
                        }
                    }
                }

            }

            //Cuenta Corriente para Organismos
            var organism = Db.Select(Db.From<Domain.Financials.DebtManagement.Organism>().Where(o => o.PersonId == request.IssuerId && o.TypeId == 1)).SingleOrDefault();
            if (organism != null)
            {
                var businessPartnerOrganism = Db.Select(Db.From<BusinessPartner>().Where(bp => bp.PersonId == organism.PersonId
                    && bp.TypeId == 9)).SingleOrDefault();
                if (businessPartnerOrganism != null)
                {
                    var businessPartnerOrganismAccount = Db.Select(Db.From<BusinessPartnerAccount>().Where(bpa => bpa.BusinessPartnerId == businessPartnerOrganism.Id)).SingleOrDefault();
                    if (businessPartnerOrganismAccount != null)
                    {
                        BusinessPartnerAccountEntry accountEntry = new BusinessPartnerAccountEntry
                        {
                            AccountId = businessPartnerOrganismAccount.Id,
                            Amount = -1 * request.Total,
                            Description = "Expediente Nr: " + request.Number,
                            CreateDate = DateTime.UtcNow,
                            LinkedDocumentId = request.Id,
                            LinkedDocumentTypeId = request.TypeId,
                            PostingDate = DateTime.UtcNow
                        };
                        Db.Insert(accountEntry);
                    }
                }

                //Cuenta Corriente para Deudores
                foreach (var item in request.Items)
                {

                    if (item.Debtors != null)
                    {
                        foreach (var itemDebtor in item.Debtors)
                        {
                            var debtor = Db.SingleById<Domain.Financials.DebtManagement.Debtor>(itemDebtor);

                            var businessPartnerDebtor = Db.Select(Db.From<BusinessPartner>().Where(bp => bp.PersonId == debtor.PersonId
                                                            && bp.TypeId == 8)).SingleOrDefault();
                            if (businessPartnerDebtor != null)
                            {
                                var businessPartnerDebtorAccount = Db.Select(Db.From<BusinessPartnerAccount>().Where(bpa => bpa.BusinessPartnerId == businessPartnerDebtor.Id)).SingleOrDefault();
                                if (businessPartnerDebtorAccount != null)
                                {
                                    var product = Db.SingleById<Product>(item.ProductId);
                                    BusinessPartnerAccountEntry accountEntry = new BusinessPartnerAccountEntry
                                    {
                                        AccountId = businessPartnerDebtorAccount.Id,
                                        Amount = item.UnitPrice,
                                        Description = "Expediente Nr: " + request.Number + " concepto: " + product.Name,
                                        CreateDate = DateTime.UtcNow,
                                        LinkedDocumentId = request.Id,
                                        LinkedDocumentTypeId = request.TypeId,
                                        PostingDate = DateTime.UtcNow
                                    };
                                    Db.Insert(accountEntry);
                                }
                            }
                        }
                    }
                    
                }

                //Cuenta Corriente para Acreedores
                foreach (var item in request.Items)
                {
                    if (item.Creditors != null)
                    {
                        foreach (var itemCreditor in item.Creditors)
                        {
                            var creditor = Db.SingleById<Domain.Financials.DebtManagement.Creditor>(itemCreditor);

                            var businessPartnerCreditor = Db.Select(Db.From<BusinessPartner>().Where(bp => bp.PersonId == creditor.PersonId
                                                            && bp.TypeId == 12)).SingleOrDefault();
                            if (businessPartnerCreditor != null)
                            {
                                var businessPartnerCreditorAccount = Db.Select(Db.From<BusinessPartnerAccount>().Where(bpa => bpa.BusinessPartnerId == businessPartnerCreditor.Id)).SingleOrDefault();
                                if (businessPartnerCreditorAccount != null)
                                {
                                    var product = Db.SingleById<Product>(item.ProductId);
                                    BusinessPartnerAccountEntry accountEntry = new BusinessPartnerAccountEntry
                                    {
                                        AccountId = businessPartnerCreditorAccount.Id,
                                        Amount = -1 * item.UnitPrice,
                                        Description = "Expediente Nr: " + request.Number + " concepto: " + product.Name,
                                        CreateDate = DateTime.UtcNow,
                                        LinkedDocumentId = request.Id,
                                        LinkedDocumentTypeId = request.TypeId,
                                        PostingDate = DateTime.UtcNow
                                    };
                                    Db.Insert(accountEntry);
                                }
                            }
                        }
                    }   
                }

                Api.PostBusinessDocumentSubmitForCollect postBusinessDocumentSubmitForCollect = new Api.PostBusinessDocumentSubmitForCollect();
                postBusinessDocumentSubmitForCollect.BusinessDocumentGuid = request.Guid;
                await Post(postBusinessDocumentSubmitForCollect);



            }
        }

        public async Task<object> Post(Api.PostBusinessDocumentReckoning request)
        {
            request.CreateDate = DateTime.UtcNow;
            request.CreatedBy = Session.UserId;

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = Db.SingleById<BusinessDocument>(request.BusinessDocumentParentId);
                    if (existing == null)
                    {
                        trx.Rollback();
                        return HttpError.Conflict("ERR_BusinessDocument_NotExists");
                    }


                    var query = $"SELECT MAX(Number) Number FROM BusinessDocuments WHERE Number Like '{existing.Number}-L%' AND TypeId in (24,26,27,29)";
                    var currentNumber = Db.Scalar<string>(query);
                    var reckoningNumber = 1;
                    if (currentNumber != null)
                    {
                        reckoningNumber = Int32.Parse(currentNumber.Substring(currentNumber.Length - 3)) + 1;
                    }
            


                    var newNumber = existing.Number + "-L" + ("000" + reckoningNumber.ToString()).Substring(("000" + reckoningNumber.ToString()).Length -3,3);

                    BusinessDocument reckoningBusinessDocument = new BusinessDocument();
                    reckoningBusinessDocument.PopulateWith(existing);
                    reckoningBusinessDocument.Number = newNumber;
                    reckoningBusinessDocument.Guid = Guid.NewGuid();
                    reckoningBusinessDocument.Id = 0;
                    reckoningBusinessDocument.TypeId = request.TypeId;
                    reckoningBusinessDocument.Total = request.Total;
                    reckoningBusinessDocument.VoidDate = request.VoidDate != null ? request.VoidDate : DateTime.Now.AddDays(15); //deberia haber param default
                    reckoningBusinessDocument.FromServiceDate = request.FromServiceDate;
                    reckoningBusinessDocument.ToServiceDate = request.ToServiceDate;

                    reckoningBusinessDocument.Id = (int)Db.Insert(reckoningBusinessDocument,true);

                    var ParentItemIds = Db.Select(Db.From<BusinessDocumentItem>().Where(x => x.BusinessDocumentId == existing.Id)).Select(x => x.Id).ToList();
                    var ParentCreditorsIds = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => Sql.In(bdic.BusinessDocumentItemId, ParentItemIds))).ToList().Select(x => x.CreditorId).Distinct();
                    var ParentDebtorsIds = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdid => Sql.In(bdid.BusinessDocumentItemId, ParentItemIds))).ToList().Select(x => x.DebtorId).Distinct();
                    
                    //var ParentCreditorsIds = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => Sql.In(bdic.BusinessDocumentItemId, request.ParentItems.Select(x => x.ParentId).ToList()))).ToList().Select(x => x.CreditorId).Distinct();
                    //var ParentDebtorsIds = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdid => Sql.In(bdid.BusinessDocumentItemId, request.ParentItems.Select(x => x.ParentId).ToList()))).ToList().Select(x => x.DebtorId).Distinct();


                    var BaseAmount = request.Total - request.InterestTotal - request.Items.Sum(x => x.UnitPrice);
                    var ProductId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();

                    BusinessDocumentItem baseAmountBusinessDocumentItem = new BusinessDocumentItem();
                    baseAmountBusinessDocumentItem.UnitTypeId = 1;
                    baseAmountBusinessDocumentItem.Bonus = 0;
                    baseAmountBusinessDocumentItem.VatRate = 0;
                    baseAmountBusinessDocumentItem.UnitPrice = BaseAmount;
                    baseAmountBusinessDocumentItem.ProductId = ProductId;
                    baseAmountBusinessDocumentItem.BusinessDocumentId = reckoningBusinessDocument.Id;
                    baseAmountBusinessDocumentItem.OriginalAmount = BaseAmount;
                    baseAmountBusinessDocumentItem.AppliedAmount = 0;
                    baseAmountBusinessDocumentItem.AppliedInterest = 0;
                    baseAmountBusinessDocumentItem.Id = (int)Db.Insert(baseAmountBusinessDocumentItem, true);

                    foreach (var parent in request.ParentItems)
                    {
                        BusinessDocumentItemLink businessDocumentItemLink = new BusinessDocumentItemLink();
                        businessDocumentItemLink.DocumentItemId = baseAmountBusinessDocumentItem.Id;
                        businessDocumentItemLink.DocumentItemRelatedId = parent.ParentId;
                        businessDocumentItemLink.Amount = parent.Amount;
                        businessDocumentItemLink.FromDate = parent.FromDate;
                        businessDocumentItemLink.ToDate = parent.ToDate;
                        businessDocumentItemLink.AppliedAmount = 0;
                        Db.Insert(businessDocumentItemLink);
                    }

                    foreach (var parentCreditorId in ParentCreditorsIds)
                    {
                        BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor();
                        businessDocumentItemCreditor.BusinessDocumentItemId = baseAmountBusinessDocumentItem.Id;
                        businessDocumentItemCreditor.CreditorId = parentCreditorId;
                        businessDocumentItemCreditor.Id = (int)await Db.InsertAsync(businessDocumentItemCreditor, true);
                    }
                    foreach (var parentDebtorId in ParentDebtorsIds)
                    {
                        BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor();
                        businessDocumentItemDebtor.BusinessDocumentItemId = baseAmountBusinessDocumentItem.Id;
                        businessDocumentItemDebtor.DebtorId = parentDebtorId;
                        businessDocumentItemDebtor.Id = (int)await Db.InsertAsync(businessDocumentItemDebtor, true);
                    }

                    ProductId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();
                    BusinessDocumentItem interestBusinessDocumentItem = new BusinessDocumentItem();
                    interestBusinessDocumentItem.UnitTypeId = 1;
                    interestBusinessDocumentItem.Bonus = 0;
                    interestBusinessDocumentItem.VatRate = 0;
                    interestBusinessDocumentItem.UnitPrice = request.InterestTotal;
                    interestBusinessDocumentItem.BusinessDocumentId = reckoningBusinessDocument.Id;
                    interestBusinessDocumentItem.ProductId = ProductId;
                    interestBusinessDocumentItem.OriginalAmount = request.InterestTotal;
                    interestBusinessDocumentItem.AppliedAmount = 0;
                    interestBusinessDocumentItem.AppliedInterest = 0;
                    interestBusinessDocumentItem.Id = (int)await Db.InsertAsync(interestBusinessDocumentItem, true);

                    foreach (var parent in request.ParentItems)
                    {
                        BusinessDocumentItemLink businessDocumentItemLink = new BusinessDocumentItemLink();
                        businessDocumentItemLink.DocumentItemId = interestBusinessDocumentItem.Id;
                        businessDocumentItemLink.DocumentItemRelatedId = parent.ParentId;
                        businessDocumentItemLink.Amount = parent.AmountInterest + (parent.PendingInterest ?? 0);
                        businessDocumentItemLink.FromDate = parent.FromDate;
                        businessDocumentItemLink.ToDate = parent.ToDate;
                        businessDocumentItemLink.AppliedAmount = 0;
                        Db.Insert(businessDocumentItemLink);
                    }

                    foreach (var item in request.Items)
                    {
                        item.UnitTypeId = 1;
                        item.Bonus = 0;
                        item.VatRate = 0;
                        item.BusinessDocumentId = reckoningBusinessDocument.Id;
                        BusinessDocumentItem businessDocumentItem = new BusinessDocumentItem();
                        businessDocumentItem.PopulateWith(item);
                        businessDocumentItem.OriginalAmount = businessDocumentItem.UnitPrice;
                        businessDocumentItem.AppliedAmount = 0;
                        businessDocumentItem.AppliedInterest = 0;
                        businessDocumentItem.Id = (int)await Db.InsertAsync(businessDocumentItem, true);


                        foreach(var parentCreditorId in ParentCreditorsIds)
                        {
                            BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor();
                            businessDocumentItemCreditor.BusinessDocumentItemId = businessDocumentItem.Id;
                            businessDocumentItemCreditor.CreditorId = parentCreditorId;
                            businessDocumentItemCreditor.Id = (int)await Db.InsertAsync(businessDocumentItemCreditor, true);
                        }
                        foreach (var parentDebtorId in ParentDebtorsIds)
                        {
                            BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor();
                            businessDocumentItemDebtor.BusinessDocumentItemId = businessDocumentItem.Id;
                            businessDocumentItemDebtor.DebtorId = parentDebtorId;
                            businessDocumentItemDebtor.Id = (int)await Db.InsertAsync(businessDocumentItemDebtor, true);
                        }
                    }


                    foreach (var parentCreditorId in ParentCreditorsIds)
                    {
                        BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor();
                        businessDocumentItemCreditor.BusinessDocumentItemId = interestBusinessDocumentItem.Id;
                        businessDocumentItemCreditor.CreditorId = parentCreditorId;
                        businessDocumentItemCreditor.Id = (int)await Db.InsertAsync(businessDocumentItemCreditor, true);
                    }
                    foreach (var parentDebtorId in ParentDebtorsIds)
                    {
                        BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor();
                        businessDocumentItemDebtor.BusinessDocumentItemId = interestBusinessDocumentItem.Id;
                        businessDocumentItemDebtor.DebtorId = parentDebtorId;
                        businessDocumentItemDebtor.Id = (int)await Db.InsertAsync(businessDocumentItemDebtor, true);
                    }

                    BusinessDocumentLink businessDocumentLink = new BusinessDocumentLink();

                    businessDocumentLink.DocumentId = existing.Id;
                    businessDocumentLink.LinkedDocumentId = reckoningBusinessDocument.Id;
                    businessDocumentLink.TypeId = 1;

                    Db.Insert(businessDocumentLink);

                    trx.Commit();
                }
                catch (Exception e)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return request;

        }

        public async Task<bool> Post(Api.PostBusinessDocumentPaymentCoupon request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var businessDocumentReckoning = Db.SingleById<BusinessDocument>(request.Id);
                    var businessDocumentParent = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.TypeId == 1 && bdl.LinkedDocumentId == request.Id)).FirstOrDefault();

                    if (businessDocumentReckoning != null && businessDocumentParent.Id != default(int))
                    {
                        //TENGO QUE FIJARME SI HAY ALGUN INSTRUMENTO VIGENTE

                        //traigo todos las NAD del expediente que estan vigentes
                        var paymentCoupons = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.LinkedDocumentId && bdl.TypeId == 2 
                                                                                                                           && bdl.DocumentId == businessDocumentParent.Id 
                                                                                                                           && bd.VoidDate > DateTime.Now
                                                                                                                           && bd.Status != BusinessDocumentStatus.Voided
                                                                                                                           && bd.Status != BusinessDocumentStatus.Paid
                                                                                                                           && bd.Status != BusinessDocumentStatus.Partial));
                        var paymentCouponsIds = paymentCoupons.ToList().Select(x => x.Id);

                        //debo saber los items originales de la liquidacion actual
                        var currentReckoningItemsIds = Db.Select(Db.From<BusinessDocumentItem>().Where(x => x.BusinessDocumentId == businessDocumentReckoning.Id)).ToList().Select(x => x.Id);
                        var originalItemsIds = Db.Select(Db.From<BusinessDocumentItem>().Where(x => x.BusinessDocumentId == businessDocumentParent.Id)).ToList().Select(x => x.Id);
                        var itemsToCheck = Db.Select(Db.From<BusinessDocumentItemLink>().Where(x => Sql.In(x.DocumentItemRelatedId, originalItemsIds) && Sql.In(x.DocumentItemId, currentReckoningItemsIds))).ToList().Select(x => x.DocumentItemRelatedId);

                        //debo validar que ningun item de la liquidacion se encuentra en otra liquidacion con NAD vigente
                        var check = Db.Select(Db.From<BusinessDocumentItemLink>()
                                                .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdil, bdi) => Sql.In(bdil.DocumentItemRelatedId, itemsToCheck) && bdil.DocumentItemId == bdi.Id)
                                                .Join<BusinessDocumentItem, BusinessDocumentLink>((bdi, bdl) => bdi.BusinessDocumentId == bdl.DocumentId && Sql.In(bdl.LinkedDocumentId, paymentCouponsIds)));

                        if (check == null || check.Count() == 0)
                        {

                            var paymentCoupon = new BusinessDocument();
                            paymentCoupon.PopulateWith(businessDocumentReckoning);
                            paymentCoupon.Id = 0;                            
                            paymentCoupon.ApprovalWorkflowInstanceId = null;
                            paymentCoupon.Guid = Guid.NewGuid();

                            var query = $"SELECT MAX(Number) Number FROM BusinessDocuments WHERE Number Like '{businessDocumentParent.Number}-NAD%' AND TypeId = 23";

                            paymentCoupon.TypeId = 23;
                            if (businessDocumentReckoning.TypeId == 27)
                            {
                                paymentCoupon.TypeId = 31;
                                query = $"SELECT MAX(Number) Number FROM BusinessDocuments WHERE Number Like '{businessDocumentParent.Number}-DGJ%' AND TypeId = 31";
                            }
                            if (businessDocumentReckoning.TypeId == 29)
                            {
                                paymentCoupon.TypeId = 30;
                                query = $"SELECT MAX(Number) Number FROM BusinessDocuments WHERE Number Like '{businessDocumentParent.Number}-IGB%' AND TypeId = 30";
                            }
                            
                            var currentNumber = Db.Scalar<string>(query);
                            var paymentCouponNumber = 1;
                            if (currentNumber != null)
                            {
                                paymentCouponNumber = Int32.Parse(currentNumber.Substring(currentNumber.Length - 3)) + 1;
                            }
                            var newNumber = businessDocumentParent.Number + (paymentCoupon.TypeId == 23 ? "-NAD" : paymentCoupon.TypeId == 31 ? "-DGJ" : "-IGB") + ("000" + paymentCouponNumber.ToString()).Substring(("000" + paymentCouponNumber.ToString()).Length - 3, 3);
                            paymentCoupon.Number = newNumber;

                            paymentCoupon.Id = (int)await Db.InsertAsync(paymentCoupon, true);

                            var businessDocumentItems = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentReckoning.Id)).ToList();
                            var businessDocumentItemsIds = businessDocumentItems.Select(bdi => bdi.Id);
                            var ParentCreditorsIds = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => Sql.In(bdic.BusinessDocumentItemId, businessDocumentItemsIds))).ToList().Select(x => x.CreditorId);
                            var ParentDebtorsIds = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdid => Sql.In(bdid.BusinessDocumentItemId, businessDocumentItemsIds))).ToList().Select(x => x.DebtorId);
                            foreach (var businessDocumentItem in businessDocumentItems)
                            {
                                var paymentCouponItem = new BusinessDocumentItem();
                                paymentCouponItem.PopulateWith(businessDocumentItem);
                                paymentCouponItem.Id = 0;
                                paymentCouponItem.BusinessDocumentId = paymentCoupon.Id;
                                paymentCouponItem.OriginalAmount = businessDocumentItem.UnitPrice;
                                paymentCouponItem.AppliedAmount = 0;
                                paymentCouponItem.AppliedInterest = 0;

                                paymentCouponItem.Id = (int)await Db.InsertAsync(paymentCouponItem, true);

                                foreach (var parentCreditorId in ParentCreditorsIds)
                                {
                                    BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor();
                                    businessDocumentItemCreditor.BusinessDocumentItemId = paymentCouponItem.Id;
                                    businessDocumentItemCreditor.CreditorId = parentCreditorId;
                                    businessDocumentItemCreditor.Id = (int)await Db.InsertAsync(businessDocumentItemCreditor, true);
                                }
                                foreach (var parentDebtorId in ParentDebtorsIds)
                                {
                                    BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor();
                                    businessDocumentItemDebtor.BusinessDocumentItemId = paymentCouponItem.Id;
                                    businessDocumentItemDebtor.DebtorId = parentDebtorId;
                                    businessDocumentItemDebtor.Id = (int)await Db.InsertAsync(businessDocumentItemDebtor, true);
                                }

                                //debo generar links entre items de NAD y Expediente
                                var currentReckoningItemLinks = Db.Select(Db.From<BusinessDocumentItemLink>().Where(x => x.DocumentItemId == businessDocumentItem.Id));
                                foreach (var currentReckoningItemLink in currentReckoningItemLinks)
                                {
                                    BusinessDocumentItemLink paymentCouponToParentItemLink = new BusinessDocumentItemLink();
                                    paymentCouponToParentItemLink.PopulateWith(currentReckoningItemLink);
                                    paymentCouponToParentItemLink.DocumentItemId = paymentCouponItem.Id;
                                    paymentCouponToParentItemLink.AppliedAmount = 0;
                                    await Db.InsertAsync(paymentCouponToParentItemLink, true);
                                }

                            }

                            //Link Reckonings -> PaymentCoupon
                            var businessDocumentLink = new BusinessDocumentLink();
                            businessDocumentLink.DocumentId = businessDocumentReckoning.Id;
                            businessDocumentLink.LinkedDocumentId = paymentCoupon.Id;
                            businessDocumentLink.TypeId = 2;
                            await Db.InsertAsync(businessDocumentLink);

                            //Link BusinessDocument (exp) -> PaymentCoupon
                            businessDocumentLink = new BusinessDocumentLink();
                            businessDocumentLink.DocumentId = businessDocumentParent.Id;
                            businessDocumentLink.LinkedDocumentId = paymentCoupon.Id;
                            businessDocumentLink.TypeId = 2;
                            await Db.InsertAsync(businessDocumentLink);

                            Api.PostBusinessDocumentSubmitForDebtCollect postBusinessDocumentSubmitForDebtCollect = new Api.PostBusinessDocumentSubmitForDebtCollect();
                            postBusinessDocumentSubmitForDebtCollect.BusinessDocumentGuid = paymentCoupon.Guid;
                            await Post(postBusinessDocumentSubmitForDebtCollect);


                            trx.Commit();
                            return true;
                        }
                        else
                        {
                            return false;
                        }


                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw;

                }
            }
        }

        public async Task<bool> Post(Api.PostBusinessDocumentExecution request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var businessDocument = Db.SingleById<BusinessDocument>(request.Id); //traigo liquidacion
                    var businessDocumentParent = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.TypeId == 1 && bdl.LinkedDocumentId == request.Id)).FirstOrDefault(); //triago expediente

                    if (businessDocument != null && businessDocumentParent.Id != default(int))
                    {
                        var executionDocument = new BusinessDocument();
                        executionDocument.PopulateWith(businessDocument);
                        executionDocument.ApprovalWorkflowInstanceId = null;
                        executionDocument.Id = 0;
                        executionDocument.TypeId = 28;
                        var query = $"SELECT MAX(Number) Number FROM BusinessDocuments WHERE Number Like '{businessDocumentParent.Number}-TE%' AND TypeId = 28";
                        var currentNumber = Db.Scalar<string>(query);
                        var executionNumber = 1;
                        if (currentNumber != null)
                        {
                            executionNumber = Int32.Parse(currentNumber.Substring(currentNumber.Length - 3)) + 1;
                        }
                        var newNumber = businessDocumentParent.Number + "-TE" + ("000" + executionNumber.ToString()).Substring(("000" + executionNumber.ToString()).Length - 3, 3);
                        executionDocument.Number = newNumber;
                        executionDocument.Guid = Guid.NewGuid();
                        executionDocument.Id = (int)await Db.InsertAsync(executionDocument, true);

                        var businessDocumentItems = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocument.Id)).ToList();


                        //Link Reckonings -> ExecutionDocument
                        var businessDocumentLink = new BusinessDocumentLink();
                        businessDocumentLink.DocumentId = businessDocument.Id;
                        businessDocumentLink.LinkedDocumentId = executionDocument.Id;
                        businessDocumentLink.TypeId = 3;
                        await Db.InsertAsync(businessDocumentLink);

                        //Link BusinessDocument (exp) -> ExecutionDocument
                        businessDocumentLink = new BusinessDocumentLink();
                        businessDocumentLink.DocumentId = businessDocumentParent.Id;
                        businessDocumentLink.LinkedDocumentId = executionDocument.Id;
                        businessDocumentLink.TypeId = 3;
                        await Db.InsertAsync(businessDocumentLink);

                        var businessDocumentItemsIds = businessDocumentItems.Select(bdi => bdi.Id);

                        var ParentCreditorsIds = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => Sql.In(bdic.BusinessDocumentItemId, businessDocumentItemsIds))).ToList().Select(x => x.CreditorId).Distinct();
                        var ParentDebtorsIds = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdid => Sql.In(bdid.BusinessDocumentItemId, businessDocumentItemsIds))).ToList().Select(x => x.DebtorId).Distinct();

                        foreach (var businessDocumentItem in businessDocumentItems)
                        {
                            var executionItem = new BusinessDocumentItem();
                            executionItem.PopulateWith(businessDocumentItem);
                            executionItem.Id = 0;
                            executionItem.BusinessDocumentId = executionDocument.Id;
                            executionItem.OriginalAmount = executionItem.UnitPrice;
                            executionItem.AppliedAmount = 0;
                            executionItem.AppliedInterest = 0;
                            executionItem.Id = (int)await Db.InsertAsync(executionItem, true);

                            foreach (var parentCreditorId in ParentCreditorsIds)
                            {
                                BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor();
                                businessDocumentItemCreditor.BusinessDocumentItemId = executionItem.Id;
                                businessDocumentItemCreditor.CreditorId = parentCreditorId;
                                businessDocumentItemCreditor.Id = (int)await Db.InsertAsync(businessDocumentItemCreditor, true);
                            }
                            foreach (var parentDebtorId in ParentDebtorsIds)
                            {
                                BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor();
                                businessDocumentItemDebtor.BusinessDocumentItemId = executionItem.Id;
                                businessDocumentItemDebtor.DebtorId = parentDebtorId;
                                businessDocumentItemDebtor.Id = (int)await Db.InsertAsync(businessDocumentItemDebtor, true);
                            }
                        }


                        Api.PostBusinessDocumentSubmitForDebtCollect postBusinessDocumentSubmitForDebtCollect = new Api.PostBusinessDocumentSubmitForDebtCollect();
                        postBusinessDocumentSubmitForDebtCollect.BusinessDocumentGuid = executionDocument.Guid;
                        await Post(postBusinessDocumentSubmitForDebtCollect);


                        trx.Commit();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw;

                }
            }
        }

        private ServiceModel.System.Messages.Message.QueryResult AddMessage(List<ServiceModel.System.Messages.Message.QueryResult> messages, ServiceModel.System.Messages.Message.QueryResult parent)
        {
            parent.ReplyToMessageId = null;
            foreach (var reply in messages.Where(x => x.ReplyToMessageId == parent.Id).ToList())
            {
                parent.Replies.Add(AddMessage(messages, reply));
            }

            return parent;
        }

        private async Task<Api.PostBusinessDocumentCollection> GetBusinessDocumentCollect(Api.GetBusinessDocumentCollect request)
        {
            var documentTypes = Db.Select(Db.From<BusinessDocumentType>()).ToList();
            var document = (await Db.SingleByIdAsync<BusinessDocument>(request.Id)).ConvertTo<Api.PostBusinessDocumentCollection>();

            var documentTypeName = documentTypes.Where(bdt => bdt.Id == document.TypeId).FirstOrDefault().ShortName;
            document.TypeName = documentTypeName != null ? documentTypeName : "";

            try
            {
                Conversions converter = new Conversions();
                document.TotalText = converter.enletras(document.Total.ToString());
            }
            catch (Exception)
            {
                throw;
            }

            var items = await Db.SelectAsync<BusinessDocumentItem>(w => w.BusinessDocumentId == document.Id);

            List<Product> products = null;
            var siteIds = new List<int>();
            if (items.Count > 0)
            {
                var productIds = items.Select(x => x.ProductId);
                products = await Db.SelectAsync<Product>(w => Sql.In(w.Id, productIds));

                siteIds.AddRange(items.Where(w => w.InventorySiteId.HasValue).Select(x => x.InventorySiteId.Value).Distinct());
            }
            if (document.InventorySiteId.HasValue)
            {
                siteIds.AddIfNotExists(document.InventorySiteId.Value);
            }

            var sites = await Db.SelectAsync<InventorySite>(w => Sql.In(w.Id, siteIds));
            foreach (var item in items)
            {
                var itemModel = item.ConvertTo<Api.BusinessDocumentItemDetail>();
                if (item.PrescriptionDate < DateTime.UtcNow)
                {
                    itemModel.Status = 2;
                }
                else
                {
                    if (item.PrescriptionDate > DateTime.UtcNow)
                    {
                        itemModel.Status = 0;
                        if (item.PrescriptionDate < DateTime.UtcNow.AddDays(30))
                        {
                            itemModel.Status = 1;
                        }
                    }
                }
                //falta el estado recuperado.



                if (!request.Edit && products != null)
                {
                    itemModel.Product = products.Single(x => x.Id == itemModel.ProductId);
                }
                if (!request.Edit && sites != null && itemModel.InventorySiteId != null)
                {
                    itemModel.Site = sites.Single(x => x.Id == itemModel.InventorySiteId);
                }

                var creditorsByItem = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => bdic.BusinessDocumentItemId == item.Id)).Select(x => x.CreditorId).ToList();
                itemModel.Creditors = creditorsByItem;

                var debtorsByItem = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdid => bdid.BusinessDocumentItemId == item.Id)).Select(x => x.DebtorId).ToList();
                itemModel.Debtors = debtorsByItem;

                var lawsByItem = Db.Select(Db.From<BusinessDocumentItemLaw>().Where(bdil => bdil.BusinessDocumentItemId == item.Id)).ToList();

                if (lawsByItem != null)
                {
                    itemModel.LawTexts = new List<Api.BusinessDocumentItemDetailaw>();
                    foreach (var lawByItem in lawsByItem)
                    {
                        Api.BusinessDocumentItemDetailaw businessDocumentItemDetailaw = new Api.BusinessDocumentItemDetailaw();
                        businessDocumentItemDetailaw.LawId = lawByItem.LawId;
                        businessDocumentItemDetailaw.Text = lawByItem.Observation;
                        var law = Db.SingleById<Domain.Financials.DebtManagement.Law>(lawByItem.LawId);
                        if (law != null)
                        {
                            businessDocumentItemDetailaw.Name = law.Name;
                            businessDocumentItemDetailaw.Prescription = law.Prescription;
                        }
                        itemModel.LawTexts.Add(businessDocumentItemDetailaw);
                    }

                }

                document.Items.Add(itemModel);
            }

            var ProductBaseAmountId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
            var ProductInterestAmountId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();


            
            if (document.TypeId != 23)
            {
                var parentItemsBase = Db.Select<ServiceModel.BusinessDocuments.ParentItem>
                                (Db.From<BusinessDocumentItemLink>()
                                .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdil, bdi) => bdi.ProductId == ProductBaseAmountId
                                                                            && bdil.DocumentItemId == bdi.Id)
                                .Where(x => Sql.In(x.DocumentItemId, items.Select(y => y.Id).ToList())));
                foreach (var parentItem in parentItemsBase)
                {
                    parentItem.ProductName = Db.Select(Db.From<Product>().Join<Product, BusinessDocumentItem>((p, bdi) => p.Id == bdi.ProductId && bdi.Id == parentItem.DocumentItemRelatedId)).FirstOrDefault().Name;
                    document.ParentItems.Add(parentItem);
                }

                var parentItemsInterest = Db.Select<ServiceModel.BusinessDocuments.ParentItem>
                                            (Db.From<BusinessDocumentItemLink>()
                                            .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdil, bdi) => bdi.ProductId == ProductInterestAmountId
                                                                                        && bdil.DocumentItemId == bdi.Id)
                                            .Where(x => Sql.In(x.DocumentItemId, items.Select(y => y.Id).ToList())));
                foreach (var parentItem in parentItemsInterest)
                {
                    parentItem.ProductName = Db.Select(Db.From<Product>().Join<Product, BusinessDocumentItem>((p, bdi) => p.Id == bdi.ProductId && bdi.Id == parentItem.DocumentItemRelatedId)).FirstOrDefault().Name;
                    document.ParentItems.Where(x => x.DocumentItemRelatedId == parentItem.DocumentItemRelatedId).FirstOrDefault().AmountInterest = parentItem.Amount;
                }
            }
            else
            {
                var parentDocumentId = Db.Select(Db.From<BusinessDocumentLink>().Where(x => x.LinkedDocumentId == document.Id && x.TypeId == 2)).OrderByDescending(x => x.DocumentId).Select(x => x.DocumentId).FirstOrDefault();
                items = await Db.SelectAsync<BusinessDocumentItem>(w => w.BusinessDocumentId == parentDocumentId);

                var parentItemsBase = Db.Select<ServiceModel.BusinessDocuments.ParentItem>
                   (Db.From<BusinessDocumentItemLink>()
                   .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdil, bdi) => bdi.ProductId == ProductBaseAmountId
                                                               && bdil.DocumentItemId == bdi.Id)
                   .Where(x => Sql.In(x.DocumentItemId, items.Select(y => y.Id).ToList())));
                foreach (var parentItem in parentItemsBase)
                {
                    parentItem.ProductName = Db.Select(Db.From<Product>().Join<Product, BusinessDocumentItem>((p, bdi) => p.Id == bdi.ProductId && bdi.Id == parentItem.DocumentItemRelatedId)).FirstOrDefault().Name;
                    document.ParentItems.Add(parentItem);
                }

                var parentItemsInterest = Db.Select<ServiceModel.BusinessDocuments.ParentItem>
                                            (Db.From<BusinessDocumentItemLink>()
                                            .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdil, bdi) => bdi.ProductId == ProductInterestAmountId
                                                                                        && bdil.DocumentItemId == bdi.Id)
                                            .Where(x => Sql.In(x.DocumentItemId, items.Select(y => y.Id).ToList())));
                foreach (var parentItem in parentItemsInterest)
                {
                    parentItem.ProductName = Db.Select(Db.From<Product>().Join<Product, BusinessDocumentItem>((p, bdi) => p.Id == bdi.ProductId && bdi.Id == parentItem.DocumentItemRelatedId)).FirstOrDefault().Name;
                    document.ParentItems.Where(x => x.DocumentItemRelatedId == parentItem.DocumentItemRelatedId).FirstOrDefault().AmountInterest = parentItem.Amount;
                }


                foreach (var parentItem in document.ParentItems)
                {
                    parentItem.Applications = Db.Select<Api.BusinessDocumentItemApplication>(Db.From<BusinessDocumentItemLink>()
                                                                                                  .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdl,bdi) => bdl.DocumentItemId == bdi.Id)
                                                                                                  .Join<BusinessDocumentItem,BusinessDocument>((bdi,bd) => bdi.BusinessDocumentId == bd.Id && bd.TypeId == 23)
                                                                                                  .Join<BusinessDocumentItem, Product>((bdi,pr) => bdi.ProductId == pr.Id)
                                                                                                  .Where<BusinessDocumentItemLink>(bdil => bdil.DocumentItemRelatedId == parentItem.DocumentItemRelatedId)
                                                                                                  //                                         && bdil.AppliedAmount > 0)
                                                                                                                                           .Where<BusinessDocument>(bd => bd.Id != document.Id)
                                                                                                .Select<BusinessDocumentItemLink, BusinessDocument, Product>((bdil, bd, pr) => new
                                                                                                {
                                                                                                    ApplicationDocumentNumber = bd.Number,
                                                                                                    ApplicationDocumentId = bd.Id,
                                                                                                    ApplicationDocumentCreateDate = bd.CreateDate,
                                                                                                    ProductId = pr.Id,
                                                                                                    ProductName = pr.Name,
                                                                                                    FromDate = bdil.FromDate,
                                                                                                    ToDate = bdil.ToDate,
                                                                                                    Amount = bdil.Amount,
                                                                                                    AppliedAmount = bdil.AppliedAmount
                                                                                                })).ToList();
                    var itemRelated = Db.SingleById<BusinessDocumentItem>(parentItem.DocumentItemRelatedId);
                    parentItem.AppliedAmount = (double)itemRelated.AppliedAmount;

                }

            }



            // AuthorizationWorkflowInstance
            if (document.ApprovalWorkflowInstanceId.HasValue)
            {
                var workflowInstance = await _workflowInstanceRepository.GetWorkflowInstance(Db, Session, document.ApprovalWorkflowInstanceId.Value);
                document.CollectWorkflowInstance = workflowInstance;
            }

            // Edit Permissions
            document.EditPermissions = false;
            if (Session.Roles.Contains("admin"))
            {
                document.EditPermissions = true;
            }
            else
            {
                if (document.CollectWorkflowInstance != null && document.CollectWorkflowInstance.CurrentWorkflowActivityId != default(int))
                {
                    var userRolesIds = Db.Select(Db.From<Domain.System.UserRole>().Where(ur => ur.UserId == Session.UserId)).ToList().Select(ur => ur.RoleId).ToList();
                    var activityRolesIds = Db.Select(Db.From<WorkflowActivityRole>().Where(war => Sql.In(war.RoleId, userRolesIds) && war.WorkflowActivityId == document.CollectWorkflowInstance.CurrentWorkflowActivityId)).Select(war => war.Id);
                    if (activityRolesIds != null)
                    {
                        document.EditPermissions = true;
                    }
                }
            }

            // Messages
            var q = Db.From<Domain.System.Messages.Message>()
                .Join<Domain.System.Messages.Message, Domain.System.User>((m, u) => m.SenderId == u.Id)
                .Join<Domain.System.User, Domain.System.Persons.Person>()
                .Join<Domain.System.Messages.Message, Domain.System.Messages.MessageThread>()
                .Join<Domain.System.Messages.MessageThread, BusinessDocument>()
                .Where<BusinessDocument>(x => x.Id == request.Id);

            var messages = Db.Select<ServiceModel.System.Messages.Message.QueryResult>(q);
            var modelMessages = new List<ServiceModel.System.Messages.Message.QueryResult>();

            foreach (var rootMessage in messages.Where(x => !x.ReplyToMessageId.HasValue).ToList())
            {
                modelMessages.Add(AddMessage(messages, rootMessage));
            }

            document.Messages = modelMessages;

            // Reckonings
            var businessDocumentReckoningsIds = Db.Select(Db.From<BusinessDocumentLink>().Where(bdl => bdl.TypeId == 1 && bdl.DocumentId == document.Id)).Select(bdl => bdl.LinkedDocumentId);
            var businessDocumentReckonings = Db.Select(Db.From<BusinessDocument>().Where(bd => Sql.In(bd.Id, businessDocumentReckoningsIds))).ToList();


            var businessDocumentLinkedNumbers =
                Db.Select<Api.BusinessDocumentLinkResults>(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.LinkedDocumentId && (bdl.TypeId == 2 || bdl.TypeId == 3) && Sql.In(bdl.DocumentId, businessDocumentReckoningsIds), Db.JoinAlias("Links"))
                .Select<BusinessDocument, BusinessDocumentLink>((bd, bdl) => new
                {
                    bd.Id,
                    bd.Number,
                    LinkedId = Sql.JoinAlias(bdl.DocumentId, "Links")
                }));

            foreach (var businessDocumentReckoning in businessDocumentReckonings)
            {
                var user = Db.SingleById<User>(businessDocumentReckoning.CreatedBy);

                Api.BusinessDocumentHeader businessDocumentHeader = new Api.BusinessDocumentHeader();
                businessDocumentHeader.CreateDate = businessDocumentReckoning.CreateDate;
                businessDocumentHeader.CreateUser = user.Name;
                businessDocumentHeader.Total = businessDocumentReckoning.Total;
                businessDocumentHeader.Number = businessDocumentReckoning.Number;
                businessDocumentHeader.VoidDate = businessDocumentReckoning.VoidDate;
                businessDocumentHeader.FromServiceDate = businessDocumentReckoning.FromServiceDate;
                businessDocumentHeader.ToServiceDate = businessDocumentReckoning.ToServiceDate;

                var typeName = documentTypes.Where(bdt => bdt.Id == businessDocumentReckoning.TypeId).FirstOrDefault().ShortName;
                businessDocumentHeader.TypeName = typeName != null ? typeName : "";

                businessDocumentHeader.LinkedNumber = businessDocumentLinkedNumbers.Where(bdlr => bdlr.LinkedId == businessDocumentReckoning.Id).Select(bdlr => bdlr.Number).FirstOrDefault();

                var productId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
                var baseAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentReckoning.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();
                businessDocumentHeader.Base = baseAmount;

                productId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();
                var interestAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentReckoning.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();

                businessDocumentHeader.Interest = interestAmount;
                businessDocumentHeader.Id = businessDocumentReckoning.Id;
                document.Reckonings.Add(businessDocumentHeader);
            }

            // PaymentCoupons
            var businessDocumentPaymentCouponsIds = Db.Select(Db.From<BusinessDocumentLink>().Where(bdl => bdl.TypeId == 2 && bdl.DocumentId == document.Id)).Select(bdl => bdl.LinkedDocumentId);
            var businessDocumentPaymentCoupons = Db.Select(Db.From<BusinessDocument>().Where(bd => Sql.In(bd.Id, businessDocumentPaymentCouponsIds))).ToList();
            foreach (var businessDocumentPaymentCoupon in businessDocumentPaymentCoupons)
            {
                var user = Db.SingleById<User>(businessDocumentPaymentCoupon.CreatedBy);

                Api.BusinessDocumentHeader businessDocumentHeader = new Api.BusinessDocumentHeader();
                businessDocumentHeader.CreateDate = businessDocumentPaymentCoupon.CreateDate;
                businessDocumentHeader.CreateUser = user.Name;
                businessDocumentHeader.Total = businessDocumentPaymentCoupon.Total;
                businessDocumentHeader.Number = businessDocumentPaymentCoupon.Number;
                businessDocumentHeader.VoidDate = businessDocumentPaymentCoupon.VoidDate;
                businessDocumentHeader.FromServiceDate = businessDocumentPaymentCoupon.FromServiceDate;
                businessDocumentHeader.ToServiceDate = businessDocumentPaymentCoupon.ToServiceDate;
                businessDocumentHeader.CurrentWorkflowActivityName = "";

                if (businessDocumentPaymentCoupon.ApprovalWorkflowInstanceId != null)
                {
                    var workflowInstance = Db.SingleById<WorkflowInstance>(businessDocumentPaymentCoupon.ApprovalWorkflowInstanceId);
                    var workflowActivity = Db.SingleById<WorkflowActivity>(workflowInstance.CurrentWorkflowActivityId);
                    businessDocumentHeader.CurrentWorkflowActivityName = workflowInstance.IsTerminated ? "Cancelada" : workflowActivity.Name;
                }

                var typeName = documentTypes.Where(bdt => bdt.Id == businessDocumentPaymentCoupon.TypeId).FirstOrDefault().ShortName;
                businessDocumentHeader.TypeName = typeName != null ? typeName : "";

                var productId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
                var baseAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentPaymentCoupon.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();
                businessDocumentHeader.Base = baseAmount;

                productId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();
                var interestAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentPaymentCoupon.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();

                businessDocumentHeader.Interest = interestAmount;
                businessDocumentHeader.Id = businessDocumentPaymentCoupon.Id;
                document.PaymentCoupons.Add(businessDocumentHeader);

            }


            //ExecutionDocuments
            var businessDocumentExecutionIds = Db.Select(Db.From<BusinessDocumentLink>().Where(bdl => bdl.TypeId == 3 && bdl.DocumentId == document.Id)).Select(bdl => bdl.LinkedDocumentId);
            var businessDocumentExecutions = Db.Select(Db.From<BusinessDocument>().Where(bd => Sql.In(bd.Id, businessDocumentExecutionIds))).ToList();
            foreach (var businessDocumentExecution in businessDocumentExecutions)
            {
                var user = Db.SingleById<User>(businessDocumentExecution.CreatedBy);

                Api.BusinessDocumentHeader businessDocumentHeader = new Api.BusinessDocumentHeader();
                businessDocumentHeader.CreateDate = businessDocumentExecution.CreateDate;
                businessDocumentHeader.CreateUser = user.Name;
                businessDocumentHeader.Total = businessDocumentExecution.Total;
                businessDocumentHeader.Number = businessDocumentExecution.Number;
                businessDocumentHeader.VoidDate = businessDocumentExecution.VoidDate;
                businessDocumentHeader.CurrentWorkflowActivityName = "";

                if (businessDocumentExecution.ApprovalWorkflowInstanceId != null)
                {
                    var workflowInstance = Db.SingleById<WorkflowInstance>(businessDocumentExecution.ApprovalWorkflowInstanceId);
                    var workflowActivity = Db.SingleById<WorkflowActivity>(workflowInstance.CurrentWorkflowActivityId);
                    businessDocumentHeader.CurrentWorkflowActivityName = workflowInstance.IsTerminated ? "Cancelado" : workflowActivity.Name;
                }

                var typeName = documentTypes.Where(bdt => bdt.Id == businessDocumentExecution.TypeId).FirstOrDefault().ShortName;
                businessDocumentHeader.TypeName = typeName != null ? typeName : "";

                var productId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
                var baseAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentExecution.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();
                businessDocumentHeader.Base = baseAmount;

                productId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();
                var interestAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentExecution.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();

                businessDocumentHeader.Interest = interestAmount;
                businessDocumentHeader.Id = businessDocumentExecution.Id;
                document.ExecutionDocuments.Add(businessDocumentHeader);

            }

            var organism = Db.Select(Db.From<Domain.Financials.DebtManagement.Organism>().Where(o => o.TypeId == 1 && o.PersonId == document.IssuerId)).SingleOrDefault();



            var businessDocumentParent = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.TypeId == 3 && bdl.LinkedDocumentId == request.Id)).OrderByDescending(x => x.Id).FirstOrDefault(); //traigo expediente
            if (businessDocumentParent == null)
            {
                businessDocumentParent = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.TypeId == 2 && bdl.LinkedDocumentId == request.Id)).OrderBy(x => x.Id).FirstOrDefault(); //traigo expediente
            }

            var debtorId = document.Items.FirstOrDefault().Debtors.FirstOrDefault();
            if (debtorId == null || debtorId == default(int))
            {
                debtorId = Db.Select(Db.From<BusinessDocumentItemDebtor>().Join<BusinessDocumentItemDebtor, BusinessDocumentItem>((bdid, bdi) => bdid.BusinessDocumentItemId == bdi.Id && bdi.BusinessDocumentId == businessDocumentParent.Id)).FirstOrDefault().DebtorId;
            }

            



            var debtor = Db.SingleById<Domain.Financials.DebtManagement.Debtor>(debtorId);
            var debtorPerson = Db.SingleById<Domain.System.Persons.Person>(debtor.PersonId);
            var debtorAddress = Db.Select(Db.From<Domain.System.Location.Address>().Join<Domain.System.Persons.PersonAddress>().Where<Domain.System.Persons.PersonAddress>(x => x.PersonId == debtorPerson.Id)).FirstOrDefault();
            var person = Db.SingleById<Domain.System.Persons.Person>(organism.PersonId);
            document.IssuerId = organism.Id;
            document.IssuerName = person.Name;
            document.DebtorName = debtorPerson.Name;
            document.DebtorCode = debtorPerson.Code;

            var creditorId = document.Items.FirstOrDefault().Creditors.FirstOrDefault();
            if (creditorId == null || creditorId == default(int))
            {
                creditorId = Db.Select(Db.From<BusinessDocumentItemCreditor>().Join<BusinessDocumentItemCreditor, BusinessDocumentItem>((bdid, bdi) => bdid.BusinessDocumentItemId == bdi.Id && bdi.BusinessDocumentId == businessDocumentParent.Id)).FirstOrDefault().CreditorId;
            }

            var creditor = Db.SingleById<Domain.Financials.DebtManagement.Creditor>(creditorId);
            var bankAccount = Db.Select(Db.From<Domain.Financials.BankAccount>().Where(x => x.PersonId == creditor.PersonId)).FirstOrDefault();
            if (bankAccount != null)
            {
                document.CreditorBankAccountCode = bankAccount.Code ?? "";
                document.CreditorBankAccountNumber = bankAccount.Number ?? "";
                document.CreditorBankAccountDescription = bankAccount.Description ?? "";
            }



            document.DebtorAddress = debtorAddress == null ? "" : debtorAddress.Street + " " + debtorAddress.StreetNumber + " " + (debtorAddress.Floor ?? "") + " " + (debtorAddress.Appartment ?? "") + " (C.P. " + (debtorAddress.ZipCode ?? "sin dato") + ")";

            return document;
        }

        private async Task<Api.PostBusinessDocumentCollection> GetBusinessDocumentCollectExecute(Api.GetBusinessDocumentCollectExecute request)
        {
            //SELECT * FROM BusinessDocumentTypes
            var documentTypes = Db.Select(Db.From<BusinessDocumentType>()).ToList();
            // SELECT * FROM BusinessDocument Where id = request.id
            var document = (await Db.SingleByIdAsync<BusinessDocument>(request.Id)).ConvertTo<Api.PostBusinessDocumentCollection>();
           
            var businessDocumentParent = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.TypeId == 3 && bdl.LinkedDocumentId == request.Id)).OrderByDescending(x => x.Id).FirstOrDefault(); //traigo expediente

            var documentTypeName = documentTypes.Where(bdt => bdt.Id == document.TypeId).FirstOrDefault().ShortName;
            document.TypeName = documentTypeName != null ? documentTypeName : "";
                            
            try
            {
                Conversions converter = new Conversions();
                document.TotalText = converter.enletras(document.Total.ToString());
            }
            catch (Exception)
            {
                throw;
            }

            var items = await Db.SelectAsync<BusinessDocumentItem>(w => w.BusinessDocumentId == document.Id);

            List<Product> products = null;
            var siteIds = new List<int>();
            if (items.Count > 0)
            {
                var productIds = items.Select(x => x.ProductId);
                products = await Db.SelectAsync<Product>(w => Sql.In(w.Id, productIds));

                siteIds.AddRange(items.Where(w => w.InventorySiteId.HasValue).Select(x => x.InventorySiteId.Value).Distinct());
            }
            if (document.InventorySiteId.HasValue)
            {
                siteIds.AddIfNotExists(document.InventorySiteId.Value);
            }

            var sites = await Db.SelectAsync<InventorySite>(w => Sql.In(w.Id, siteIds));
            foreach (var item in items)
            {
                var itemModel = item.ConvertTo<Api.BusinessDocumentItemDetail>();
                if (!request.Edit && products != null)
                {
                    itemModel.Product = products.Single(x => x.Id == itemModel.ProductId);
                }
                if (!request.Edit && sites != null && itemModel.InventorySiteId != null)
                {
                    itemModel.Site = sites.Single(x => x.Id == itemModel.InventorySiteId);
                }

                var creditorsByItem = Db.Select(Db.From<BusinessDocumentItemCreditor>().Where(bdic => bdic.BusinessDocumentItemId == item.Id)).Select(x => x.CreditorId).ToList();
                itemModel.Creditors = creditorsByItem;

                var debtorsByItem = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(bdid => bdid.BusinessDocumentItemId == item.Id)).Select(x => x.DebtorId).ToList();
                itemModel.Debtors = debtorsByItem;

                var lawsByItem = Db.Select(Db.From<BusinessDocumentItemLaw>().Where(bdil => bdil.BusinessDocumentItemId == item.Id)).ToList();

                if (lawsByItem != null)
                {
                    itemModel.LawTexts = new List<Api.BusinessDocumentItemDetailaw>();
                    foreach (var lawByItem in lawsByItem)
                    {
                        Api.BusinessDocumentItemDetailaw businessDocumentItemDetailaw = new Api.BusinessDocumentItemDetailaw();
                        businessDocumentItemDetailaw.LawId = lawByItem.LawId;
                        businessDocumentItemDetailaw.Text = lawByItem.Observation;
                        var law = Db.SingleById<Domain.Financials.DebtManagement.Law>(lawByItem.LawId);
                        if (law != null)
                        {
                            businessDocumentItemDetailaw.Name = law.Name;
                            businessDocumentItemDetailaw.Prescription = law.Prescription;
                        }
                        itemModel.LawTexts.Add(businessDocumentItemDetailaw);
                    }

                }

                document.Items.Add(itemModel);
            }


            var ProductBaseAmountId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
            var ProductInterestAmountId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();

            if (businessDocumentParent == null)
            {
                businessDocumentParent = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.TypeId == 2 && bdl.LinkedDocumentId == request.Id)).OrderByDescending(x => x.Id).FirstOrDefault(); //triago expediente
            }
            
            var parentItems = await Db.SelectAsync<BusinessDocumentItem>(w => w.BusinessDocumentId == businessDocumentParent.Id);

            //Normativas
            document.Normatives = String.Join("",
                                    Db.Select(Db.From<Domain.Financials.DebtManagement.Normative>()
                                    .Join<Domain.Financials.DebtManagement.Normative, Domain.Financials.DebtManagement.NormativeLaw>((n, nl) => nl.NormativeId == n.Id)
                                    .Join<Domain.Financials.DebtManagement.NormativeLaw, BusinessDocumentItemLaw>((nl, bdil) => nl.LawId == bdil.LawId)
                                    .Join<BusinessDocumentItemLaw, BusinessDocumentItem>((bdil, bdi) => bdil.BusinessDocumentItemId == bdi.Id)// && bdi.BusinessDocumentId == businessDocumentParent.Id)
                                    .Join<BusinessDocumentItem, BusinessDocumentItemLink>((bdi, bdilink) => bdi.Id == bdilink.DocumentItemRelatedId && Sql.In(bdilink.DocumentItemId, parentItems.Select(x => x.Id)))
                                    ).Select(x => x.Name).Distinct().ToList()
                                    );


            var parentItemsBase = Db.Select<ServiceModel.BusinessDocuments.ParentItem>
                            (Db.From<BusinessDocumentItemLink>()
                            .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdil, bdi) => bdi.ProductId == ProductBaseAmountId
                                                                        && bdil.DocumentItemId == bdi.Id)
                            .Where(x => Sql.In(x.DocumentItemId, parentItems.Select(y => y.Id).ToList())));
            foreach (var parentItem in parentItemsBase)
            {
                parentItem.ProductName = Db.Select(Db.From<Product>().Join<Product, BusinessDocumentItem>((p, bdi) => p.Id == bdi.ProductId && bdi.Id == parentItem.DocumentItemRelatedId)).FirstOrDefault().Name;
                if (parentItem.ProductName.ToUpper() == "FACTURA")
                {
                    var objects = JArray.Parse(Db.SingleById<BusinessDocumentItem>(parentItem.DocumentItemRelatedId).FieldsJSON);
                    foreach (JObject root in objects)
                    {
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            if (app.Key == "value")
                            {
                                parentItem.ProductName = (String)app.Value;
                                parentItem.ProductName = (String)app.Value;
                            }

                        }
                    }
                }
                document.ParentItems.Add(parentItem);
            }

            var parentItemsInterest = Db.Select<ServiceModel.BusinessDocuments.ParentItem>
                                        (Db.From<BusinessDocumentItemLink>()
                                        .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdil, bdi) => bdi.ProductId == ProductInterestAmountId
                                                                                    && bdil.DocumentItemId == bdi.Id)
                                        .Where(x => Sql.In(x.DocumentItemId, parentItems.Select(y => y.Id).ToList())));
            foreach (var parentItem in parentItemsInterest)
            {
                parentItem.ProductName = Db.Select(Db.From<Product>().Join<Product, BusinessDocumentItem>((p, bdi) => p.Id == bdi.ProductId && bdi.Id == parentItem.DocumentItemRelatedId)).FirstOrDefault().Name;
                document.ParentItems.Where(x => x.DocumentItemRelatedId == parentItem.DocumentItemRelatedId).FirstOrDefault().AmountInterest = parentItem.Amount;
            }


            foreach (var parentItem in document.ParentItems)
            {
                parentItem.Applications = Db.Select<Api.BusinessDocumentItemApplication>(Db.From<BusinessDocumentItemLink>()
                                                                                              .Join<BusinessDocumentItemLink, BusinessDocumentItem>((bdl, bdi) => bdl.DocumentItemId == bdi.Id)
                                                                                              .Join<BusinessDocumentItem, BusinessDocument>((bdi, bd) => bdi.BusinessDocumentId == bd.Id && bd.TypeId == 23)
                                                                                              .Join<BusinessDocumentItem, Product>((bdi, pr) => bdi.ProductId == pr.Id)
                                                                                              .Where<BusinessDocumentItemLink>(bdil => bdil.DocumentItemRelatedId == parentItem.DocumentItemRelatedId)
                                                                                                                                       .Where<BusinessDocument>(bd => bd.Id != document.Id
                                                                                                                                       && bd.Id < document.Id)
                                                                                            .Select<BusinessDocumentItemLink, BusinessDocument, Product>((bdil, bd, pr) => new
                                                                                            {
                                                                                                ApplicationDocumentNumber = bd.Number,
                                                                                                ApplicationDocumentId = bd.Id,
                                                                                                ApplicationDocumentCreateDate = bd.CreateDate,
                                                                                                ProductId = pr.Id,
                                                                                                ProductName = pr.Name,
                                                                                                FromDate = bdil.FromDate,
                                                                                                ToDate = bdil.ToDate,
                                                                                                Amount = bdil.Amount,
                                                                                                AppliedAmount = bdil.AppliedAmount
                                                                                            })).ToList();
                var itemRelated = Db.SingleById<BusinessDocumentItem>(parentItem.DocumentItemRelatedId);
                parentItem.AppliedAmount = (double)itemRelated.AppliedAmount;

            }


            // AuthorizationWorkflowInstance
            if (document.ApprovalWorkflowInstanceId.HasValue)
            {
                var workflowInstance = await _workflowInstanceRepository.GetWorkflowInstance(Db, Session, document.ApprovalWorkflowInstanceId.Value);
                document.CollectWorkflowInstance = workflowInstance;
            }

            // Edit Permissions
            document.EditPermissions = false;
            if (Session.Roles.Contains("admin"))
            {
                document.EditPermissions = true;
            }
            else
            {
                if (document.CollectWorkflowInstance != null && document.CollectWorkflowInstance.CurrentWorkflowActivityId != default(int))
                {
                    var userRolesIds = Db.Select(Db.From<Domain.System.UserRole>().Where(ur => ur.UserId == Session.UserId)).ToList().Select(ur => ur.RoleId).ToList();
                    var activityRolesIds = Db.Select(Db.From<WorkflowActivityRole>().Where(war => Sql.In(war.RoleId, userRolesIds) && war.WorkflowActivityId == document.CollectWorkflowInstance.CurrentWorkflowActivityId)).Select(war => war.Id);
                    if (activityRolesIds != null)
                    {
                        document.EditPermissions = true;
                    }
                }
            }

            // Messages
            var q = Db.From<Domain.System.Messages.Message>()
                .Join<Domain.System.Messages.Message, Domain.System.User>((m, u) => m.SenderId == u.Id)
                .Join<Domain.System.User, Domain.System.Persons.Person>()
                .Join<Domain.System.Messages.Message, Domain.System.Messages.MessageThread>()
                .Join<Domain.System.Messages.MessageThread, BusinessDocument>()
                .Where<BusinessDocument>(x => x.Id == request.Id);

            var messages = Db.Select<ServiceModel.System.Messages.Message.QueryResult>(q);
            var modelMessages = new List<ServiceModel.System.Messages.Message.QueryResult>();

            foreach (var rootMessage in messages.Where(x => !x.ReplyToMessageId.HasValue).ToList())
            {
                modelMessages.Add(AddMessage(messages, rootMessage));
            }

            document.Messages = modelMessages;

            // Reckonings
            var businessDocumentReckoningsIds = Db.Select(Db.From<BusinessDocumentLink>().Where(bdl => bdl.TypeId == 1 && bdl.DocumentId == document.Id)).Select(bdl => bdl.LinkedDocumentId);
            var businessDocumentReckonings = Db.Select(Db.From<BusinessDocument>().Where(bd => Sql.In(bd.Id, businessDocumentReckoningsIds))).ToList();


            var businessDocumentLinkedNumbers =
                Db.Select<Api.BusinessDocumentLinkResults>(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.LinkedDocumentId && (bdl.TypeId == 2 || bdl.TypeId == 3) && Sql.In(bdl.DocumentId, businessDocumentReckoningsIds), Db.JoinAlias("Links"))
                .Select<BusinessDocument, BusinessDocumentLink>((bd, bdl) => new
                {
                    bd.Id,
                    bd.Number,
                    LinkedId = Sql.JoinAlias(bdl.DocumentId, "Links")
                }));

            foreach (var businessDocumentReckoning in businessDocumentReckonings)
            {
                var user = Db.SingleById<User>(businessDocumentReckoning.CreatedBy);

                Api.BusinessDocumentHeader businessDocumentHeader = new Api.BusinessDocumentHeader();
                businessDocumentHeader.CreateDate = businessDocumentReckoning.CreateDate;
                businessDocumentHeader.CreateUser = user.Name;
                businessDocumentHeader.Total = businessDocumentReckoning.Total;
                businessDocumentHeader.Number = businessDocumentReckoning.Number;
                businessDocumentHeader.VoidDate = businessDocumentReckoning.VoidDate;
                businessDocumentHeader.FromServiceDate = businessDocumentReckoning.FromServiceDate;
                businessDocumentHeader.ToServiceDate = businessDocumentReckoning.ToServiceDate;

                var typeName = documentTypes.Where(bdt => bdt.Id == businessDocumentReckoning.TypeId).FirstOrDefault().ShortName;
                businessDocumentHeader.TypeName = typeName != null ? typeName : "";

                businessDocumentHeader.LinkedNumber = businessDocumentLinkedNumbers.Where(bdlr => bdlr.LinkedId == businessDocumentReckoning.Id).Select(bdlr => bdlr.Number).FirstOrDefault();

                var productId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
                var baseAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentReckoning.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();
                businessDocumentHeader.Base = baseAmount;

                productId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();
                var interestAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentReckoning.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();

                businessDocumentHeader.Interest = interestAmount;
                businessDocumentHeader.Id = businessDocumentReckoning.Id;
                document.Reckonings.Add(businessDocumentHeader);
            }

            // PaymentCoupons
            var businessDocumentPaymentCouponsIds = Db.Select(Db.From<BusinessDocumentLink>().Where(bdl => bdl.TypeId == 2 && bdl.DocumentId == document.Id)).Select(bdl => bdl.LinkedDocumentId);
            var businessDocumentPaymentCoupons = Db.Select(Db.From<BusinessDocument>().Where(bd => Sql.In(bd.Id, businessDocumentPaymentCouponsIds))).ToList();
            foreach (var businessDocumentPaymentCoupon in businessDocumentPaymentCoupons)
            {
                var user = Db.SingleById<User>(businessDocumentPaymentCoupon.CreatedBy);

                Api.BusinessDocumentHeader businessDocumentHeader = new Api.BusinessDocumentHeader();
                businessDocumentHeader.CreateDate = businessDocumentPaymentCoupon.CreateDate;
                businessDocumentHeader.CreateUser = user.Name;
                businessDocumentHeader.Total = businessDocumentPaymentCoupon.Total;
                businessDocumentHeader.Number = businessDocumentPaymentCoupon.Number;
                businessDocumentHeader.VoidDate = businessDocumentPaymentCoupon.VoidDate;
                businessDocumentHeader.FromServiceDate = businessDocumentPaymentCoupon.FromServiceDate;
                businessDocumentHeader.ToServiceDate = businessDocumentPaymentCoupon.ToServiceDate;
                businessDocumentHeader.CurrentWorkflowActivityName = "";

                if (businessDocumentPaymentCoupon.ApprovalWorkflowInstanceId != null)
                {
                    var workflowInstance = Db.SingleById<WorkflowInstance>(businessDocumentPaymentCoupon.ApprovalWorkflowInstanceId);
                    var workflowActivity = Db.SingleById<WorkflowActivity>(workflowInstance.CurrentWorkflowActivityId);
                    businessDocumentHeader.CurrentWorkflowActivityName = workflowInstance.IsTerminated ? "Cancelada" : workflowActivity.Name;
                }

                var typeName = documentTypes.Where(bdt => bdt.Id == businessDocumentPaymentCoupon.TypeId).FirstOrDefault().ShortName;
                businessDocumentHeader.TypeName = typeName != null ? typeName : "";

                var productId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
                var baseAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentPaymentCoupon.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();
                businessDocumentHeader.Base = baseAmount;

                productId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();
                var interestAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentPaymentCoupon.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();

                businessDocumentHeader.Interest = interestAmount;
                businessDocumentHeader.Id = businessDocumentPaymentCoupon.Id;
                document.PaymentCoupons.Add(businessDocumentHeader);

            }


            //ExecutionDocuments
            var businessDocumentExecutionIds = Db.Select(Db.From<BusinessDocumentLink>().Where(bdl => bdl.TypeId == 3 && bdl.DocumentId == document.Id)).Select(bdl => bdl.LinkedDocumentId);
            var businessDocumentExecutions = Db.Select(Db.From<BusinessDocument>().Where(bd => Sql.In(bd.Id, businessDocumentExecutionIds))).ToList();
            foreach (var businessDocumentExecution in businessDocumentExecutions)
            {
                var user = Db.SingleById<User>(businessDocumentExecution.CreatedBy);

                Api.BusinessDocumentHeader businessDocumentHeader = new Api.BusinessDocumentHeader();
                businessDocumentHeader.CreateDate = businessDocumentExecution.CreateDate;
                businessDocumentHeader.CreateUser = user.Name;
                businessDocumentHeader.Total = businessDocumentExecution.Total;
                businessDocumentHeader.Number = businessDocumentExecution.Number;
                businessDocumentHeader.VoidDate = businessDocumentExecution.VoidDate;
                businessDocumentHeader.CurrentWorkflowActivityName = "";

                if (businessDocumentExecution.ApprovalWorkflowInstanceId != null)
                {
                    var workflowInstance = Db.SingleById<WorkflowInstance>(businessDocumentExecution.ApprovalWorkflowInstanceId);
                    var workflowActivity = Db.SingleById<WorkflowActivity>(workflowInstance.CurrentWorkflowActivityId);
                    businessDocumentHeader.CurrentWorkflowActivityName = workflowActivity.Name;
                }

                var typeName = documentTypes.Where(bdt => bdt.Id == businessDocumentExecution.TypeId).FirstOrDefault().ShortName;
                businessDocumentHeader.TypeName = typeName != null ? typeName : "";

                var productId = Db.Select(Db.From<Product>().Where(p => p.Name == "DEUDA ORIGINAL")).Select(p => p.Id).FirstOrDefault();
                var baseAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentExecution.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();
                businessDocumentHeader.Base = baseAmount;

                productId = Db.Select(Db.From<Product>().Where(p => p.Name == "INTERES")).Select(p => p.Id).FirstOrDefault();
                var interestAmount = Db.Select(Db.From<BusinessDocumentItem>().Where(bdi => bdi.BusinessDocumentId == businessDocumentExecution.Id && bdi.ProductId == productId)).Select(bdi => bdi.UnitPrice).FirstOrDefault();

                businessDocumentHeader.Interest = interestAmount;
                businessDocumentHeader.Id = businessDocumentExecution.Id;
                document.ExecutionDocuments.Add(businessDocumentHeader);

            }

            ///////////////////////////
            var organism = Db.Select(Db.From<Domain.Financials.DebtManagement.Organism>().Where(o => o.TypeId == 1 && o.PersonId == document.IssuerId)).SingleOrDefault();
            if (businessDocumentParent == null)
            {
                businessDocumentParent = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.TypeId == 2 && bdl.LinkedDocumentId == request.Id)).OrderBy(x => x.Id).FirstOrDefault(); //triago expediente
            }


            var debtorId = document.Items.FirstOrDefault().Debtors.FirstOrDefault();
            if (debtorId == null || debtorId == default(int))
            {
                var debtorTmp = Db.Select(Db.From<BusinessDocumentItemDebtor>().Join<BusinessDocumentItemDebtor, BusinessDocumentItem>((bdid, bdi) => bdid.BusinessDocumentItemId == bdi.Id && bdi.BusinessDocumentId == businessDocumentParent.Id)).FirstOrDefault();
                if (debtorTmp == null)
                {
                    businessDocumentParent.Id = Db.Select(Db.From<BusinessDocument>().Join<BusinessDocumentLink>((bd, bdl) => bd.Id == bdl.DocumentId && bdl.TypeId == 2 && bdl.LinkedDocumentId == request.Id)).Min(x => x.Id); 
                }
                debtorId = Db.Select(Db.From<BusinessDocumentItemDebtor>().Join<BusinessDocumentItemDebtor, BusinessDocumentItem>((bdid, bdi) => bdid.BusinessDocumentItemId == bdi.Id && bdi.BusinessDocumentId == businessDocumentParent.Id)).FirstOrDefault().DebtorId;
            }
            
            var debtor = Db.SingleById<Domain.Financials.DebtManagement.Debtor>(debtorId);
            var debtorPerson = Db.SingleById<Domain.System.Persons.Person>(debtor.PersonId);
            //direccion constituida
            var debtorAddress = Db.Select(Db.From<Domain.System.Location.Address>().Join<Domain.System.Persons.PersonAddress>().Where<Domain.System.Persons.PersonAddress>(x => x.PersonId == debtorPerson.Id && x.TypeId == 5)).FirstOrDefault();
            if (debtorAddress == null)
            {
                document.DebtorAddress = "";
            }
            else
            {
                document.DebtorAddress = debtorAddress == null ? "" : debtorAddress.Street + " N° " + debtorAddress.StreetNumber;
                if (!String.IsNullOrEmpty(debtorAddress.Floor))
                {
                    document.DebtorAddress += " Piso " + debtorAddress.Floor;
                }
                if (!String.IsNullOrEmpty(debtorAddress.Appartment))
                {
                    document.DebtorAddress += " Dpto " + debtorAddress.Appartment;
                }
                document.DebtorAddress += " (C.P. " + (debtorAddress.ZipCode ?? "sin dato") + ")"; 
            }
            
            if (debtorAddress != null)
            {
                if (debtorAddress.PlaceId != default(int))
                {
                    var parentPlace = Db.SingleById<Domain.System.Location.Place>(debtorAddress.PlaceId);
                    if (parentPlace != null)
                    {
                        document.DebtorAddress += " " + parentPlace.Name;
                        if (parentPlace.ParentId != null && parentPlace.ParentId != default(int))
                        {
                            parentPlace = Db.SingleById<Domain.System.Location.Place>(parentPlace.ParentId);
                            if (parentPlace != null)
                            {
                                document.DebtorAddress += " - " + parentPlace.Name;
                            }
                        }
                    }
                }
            }


            //direccion NO constituida
            debtorAddress = Db.Select(Db.From<Domain.System.Location.Address>().Join<Domain.System.Persons.PersonAddress>().Where<Domain.System.Persons.PersonAddress>(x => x.PersonId == debtorPerson.Id && x.TypeId != 5)).FirstOrDefault();
            if (debtorAddress == null)
            {
                document.DebtorAddressTwo = "";
            }
            else
            {
                document.DebtorAddressTwo = debtorAddress == null ? "" : debtorAddress.Street + " N° " + debtorAddress.StreetNumber;
                if (!String.IsNullOrEmpty(debtorAddress.Floor))
                {
                    document.DebtorAddressTwo += " Piso " + debtorAddress.Floor;
                }
                if (!String.IsNullOrEmpty(debtorAddress.Appartment))
                {
                    document.DebtorAddressTwo += " Dpto " + debtorAddress.Appartment;
                }
                document.DebtorAddressTwo += " (C.P. " + (debtorAddress.ZipCode ?? "sin dato") + ")";
            }

            if (debtorAddress != null)
            {
                if (debtorAddress.PlaceId != default(int))
                {
                    var parentPlace = Db.SingleById<Domain.System.Location.Place>(debtorAddress.PlaceId);
                    if (parentPlace != null)
                    {
                        document.DebtorAddressTwo += " " + parentPlace.Name;
                        if (parentPlace.ParentId != null && parentPlace.ParentId != default(int))
                        {
                            parentPlace = Db.SingleById<Domain.System.Location.Place>(parentPlace.ParentId);
                            if (parentPlace != null)
                            {
                                document.DebtorAddressTwo += " - " + parentPlace.Name;
                            }
                        }
                    }
                }

            }

            var person = Db.SingleById<Domain.System.Persons.Person>(organism.PersonId);
            document.IssuerId = organism.Id;
            document.IssuerName = person.Name;
            document.IssuerCode = person.Code;
            document.DebtorName = debtorPerson.Name;
            document.DebtorCode = debtorPerson.Code;
            document.DebtorRNOS = debtorPerson.RNOS ?? null;

            var creditorId = document.Items.FirstOrDefault().Creditors.FirstOrDefault();
            if (creditorId == null || creditorId == default(int))
            {
                creditorId = Db.Select(Db.From<BusinessDocumentItemCreditor>().Join<BusinessDocumentItemCreditor, BusinessDocumentItem>((bdid, bdi) => bdid.BusinessDocumentItemId == bdi.Id && bdi.BusinessDocumentId == businessDocumentParent.Id)).FirstOrDefault().CreditorId;
            }

            var creditor = Db.SingleById<Domain.Financials.DebtManagement.Creditor>(creditorId);
            var bankAccount = Db.Select(Db.From<Domain.Financials.BankAccount>().Where(x => x.PersonId == creditor.PersonId)).FirstOrDefault();
            Domain.Financials.BankBranch bankBranch = new Domain.Financials.BankBranch();
            if (bankAccount != null && bankAccount.BankBranchId != null)
            {
                bankBranch = Db.SingleById<Domain.Financials.BankBranch>(bankAccount.BankBranchId);
            }
            document.CreditorBankAccountCode = bankAccount.Code ?? "";
            document.CreditorBankAccountNumber = bankAccount.Number ?? "";
            document.CreditorBankAccountDescription = bankAccount.Description ?? "";
            document.CreditorBankAccountBranch = bankBranch != null ? bankBranch.Name ?? "" : "";
            var creditorPerson = Db.SingleById<Domain.System.Persons.Person>(creditor.PersonId);
            document.CreditorName = creditorPerson.Name ?? "";
            document.CreditorCode = creditorPerson.Code ?? "";


            

            return document;
        }

        [Authenticate]
        public async Task<byte[]> Get(Api.GetBusinessDocumentGetTePDF request)
        {

            Api.GetBusinessDocumentCollectExecute functionRequest = new Api.GetBusinessDocumentCollectExecute();
            functionRequest.Edit = false;
            functionRequest.Id = request.Id;
            var contentRequest = await GetBusinessDocumentCollectExecute(functionRequest);

            var httpClient = new HttpClient();
            var content = new StringContent(contentRequest.ToJson(), Encoding.UTF8, "application/json");
            var functionUrl = "";
            if (contentRequest.TypeId == 31)
            {
                functionUrl = "http://localhost:7071/api/modelonota";
                //functionUrl = "https://func-centraloperativa.azurewebsites.net/api/modelonota?code=ta4j1hCDrqh4VebZMA4w/fRoObWkAvaS/BIbiCAr28O7AsbBrEXZUA==";
            }
            else
            {
                if (contentRequest.TypeId == 30)
                {
                    functionUrl = "http://localhost:7071/api/igb";
                    //functionUrl = "https://func-centraloperativa.azurewebsites.net/api/igb?code=ta4j1hCDrqh4VebZMA4w/fRoObWkAvaS/BIbiCAr28O7AsbBrEXZUA==";
                }
                else
                {
                    //functionUrl = "http://localhost:7071/api/anexotedpgrcf";
                    functionUrl = "http://localhost:7071/api/titulodpgrcf";
                    //functionUrl = "https://func-centraloperativa.azurewebsites.net/api/titulodpgrcf?code=1DdkpdKQvkfwf2A7mIosbt3SZLu9FsG8Samw/OuKUNtp0wwttJ32SA==";
                }
            }
            var response = await httpClient.PostAsync(functionUrl, content);
            return await response.Content.ReadAsByteArrayAsync();
        }

        [Authenticate]
        public async Task<byte[]> Get(Api.GetBusinessDocumentGetTeAttachPDF request)
        {

            Api.GetBusinessDocumentCollectExecute functionRequest = new Api.GetBusinessDocumentCollectExecute();
            functionRequest.Edit = false;
            functionRequest.Id = request.Id;
            var contentRequest = await GetBusinessDocumentCollectExecute(functionRequest);

            var httpClient = new HttpClient();
            var content = new StringContent(contentRequest.ToJson(), Encoding.UTF8, "application/json");
            var functionUrl = "";

            //functionUrl = "https://func-centraloperativa.azurewebsites.net/api/anexotedpgrcf?code=lYINOnPBWuSQ8bvPEOdmznOFopTeSTmYamdnbsUMn9XuA8ZMVSuCaw==";
            functionUrl = "http://localhost:7071/api/anexotedpgrcf";

            var response = await httpClient.PostAsync(functionUrl, content);

            return await response.Content.ReadAsByteArrayAsync();
        }

        [Authenticate]
        public async Task<byte[]> Get(Api.GetBusinessDocumentGetPDF request)
        {
            Api.GetBusinessDocumentCollectExecute functionRequest = new Api.GetBusinessDocumentCollectExecute();
            functionRequest.Edit = false;
            functionRequest.Id = request.Id;
            var contentRequest = await GetBusinessDocumentCollectExecute(functionRequest);

            var httpClient = new HttpClient();
            var content = new StringContent(contentRequest.ToJson(), Encoding.UTF8, "application/json");
            var functionUrl = "";

            functionUrl = "http://localhost:7071/api/naddpgrcf";
            //functionUrl = "https://func-centraloperativa.azurewebsites.net/api/naddpgrcf?code=73MiGxEnBEHTdReazjaFq3SWcoaN80XoClU1OD29GqCJMzzH2Ujogw==";

            var response = await httpClient.PostAsync(functionUrl, content);

            return await response.Content.ReadAsByteArrayAsync();
        
        //var businessDocument = Db.SingleById<BusinessDocument>(request.Id);

        //var businessDocumentTypeParams = Db.Select(Db.From<BusinessDocumentTypeParam>().Where(x => x.TypeId == businessDocument.TypeId
        //                                                                                 && x.TenantId == Session.TenantId)).FirstOrDefault();
        //var businessDocumentItems = Db.Select(Db.From<BusinessDocumentItem>().Where(x => x.BusinessDocumentId == businessDocument.Id)).ToList();
        //var businessDocumentItemsIds = businessDocumentItems.Select(x => x.Id);

        //var businessDocumentDebtors = Db.Select(Db.From<BusinessDocumentItemDebtor>().Where(x => Sql.In(x.BusinessDocumentItemId, businessDocumentItemsIds)));
        //var Debtor = Db.SingleById<Domain.Financials.DebtManagement.Debtor>(businessDocumentDebtors.FirstOrDefault().DebtorId);
        //var DebtorPerson = Db.SingleById<Domain.System.Persons.Person>(Debtor.PersonId);

        //var Receiver = Db.SingleById<Domain.System.Persons.Person>(businessDocument.ReceiverId);

        ////var Categories = Db.Select(Db.From<Category>().Where(x => x.Name == "CONCEPTOS ADMINISTRATIVOS" || x.Name == "CONCEPTOS JUDICIALES" || x.Name == "CONCEPTOS APREMIO"));
        ////var categoriesIds = Categories.ToList().Select(x => x.Id);               

        //Globalization.CultureInfo culture = new Globalization.CultureInfo("es-AR");
        //string DocumentDate = businessDocument.DocumentDate.ToString("dddd dd \\de MMMM, yyyy", culture);

        //var BusinessDocumentOriginalNumber = businessDocument.Number.ToString();
        //var subNumberPosition = BusinessDocumentOriginalNumber.IndexOf("NAD") == -1 ? BusinessDocumentOriginalNumber.IndexOf("TE") : BusinessDocumentOriginalNumber.IndexOf("NAD");

        //if (subNumberPosition > 0)
        //{
        //    BusinessDocumentOriginalNumber = BusinessDocumentOriginalNumber.Substring(0, subNumberPosition - 1);
        //}

        //var businessDocumentDebtCollectNumber = businessDocument.Number.ToString();
        //if (subNumberPosition > 0)
        //{
        //    businessDocumentDebtCollectNumber = businessDocumentDebtCollectNumber.Substring(subNumberPosition, businessDocumentDebtCollectNumber.Length - subNumberPosition);
        //}


        //var businessDocumentDebtorList = "";

        //var debtorsItems = Db.Select(Db.From<Domain.System.Persons.Person>()
        //                                .Join<Domain.System.Persons.Person, Domain.Financials.DebtManagement.Debtor>((p, d) => p.Id == d.PersonId)
        //                                .Join<Domain.Financials.DebtManagement.Debtor, BusinessDocumentItemDebtor>((d, id) => d.Id == id.DebtorId)
        //                                .Join<BusinessDocumentItemDebtor, BusinessDocumentItem>((id, i) => id.BusinessDocumentItemId == i.Id
        //                                                                                                    && i.BusinessDocumentId == businessDocument.Id)
        //    ).Select(p => new { p.Id, p.Code, p.Name }).Distinct();

        ////SelectDistinct(dt => new { dt.Bar, dt.Foo });

        //foreach (var debtor in debtorsItems)
        //{
        //    businessDocumentDebtorList += "<p> Nombre / Razón social : " + debtor.Name + "</p>";
        //    businessDocumentDebtorList += "<p> Documento de Identidad : " + debtor.Code + "</p>";
        //    var debtorAddress = Db.Select(Db.From<Domain.System.Location.Address>()
        //                                        .Join<Domain.System.Location.Address, Domain.System.Persons.PersonAddress>((a, pa) => a.Id == pa.AddressId && pa.PersonId == debtor.Id)).FirstOrDefault();


        //    if (debtorAddress != null)
        //    {
        //        businessDocumentDebtorList += "<p> Domicilio constituido (Art 31 Ley N° 10.397) : " 
        //                                        + ( debtorAddress.Street == null ? "" : debtorAddress.Street) 
        //                                        + ( debtorAddress.StreetNumber == null ? "" : debtorAddress.StreetNumber) 
        //                                        + ( debtorAddress.Floor == null ? "" : debtorAddress.Floor)
        //                                        + (debtorAddress.Appartment == null ? "" : debtorAddress.Appartment)
        //                                        + "</p>";
        //    }
        //    else
        //    {
        //        businessDocumentDebtorList += "<p> Domicilio constituido (Art 31 Ley N° 10.397) : </p>";
        //    }
        //    businessDocumentDebtorList += "<p></p>";
        //}


        //var BusinessDocumentNadConceptsTable = "<table><tbody>";

        ////Conceptos A
        //var originalAmounts = Db.Select(Db.From<BusinessDocumentItem>().Join<Product>((bdi, pr) => bdi.ProductId == pr.Id && bdi.BusinessDocumentId == businessDocument.Id
        //                                                                    && pr.Name == "DEUDA ORIGINAL")).Select(x => x.UnitPrice);
        //BusinessDocumentNadConceptsTable += "<tr>";
        //BusinessDocumentNadConceptsTable += "<td>";
        //BusinessDocumentNadConceptsTable += "<p>A) Monto Original:</p>";
        //BusinessDocumentNadConceptsTable += "</td>";
        ////hay que iterar por valores filtrando por categoria de producto/concepto
        //BusinessDocumentNadConceptsTable += "<td>";
        //foreach (var amount in originalAmounts)
        //{
        //    BusinessDocumentNadConceptsTable += "<p>" + amount.ToString("C") + "</p>";   
        //}
        //BusinessDocumentNadConceptsTable += "</td>";
        //BusinessDocumentNadConceptsTable += "</tr>";

        ////Conceptos B
        //var interestAmounts = Db.Select(Db.From<BusinessDocumentItem>().Join<Product>((bdi, pr) => bdi.ProductId == pr.Id && bdi.BusinessDocumentId == businessDocument.Id
        //                                                                    && pr.Name == "INTERES")).Select(x => x.UnitPrice);
        //BusinessDocumentNadConceptsTable += "<tr>";
        //BusinessDocumentNadConceptsTable += "<td>";
        //BusinessDocumentNadConceptsTable += "<p>B) Intereses:</p>";
        //BusinessDocumentNadConceptsTable += "<p>Art. 96 Ley 10.397 (modif. y compl.)</p>";
        //BusinessDocumentNadConceptsTable += "</td>";
        ////hay que iterar por valores filtrando por categoria de producto/concepto
        //BusinessDocumentNadConceptsTable += "<td>";
        //foreach (var amount in interestAmounts)
        //{
        //    BusinessDocumentNadConceptsTable += "<p>" + amount.ToString("C") + "</p>";

        //}
        //if (interestAmounts.Count() < 2) { BusinessDocumentNadConceptsTable += "<p>&nbsp;</p>"; }
        //BusinessDocumentNadConceptsTable += "</td>";
        //BusinessDocumentNadConceptsTable += "</tr>";

        ////Conceptos C
        //var otherAmounts = Db.Select(Db.From<BusinessDocumentItem>().Join<Product>((bdi, pr) => bdi.ProductId == pr.Id && bdi.BusinessDocumentId == businessDocument.Id
        //                                                        && (pr.Name == "TASA" || pr.Name == "ARANCEL"))).Select(x => x.UnitPrice);
        //BusinessDocumentNadConceptsTable += "<tr>";
        //BusinessDocumentNadConceptsTable += "<td>";
        //BusinessDocumentNadConceptsTable += "<p>C) Tasas  Retributivas de Servicios Administrativos:</p>";
        //BusinessDocumentNadConceptsTable += "<p>(Art. 328 y 331  Ley Nº 10.397 (modif. y compl.), Art. 68 Inc. A) ap. 1) y  ap. 3) Ley 14.880)</p>";
        //BusinessDocumentNadConceptsTable += "</td>";
        ////hay que iterar por valores filtrando por categoria de producto/concepto
        //BusinessDocumentNadConceptsTable += "<td>";
        //foreach (var amount in otherAmounts)
        //{
        //    BusinessDocumentNadConceptsTable += "<p>" + amount.ToString("C") + "0</p>";

        //}
        //if (otherAmounts.Count() < 2) { BusinessDocumentNadConceptsTable += "<p>&nbsp;</p>"; }
        ////BusinessDocumentNadConceptsTable += "<p style=\"font-size:9px; font-weight: bold;\"> $58,00</p>";
        //BusinessDocumentNadConceptsTable += "</td>";
        //BusinessDocumentNadConceptsTable += "</tr>";

        //BusinessDocumentNadConceptsTable += "</tbody></table>";


        //var template = Db.SingleById<Domain.System.Notifications.EmailTemplate>(businessDocumentTypeParams.PrintingTemplateId);

        //var tokenizer = new Tokenizer();
        //Conversions converter = new Conversions();
        //var businessDocumentAmountInteres = originalAmounts.Sum() + interestAmounts.Sum();
        //var businessDocumentAmountInteresText = converter.enletras(businessDocumentAmountInteres.ToString());

        //var businessDocumentOthers = otherAmounts.Sum();
        //var businessDocumentOthersText = converter.enletras(businessDocumentOthers.ToString());

        //Dictionary<string, object> SubstitutionData = new Dictionary<string, object>();
        //try
        //{
        //    var documentTokens = new List<Token>()
        //        {
        //            //new Token("BusinessDocumentNadConceptsTable",BusinessDocumentNadConceptsTable),
        //            new Token("BusinessDocumentNumber", businessDocument.Number.ToString()),
        //            new Token("BusinessDocumentDateText", DocumentDate),
        //            new Token("BusinessDocumentReceiverName", DebtorPerson.Name.ToString()),
        //            new Token("BusinessDocumentReceiverAddressStreet", "Street Callao"),
        //            new Token("BusinessDocumentReceiverAddressStreetNumber", "StreetNumber 101"),
        //            new Token("BusinessDocumentVoidDate", businessDocument.VoidDate.ToString()),
        //            new Token("BusinessDocumentTotal", businessDocument.Total.ToString()),
        //            new Token("BusinessDocumentAmountInteresText",businessDocumentAmountInteresText),
        //            new Token("BusinessDocumentAmountInteres",businessDocumentAmountInteres.ToString()),
        //            new Token("BusinessDocumentOthersText",businessDocumentOthersText),
        //            new Token("BusinessDocumentOthers",businessDocumentOthers.ToString()),
        //            new Token("BusinessDocumentReceiverCode", DebtorPerson.Code == null ? "Sin Dato":DebtorPerson.Code),
        //            new Token("BusinessDocumentOriginalNumber", BusinessDocumentOriginalNumber),
        //            new Token("businessDocumentDebtCollectNumber", businessDocumentDebtCollectNumber),
        //            new Token("businessDocumentDebtorList", businessDocumentDebtorList)
        //        };
        //    documentTokens.ForEach(x => SubstitutionData[x.Key] = x.Value);
        //    template.Body = tokenizer.Replace(template.Body.Trim(), documentTokens);
        //}
        //catch(Exception ex)
        //{
        //    throw ex;
        //}


        //var httpClient = new HttpClient();
        //var content = new StringContent(template.ToJson(), Encoding.UTF8, "application/json");

        //var response = await httpClient.PostAsync("https://func-centraloperativa.azurewebsites.net/api/converthtmltopdf?code=02L43OvRGis9E4lEvFRAeMGylwhlxQsmbjNhW5avyI9KGjW1wvqqJw==", content);

        ////var fileLogicNameDocument = businessDocument.Number.ToString() + DateTime.Now.ToString("yyyyMMdd") + ".pdf";
        ////Byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
        ////var file = await _fileRepository.CreateFile(Db, responseBytes, "centraloperativa-files", fileLogicNameDocument, "application/pdf");


        //return await response.Content.ReadAsByteArrayAsync();
        ////return response;
    }
        public Api.PostBusinessDocumentFileResult Post(Api.PostBusinessDocumentFile request)
        {
            var path = Path.Combine(Path.GetTempPath(), "CentralOperativa");
            Directory.CreateDirectory(path);

            var files = Request.Files;
            var httpFile = files[0];

            var strDate = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            var fileName = strDate + httpFile.FileName;

            httpFile.SaveTo(Path.Combine(path, fileName));
            request.FileName = fileName;

            var result = request.ConvertTo<Api.PostBusinessDocumentFileResult>();
            result.FileName = fileName;

            return result;
        }
        [Authenticate]
        public async Task<Api.ImportBusinessDocumentsResult> Post(Api.ImportBusinessDocuments request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    Api.ImportBusinessDocumentsResult result;
                    var path = Path.Combine(Path.GetTempPath(), "CentralOperativa");
                    var fileName = Path.Combine(path, request.FileName);
                    if (File.Exists(fileName))
                    {
                        if (fileName.EndsWith("xlsx"))
                        {
                            result = await ProcessXLS(request, fileName);
                        }
                        else
                        {
                            result = new Api.ImportBusinessDocumentsResult();
                            result.InsertedItemsCount = 0;
                        }
                    }
                    else
                    {
                        result = new Api.ImportBusinessDocumentsResult();
                        result.InsertedItemsCount = 0;
                    }

                    trx.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        [Authenticate]
        private async Task<Api.ImportBusinessDocumentsResult> ProcessXLS(Api.ImportBusinessDocuments request, string fileName)
        {
            try
            {
                const int itemNumberIndex = 1;
                const int itemDateIndex = 2;
                const int itemVoidDateIndex = 3;
                const int itemNotificationDateIndex = 4;
                const int conceptIndex = 8;
                const int amountIndex = 9;
                const int balanceIndex = 10;

                var tenantPersonId = (Db.SingleById<Tenant>(Session.TenantId)).PersonId;
                var result = request.ConvertTo<Api.ImportBusinessDocumentsResult>();
                var products = Db.Select(Db.From<Product>().Where(p => p.TenantId == Session.TenantId)).ToList();

                var creditor = Db.SingleById<Domain.Financials.DebtManagement.Creditor>(request.CreditorId);
                var debtor = Db.SingleById<Domain.Financials.DebtManagement.Debtor>(request.DebtorId);

                if (creditor == null || debtor == null)
                {
                    throw HttpError.Conflict("Missing Information");
                }



                using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                    
                        ///
                        DataSet dataset = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });

                    

                        var users = Db.Select(Db.From<User>());
                        var tenantUsers = Db.Select(Db.From<TenantUser>().Where(tu => tu.TenantId == Session.TenantId));
                        var controlDate = DateTime.Parse("01/08/2020", new CultureInfo("es-AR"));


                        Domain.System.Persons.Person person = new Domain.System.Persons.Person();
                        BusinessDocument businessDocument = new BusinessDocument();
                        List<BusinessDocumentItem> businessDocumentItems = new List<BusinessDocumentItem>();

                        var rowIndex = 0;
                        var insertedItemsCount = 0;
                        foreach (DataTable table in dataset.Tables)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                if (rowIndex >= 8)
                                {
                                    var itemNumber = row.ItemArray[itemNumberIndex];
                                    var itemDate = row.ItemArray[itemDateIndex];
                                    var itemVoidDate = row.ItemArray[itemVoidDateIndex];
                                    var itemNotificationDate = row.ItemArray[itemNotificationDateIndex];
                                    var concept = row.ItemArray[conceptIndex] ?? "";
                                    var amount = row.ItemArray[amountIndex];
                                    var balance = row.ItemArray[balanceIndex];
                                

                                    if (rowIndex == 8)
                                    {
                                        businessDocument.TypeId = 25; //Papel
                                        businessDocument.IssuerId = 55305;
                                        businessDocument.ReceiverId = 55305;
                                        businessDocument.ItemTypesId = 0; //1-productos 2-servicios
                                        businessDocument.Number = request.BusinessDocumentNumber;
                                        businessDocument.Guid = Guid.NewGuid();
                                        businessDocument.Status = BusinessDocumentStatus.PendingApproval;
                                        businessDocument.CreatedBy = Session.UserId;
                                        businessDocument.CreateDate = DateTime.Now;
                                        businessDocument.DocumentDate = DateTime.Now;
                                        businessDocument.VoidDate = DateTime.Now;
                                        businessDocument.NotificationDate = DateTime.Now;
                                        businessDocument.CategoryId = 11; //SAMO
                                                                          //businessDocument.VoidDate = DateTime.ParseExact(voidDate.ToString(), "yyyyMMdd", new ApiSystem.CultureInfo("es-AR", false)); // Convert.ToDateTime(voidDate);
                                        businessDocument.Total = 0;
                                        businessDocument.Id = (int)Db.Insert(businessDocument, true);
                                    }
                                

                                    if (businessDocument.Id != default(int))
                                    {
                                        BusinessDocumentItem businessDocumentItem = new BusinessDocumentItem
                                        {
                                            ProductId = 12,
                                            UnitTypeId = 7,
                                            Quantity = 1,
                                            UnitPrice = Convert.ToDecimal(balance.ToString()),
                                            VatRate = 0,
                                            Bonus = 0,
                                            ItemDate = Convert.ToDateTime(itemDate.ToString()),
                                            VoidDate = Convert.ToDateTime(itemVoidDate.ToString()),
                                            NotificationDate = Convert.ToDateTime(itemNotificationDate.ToString()),
                                            FieldsJSON = "[{\"id\":3,\"name\":\"Numero de factura\",\"type\":\"text\",\"list\": [],\"value\":\"" + (itemNumber ?? "") + "\"} ]",
                                            BusinessDocumentId = businessDocument.Id,
                                            OriginalAmount = Convert.ToDecimal(balance.ToString()),
                                            AppliedInterest = 0,
                                            AppliedAmount = 0
                                        };


                                        if (businessDocumentItem.VoidDate.Value.Year < 2015 || (businessDocumentItem.VoidDate.Value.Year == 2015 && businessDocumentItem.VoidDate.Value.Month < 8))
                                        {
                                            //FACTURAS EMITIDAS ANTES 1/08/2015 son 10 años
                                            businessDocumentItem.PrescriptionDate = businessDocumentItem.VoidDate.Value.AddYears(10);
                                            if (businessDocumentItem.PrescriptionDate > controlDate)
                                            {
                                                businessDocumentItem.PrescriptionDate = DateTime.Parse("01/08/2020", new CultureInfo("es-AR"));
                                            }
                                        }
                                        else
                                        {
                                            //FACTURAS EMITIDAS DESPUES DEL 1/08/2015 son 5 años
                                            businessDocumentItem.PrescriptionDate = businessDocumentItem.VoidDate.Value.AddYears(5);
                                        }

                                        businessDocumentItem.Id = (int)Db.Insert(businessDocumentItem, true);
                                        businessDocumentItems.Add(businessDocumentItem);

                                        BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor
                                        {
                                            BusinessDocumentItemId = businessDocumentItem.Id,
                                            Amount = businessDocumentItem.UnitPrice,
                                            CreditorId = creditor.Id
                                        };
                                        businessDocumentItemCreditor.Id = (int)Db.Insert(businessDocumentItemCreditor, true);

                                        BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor
                                        {
                                            BusinessDocumentItemId = businessDocumentItem.Id,
                                            Amount = businessDocumentItem.UnitPrice,
                                            DebtorId = debtor.Id
                                        };
                                        businessDocumentItemDebtor.Id = (int)Db.Insert(businessDocumentItemDebtor, true);

                                        //76	CodCivil
                                        //75  CodCiv01082015
                                        if (businessDocumentItem.ItemDate.Value.Year < 2015 || (businessDocumentItem.ItemDate.Value.Year == 2015 && businessDocumentItem.ItemDate.Value.Month < 8))
                                        {
                                            BusinessDocumentItemLaw businessDocumentItemLaw = new BusinessDocumentItemLaw();
                                            businessDocumentItemLaw.LawId = 75;
                                            businessDocumentItemLaw.BusinessDocumentItemId = businessDocumentItem.Id;
                                            businessDocumentItemLaw.Id = (int)Db.Insert(businessDocumentItemLaw, true);
                                        }
                                        else
                                        {
                                            BusinessDocumentItemLaw businessDocumentItemLaw = new BusinessDocumentItemLaw();
                                            businessDocumentItemLaw.LawId = 76;
                                            businessDocumentItemLaw.BusinessDocumentItemId = businessDocumentItem.Id;
                                            businessDocumentItemLaw.Id = (int)Db.Insert(businessDocumentItemLaw, true);
                                        }

                                        insertedItemsCount++;
                                    }
                                }
                                rowIndex++;
                            }
                        }
                        ///
                        if (businessDocument.Id != default(int))
                        {
                            businessDocument.Total = businessDocumentItems.Sum(x => x.UnitPrice);
                            businessDocument.VoidDate = businessDocumentItems.Max(x => x.VoidDate);
                            businessDocument.CAEVoidDate = businessDocumentItems.Max(x => x.VoidDate);
                            businessDocument.NotificationDate = businessDocumentItems.Max(x => x.NotificationDate);
                            Db.Update(businessDocument);

                            Api.PostBusinessDocumentSubmitForCollect postBusinessDocumentSubmitForCollect = new Api.PostBusinessDocumentSubmitForCollect();
                            postBusinessDocumentSubmitForCollect.BusinessDocumentGuid = businessDocument.Guid;
                            await Post(postBusinessDocumentSubmitForCollect);
                        }

                        result.InsertedItemsCount = insertedItemsCount;

                    }
                }
        
                return result;

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }
        }

    }
}
