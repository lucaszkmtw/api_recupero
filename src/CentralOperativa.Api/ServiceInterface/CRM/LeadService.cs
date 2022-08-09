using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.CRM;
using CentralOperativa.ServiceInterface.System.Persons;
using CentralOperativa.Domain.Loans;
using CentralOperativa.ServiceInterface.System.Workflows;
using CentralOperativa.Domain.System;
using CentralOperativa.Domain.System.Workflows;
using System.Data;
using System.Threading.Tasks;
using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.ServiceInterface.CRM
{
    [Authenticate]
    public class LeadService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(LeadService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;
        private readonly WorkflowActivityRepository _workflowActivityRepository;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;

        public LeadService(IAutoQueryDb autoQuery, PersonRepository personRepository, WorkflowActivityRepository workflowActivityRepository, WorkflowInstanceRepository workflowInstanceRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _workflowActivityRepository = workflowActivityRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
        }

        public object Get(Api.QueryLeads request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }
            var p = Request.GetRequestParams();

            if (p.ContainsKey("sidx") && p["sidx"] != "userName")
            {
                if (p["sidx"] == "startDate_")
                {
                    p["sidx"] = "startDate";
                }
                if (p["sidx"] == "endDate_")
                {
                    p["sidx"] = "endDate";
                }
                if (p.ContainsKey("sord") && p["sord"] == "desc")
                {
                    request.OrderByDesc = p["sidx"];
                    request.OrderBy = null;
                }
                else
                {
                    request.OrderBy = p["sidx"];
                }
            }
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.LeftJoin<Domain.Cms.Forms.FormResponse, User>((fr, u) => fr.CreatedById == u.Id);
            q.Join<Domain.Cms.Forms.FormResponse, Domain.Cms.Forms.Form>((fr, f) => fr.FormId == f.Id && f.TenantId == Session.TenantId && f.TypeId == 2);
            q.Join<Domain.Cms.Forms.Form, Domain.CRM.CampaignForm>();
            q.Join<Domain.CRM.CampaignForm, Domain.CRM.Campaign>();
            q.LeftJoin<Domain.Cms.Forms.FormResponse, Domain.CRM.Lead>((fr, l) => fr.Id == l.FormResponseId);
            q.UnsafeSelect(@"FormResponses.{0} as Id,
                            Persons.Name as Name,
                            Users.Name as UserName,
                            FormResponses.StartDate as StartDate,
                            FormResponses.EndDate as EndDate,
                            Campaigns.Name as CampaignName,
                            Persons.IsValid as PersonIsValid,
                            FormResponses.FormId as FormId,
                            Leads.Id as LeadId
                        ".Fmt("Id".SqlColumn()));
            q.Where<Domain.Cms.Forms.FormResponse>(fr => fr.StatusId == 1);

            if (p.ContainsKey("userName"))
            {
                q.UnsafeWhere("Users.Name LIKE {0}", Utils.SqlLike(p["userName"]));
            }

            if (p.ContainsKey("startDate_"))
            {
                q.UnsafeWhere("FormResponses.StartDate >= {0}", p["startDate_"] + " 00:00:0");
                q.UnsafeWhere("FormResponses.StartDate <= {0}", p["startDate_"] + " 23:59:59");
            }

            if (p.ContainsKey("endDate_"))
            {
                q.UnsafeWhere("FormResponses.EndDate >= {0}", p["endDate_"] + " 00:00:0");
                q.UnsafeWhere("FormResponses.EndDate <= {0}", p["endDate_"] + " 23:59:59");
            }

            if (p.ContainsKey("campaignName"))
            {
                q.UnsafeWhere("Campaigns.Name LIKE {0}", Utils.SqlLike(p["campaignName"]));
            }

            if (p.ContainsKey("sidx") && p["sidx"] == "userName")
            {
                q.UnsafeOrderBy("Users.Name " + p["sord"]);
            }

            return _autoQuery.Execute(request, q);
        }

        public object Get(Api.QueryLeadFormResponses request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }
            var p = Request.GetRequestParams();

            if (p.ContainsKey("sidx") && p["sidx"] != "userName")
            {
                if (p["sidx"] == "startDate_")
                {
                    p["sidx"] = "startDate";
                }
                if (p["sidx"] == "endDate_")
                {
                    p["sidx"] = "endDate";
                }
                if (p.ContainsKey("sord") && p["sord"] == "desc")
                {
                    request.OrderByDesc = p["sidx"];
                    request.OrderBy = null;
                }
                else
                {
                    request.OrderBy = p["sidx"];
                }
            }
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.LeftJoin<Domain.Cms.Forms.FormResponse, User>((fr, u) => fr.CreatedById == u.Id);
            q.Join<Domain.Cms.Forms.FormResponse, Domain.Cms.Forms.Form>((fr, f) => fr.FormId == f.Id && f.TenantId == Session.TenantId && f.TypeId == 0);
            q.LeftJoin<Domain.Cms.Forms.FormResponse, Domain.CRM.LeadFormResponse>((fr, l) => fr.Id == l.FormResponseId);
            q.LeftJoin<Domain.CRM.LeadFormResponse, Domain.CRM.Lead>((lfr, l) => lfr.LeadId == l.Id);
            q.LeftJoin<Domain.CRM.Lead, Domain.CRM.Campaign>((l, c) => l.CampaignId == c.Id);
            q.UnsafeSelect(@"FormResponses.{0} as Id,
                            Persons.Name as Name,
                            Users.Name as UserName,
                            FormResponses.StartDate as StartDate,
                            FormResponses.EndDate as EndDate,
                            Campaigns.Name as CampaignName,
                            Persons.IsValid as PersonIsValid,
                            FormResponses.FormId as FormId,
                            Leads.Id as LeadId,
                            FormResponses.StatusId as StatusId
                        ".Fmt("Id".SqlColumn()));
            //q.Where<Domain.Cms.Forms.FormResponse>(fr => fr.StatusId == 1);

            if (p.ContainsKey("userName"))
            {
                q.UnsafeWhere("Users.Name LIKE {0}", Utils.SqlLike(p["userName"]));
            }

            if (p.ContainsKey("startDate_"))
            {
                q.UnsafeWhere("FormResponses.StartDate >= {0}", p["startDate_"] + " 00:00:0");
                q.UnsafeWhere("FormResponses.StartDate <= {0}", p["startDate_"] + " 23:59:59");
            }

            if (p.ContainsKey("endDate_"))
            {
                q.UnsafeWhere("FormResponses.EndDate >= {0}", p["endDate_"] + " 00:00:0");
                q.UnsafeWhere("FormResponses.EndDate <= {0}", p["endDate_"] + " 23:59:59");
            }

            if (p.ContainsKey("campaignName"))
            {
                q.UnsafeWhere("Campaigns.Name LIKE {0}", Utils.SqlLike(p["campaignName"]));
            }

            if (p.ContainsKey("sidx") && p["sidx"] == "userName")
            {
                q.UnsafeOrderBy("Users.Name " + p["sord"]);
            }

            return _autoQuery.Execute(request, q);
        }

        public async Task<object> Get(Api.QueryPersons request)
        {
            var queryPerson = (await _personRepository.GetPerson(Db, request.Id)).ConvertTo<Api.QueryPersons>();
            queryPerson.Employee = (await Db.SelectAsync(Db.From<Domain.CRM.Contacts.Employee>().Where(e => e.PersonId == queryPerson.Id))).SingleOrDefault();
            if (queryPerson.Employee != null)
            {
                queryPerson.Employer = await _personRepository.GetPerson(Db, queryPerson.Employee.EmployerId);
                queryPerson.BankAccount = Db.Select(Db.From<Domain.Financials.BankAccount>().Where(ba => ba.Id == queryPerson.Employee.BankAccountId)).SingleOrDefault();
                if (queryPerson.BankAccount != null)
                {
                    var bankBranch = Db.Select(Db.From<Domain.Financials.BankBranch>().Where(bb => bb.Id == queryPerson.BankAccount.BankBranchId)).SingleOrDefault();
                    if (bankBranch != null)
                    {
                        queryPerson.Bank = Db.Select(Db.From<Domain.Financials.Bank>().Where(b => b.Id == bankBranch.BankId)).SingleOrDefault();
                    }
                }
            }
            queryPerson.ProcurementsCount = Db.Select(Db.From<Domain.BusinessDocuments.BusinessDocument>().Where(b => b.IssuerId == queryPerson.Id)).Count;
            queryPerson.SalesCount = Db.Select(Db.From<Domain.BusinessDocuments.BusinessDocument>().Where(b => b.ReceiverId == queryPerson.Id)).Count;
            queryPerson.PollsCount = Db.Select(Db.From<Domain.Cms.Forms.FormResponse>()
                .Join<Domain.Cms.Forms.FormResponse, Domain.Cms.Forms.Form>((fr, f) => fr.FormId == f.Id && f.TenantId == Session.TenantId && f.TypeId == 1)
                .Where(fr => fr.PersonId == queryPerson.Id)).Count;

            queryPerson.LoansCount = Db.Select<ServiceModel.Loans.GetLoanResult>(
                Db.From<Loan>()
                .Join<Loan, LoanPerson>((l, lp) => l.Id == lp.LoanId && lp.Role == LoanPersonRole.Applicant && l.TenantId == Session.TenantId)
                .Join<LoanPerson, Domain.System.Persons.Person>()
                .Where<Domain.System.Persons.Person>(w => w.Id == queryPerson.Id)).Count;

            return queryPerson;
        }

        public async Task<bool> Post(Api.PostLeadSubmitForAuthorization request)
        {
            var formResponse = Db.Single<Domain.Cms.Forms.FormResponse>(w => w.Id == request.Id);

            if (!formResponse.PersonId.HasValue)
            {
                return false;
            }

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var campaign = (await Db.SelectAsync(
                        Db.From<Domain.CRM.Campaign>()
                        .Join<Domain.CRM.Campaign, Domain.CRM.CampaignForm>()
                        .Join<Domain.CRM.CampaignForm, Domain.Cms.Forms.Form>()
                        .Where<Domain.Cms.Forms.Form>(w => w.Id == formResponse.FormId))).FirstOrDefault();

                    if (campaign == null)
                    {
                        throw new ApplicationException($"There is no campaign for form: {formResponse.FormId}");
                    }

                    var lead = new Domain.CRM.Lead
                    {
                        PersonId = formResponse.PersonId.Value,
                        Status = 0,
                        FormResponseId = formResponse.Id,
                        CampaignId = campaign.Id
                    };
                    lead.Id = (int)Db.Insert(lead, true);
                    await SaveRelationships(Db, lead, Session.UserId);
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

        public async Task SaveRelationships(IDbConnection db, Domain.CRM.Lead lead, int userId)
        {
            if (lead.AuthorizationWorkflowInstanceId.HasValue)
            {
                throw new ApplicationException($"Lead {lead.Id} has already an authorization workflow initiated.");
            }

            var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 1, (short)WellKnownWorkflowTypes.LeadApproval);
            var workflowInstance = new WorkflowInstance
            {
                CreateDate = DateTime.UtcNow,
                CreatedByUserId = userId,
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
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    IsActive = true
                };
                await Db.InsertAsync(workflowInstanceRole);
            }

            lead.AuthorizationWorkflowInstanceId = workflowInstance.Id;
            lead.Status = 1;
            await Db.UpdateAsync(lead);

            var campaignProducts = await Db.SelectAsync(
                Db.From<Domain.CRM.CampaignProduct>()
                .Where(w => w.CampaignId == lead.CampaignId));

            var productIds = new List<int>();
            foreach (var leadProduct in campaignProducts)
            {
                await Db.InsertAsync(new Domain.CRM.LeadProduct
                {
                    LeadId = lead.Id,
                    ProductId = leadProduct.ProductId
                });
                productIds.Add(leadProduct.ProductId);
            }

            var productForms = await Db.SelectAsync(
                    Db.From<ProductForm>()
                    .Where(w => Sql.In(w.ProductId, productIds))
                    .SelectDistinct(w => w.FormId)
                );

            foreach (var leadForm in productForms)
            {
                await Db.InsertAsync(new Domain.CRM.LeadForm
                {
                    LeadId = lead.Id,
                    FormId = leadForm.FormId
                });
            }
        }


        public object Get(Api.QueryQualifications request)
        {
            var parameters = Request.GetRequestParams();

            var roleIds = Db.Column<int>(Session.Roles.Contains("admin") ?
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId).Select(x => x.Id) :
                Db.From<Role>().Where(w => w.TenantId == Session.TenantId && Sql.In(w.Name, Session.Roles)));

            var q = _autoQuery.CreateQuery(request, parameters)
                .Join<Domain.CRM.Lead, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id)
                .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id)
                .Join<WorkflowInstance, Workflow>()
                .Join<Domain.CRM.Lead, Domain.CRM.Campaign>((l, c) => l.CampaignId == c.Id);

            if (request.OrderByDesc == null)
            {
                q.OrderByDescending<WorkflowInstance>(o => o.CreateDate);
            }

            SqlExpression<Domain.CRM.Lead> wiIdsQ = null;
            switch (request.View)
            {
                case 0: //Own
                    wiIdsQ = Db.From<Domain.CRM.Lead>()
                        .Join<Domain.CRM.Lead, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && !wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && !wa.IsFinal)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wia) => wi.Id == wia.WorkflowInstanceId && wia.IsActive && Sql.In(wia.RoleId, roleIds))
                        .Join<Domain.CRM.Lead, Domain.CRM.Campaign>((l, c) => l.CampaignId == c.Id)
                        .Join<Domain.CRM.Lead, Domain.System.Persons.Person>((l, p) => l.PersonId == p.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 1: //Supervised
                    wiIdsQ = Db.From<Domain.CRM.Lead>()
                        .Join<Domain.CRM.Lead, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && !wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && !wa.IsFinal)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission == 2 && Sql.In(warp.RoleId, roleIds))
                        .Join<Domain.CRM.Lead, Domain.CRM.Campaign>((l, c) => l.CampaignId == c.Id)
                        .Join<Domain.CRM.Lead, Domain.System.Persons.Person>((l, p) => l.PersonId == p.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 2: //Others
                    wiIdsQ = Db.From<Domain.CRM.Lead>()
                        .Join<Domain.CRM.Lead, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && !wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && !wa.IsFinal)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission < 2 && Sql.In(warp.RoleId, roleIds))
                        .Join<Domain.CRM.Lead, Domain.CRM.Campaign>((l, c) => l.CampaignId == c.Id)
                        .Join<Domain.CRM.Lead, Domain.System.Persons.Person>((l, p) => l.PersonId == p.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 3: //Terminated
                    wiIdsQ = Db.From<Domain.CRM.Lead>()
                        .Join<Domain.CRM.Lead, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && !wa.IsFinal)
                        .Join<WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive)
                        .Join<WorkflowInstance, WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<WorkflowActivityRole, WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && Sql.In(warp.RoleId, roleIds))
                        .Join<Domain.CRM.Lead, Domain.CRM.Campaign>((l, c) => l.CampaignId == c.Id)
                        .Join<Domain.CRM.Lead, Domain.System.Persons.Person>((l, p) => l.PersonId == p.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 4: //Finished
                    wiIdsQ = Db.From<Domain.CRM.Lead>()
                        .Join<Domain.CRM.Lead, WorkflowInstance>((l, wi) => l.AuthorizationWorkflowInstanceId == wi.Id && !wi.IsTerminated)
                        .Join<WorkflowInstance, WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal)
                        .Join<Domain.CRM.Lead, Domain.CRM.Campaign>((l, c) => l.CampaignId == c.Id)
                        .Join<Domain.CRM.Lead, Domain.System.Persons.Person>((l, p) => l.PersonId == p.Id)
                        .SelectDistinct(x => x.AuthorizationWorkflowInstanceId);
                    break;
                case 5: //All
                    break;
            }

            if (!string.IsNullOrEmpty(request.Q))
            {
                wiIdsQ.WhereExpression += " (";
                wiIdsQ.And<WorkflowActivity>(w => w.Name.Contains(request.Q));
                wiIdsQ.Or<Domain.System.Persons.Person>(w => w.Name.Contains(request.Q));
                wiIdsQ.Or<Domain.CRM.Campaign>(w => w.Name.Contains(request.Q));
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

            wiIdsQ.And<Domain.CRM.Campaign>(w => w.TenantId == Session.TenantId);
            var wiIds = Db.Select<int>(wiIdsQ);

            if (wiIds.Count == 0)
            {
                wiIds.Add(0);
            }

            q.Where<WorkflowInstance>(w => Sql.In(w.Id, wiIds));

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


        public async Task<object> Get(Api.GetLead request)
        {
            var pollFormResponseRequest = new Api.GetLead
            {
                FormResponse = (await Db.SelectAsync(Db.From<Domain.Cms.Forms.FormResponse>().Where(fr => fr.Id == request.FormResponseId))).SingleOrDefault()
            };
            if (pollFormResponseRequest.FormResponse != null)
            {
                pollFormResponseRequest.Form = (await Db.SelectAsync(Db.From<Domain.Cms.Forms.Form>().Where(f => f.Id == pollFormResponseRequest.FormResponse.FormId))).SingleOrDefault();

                if (pollFormResponseRequest.FormResponse.PersonId != null)
                {
                    var personId = (int)pollFormResponseRequest.FormResponse.PersonId;
                    pollFormResponseRequest.Person = await _personRepository.GetPerson(Db, personId);
                }

                pollFormResponseRequest.Lead = Db.Select(Db.From<Domain.CRM.Lead>().Where(l => l.FormResponseId == pollFormResponseRequest.FormResponse.Id)).SingleOrDefault();
            }
            return pollFormResponseRequest;
        }

        public async Task<Api.GetLeadAuthorization> Get(Api.GetLeadAuthorization request)
        {
            var leads = await Db.SelectAsync(Db.From<Domain.CRM.Lead>().Where(fr => fr.Id == request.Id));
            var lead = leads.SingleOrDefault().ConvertTo<Api.GetLeadAuthorization>();
            if (lead != null)
            {

                var personId = lead.PersonId;
                lead.Person = await _personRepository.GetPerson(Db, personId);
                if (lead.AuthorizationWorkflowInstanceId != null)
                {
                    var authorizationWorkflowInstanceId = (int)lead.AuthorizationWorkflowInstanceId;
                    lead.WorkflowInstance = await _workflowInstanceRepository.GetWorkflowInstance(Db, Session, authorizationWorkflowInstanceId);
                }

                //Employee
                var employee = (await Db.SelectAsync(Db.From<Domain.CRM.Contacts.Employee>().Where(w => w.PersonId == personId).OrderByDescending(o => o.FromDate))).FirstOrDefault();
                lead.Employee = employee;

                var products = await Db.SelectAsync<Api.GetLeadAuthorization.Product>(
                        Db.From<Domain.CRM.LeadProduct>()
                        .Join<Domain.CRM.LeadProduct, Product>()
                        .Where<Domain.CRM.LeadProduct>(x => x.LeadId == lead.Id)
                        );
                lead.Products = products;

                var forms = await Db.SelectAsync<Api.GetLeadAuthorization.Form>(
                        Db.From<Domain.CRM.LeadForm>()
                        .Join<Domain.CRM.LeadForm, Domain.Cms.Forms.Form>()
                        .Where<Domain.CRM.LeadForm>(x => x.LeadId == lead.Id)
                        );

                lead.Forms = forms;
                foreach (var form in lead.Forms)
                {
                    var leadFormResponse = (await Db.SelectAsync(
                        Db.From<Domain.CRM.LeadFormResponse>()
                        .Join<Domain.CRM.LeadFormResponse, Domain.Cms.Forms.FormResponse>((lfr, fr) => lfr.FormResponseId == fr.Id && fr.FormId == form.FormId)
                        .Where<Domain.CRM.LeadFormResponse>(lfr => lfr.LeadId == lead.Id)
                        ))
                        .FirstOrDefault();
                    if (leadFormResponse != null)
                    {

                        form.FormResponseId = leadFormResponse.FormResponseId;
                    }
                }


                lead.Campaign = (await Db.SelectAsync(
                    Db.From<Domain.CRM.Campaign>()
                    .Where<Domain.CRM.Campaign>(c => c.Id == lead.CampaignId)
                ))
                .FirstOrDefault();

            }
            return lead;
        }

        public object Put(Api.PutLead request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {

                    var leadproductIds = request.Products.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
                    if (leadproductIds.Any())
                    {
                        Db.Delete<Domain.CRM.LeadProduct>(x => x.LeadId == request.Id && !Sql.In(x.Id, leadproductIds));
                    }
                    else
                    {
                        Db.Delete<Domain.CRM.LeadProduct>(x => x.LeadId == request.Id);
                    }

                    foreach (var leadProduct in request.Products)
                    {
                        if (leadProduct.Id.HasValue)
                        {
                            Db.Update(new Domain.CRM.LeadProduct
                            {
                                Id = leadProduct.Id.Value,
                                LeadId = request.Id,
                                ProductId = leadProduct.ProductId
                            });
                        }
                        else
                        {
                            Db.Insert(new Domain.CRM.LeadProduct
                            {
                                LeadId = request.Id,
                                ProductId = leadProduct.ProductId
                            });
                        }
                    }

                    var leadformIds = request.Forms.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
                    if (leadformIds.Any())
                    {
                        Db.Delete<Domain.CRM.LeadForm>(x => x.LeadId == request.Id && !Sql.In(x.Id, leadformIds));
                    }
                    else
                    {
                        Db.Delete<Domain.CRM.LeadForm>(x => x.LeadId == request.Id);
                    }

                    foreach (var leadForm in request.Forms)
                    {
                        if (leadForm.Id.HasValue)
                        {
                            Db.Update(new Domain.CRM.LeadForm
                            {
                                Id = leadForm.Id.Value,
                                LeadId = request.Id,
                                FormId = leadForm.FormId
                            });
                        }
                        else
                        {
                            Db.Insert(new Domain.CRM.LeadForm
                            {
                                LeadId = request.Id,
                                FormId = leadForm.FormId
                            });
                        }
                    }
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
    }
}