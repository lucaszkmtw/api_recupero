using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.BusinessDocuments;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Loans;
using CentralOperativa.Domain.System;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.BusinessPartners;
using CentralOperativa.ServiceInterface.Financials;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.ServiceModel.System.Notifications;
using CentralOperativa.ServiceInterface.System.Notifications;
using Api = CentralOperativa.ServiceModel.System.Workflows.WorkflowInstance;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    public class WorkflowInstanceRepository
    {
        private readonly TenantRepository _tenantRepository;
        private readonly BusinessPartnerRepository _businessPartnerRepository;
        private readonly WorkflowActivityRepository _workflowActivityRepository;
        private readonly PaymentDocumentRepository _paymentDocumentRepository;

        public WorkflowInstanceRepository(
            TenantRepository tenantRepository,
            BusinessPartnerRepository businessPartnerRepository,
            WorkflowActivityRepository workflowActivityRepository,
            PaymentDocumentRepository paymentDocumentRepository)
        {
            _tenantRepository = tenantRepository;
            _businessPartnerRepository = businessPartnerRepository;
            _workflowActivityRepository = workflowActivityRepository;
            _paymentDocumentRepository = paymentDocumentRepository;
        }

        public async Task<Api.GetWorkflowInstanceResponse> GetWorkflowInstance(IDbConnection db, Session session, int id)
        {
            try
            {
                var workflowInstance = (await db.SingleByIdAsync<WorkflowInstance>(id)).ConvertTo<Api.GetWorkflowInstanceResponse>();

                var roleIds = await db.ColumnAsync<int>(session.Roles.Contains("admin") ?
                    db.From<Role>().Where(w => w.Id != 1).Select(x => x.Id) :
                    db.From<Role>().Where(w => Sql.In(w.Name, session.Roles)));

                workflowInstance.Workflow = await db.SingleByIdAsync<Workflow>(workflowInstance.WorkflowId);

                var activities = await db.SelectAsync(
                    db.From<WorkflowActivity>()
                        .Where(x => x.WorkflowId == workflowInstance.WorkflowId)
                        .OrderBy(o => o.ListIndex));

                // CurrentWorkflowActivity
                workflowInstance.CurrentWorkflowActivity = await _workflowActivityRepository.GetWorkflowActivity(db, workflowInstance.CurrentWorkflowActivityId);

                // Permissions
                var assignedRoles = await db.SelectAsync<ServiceModel.System.Workflows.WorkflowInstanceAssignedRole>(db
                    .From<WorkflowInstanceAssignments>()
                    .Join<Role>()
                    .Where<WorkflowInstanceAssignments>(w => w.WorkflowInstanceId == workflowInstance.Id)
                    .And<WorkflowInstanceAssignments>(w => w.IsActive));
                workflowInstance.AssignedRoles = assignedRoles;

                workflowInstance.CanAssignToRoles = (await db.SqlListAsync<Role>("EXEC SYWFGetWorkflowInstanceAssignableRoles " + workflowInstance.Id)).OrderBy(x => x.Name).ToList();

                //Tags
                var tags = await db.SelectAsync<ServiceModel.System.Workflows.WorkflowInstanceTag.GetWorkflowInstanceTagResponse>(
                    db.From<WorkflowInstanceTag>()
                        .Join<WorkflowInstanceTag, WorkflowTagPath>((wit, wtp) => wit.WorkflowTagId == wtp.Id)
                        .Where(w => w.WorkflowInstanceId == workflowInstance.Id));
                workflowInstance.Tags.AddRange(tags);

                var userPermissionsQ =
                    db.From<WorkflowActivityRolePermission>()
                        .Join<WorkflowActivityRolePermission, Role>()
                        .Join<WorkflowActivityRolePermission, WorkflowActivityRole>()
                        .Join<WorkflowActivityRole, WorkflowInstance>((war, wi) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>()
                        .Where<WorkflowActivityRolePermission>(w => Sql.In(w.RoleId, roleIds))
                        .And<WorkflowInstanceAssignments>(w => w.IsActive)
                        .And<WorkflowInstance>(w => w.Id == workflowInstance.Id);

                userPermissionsQ.FromExpression = userPermissionsQ.FromExpression.Replace(
                    "(\"WorkflowInstances\".\"Id\" = \"WorkflowInstanceAssignments\".\"WorkflowInstanceId\")",
                    "(\"WorkflowInstances\".\"Id\" = \"WorkflowInstanceAssignments\".\"WorkflowInstanceId\" AND \"WorkflowActivityRoles\".\"RoleId\" = \"WorkflowInstanceAssignments\".\"RoleId\")");
                var userPermissions = await db.SelectAsync<Api.RoleWithPermission>(userPermissionsQ);
                workflowInstance.UserPermissions = userPermissions;

                // Si la instancia esta asignada a un rol al que el usuario no pertenece sólo podrá asignar a otros roles si el usuario es supervisor de alguno de los roles al cual está asignada la instancia.
                //.And<Domain.System.Workflows.WorkflowActivityRole>(w => Sql.In(w.RoleId, roleIds)));

                // Audit
                workflowInstance.CreatedBy = await db.SingleAsync(db.From<Domain.System.Persons.Person>().Join<User>().Where<User>(w => w.Id == workflowInstance.CreatedByUserId));
                var transitions = await db.SqlListAsync<Api.WorkflowInstanceHistoryGenericItem>("EXEC GetWorkflowInstanceTransitions @workflowInstanceId", new { workflowInstanceId = workflowInstance.Id });
                transitions.Insert(0, new Api.WorkflowInstanceHistoryGenericItem
                {
                    CreateDate = workflowInstance.CreateDate,
                    FromWorkflowActivityName = "Solicitud creada",
                    ToWorkflowActivityName = activities[0].Name,
                    PersonName = workflowInstance.CreatedBy.Name,
                });

                List<ServiceModel.System.Workflows.WorkflowInstanceAssignedRoleHistory> transitionsRole = await db.SelectAsync<ServiceModel.System.Workflows.WorkflowInstanceAssignedRoleHistory>(
                    db.From<WorkflowInstanceAssignments>()
                        .Join<WorkflowInstanceAssignments, Role>((c, o) => c.RoleId == o.Id)
                        .Join<WorkflowInstanceAssignments, User>((c, o) => c.UserId == o.Id)
                        .Join<User, Domain.System.Persons.Person>((c, o) => c.PersonId == o.Id)
                        .Where<WorkflowInstanceAssignments>(w => w.WorkflowInstanceId == workflowInstance.Id)
                    );

                foreach (var obj in transitionsRole)
                {
                    var objGeneric = new Api.WorkflowInstanceHistoryGenericItem
                    {
                        Type = 1,
                        CreateDate = obj.CreateDate,
                        User = obj.UsersName,
                        Rol = obj.RoleName,
                        PersonName = obj.PersonName
                    };
                    transitions.Add(objGeneric);
                }

                transitions = transitions.OrderBy(q => q.CreateDate).ToList();
                workflowInstance.History.AddRange(transitions);

                //Approvals
                List<WorkflowInstanceApproval> approvals = await db.SelectAsync(db.From<WorkflowInstanceApproval>().Where(w => w.WorkflowInstanceId == workflowInstance.Id));
                //TODO : Ver la manera de marcar las aprobaciones de la actividad actual

                foreach (var approval in approvals)
                {
                    var item = approval.ConvertTo<ServiceModel.System.Workflows.WorkflowInstanceApproval.GetResult>();
                    if (item.UserId.HasValue)
                    {
                        item.UserName = (await db.SingleByIdAsync<User>(item.UserId.Value)).Name;
                    }
                    if (item.RoleId.HasValue)
                    {
                        item.RoleName = (await db.SingleByIdAsync<Role>(item.RoleId.Value)).Name;
                    }
                    workflowInstance.Approvals.Add(item);
                }


                //Generar lista de Actividades posibles a setear
                workflowInstance.CanSetActivities = db.Select(db.From<WorkflowActivity>().Where(wa => wa.WorkflowId == workflowInstance.WorkflowId && wa.IsFinal == false)).ToList();

                return workflowInstance;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Api.GetWorkflowInstanceResponse> InsertWorkflowInstance(IDbConnection db, Session session, Api.PostWorkflowInstance workflowInstance)
        {
            var userId = session.UserId;
            workflowInstance.Progress = 0;
            workflowInstance.Id = (int) await db.InsertAsync((WorkflowInstance)workflowInstance, true);
            Save(db, userId, workflowInstance);

            // WorkflowInstanceRoles
            var activityRoles = await db.SelectAsync(db.From<WorkflowActivityRole>().Where(w => w.WorkflowActivityId == workflowInstance.CurrentWorkflowActivityId));

            //Assign workflowinstance roles
            foreach (var activityRole in activityRoles.Where(w => w.IsDefault))
            {
                var workflowInstanceAssignment = new WorkflowInstanceAssignments
                {
                    WorkflowInstanceId = workflowInstance.Id,
                    WorkflowActivityId = workflowInstance.CurrentWorkflowActivityId,
                    RoleId = activityRole.RoleId,
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    IsActive = true
                };
                await db.InsertAsync(workflowInstanceAssignment);
            }

            return await GetWorkflowInstance(db, session, workflowInstance.Id);
        }

        public async Task<Api.GetWorkflowInstanceResponse> UpdateWorkflowInstance(IDbConnection db, Session session, Api.PostWorkflowInstance workflowInstance)
        {
            var userId = session.UserId;
            Save(db, userId, workflowInstance);
            db.Update((WorkflowInstance)workflowInstance);
            return await GetWorkflowInstance(db, session, workflowInstance.Id);
        }

        private static void Save(IDbConnection db, int userId, Api.PostWorkflowInstance request)
        {
            //Tags
            var tagIds = request.Tags.Where(x => x.Id != 0).Select(x => x.Id).ToList();
            if (tagIds.Any())
            {
                db.Delete<WorkflowInstanceTag>(x => x.WorkflowInstanceId == request.Id && !Sql.In(x.Id, tagIds));
            }
            else
            {
                db.Delete<WorkflowInstanceTag>(x => x.WorkflowInstanceId == request.Id);
            }

            foreach (var tag in request.Tags)
            {
                if (tag.Id == 0 && tag.WorkflowTagId.HasValue)
                {
                    tag.WorkflowInstanceId = request.Id;
                    tag.CreateDate = DateTime.UtcNow;
                    tag.CreatedById = userId;
                    db.Insert(tag);
                }
            }
        }

        public async Task ApproveWorkflowInstance(IDbConnection db, Session session, int id)
        {
            var instance = await db.SingleByIdAsync<WorkflowInstance>(id);
            var workflow = await db.SingleByIdAsync<Workflow>(instance.WorkflowId);
            var activities = await db.SelectAsync(db.From<WorkflowActivity>().Where(w => w.WorkflowId == instance.WorkflowId));
            var currentActivity = activities.Single(x => x.Id == instance.CurrentWorkflowActivityId);
            var roleIds = await db.ColumnAsync<int>(db.From<Role>().Where(w => Sql.In(w.Name, session.Roles)).Select(x => x.Id));
            var workflowActivitiesTransitions = db.Select(db.From<WorkflowActivityTransition>().Where(x => x.WorkflowId == workflow.Id)).ToList();

            // Si la actividad actual tiene definidas reglas de aprobación, las proceso
            var workflowInstanceApprovalRules = (await db.SelectAsync<WorkflowInstanceApproval>(w => w.WorkflowInstanceId == instance.Id && w.WorkflowActivityId == currentActivity.Id)).ToList();
            if (workflowInstanceApprovalRules.Count > 0)
            {
                // UserApprovalRule
                var workflowInstanceApprovalRule = workflowInstanceApprovalRules.Where(w => w.UserId.HasValue && w.UserId == session.UserId).SingleOrDefault() ??
                                                   workflowInstanceApprovalRules.Where(w => w.RoleId.HasValue && roleIds.Contains(w.RoleId.Value)).FirstOrDefault();
                // RoleApprovalRule

                if (workflowInstanceApprovalRule != null && workflowInstanceApprovalRule.Status == WorkflowInstanceApprovalStatus.Pending)
                {
                    workflowInstanceApprovalRule.Status = WorkflowInstanceApprovalStatus.Approved;
                    workflowInstanceApprovalRule.Date = DateTime.UtcNow;
                    await db.UpdateAsync(workflowInstanceApprovalRule);
                }
            }

            var pendingApprovalRules = workflowInstanceApprovalRules.Where(x => x.Status == WorkflowInstanceApprovalStatus.Pending).Count() == 0;
            var isAdmin = session.HasRole("admin", null);

            if (isAdmin || workflowInstanceApprovalRules.Count == 0 || pendingApprovalRules)
            {
                var nextActivity = activities.First(x => x.ListIndex == currentActivity.ListIndex + 1);

                if (workflowActivitiesTransitions.Count() > 0)
                {
                    //se establecio reglas de transicion de actividades
                    var currentWorkflowActivityTransitions = workflowActivitiesTransitions.Where(x => x.FromWorkflowActivityId == currentActivity.Id);
                    if (currentWorkflowActivityTransitions.Count() > 0)
                    {
                        var currentWorkflowActivityTransitionsIds = currentWorkflowActivityTransitions.Select(x => x.ToWorkflowActivityId);
                        nextActivity = db.Select(db.From<WorkflowActivity>().Where(x => Sql.In(x.Id, currentWorkflowActivityTransitionsIds) && x.ListIndex > currentActivity.ListIndex)).OrderBy(x => x.ListIndex).FirstOrDefault();
                        if (nextActivity == null)
                        {
                            nextActivity = db.Select(db.From<WorkflowActivity>().Where(x => Sql.In(x.Id, currentWorkflowActivityTransitionsIds) && x.ListIndex <= currentActivity.ListIndex)).OrderBy(x => x.ListIndex).FirstOrDefault();
                        }
                    }
                    else
                    {
                        nextActivity = null;
                    }
                }
                
                if (nextActivity != null)
                {
                    var workflowActivityEvents = await db.SelectAsync(db.From<WorkflowActivityEvent>().Where(we => we.WorkflowActivityId == currentActivity.Id));
                    foreach (var workflowActivityEvent in workflowActivityEvents)
                    {
                        var workflowEvent = await db.SingleByIdAsync<WorkflowEvent>(workflowActivityEvent.WorkflowEventId);
                        if (workflowEvent != null)
                        {
                            switch (workflowEvent.Name)
                            {
                                case "CREATE_USER_FOR_TENANT":
                                    if (workflow.TypeId == (short)WellKnownWorkflowTypes.LeadApproval)
                                    {
                                        var lead = db.Select(db.From<Domain.CRM.Lead>().Where(x => x.AuthorizationWorkflowInstanceId == instance.Id)).FirstOrDefault();
                                        if (lead == null)
                                        {
                                            throw new ApplicationException("Lead missing");
                                        }

                                        var person = db.SingleById<Person>(lead.PersonId);
                                        if (person == null)
                                        {
                                            throw new ApplicationException("Person missing");
                                        }

                                        var email = db.Select(db.From<PersonEmail>().Where(p => p.PersonId == lead.PersonId)).FirstOrDefault();
                                        var currentUser = db.Select(db.From<User>().Where(x => x.PersonId == lead.PersonId)).FirstOrDefault();
                                        if (currentUser == null)
                                        {
                                            currentUser = new User();
                                            currentUser.PersonId = lead.PersonId;
                                            currentUser.Name = person.Code;
                                            currentUser.Password = Guid.NewGuid().ToString("n").Substring(0, 8);
                                            currentUser.Id = (int)db.Insert(currentUser, true);

                                            var fromAddress = "arventgroup@rapicobros.com";
                                            var toAddresses = new List<string>();
                                            var bccAddresses = new List<string> { "javiergindre@gmail.com", "sebastian.vigliola@gmail.com" };
                                            var to = email?.Address;
                                            if (!string.IsNullOrEmpty(to))
                                            {
                                                var emailTemplate = await db.LoadSingleByIdAsync<Domain.System.Notifications.EmailTemplate>(31);
                                                if (emailTemplate == null)
                                                {
                                                    throw new ApplicationException("Notification template missing");
                                                }
                                                Dictionary<string, object> SubstitutionData = new Dictionary<string, object>();
                                                var clientTokens = new List<Token>()
                                                    {
                                                        new Token("UserName", currentUser.Name),
                                                        new Token("UserPassword", currentUser.Password)
                                                    };
                                                clientTokens.ForEach(x => SubstitutionData[x.Key] = x.Value);
                                                var tokenizer = new Tokenizer();


                                                toAddresses.Add(to);
                                                var task = new MailingTask
                                                {
                                                    From = new EmailAddress
                                                    {
                                                        Name = fromAddress,
                                                        Address = fromAddress
                                                    },
                                                    Subject = emailTemplate.Subject,
                                                    Template = emailTemplate.Body,
                                                    To = toAddresses.Select(x =>
                                                        new EmailAddress { Name = x, Address = x }).ToList(),
                                                    Bcc = bccAddresses.Select(x =>
                                                        new EmailAddress { Name = x, Address = x }).ToList(),
                                                    UseSES = true
                                                };
                                                task.Template = tokenizer.Replace(task.Template.Trim(), clientTokens);
                                                task.Template += "";
                                                task.UseSES = false;
                                                await NotificationService.SendMail(task);

                                                //enviar mail de creacion
                                            }
                                        }
                                        var currentUserTenant = db.Select(db.From<TenantUser>().Where(x => x.UserId == currentUser.Id && x.TenantId == session.TenantId)).FirstOrDefault();
                                        if (currentUserTenant == null)
                                        {
                                            currentUserTenant = new TenantUser();
                                            currentUserTenant.TenantId = session.TenantId;
                                            currentUserTenant.UserId = currentUser.Id;
                                            currentUserTenant.CreateDate = DateTime.UtcNow;
                                            currentUserTenant.CreatedById = session.UserId;
                                            currentUserTenant.IsDefault = true;
                                            currentUserTenant.InitialState = "app.benefits.cupons";
                                            currentUserTenant.Id = (int)db.Insert(currentUserTenant, true);

                                        }

                                        UserRole userRole = new UserRole();
                                        userRole.UserId = currentUser.Id;
                                        userRole.RoleId = 3128; //FIXME
                                        db.Insert(userRole, true);

                                    }
                                    break;
                                case "STATE_TE_DEBT_COLLECTOR":
                                    //verificar si importe > 20k o menor a 20K
                                    //si es menor o igual pasa a estado IGB WorkflowActivityId = 2339
                                    //si es mayor pasa a estado DGJ WorkflowActivityId = 2340
                                    var businessDocument = db.Select(db.From<BusinessDocument>().Where(x => x.ApprovalWorkflowInstanceId == instance.Id)).FirstOrDefault();
                                    //El WorkFlowActiviteEvent tiene que tener param (JSON) y para este evento traer:

                                    var LimitAmount = 20000;
                                    var ActivityIdLessThan = 2339;
                                    var ActivityIdMoreThan = 2340;
                                    if (businessDocument.Total <= LimitAmount)
                                    {
                                        nextActivity = db.Select(db.From<WorkflowActivity>().Where(x => x.Id == ActivityIdLessThan)).FirstOrDefault();
                                    }
                                    else
                                    {
                                        nextActivity = db.Select(db.From<WorkflowActivity>().Where(x => x.Id == ActivityIdMoreThan)).FirstOrDefault();
                                    }

                                    break;
                            }
                        }
                    }

                    var transition = new WorkflowInstanceTransition
                    {
                        CreateDate = DateTime.UtcNow,
                        FromWorkflowActivityId = currentActivity.Id,
                        ToWorkflowActivityId = nextActivity.Id,
                        UserId = session.UserId,
                        WorkflowInstanceId = instance.Id,
                        IsTerminated = false
                    };
                    await db.InsertAsync(transition);

                    //Actualizo el id de actividad actual de la instancia del workflow
                    instance.CurrentWorkflowActivityId = nextActivity.Id;

                    // New activity approvals
                    var nextActivityRules = (await db.SelectAsync<WorkflowActivityApprovalRule>(w => w.WorkflowActivityId == nextActivity.Id)).ToList();
                    foreach (var approvalRule in nextActivityRules)
                    {
                        var workflowInstanceApproval = new WorkflowInstanceApproval
                        {
                            WorkflowInstanceId = instance.Id,
                            RoleId = approvalRule.RoleId,
                            UserId = approvalRule.UserId,
                            WorkflowActivityId = approvalRule.WorkflowActivityId,
                            Status = WorkflowInstanceApprovalStatus.Pending,
                            CreateDate = DateTime.UtcNow
                        };
                        await db.InsertAsync(workflowInstanceApproval);
                    }

                    //Current activity roles
                    var activityRoles = await db.SelectAsync<WorkflowActivityRole>(w => w.WorkflowActivityId == instance.CurrentWorkflowActivityId);

                    //Deactivate current active Assignments
                    await db.ExecuteNonQueryAsync("UPDATE WorkflowInstanceAssignments SET IsActive = 0 WHERE WorkflowInstanceId = @workflowInstanceId", new { workflowInstanceId = instance.Id });

                    //Assign workflowinstance
                    foreach (var activityRole in activityRoles.Where(x => x.IsDefault))
                    {
                        var workflowInstanceRole = new WorkflowInstanceAssignments
                        {
                            WorkflowInstanceId = instance.Id,
                            WorkflowActivityId = instance.CurrentWorkflowActivityId,
                            RoleId = activityRole.RoleId,
                            UserId = session.UserId,
                            CreateDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        await db.InsertAsync(workflowInstanceRole);
                    }

                    instance.Progress = (decimal)((nextActivity.ListIndex - 1) * 100) /
                                        activities.Where(x => !x.IsStart && !x.IsFinal).Count();

                    await db.UpdateAsync(instance);


                    // Hardcoding para BusinessDocumentWorkflow
                    if (workflow.TypeId == (short)WellKnownWorkflowTypes.InvoiceApproval)
                    {
                        var businessDocument = await db.SingleAsync<BusinessDocument>(x => x.ApprovalWorkflowInstanceId == instance.Id);
                        switch (nextActivity.ListIndex)
                        {
                            case 1: //26
                                businessDocument.Status = BusinessDocumentStatus.PendingApproval;
                                break;
                            case 2: // Autorización
                                businessDocument.Status = BusinessDocumentStatus.PendingApproval;
                                break;
                            case 3: // Pagar
                                businessDocument.Status = BusinessDocumentStatus.Approved;

                                // Generar OP
                                var paymentOrderRequest = new ServiceModel.Financials.PostPaymentDocumentRequest
                                {
                                    DocumentDate = DateTime.UtcNow,
                                    IssuerId = businessDocument.ReceiverId,
                                    ReceiverId = businessDocument.IssuerId,
                                    TypeId = 1
                                };

                                var paymentOrderItem = new Domain.Financials.PaymentDocumentItem
                                {
                                    LinkedDocumentTypeId = 6, // TODO: Pasar a servicio DI BusinessDocument
                                    LinkedDocumentId = businessDocument.Id,
                                    Description = "Cancelación factura N° " + businessDocument.Number,
                                    OriginalAmount = businessDocument.Total,
                                    AmountToPay = businessDocument.Total
                                };
                                paymentOrderRequest.Items.Add(paymentOrderItem.ConvertTo<CentralOperativa.ServiceModel.Financials.PaymentDocumentItemCollect>());
                                var paymentOrder = await _paymentDocumentRepository.CreatePaymentDocument(db, session, paymentOrderRequest);

                                // PaymentDocumentLink
                                var paymentDocumentLink = new Domain.Financials.PaymentDocumentLink
                                {
                                    PaymentDocumentId = paymentOrder.Id,
                                    LinkedDocumentTypeId = 6, // Loan
                                    LinkedDocumentId = businessDocument.Id
                                };
                                await db.InsertAsync(paymentDocumentLink);

                                break;
                            case 4: // Pagado
                                businessDocument.Status = BusinessDocumentStatus.Paid;
                                break;
                        }
                        await db.UpdateAsync(businessDocument);
                    }

                    //HardCoding Para Remitos
                    if (workflow.TypeId == (short)WellKnownWorkflowTypes.InventoryDispatch)
                    {
                        var businessDocument = (await db.SelectAsync<BusinessDocument>(x => x.ApprovalWorkflowInstanceId == instance.Id)).Single();
                        switch (nextActivity.ListIndex)
                        {
                            case 1: //Armado Pedido
                                businessDocument.Status = BusinessDocumentStatus.InProcess;
                                break;
                            case 2: // A Retirar
                                businessDocument.Status = BusinessDocumentStatus.PendingDelivery;
                                break;
                            case 3: // En transito
                                businessDocument.Status = BusinessDocumentStatus.InTransit;
                                await UpdateInventory(db, businessDocument);
                                break;
                            case 4: // Recibido
                                businessDocument.Status = BusinessDocumentStatus.Delivered;
                                break;
                        }
                        await db.UpdateAsync(businessDocument);
                    }

                    if (workflow.TypeId == (short)WellKnownWorkflowTypes.InventoryReceipt)
                    {
                        var businessDocument = (await db.SelectAsync<BusinessDocument>(x => x.ApprovalWorkflowInstanceId == instance.Id)).Single();
                        switch (nextActivity.ListIndex)
                        {
                            case 1: // Control Recepción
                                businessDocument.Status = BusinessDocumentStatus.Control;
                                break;
                            case 2: // Ubicación depósito
                                businessDocument.Status = BusinessDocumentStatus.Delivered;
                                await UpdateInventory(db, businessDocument);
                                break;
                        }
                        await db.UpdateAsync(businessDocument);
                    }
                    
                        // Hardcoding para Loans
                        if (instance.WorkflowId == 37) // Loans para TDD
                        {
                        var loan = await db.SingleAsync<Loan>(x => x.AuthorizationWorkflowInstanceId == instance.Id);
                        switch (nextActivity.Id)
                        {
                            case 147: // Evaluación
                                loan.Status = LoanStatus.InEvaluation;
                                break;
                            case 148: // Firma documentación
                                loan.Status = LoanStatus.Approved;
                                break;
                            case 149: // Liquidacion
                                loan.Status = LoanStatus.ToExecute;
                                break;
                            case 150: // Gestión pago
                                loan.Status = LoanStatus.PendingPayment;

                                //Generar la OP, no permitir ir para atras el WI desde esta actividad.
                                var loanItems = await db.SelectAsync(db.From<LoanItem>().Where(w => w.LoanId == loan.Id));
                                var loanItemsIds = loanItems.Select(x => x.Id).ToList();
                                var loanConceptIds = loanItems.Select(x => x.ConceptId).Distinct().ToList();
                                var loanConcepts = await db.SelectAsync(db.From<LoanConcept>().Where(w => Sql.In(w.Id, loanConceptIds)));
                                var distributions = await db.SelectAsync(db.From<LoanItemDistribution>().Where(w => Sql.In(w.LoanItemId, loanItemsIds)));
                                var loanPersons = await db.SelectAsync(db.From<LoanPerson>().Where(w => w.LoanId == loan.Id));

                                foreach (var distribution in distributions)
                                {
                                    if (distribution.PersonRole.HasValue && !distribution.BusinessPartnerId.HasValue)
                                    {
                                        var role = distribution.PersonRole.Value;
                                        var loanItem = loanItems.Single(x => x.Id == distribution.LoanItemId);
                                        var loanConcept = loanConcepts.Single(x => x.Id == loanItem.ConceptId);
                                        var loanPerson = loanPersons.Where(w => w.Role == role).FirstOrDefault(); // Take first person TODO: distribution rules.
                                        var businessPartner = (await db.SelectAsync(db
                                            .From<BusinessPartner>()
                                            .Where(x => x.TenantId == session.TenantId && x.TypeId == loanConcept.OperatingAccountPostingType && x.PersonId == loanPerson.PersonId)))
                                            .SingleOrDefault();
                                        if(businessPartner != null)
                                        {
                                            distribution.BusinessPartnerId = businessPartner.Id;
                                        }
                                        else
                                        {
                                            var businessPartnerRequest = new ServiceModel.BusinessPartners.PostBusinessPartner
                                            {
                                                TenantId = session.TenantId,
                                                PersonId = loanPerson.PersonId,
                                                TypeId = loanConcept.OperatingAccountPostingType
                                            };

                                            var businessPartnerOperation = await _businessPartnerRepository.InsertBusinessPartner(db, session, businessPartnerRequest);
                                            businessPartner = businessPartnerOperation.Item1;
                                            var createNew = businessPartnerOperation.Item2;

                                            //Derived objects TODO: ver esto de hacerlo generico
                                            if (createNew)
                                            {
                                                switch (businessPartner.TypeId)
                                                {
                                                    case 1:
                                                        await db.InsertAsync(new Domain.Sales.Client { Id = businessPartner.Id });
                                                        break;
                                                    case 2:
                                                        await db.InsertAsync(new Domain.Procurement.Vendor
                                                        {
                                                            Id = businessPartner.Id
                                                        });
                                                        break;
                                                }
                                            }
                                        }
                                        
                                    }
                                }

                                var applicantId = loanPersons.First(x => x.Role == LoanPersonRole.Applicant).PersonId;
                                var applicantBusinessPartner = (await db.SelectAsync(db.From<BusinessPartner>().Where(w => w.TenantId == session.TenantId && w.TypeId == 1 && w.PersonId == applicantId))).SingleOrDefault();
                                if(applicantBusinessPartner == null)
                                {
                                    var businessPartnerRequest = new ServiceModel.BusinessPartners.PostBusinessPartner
                                    {
                                        TenantId = session.TenantId,
                                        PersonId = applicantId,
                                        TypeId = 1 // Client
                                    };
                                    applicantBusinessPartner = (await _businessPartnerRepository.InsertBusinessPartner(db, session, businessPartnerRequest)).Item1;
                                }

                                var applicantBusinessPartnerId = applicantBusinessPartner.Id;
                                var businessPartnerIds = distributions.Select(x => x.BusinessPartnerId).Distinct().ToList();
                                if (!businessPartnerIds.Contains(applicantId))
                                {
                                    businessPartnerIds.Add(applicantBusinessPartnerId);
                                }

                                var businessPartners = await db.SelectAsync(db.From<BusinessPartner>().Where(w => Sql.In(w.Id, businessPartnerIds)));
                                var tenant = await _tenantRepository.GetTenant(db, session.TenantId);
                                foreach (var businessPartner in businessPartners)
                                {
                                    ServiceModel.Financials.PostPaymentDocumentRequest paymentOrderRequest = null;
                                    // OP e item en caso que la persona sea el applicant TODO:Pasar esto a un LoanConcept fijo
                                    if (businessPartner.Id == applicantBusinessPartnerId)
                                    {
                                        paymentOrderRequest = new ServiceModel.Financials.PostPaymentDocumentRequest
                                        {
                                            DocumentDate = DateTime.UtcNow,
                                            IssuerId = tenant.PersonId,
                                            ReceiverId = businessPartner.PersonId,
                                            TypeId = 1,
                                            Comments = "Solicitud de prestamo N° " + loan.Number
                                        };

                                        paymentOrderRequest.Items.Add(new Domain.Financials.PaymentDocumentItem
                                        {
                                            LinkedDocumentTypeId = 4, // TODO: Pasar a servicio DI Loan
                                            LinkedDocumentId = loan.Id,
                                            Description = "Solicitud de prestamo N° " + loan.Number,
                                            OriginalAmount = loan.Amount - loan.Expenses,
                                            AmountToPay = loan.Amount - loan.Expenses
                                        }.ConvertTo<CentralOperativa.ServiceModel.Financials.PaymentDocumentItemCollect>());
                                    }

                                    var personDistributions = distributions.Where(x => x.BusinessPartnerId == businessPartner.Id);
                                    foreach (var personDistribution in personDistributions)
                                    {
                                        var loanItem = loanItems.Single(x => x.Id == personDistribution.LoanItemId);
                                        var loanConcept = loanConcepts.Single(x => x.Id == loanItem.ConceptId);

                                        //BusinessPartnerAccountEntry
                                        var account = (await _businessPartnerRepository.GetBusinessPartner(db, businessPartner.Id, true)).Accounts.Items.OrderBy(o => o.Id).FirstOrDefault();
                                        if (account != null)
                                        {
                                            var amount = personDistribution.Value;
                                            if (loanConcept.OperationSign)
                                            {
                                                amount = amount * -1;
                                            }
                                            var accountEntry = new BusinessPartnerAccountEntry { AccountId = account.Id, Amount = amount, Description = loanConcept.Name, CreateDate = DateTime.UtcNow, LinkedDocumentId = personDistribution.Id, LinkedDocumentTypeId = 5, PostingDate = DateTime.UtcNow };
                                            await db.InsertAsync(accountEntry);
                                        }

                                        //PaymentOrder
                                        if (loanConcept.PostDirectPaymentOrder)
                                        {
                                            if (paymentOrderRequest == null)
                                            {
                                                paymentOrderRequest = new ServiceModel.Financials.
                                                    PostPaymentDocumentRequest
                                                {
                                                    DocumentDate = DateTime.UtcNow,
                                                    IssuerId = tenant.PersonId,
                                                    ReceiverId = businessPartner.PersonId,
                                                    TypeId = 1,
                                                    Comments = "Solicitud de prestamo N° " + loan.Number
                                                };
                                            }

                                            //PaymentOrderItem
                                            var paymentOrderItem = new Domain.Financials.PaymentDocumentItem
                                            {
                                                LinkedDocumentTypeId = 5,
                                                // TODO: Pasar a servicio DI LoanConceptDistribution
                                                LinkedDocumentId = personDistribution.Id,
                                                Description = loanConcept.Name,
                                                OriginalAmount = personDistribution.Value,
                                                AmountToPay = personDistribution.Value
                                            };
                                            paymentOrderRequest.Items.Add(paymentOrderItem.ConvertTo<CentralOperativa.ServiceModel.Financials.PaymentDocumentItemCollect>());
                                        }
                                    }

                                    if (paymentOrderRequest != null)
                                    {
                                        var paymentDocument = _paymentDocumentRepository.CreatePaymentDocument(db, session, paymentOrderRequest);

                                        // PaymentDocumentLink
                                        var paymentDocumentLink = new Domain.Financials.PaymentDocumentLink
                                        {
                                            PaymentDocumentId = paymentDocument.Id,
                                            LinkedDocumentTypeId = 4, // Loan
                                            LinkedDocumentId = loan.Id
                                        };
                                        await db.InsertAsync(paymentDocumentLink);

                                        if (businessPartner.Id == applicantBusinessPartnerId)
                                        {
                                            //BusinessPartnerAccountEntry
                                            var account = (await _businessPartnerRepository.GetBusinessPartner(db, businessPartner.Id, true))
                                                    .Accounts.Items.OrderBy(o => o.Id)
                                                    .FirstOrDefault();
                                            if (account != null)
                                            {
                                                var accountEntry = new BusinessPartnerAccountEntry
                                                {
                                                    AccountId = account.Id,
                                                    Amount = loan.Amount - loan.Expenses,
                                                    Description = paymentOrderRequest.Comments,
                                                    CreateDate = DateTime.UtcNow,
                                                    LinkedDocumentId = paymentDocument.Id,
                                                    LinkedDocumentTypeId = 4,
                                                    PostingDate = DateTime.UtcNow
                                                };
                                                await db.InsertAsync(accountEntry);
                                            }
                                        }
                                    }
                                }
                                break;
                            case 151: // Archivo legajo
                                loan.Status = LoanStatus.Paid;
                                break;
                            case 152: // Finalizado
                                loan.Status = LoanStatus.Portfolio;
                                break;
                        }
                        db.Update(loan);
                    }
                }
            }

        }

        private async Task UpdateInventory(IDbConnection db, BusinessDocument businessDocument)
        {
            //Genero movimientos del inventario
            var businessDocumentType = await db.SingleByIdAsync<BusinessDocumentType>(businessDocument.TypeId);
            var items = await db.SelectAsync<BusinessDocumentItem>(w => w.BusinessDocumentId == businessDocument.Id);
            foreach (var item in items)
            {
                if (item.InventorySiteId.HasValue && item.ProductId.HasValue)
                {
                    var inventoryEntry = new Domain.Inv.InventoryEntry
                    {
                        BusinessDocumentItemId = item.Id,
                        ProductId = item.ProductId.Value,
                        InventorySiteId = item.InventorySiteId.Value,
                        Quantity = (businessDocumentType.ShortName == "RME") ? item.Quantity : (item.Quantity * -1),
                        CreateDate = DateTime.UtcNow
                    };
                    await db.InsertAsync(inventoryEntry);
                }
            }
        }
    }
}