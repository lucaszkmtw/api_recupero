using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.Loans;
using CentralOperativa.Domain.System;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.ServiceInterface.System.Persons;
using CentralOperativa.ServiceInterface.System.Workflows;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Loans;

namespace CentralOperativa.ServiceInterface.Loans
{
    [Authenticate]
    public class LoansService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(LoansService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;
        private readonly WorkflowActivityRepository _workflowActivityRepository;
        private readonly LoanRepository _loanRepository;

        public LoansService(AutoQuery autoQuery, PersonRepository personRepository, WorkflowActivityRepository workflowActivityRepository, LoanRepository loanRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _workflowActivityRepository = workflowActivityRepository;
            _loanRepository = loanRepository;
        }

        public object Get(Api.GetLoanStatus request)
        {
            var enumItems = new Dictionary<string, string>();
            var values = Enum.GetValues(typeof(LoanStatus));
            var names = Enum.GetNames(typeof(LoanStatus));
            var wiIdsQ = Db.From<Loan>().Where(x => x.TenantId == Session.TenantId).SelectDistinct(x => x.Status);
            var wiIds = Db.Select<int>(wiIdsQ);
            for (var index = 0; index < values.Length; index++)
            {
                if (wiIds.Contains(Convert.ToInt32(values.GetValue(index))))
                {
                    enumItems.Add(Convert.ToInt32(values.GetValue(index)).ToString(), names[index]);
                }
            }
            return enumItems;
        }

        public object Get(Api.QueryLoansAuhtorizations request)
        {
            var parameters = Request.GetRequestParams();

            var roleIds = Db.Column<int>(Session.Roles.Contains("admin") ?
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId).Select(x => x.Id) :
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId && Sql.In(w.Name, Session.Roles)));

            var q = _autoQuery.CreateQuery(request, parameters)
                .Join<Loan, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id)
                .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id)
                .Join<WorkflowInstance, Workflow>()
                .Join<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Applicant)
                .Join<LoanPerson, Domain.System.Persons.Person>((lp, pe) => lp.PersonId == pe.Id);

            if (request.OrderByDesc == null)
            {
                q.OrderByDescending(o => o.Date);
            }

            List<int> wiIds;
            SqlExpression<Loan> wiIdsQ = null;
            switch (request.View)
            {
                case 0: //Own
                    wiIdsQ = Db.From<Loan>()
                        .Join<Loan, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wia) => wi.Id == wia.WorkflowInstanceId && wia.IsActive && Sql.In(wia.RoleId, roleIds))
                        .Join<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Applicant)
                        .Join<LoanPerson, Domain.System.Persons.Person>((lp, pe) => lp.PersonId == pe.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 1: //Supervised
                    wiIdsQ = Db.From<Loan>()
                        .Join<Loan, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission == 2 && Sql.In(warp.RoleId, roleIds))

                        .Join<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Applicant)
                        .Join<LoanPerson, Domain.System.Persons.Person>((lp, pe) => lp.PersonId == pe.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 2: //Others
                    wiIdsQ = Db.From<Loan>()
                        .Join<Loan, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission < 2 && Sql.In(warp.RoleId, roleIds))
                        .Join<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Applicant)
                        .Join<LoanPerson, Domain.System.Persons.Person>((lp, pe) => lp.PersonId == pe.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 3: //Terminated
                    wiIdsQ = Db.From<Loan>()
                        .Join<Loan, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive)
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && Sql.In(warp.RoleId, roleIds))
                        .Join<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Applicant)
                        .Join<LoanPerson, Domain.System.Persons.Person>((lp, pe) => lp.PersonId == pe.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 4: //Finished
                    wiIdsQ = Db.From<Loan>()
                        .Join<Loan, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal)
                        .Join<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Applicant)
                        .Join<LoanPerson, Domain.System.Persons.Person>((lp, pe) => lp.PersonId == pe.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
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
                        "Loans.MessageThreadId IN (SELECT DISTINCT m.MessageThreadId FROM Messages m WHERE CONTAINS(m.Body, {0}))",
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

                wiIdsQ.And(w => w.TenantId == Session.TenantId);
                wiIds = Db.Select<int>(wiIdsQ);

                if (wiIds.Count == 0)
                {
                    wiIds.Add(0);
                }

                q.Where<WorkflowInstance>(w => Sql.In(w.Id, wiIds));
            }

            var result = _autoQuery.Execute(request, q);
            var workflowInstanceIds = result.Results.Select(x => x.WorkflowInstanceId).ToList();
            if (workflowInstanceIds.Count == 0)
            {
                workflowInstanceIds.Add(0);
            }

            var roleMap = Db.Select<ServiceModel.System.Workflows.WorkflowInstanceRoleMap>(Db
                .From<WorkflowInstanceAssignments>()
                .Join<Role>()
                .Where(w => Sql.In(w.WorkflowInstanceId, workflowInstanceIds))
                .And(w => w.IsActive));
            result.Results.ForEach(x => x.Roles = string.Join(",", roleMap.Where(w => w.WorkflowInstanceId == x.WorkflowInstanceId).Select(w => w.RoleName)));
            return Request.ToOptimizedResult(result);
        }

        public async Task<bool> Post(Api.PostSubmitForAuthorizationRequest request)
        {
            var loan = await Db.SingleAsync<Loan>(w => w.Guid == request.LoanGuid);

            if (loan.AuthorizationWorkflowInstanceId.HasValue)
            {
                throw new ApplicationException($"Loan {loan.Id} has already an authroization workflow initiated.");
            }

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    ServiceModel.System.Workflows.WorkflowActivity.GetWorkflowActivityResponse currentActivity = null;
                    switch (loan.Status)
                    {
                        case LoanStatus.Pending:
                        case LoanStatus.InEvaluation:
                            currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 1, (short)WellKnownWorkflowTypes.LoanApproval); //Evaluacion
                            break;

                        case LoanStatus.Approved:
                        case LoanStatus.PendingReception:
                            currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 2, (short)WellKnownWorkflowTypes.LoanApproval); //Firma documentación
                            break;

                        case LoanStatus.Portfolio:
                        case LoanStatus.ToExecute:
                            currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 3, (short)WellKnownWorkflowTypes.LoanApproval); //Liquidacion
                            break;

                        case LoanStatus.Suspended:
                        case LoanStatus.Cancelled:
                        case LoanStatus.Voided:

                        case LoanStatus.Paid:
                            break;
                    }

                    if (currentActivity == null)
                    {
                        throw new ApplicationException($"Loan {loan.Id} has no current activity.");
                    }

                    var workflowInstance = new WorkflowInstance
                    {
                        CreateDate = DateTime.UtcNow,
                        CreatedByUserId = Session.UserId,
                        WorkflowId = currentActivity.WorkflowId,
                        CurrentWorkflowActivityId = currentActivity.Id,
                        Guid = Guid.NewGuid()
                    };
                    workflowInstance.Id = (int) await Db.InsertAsync(workflowInstance, true);

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

                    loan.AuthorizationWorkflowInstanceId = workflowInstance.Id;
                    loan.Status = LoanStatus.InEvaluation;
                    await Db.UpdateAsync(loan);
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

        public async Task<object> Post(Api.PostLoanSettlementRequest request)
        {
            var loan = await _loanRepository.GetLoan(Db, Session, request.LoanGuid, true);

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    await Db.DeleteAsync(Db.From<LoanItemDistribution>().Join<LoanItem>().Where<LoanItem>(w => w.LoanId == loan.Id));
                    await Db.DeleteAsync(Db.From<LoanItem>().Where<LoanItem>(w => w.LoanId == loan.Id));
                    foreach (var item in request.Items)
                    {
                        var itemId = (int) await Db.InsertAsync((LoanItem)item, true);

                        foreach (var distribution in item.Distributions)
                        {
                            distribution.LoanItemId = itemId;
                            distribution.Id = (int) await Db.InsertAsync((LoanItemDistribution)distribution, true);
                        }
                    }
                    trx.Commit();
                    return await _loanRepository.GetLoan(Db, Session, loan.Id);
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public object Get(Api.QueryLoans request)
        {
            var p = Request.GetRequestParams();
            var q = _autoQuery.CreateQuery(request, p);
            q.Join<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Applicant);
            q.Join<LoanPerson, Domain.System.Persons.Person>((lp, pe) => lp.PersonId == pe.Id);
            q.Join<Loan, Domain.Catalog.Product>();
            q.LeftJoin<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Seller, Db.JoinAlias("LoanSeller"));
            q.CustomJoin("LEFT JOIN Persons Seller ON (LoanSeller.PersonId = Seller.Id)");
            q.Select(@"Loans.*
                , Loans.StatusId AS Status
                , Persons.Name AS ApplicantName
                , Seller.Name AS SellerName
                , Products.Name AS ProductName");

            q.Where(w => w.TenantId == Session.TenantId);
            if (request.OrderByDesc == null)
            {
                q.OrderByDescending(x => x.Date);
            }

            if (p.ContainsKey("productName"))
            {
                q.Where<Domain.Catalog.Product>(w => w.Name.Contains(p["productName"]));
            }

            if (p.ContainsKey("sellerName"))
            {
                q.UnsafeWhere("Seller.Name LIKE {0}", Utils.SqlLike(p["sellerName"]));
            }

            if (p.ContainsKey("date_"))
            {
                q.UnsafeWhere("Loans.Date >= {0}", p["date_"] + " 00:00:0");
                q.UnsafeWhere("Loans.Date <= {0}", p["date_"] + " 23:59:59");
            }

            if (p.ContainsKey("applicantName"))
            {
                q.Where<Domain.System.Persons.Person>(w => w.Name.Contains(p["applicantName"]));
            }

            if (p.ContainsKey("personId"))
            {
                var result = int.TryParse(p["personId"], out var id);
                if (result)
                {
                    q.Where<Domain.System.Persons.Person>(w => w.Id == id);
                }
            }

            var model = _autoQuery.Execute(request, q);
            var roleMap = Db.Select<ServiceModel.System.Workflows.WorkflowInstanceRoleMap>(Db
                .From<WorkflowInstanceAssignments>()
                .Join<Role>()
                .Where(w => Sql.In(w.WorkflowInstanceId, q.Where(sw => sw.AuthorizationWorkflowInstanceId.HasValue).SelectDistinct(x => x.AuthorizationWorkflowInstanceId)))
                .And(w => w.IsActive));
            model.Results.ForEach(x =>
            {
                if (x.AuthorizationWorkflowInstanceId.HasValue)
                {
                    x.Roles = roleMap.Where(w => w.WorkflowInstanceId == x.AuthorizationWorkflowInstanceId.Value).Select(w => w.RoleName).ToList();
                }
            });

            if (p.ContainsKey("roles"))
            {

                model.Results = model.Results.Where(x => x.Roles != null && x.Roles.Contains(p["roles"])).Select(w => w).ToList();

            }

            return model;
        }

        public object Get(Api.GetLoan request)
        {
            return _loanRepository.GetLoan(Db, Session, request.Id, true);
        }

        public async Task<object> Post(Api.PostLoanBatch request)
        {
            if (Log.IsDebugEnabled) Log.DebugFormat("LNS:LNS:PosLoanBatch begin ({0}).", request.Count);
            using (var trx = Db.OpenTransaction())
            {
                string number = null;
                try
                {
                    //var concepts = Db.Dictionary<string, LoanConcept>(Db.From<LoanConcept>().Select(x => new { x.Code, x }));
                    var products = new Dictionary<string, Domain.Catalog.Product>();
                    foreach (var batchItem in request)
                    {
                        //Loan
                        number = batchItem.Loan.Number;

                        //ProductCatalogId
                        var loan = await _loanRepository.GetLoan(Db, Session, batchItem.Loan.Number); //TODO: review
                        if (loan == null)
                        {
                            Domain.Catalog.Product product;
                            var productKey = batchItem.Loan.Product.Name;
                            if (!products.ContainsKey(productKey))
                            {
                                product = (await Db.SelectAsync(Db.From<Domain.Catalog.Product>().Where(w => w.Name == batchItem.Loan.Product.Name))).FirstOrDefault();
                                if (product == null)
                                {
                                    batchItem.Loan.Product.TenantId = Session.TenantId;
                                    batchItem.Loan.Product.Id = (int) await Db.InsertAsync(batchItem.Loan.Product, true);

                                    product = batchItem.Loan.Product;
                                    batchItem.Loan.ProductId = batchItem.Loan.Product.Id;
                                    batchItem.Loan.Product.TenantId = Session.TenantId;
                                    batchItem.Loan.Product.Id = (int)Db.Insert((Domain.Catalog.Product)batchItem.Loan.Product, true);
                                    batchItem.Loan.ProductId = batchItem.Loan.Product.Id;
                                }

                                products.Add(productKey, product);
                            }
                            else
                            {
                                product = products[productKey];
                            }

                            batchItem.Loan.ProductId = product.Id;
                            batchItem.Loan.Id = (int)Db.Insert((Loan)batchItem.Loan, true);
                            foreach (var item in batchItem.Loan.Items.Where(w => w.Concept.BasedOnName == null))
                            {
                                InsertLoanItem(item, batchItem.Loan.Id);
                            }

                            foreach (var item in batchItem.Loan.Items.Where(w => w.Concept.BasedOnName != null))
                            {
                                InsertLoanItem(item, batchItem.Loan.Id);
                            }

                            //LoanPersons
                            foreach (var loanPerson in batchItem.Loan.Persons)
                            {
                                Domain.System.Persons.Person person = null;
                                if (!string.IsNullOrEmpty(loanPerson.Person.Code))
                                {
                                    person = await _personRepository.GetPerson(Db, loanPerson.Person.Code);
                                }

                                if (person == null && loanPerson.Person.Emails.Count > 0)
                                {
                                    var personDomain = Db.Select(Db
                                        .From<Domain.System.Persons.Person>()
                                        .Join<Domain.System.Persons.Person, Domain.System.Persons.PersonEmail>()
                                        .Where<Domain.System.Persons.PersonEmail>(
                                            w => w.Address == loanPerson.Person.Emails[0].Address)).FirstOrDefault();
                                    if (personDomain != null)
                                    {
                                        person = await _personRepository.GetPerson(Db, personDomain.Id);
                                    }
                                }

                                if (person == null)
                                {
                                    loanPerson.Person = await _personRepository.CreatePerson(Db, loanPerson.Person);
                                }
                                else
                                {
                                    loanPerson.Person.Id = person.Id;
                                    loanPerson.PersonId = person.Id;
                                }

                                //LoanPersons
                                loanPerson.PersonId = loanPerson.Person.Id;
                                loanPerson.LoanId = batchItem.Loan.Id;
                                loanPerson.Id = (int)Db.Insert((LoanPerson)loanPerson, true);
                            }

                            //LoanInstallments
                            _loanRepository.CalculateLoanInstallments(Db, batchItem.Loan.Id, batchItem.Loan.InstallmentBaseAmount, batchItem.Loan.Amount, batchItem.Loan.InitialVoidDate, batchItem.Loan.Term);
                        }
                        else
                        {
                            batchItem.Loan.Id = loan.Id;
                            //Db.Update((Loan)item.Loan);
                        }


                        batchItem.ImportLog.TargetId = batchItem.Loan.Id;
                        batchItem.ImportLog.LastActivity = DateTime.UtcNow;
                    }

                    //Insert import logs
                    //Db.InsertAll(request.Select(x => x.ImportLog));

                    trx.Commit();
                    if (Log.IsDebugEnabled) Log.DebugFormat("LNS:LNS:PostBatch completed.", request.Count);
                    return true;
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw new ApplicationException($"Error processing loan number: {number}.", ex);
                }
            }
        }

        private void InsertLoanItem(Api.PostLoan.PostLoanItem item, int loanId)
        {
            var concept = Db.Single<LoanConcept>(w => w.Code == item.Concept.Code);
            if (concept == null)
            {
                concept = new LoanConcept
                {
                    OperatingAccountPostingType = 2, // Default to vendor
                    PostDirectPaymentOrder = false,
                    Code = item.Concept.Code,
                    Name = item.Concept.Name,
                    Description = item.Concept.Description,
                    Source = item.Concept.Source,
                    ApplyTo = item.Concept.ApplyTo,
                    Operation = item.Concept.Operation,
                    Type = item.Concept.Type
                };

                if (!string.IsNullOrEmpty(item.Concept.BasedOnName))
                {
                    var basedOnConcept = Db.Single<LoanConcept>(w => w.Code == item.Concept.BasedOnName);
                    if (basedOnConcept != null)
                    {
                        concept.BasedOnId = basedOnConcept.Id;
                    }
                    else
                    {
                        Debug.WriteLine($"Concept {item.Concept.BasedOnName} not found.");
                        //throw new ApplicationException($"Concept {item.Concept.BasedOnName} not found.");
                    }
                }

                concept.Id = (int)Db.Insert(concept, true);
            }

            var loanItem = new LoanItem
            {
                ConceptId = concept.Id,
                LoanId = loanId,
                Value = item.Value
            };

            loanItem.Id = (int)Db.Insert(loanItem);
        }

        public bool Put(Api.PutLoan request)
        {
            try
            {
                _loanRepository.RecalculateInstallmentsVoidDate(Db, request.LoanId, request.VoidDate);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}