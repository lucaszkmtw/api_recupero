using System;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System.Workflows.WorkflowInstance;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    [Authenticate]
    public class WorkflowInstanceService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;

        public WorkflowInstanceService(IAutoQueryDb autoQuery, WorkflowInstanceRepository workflowInstanceRepository)
        {
            _autoQuery = autoQuery;
            _workflowInstanceRepository = workflowInstanceRepository;
        }

        public object Get(Api.GetWorkflowInstancesStatistics request)
        {
            var sql = "SELECT"
                      + " w.Id WorkflowId,"
                      + " w.Name WorkflowName,"
                      + " wt.Id GroupId,"
                      + " wt.Name GroupName,"
                      + " COUNT(*) 'Items'"
                      + " FROM"
                      + " WorkflowInstances wi"
                      + " INNER JOIN Workflows w ON w.Id = wi.WorkflowId"
                      + " INNER JOIN WorkflowInstanceTags wit ON wit.WorkflowInstanceId = wi.Id"
                      + " INNER JOIN WorkflowTags wt ON wt.Id = wit.WorkflowTagId"
                      + " INNER JOIN WorkflowActivities wa ON wa.Id = wi.CurrentWorkflowActivityId"
                      + " WHERE"
                      + " w.Id = ISNULL(@workflowId, w.Id)"
                      //+ " AND(wi.IsTerminated = ISNULL(@isFinal, wi.IsTerminated) AND wa.IsFinal = ISNULL(@isFinal, wa.IsFinal))"
                      + " GROUP BY"
                      + " w.Id,"
                      + " w.Name,"
                      + " wt.Id,"
                      + " wt.Name";

            var items = Db.SqlList<Api.WorkflowInstancesStatistics>(sql, new { workflowId = request.WorkflowId });
            var model = new Api.GetWorkflowInstancesStatisticsResponse();
            foreach (var item in items)
            {
                if (!model.Workflows.ContainsKey(item.WorkflowId))
                {
                    model.Workflows.Add(item.WorkflowId, item.WorkflowName);
                }

                if (!model.Groups.ContainsKey(item.GroupId))
                {
                    model.Groups.Add(item.GroupId, item.GroupName);
                }
            }

            model.Items.AddRange(items.Select(x => new []{x.GroupId, x.WorkflowId, x.Items }));
            return model;
        }

        public async Task<Api.GetWorkflowInstanceResponse> Get(Api.GetWorkflowInstance request)
        {
            return await _workflowInstanceRepository.GetWorkflowInstance(Db, Session, request.Id);
        }

        public LookupResult Get(Api.LookupWorkflowInstance request)
        {
            var query = Db.From<WorkflowInstance>();
            query.Join<Workflow>();

            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Id.ToString().Contains(request.Q));
                query.Or<Workflow>(w => w.Code.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select<Api.LookupItemModel>(query).Select(x => new LookupItem { Id = x.Id, Text = x.WorkflowCode + " - " + x.Id.ToString() }),
                Total = (int)count
            };
            return result;
        }

        public QueryResponse<WorkflowInstance> Get(Api.QueryWorkflowInstances request)
        {
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            return _autoQuery.Execute(request, q);
        }

        public async Task<object> Post(Api.PostWorkflowInstance request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    await _workflowInstanceRepository.InsertWorkflowInstance(Db, Session, request);
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

        public bool Post(Api.AssignWorkflowInstance request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var instance = Db.Single(Db.From<WorkflowInstance>().Where(w => w.Guid == request.WorkflowInstanceGuid));
                    Db.ExecuteNonQuery("UPDATE WorkflowInstanceAssignments SET IsActive = 0 WHERE WorkflowInstanceId = @workflowInstanceId", new { workflowInstanceId = instance.Id });
                    Db.Insert(new WorkflowInstanceAssignments
                    {
                        WorkflowInstanceId = instance.Id,
                        WorkflowActivityId = instance.CurrentWorkflowActivityId,
                        RoleId = request.RoleId,
                        UserId = Session.UserId,
                        IsActive = true,
                        CreateDate = DateTime.UtcNow
                    });

                    trx.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    return false;
                }
            }
        }

        public async Task<object> Post(Api.ApproveWorkflowInstance request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var instance = await Db.SingleAsync(Db.From<WorkflowInstance>().Where(w => w.Guid == request.WorkflowInstanceGuid));
                    await _workflowInstanceRepository.ApproveWorkflowInstance(Db, Session, instance.Id);
                    trx.Commit();
                    return true;
                }
                catch
                {
                    trx.Rollback();
                    return false;
                }
            }
        }

        public object Post(Api.RejectWorkflowInstance request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var instance = Db.Single(Db.From<WorkflowInstance>().Where(w => w.Guid == request.WorkflowInstanceGuid));
                    var activities = Db.Select(Db.From<WorkflowActivity>().Where(w => w.WorkflowId == instance.WorkflowId));
                    var currentActivity = activities.Single(x => x.Id == instance.CurrentWorkflowActivityId);

                    var terminationReason = new { Reason = "", ReasonId = 1 };

                    var transition = new WorkflowInstanceTransition
                    {
                        CreateDate = DateTime.UtcNow,
                        FromWorkflowActivityId = currentActivity.Id,
                        ToWorkflowActivityId = currentActivity.Id,
                        UserId = Session.UserId,
                        WorkflowInstanceId = instance.Id,
                        IsTerminated = true,
                        Data = terminationReason
                    };
                    Db.Insert(transition);

                    //Actualizo el id de actividad actual de la instancia del workflow
                    instance.CurrentWorkflowActivityId = currentActivity.Id;

                    instance.IsTerminated = true;
                    Db.Update(instance);

                    // Hardcoding para Loans
                    if (instance.WorkflowId == 37)
                    {
                        var loan = Db.Select<Domain.Loans.Loan>(x => x.AuthorizationWorkflowInstanceId == instance.Id).Single();

                        loan.Status = Domain.Loans.LoanStatus.Voided;
                                
                        Db.Update(loan);
                    }
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                }
            }

            return request;
        }

        public bool Post(Api.SetPreviousStateWorkflowInstance request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var instance = Db.Single(Db.From<WorkflowInstance>().Where(w => w.Guid == request.WorkflowInstanceGuid));
                    var activities = Db.Select(Db.From<WorkflowActivity>().Where(w => w.WorkflowId == instance.WorkflowId));
                    var currentActivity = activities.Single(x => x.Id == instance.CurrentWorkflowActivityId);
                    var prevActivity = activities.First(x => x.ListIndex == currentActivity.ListIndex - 1);

                    if (prevActivity != null)
                    {
                        var transition = new WorkflowInstanceTransition
                        {
                            CreateDate = DateTime.UtcNow,
                            FromWorkflowActivityId = currentActivity.Id,
                            ToWorkflowActivityId = prevActivity.Id,
                            UserId = Session.UserId,
                            WorkflowInstanceId = instance.Id,
                            IsTerminated = false
                        };
                        Db.Insert(transition);

                        //Actualizo el id de actividad actual de la instancia del workflow
                        instance.CurrentWorkflowActivityId = prevActivity.Id;

                        //Obtengo el rol que lo tenia asignado previamente
                        int prevRoleId;
                        var prevInstanceRole = Db.Select(Db.From<WorkflowInstanceAssignments>()
                            .Where(w => w.WorkflowInstanceId == instance.Id)
                            .And(w => w.IsActive == false)
                            .OrderByDescending(o => o.CreateDate)).FirstOrDefault();
                        if (prevInstanceRole == null)
                        {
                            //Obtengo el default desde WorkflowActivityRoles
                            var war =
                                Db.Select<WorkflowActivityRole>(
                                    w => w.WorkflowActivityId == prevActivity.Id && w.IsDefault).SingleOrDefault();
                            if (war == null)
                            {
                                throw new ApplicationException("There is an error with the workflow definition.");
                            }
                            prevRoleId = war.RoleId;
                        }
                        else
                        {
                            prevRoleId = prevInstanceRole.RoleId;
                        }



                        //Quito la asignación actual
                        //Deactivate current active Assignments
                        Db.ExecuteNonQuery("UPDATE WorkflowInstanceAssignments SET IsActive = 0 WHERE WorkflowInstanceId = @workflowInstanceId", new { workflowInstanceId = instance.Id });

                        //Le asigno la actividad al rol que lo tenía previamente asignado
                        Db.Insert(new WorkflowInstanceAssignments
                        {
                            WorkflowInstanceId = instance.Id,
                            WorkflowActivityId = instance.CurrentWorkflowActivityId,
                            RoleId = prevRoleId,
                            UserId = Session.UserId,
                            IsActive = true,
                            CreateDate = DateTime.UtcNow
                        });

                        //Si estaba cancelado lo vuelvo a estado no cancelado
                        if (instance.IsTerminated)
                        {
                            instance.IsTerminated = false;
                        }

                        instance.Progress = (decimal)((prevActivity.ListIndex - (activities.Where(x => x.IsStart).Count() > 0 ? 0 : 1)) * 100) / activities.Where(x => !x.IsStart && !x.IsFinal).Count();

                        Db.Update(instance);


                        // Hardcoding para Loans
                        if (instance.WorkflowId == 37)
                        {
                            var loan = Db.Select<Domain.Loans.Loan>(x => x.AuthorizationWorkflowInstanceId == instance.Id).Single();
                            switch (prevActivity.Id)
                            {
                                case 146: // Start
                                    loan.Status = Domain.Loans.LoanStatus.Pending;
                                    break;
                                case 147: // Evaluación
                                    loan.Status = Domain.Loans.LoanStatus.InEvaluation;
                                    break;
                                case 148: // Firma documentación
                                    loan.Status = Domain.Loans.LoanStatus.Approved;
                                    break;
                                case 149: // Liquidacion
                                    loan.Status = Domain.Loans.LoanStatus.ToExecute;
                                    break;
                                case 150: // Gestión pago
                                    loan.Status = Domain.Loans.LoanStatus.PendingPayment;
                                    break;
                                case 151: // Archivo legajo
                                    loan.Status = Domain.Loans.LoanStatus.Paid;
                                    break;
                                case 152: // Finalizado
                                    loan.Status = Domain.Loans.LoanStatus.Portfolio;
                                    break;
                            }
                            Db.Update(loan);
                        }
                    }

                    trx.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    return false;
                }
            }
        }

        //SetStateWorkflowInstance
        public object Post(Api.SetStateWorkflowInstance request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var instance = Db.Single(Db.From<WorkflowInstance>().Where(w => w.Guid == request.WorkflowInstanceGuid));
                    var activities = Db.Select(Db.From<WorkflowActivity>().Where(w => w.WorkflowId == instance.WorkflowId));
                    var currentActivity = activities.Single(x => x.Id == instance.CurrentWorkflowActivityId);
                    var currentInstanceAssignment = Db.Select(Db.From<WorkflowInstanceAssignments>().Where(w => w.WorkflowInstanceId == instance.Id)).FirstOrDefault();
                    var workflowActivitiesTransitions = Db.Select(Db.From<WorkflowActivityTransition>().Where(x => x.WorkflowId == instance.WorkflowId)).ToList();

                    var transition = new WorkflowInstanceTransition();

                    if (workflowActivitiesTransitions.Count() > 0)
                    {
                        //se establecio reglas de transicion de actividades
                        var currentWorkflowActivityTransitions = workflowActivitiesTransitions.Where(x => x.FromWorkflowActivityId == currentActivity.Id && x.ToWorkflowActivityId == request.WorkflowActivityId);
                        if (currentWorkflowActivityTransitions.Count() < 1)
                        {
                            return "false";
                        }
                    }

                    transition = new WorkflowInstanceTransition
                    {
                        CreateDate = DateTime.UtcNow,
                        FromWorkflowActivityId = currentActivity.Id,
                        ToWorkflowActivityId = request.WorkflowActivityId,
                        UserId = Session.UserId,
                        WorkflowInstanceId = instance.Id,
                        IsTerminated = false
                    };
                    

                    var workflowActivityEvents = Db.Select(Db.From<WorkflowActivityEvent>().Where(we => we.WorkflowActivityId == currentActivity.Id));
                    foreach (var workflowActivityEvent in workflowActivityEvents)
                    {
                        var workflowEvent = Db.SingleById<WorkflowEvent>(workflowActivityEvent.WorkflowEventId);
                        if (workflowEvent != null)
                        {
                            switch (workflowEvent.Name)
                            {
                                case "STATE_TE_DEBT_COLLECTOR":
                                    break;
                                default:
                                    throw new ApplicationException("There is an error with the workflowEvent definition.");
                                    
                            }
                        }
                    }

                    transition.Id = (int)Db.Insert(transition, true);
                    //Actualizo el id de actividad actual de la instancia del workflow
                    //instance.CurrentWorkflowActivityId = prevActivity.Id;
                    instance.CurrentWorkflowActivityId = request.WorkflowActivityId;


                    //Quito la asignación actual
                    //Deactivate current active Assignments
                    Db.ExecuteNonQuery("UPDATE WorkflowInstanceAssignments SET IsActive = 0 WHERE WorkflowInstanceId = @workflowInstanceId", new { workflowInstanceId = instance.Id });

                    //Le asigno la actividad al rol que lo tenía previamente asignado
                    Db.Insert(new WorkflowInstanceAssignments
                    {
                        WorkflowInstanceId = instance.Id,
                        WorkflowActivityId = instance.CurrentWorkflowActivityId,
                        //RoleId = prevRoleId,
                        RoleId = currentInstanceAssignment.RoleId,
                        UserId = Session.UserId,
                        IsActive = true,
                        CreateDate = DateTime.UtcNow
                    });

                    //Si estaba cancelado lo vuelvo a estado no cancelado
                    if (instance.IsTerminated)
                    {
                        instance.IsTerminated = false;
                    }

                    instance.Progress = (decimal)((currentActivity.ListIndex - (activities.Where(x => x.IsStart).Count() > 0 ? 0 : 1)) * 100) / activities.Where(x => !x.IsStart && !x.IsFinal).Count();

                    Db.Update(instance);

                    trx.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    return false;
                }
            }
        }

        public object Post(Api.TerminateWorkflowInstance request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var instance = Db.Single(Db.From<WorkflowInstance>().Where(w => w.Guid == request.WorkflowInstanceGuid));
                    var activities = Db.Select(Db.From<WorkflowActivity>().Where(w => w.WorkflowId == instance.WorkflowId));
                    var currentActivity = activities.Single(x => x.Id == instance.CurrentWorkflowActivityId);

                    var terminationReason = new { Reason = "", ReasonId = 1 };

                    var transition = new WorkflowInstanceTransition
                    {
                        CreateDate = DateTime.UtcNow,
                        FromWorkflowActivityId = currentActivity.Id,
                        ToWorkflowActivityId = currentActivity.Id,
                        UserId = Session.UserId,
                        WorkflowInstanceId = instance.Id,
                        IsTerminated = true,
                        Data = terminationReason
                    };
                    Db.Insert(transition);

                    //Actualizo el id de actividad actual de la instancia del workflow
                    instance.CurrentWorkflowActivityId = currentActivity.Id;

                    instance.IsTerminated = true;
                    Db.Update(instance);

                    // Hardcoding para Loans
                    if (instance.WorkflowId == 37)
                    {
                        var loan = Db.Select<Domain.Loans.Loan>(x => x.AuthorizationWorkflowInstanceId == instance.Id).Single();

                        loan.Status = Domain.Loans.LoanStatus.Cancelled;

                        Db.Update(loan);
                    }

                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                }
            }

            return request;
        }

        public async Task<object> Put(Api.PostWorkflowInstance request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    await _workflowInstanceRepository.UpdateWorkflowInstance(Db, Session, request);
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
    }
}