using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.Health;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.Persons;
using CentralOperativa.ServiceModel.System.Workflows;
using CentralOperativa.ServiceInterface.System.Workflows;
using ServiceStack;
using ServiceStack.OrmLite;
using WorkflowInstance = CentralOperativa.ServiceModel.System.Workflows.WorkflowInstance;

namespace CentralOperativa.ServiceInterface.Health
{
    using Api = ServiceModel.Health;

    [Authenticate]
    public class ClaimService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;
        private readonly WorkflowActivityRepository _workflowActivityRepository;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;

        public ClaimService(
            IAutoQueryDb autoQuery,
            PersonRepository personRepository,
            WorkflowActivityRepository workflowActivityRepository,
            WorkflowInstanceRepository workflowInstanceRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _workflowActivityRepository = workflowActivityRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
        }

        public async Task<Api.GetClaimResponse> Get(Api.GetClaim request)
        {
            var claim = (await Db.SingleByIdAsync<Claim>(request.Id)).ConvertTo<Api.GetClaimResponse>();

            var workflowInstance = await _workflowInstanceRepository.GetWorkflowInstance(Db, Session, claim.WorkflowInstanceId);
            claim.WorkflowInstance = workflowInstance;

            // LinkedPersons
            var claimPersons = await Db.SelectAsync(Db.From<ClaimPerson>().Where(w => w.ClaimId == claim.Id));
            foreach (var claimPerson in claimPersons)
            {
                var person = await _personRepository.GetPerson(Db, claimPerson.PersonId);
                var linkedPerson = new Api.LinkedPerson { Id = claimPerson.Id, Person = person };
                claim.LinkedPersons.Add(linkedPerson);
            }

            // LinkedWorkflowInstances
            var claimProcesses = await Db.SelectAsync(Db.From<ClaimProcess>().Where(w => w.ClaimId == claim.Id));
            foreach (var claimProcess in claimProcesses)
            {
                claim.LinkedProcesses.Add(claimProcess.ConvertTo<Api.LinkedProcess>());
            }

            return claim;
        }

        public async Task<object> Get(Api.QueryClaims request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            int? personId = null;
            var parameters = Request.GetRequestParams();
            if (parameters.ContainsKey("personId"))
            {
                personId = int.Parse(parameters["personId"]);
                parameters.Remove("personId");
            }

            var q = _autoQuery.CreateQuery(request, parameters)
            .Join<Claim, Domain.System.Persons.Person>()
            .Join<Claim, Domain.System.Workflows.WorkflowInstance>()
            .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id)
            .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.Workflow>();

            if (personId.HasValue)
            {
                q.Join<Claim, ClaimPerson>();

                if (q.WhereExpression == null)
                {
                    q.WhereExpression += "WHERE";
                }
                q.WhereExpression += " (";
                q.And(w => w.PersonId == personId);
                q.Or<ClaimPerson>(w => w.PersonId == personId);
                q.WhereExpression = q.WhereExpression.Replace("( AND", "(") + " )";
            }

            var roleIds = await Db.ColumnAsync<int>(Session.Roles.Contains("admin") ? 
                Db.From<Domain.System.Role>().Where(w => w.Id != 1).Select(x => x.Id) : 
                Db.From<Domain.System.Role>().Where(w => Sql.In(w.Name, Session.Roles)));

            SqlExpression<Claim> wiIdsQ = null;
            switch (request.View)
            {
                case 0: //Own
                    wiIdsQ = Db.From<Claim>()
                        .Join<Claim, Domain.System.Persons.Person>()
                        .Join<Claim, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && Sql.In(wir.RoleId, roleIds))
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 1: //Supervised
                    wiIdsQ = Db.From<Claim>()
                        .Join<Claim, Domain.System.Persons.Person>()
                        .Join<Claim, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Workflows.WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission == 2 && Sql.In(warp.RoleId, roleIds))
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 2: //Others
                    wiIdsQ = Db.From<Claim>()
                        .Join<Claim, Domain.System.Persons.Person>()
                        .Join<Claim, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Workflows.WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission < 2 && Sql.In(warp.RoleId, roleIds))
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 3: //Terminated
                    wiIdsQ = Db.From<Claim>()
                        .Join<Claim, Domain.System.Persons.Person>()
                        .Join<Claim, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Workflows.WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && Sql.In(warp.RoleId, roleIds))
                        .SelectDistinct(x => x.WorkflowInstanceId);
                    break;
                case 4: //Finished
                    wiIdsQ = Db.From<Claim>()
                        .Join<Claim, Domain.System.Persons.Person>()
                        .Join<Claim, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal)
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
                wiIdsQ.Or<Domain.System.Workflows.WorkflowActivity>(w => w.Name.Contains(request.Q));
                wiIdsQ.UnsafeOr("Claims.MessageThreadId IN (SELECT DISTINCT m.MessageThreadId FROM Messages m WHERE CONTAINS(m.Body, {0}))", "\"" + request.Q + "*\"");
                wiIdsQ.UnsafeOr("Claims.Id IN (SELECT cp.ClaimId FROM ClaimPersons cp INNER JOIN Persons p ON p.Id = cp.PersonId WHERE p.Name LIKE '%" + request.Q + "%')");
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
            var roleMap = await Db.SelectAsync<WorkflowInstanceRoleMap>(Db
                .From<WorkflowInstanceAssignments>()
                .Join<Domain.System.Role>()
                .Where(w => Sql.In(w.WorkflowInstanceId, wiIdsQ))
                .And(w => w.IsActive));
            result.Results.ForEach(x => x.Roles = string.Join(",", roleMap.Where(w => w.WorkflowInstanceId == x.WorkflowInstanceId).Select(w => w.RoleName)));
            return result;
        }

        public class IdColumn
        {
            public int Id { get; set; }
        }

        public LookupResult Get(Api.LookupClaim request)
        {
            var query = Db.From<Claim>();

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where<Domain.System.Messages.MessageThread>(q => q.CreateDate.ToString(CultureInfo.InvariantCulture).Contains(request.Q));
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

        public async Task<Api.PostClaim> Put(Api.PostClaim request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var data = request.ConvertTo<Claim>();
                    await Db.UpdateAsync(data);
                    //request.WorkflowInstance = await _workflowInstanceRepository.InsertWorkflowInstance(Db, Session, request.WorkflowInstance);
                    //Db.Update((Claim)request);
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

        public async Task<Api.PostClaim> Post(Api.PostClaim request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    //WorkflowInstance
                    var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 1, (short)WellKnownWorkflowTypes.Claim);
                    request.WorkflowInstance.WorkflowId = currentActivity.WorkflowId;
                    request.WorkflowInstance.CreatedByUserId = Session.UserId;
                    request.WorkflowInstance.CreateDate = DateTime.UtcNow;
                    request.WorkflowInstance.Guid = Guid.NewGuid();
                    request.WorkflowInstance.CurrentWorkflowActivityId = currentActivity.Id;
                    request.WorkflowInstance = (WorkflowInstance.PostWorkflowInstance)HostContext.ServiceController.Execute(request.WorkflowInstance, Request);
                    request.WorkflowInstanceId = request.WorkflowInstance.Id;

                    // Message Thread
                    if (!string.IsNullOrEmpty(request.Text))
                    {
                        var messageThread = new Domain.System.Messages.MessageThread { CreateDate = DateTime.UtcNow };
                        messageThread.Id = (int)Db.Insert(messageThread, true);
                        request.MessageThreadId = messageThread.Id;

                        var message = new Domain.System.Messages.Message
                        {
                            MessageThreadId = messageThread.Id,
                            CreateDate = DateTime.UtcNow,
                            SenderId = Session.UserId,
                            Body = request.Text
                        };
                        message.Id = (int)Db.Insert(message, true);
                    }

                    request.Id = (int)Db.Insert((Claim)request, true);
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

        private void Save(Api.PostClaim request)
        {
            //LinkedPersons
            var linkedPersonIds = request.LinkedPersons.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
            if (linkedPersonIds.Any())
            {
                Db.Delete<ClaimPerson>(x => x.ClaimId == request.Id && !Sql.In(x.Id, linkedPersonIds));
            }
            else
            {
                Db.Delete<ClaimPerson>(x => x.ClaimId == request.Id);
            }

            foreach (var linkedPerson in request.LinkedPersons)
            {
                if (!linkedPerson.Id.HasValue)
                {
                    Db.Insert(new ClaimPerson
                    {
                        ClaimId = request.Id,
                        PersonId = linkedPerson.Person.Id
                    });
                }
            }

            //LinkedWorkflowInstances
            var linkedWorkflowInstanceIds = request.LinkedWorkflowInstances.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
            if (linkedWorkflowInstanceIds.Any())
            {
                Db.Delete<ClaimWorkflowInstance>(x => x.ClaimId == request.Id && !Sql.In(x.Id, linkedWorkflowInstanceIds));
            }
            else
            {
                Db.Delete<ClaimWorkflowInstance>(x => x.ClaimId == request.Id);
            }

            foreach (var linkedWorkflowInstance in request.LinkedWorkflowInstances)
            {
                if (!linkedWorkflowInstance.Id.HasValue)
                {
                    Db.Insert(new ClaimWorkflowInstance
                    {
                        ClaimId = request.Id,
                        WorkflowInstanceId = linkedWorkflowInstance.WorkflowInstance.Id
                    });
                }
            }
        }
    }
}