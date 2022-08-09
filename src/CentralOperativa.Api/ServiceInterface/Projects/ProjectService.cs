using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.Projects;
using CentralOperativa.Domain.System.Location;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.DocumentManagement;
using CentralOperativa.ServiceInterface.System.Persons;
using CentralOperativa.ServiceModel.System.Workflows;
using CentralOperativa.ServiceInterface.System.Workflows;
using Api = CentralOperativa.ServiceModel.Projects;

namespace CentralOperativa.ServiceInterface.Projects
{
    [Authenticate]
    public class AssetService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;
        private readonly WorkflowActivityRepository _workflowActivityRepository;
        private readonly PersonRepository _personRepository;
        private readonly FolderRepository _folderRepository;
        private readonly ProjectRepository _projectRepository;

        private readonly Guid _projectsFolderGuid = new Guid("3707c62c-a8da-438e-b3c9-8247b0f8c66e");

        public AssetService(
            IAutoQueryDb autoQuery, 
            WorkflowInstanceRepository workflowInstanceRepository, 
            WorkflowActivityRepository workflowActivityRepository,
            PersonRepository personRepository,
            FolderRepository folderRepository,
            ProjectRepository projectRepository)
        {
            _autoQuery = autoQuery;
            _workflowInstanceRepository = workflowInstanceRepository;
            _workflowActivityRepository = workflowActivityRepository;
            _personRepository = personRepository;
            _folderRepository = folderRepository;
            _projectRepository = projectRepository;
        }

        public async Task<object> Put(Api.PostProject request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var data = await Db.SingleByIdAsync<Project>(request.Id);
                    // TODO: Change this to permission check
                    if (data.TenantId != Session.TenantId)
                    {
                        throw new HttpError(HttpStatusCode.Forbidden);
                    }

                    data.PopulateWith(request);
                    await Db.UpdateAsync(data);
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

        public async Task<object> Post(Api.PostProject request)
        {
            request.CreateDate = DateTime.UtcNow;
            request.CreatedBy = Session.UserId;

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var existing = await Db.SelectAsync(Db.From<Project>().Where(w => w.Number == request.Number));
                    if (existing.Any())
                    {
                        trx.Rollback();
                        return HttpError.Conflict("ERR_Project_AlreadyExists");
                    }

                    //WorkflowInstance
                    var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 1, (short)WellKnownWorkflowTypes.Project);
                    var postWorkflowInstance = new ServiceModel.System.Workflows.WorkflowInstance.PostWorkflowInstance
                    {
                        WorkflowId = currentActivity.WorkflowId,
                        CreatedByUserId = Session.UserId,
                        CreateDate = DateTime.UtcNow,
                        Guid = Guid.NewGuid(),
                        CurrentWorkflowActivityId = currentActivity.Id
                    };
                    var workflowInstance = await _workflowInstanceRepository.InsertWorkflowInstance(Db, Session, postWorkflowInstance);
                    request.WorkflowInstance = workflowInstance;
                    request.Guid = Guid.NewGuid();
                    var data = request.ConvertTo<Project>();
                    data.WorkflowInstanceId = workflowInstance.Id;
                    data.TenantId = Session.TenantId;

                    //Folder
                    if (!request.FolderGuid.HasValue)
                    {
                        var folder = await _folderRepository.CreateFolder(Db, Session, _projectsFolderGuid, request.Name);
                        data.FolderId = folder.Id;
                        request.FolderGuid = folder.Guid;
                    }

                    request.Id = (int) await Db.InsertAsync(data, true);
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

        private async Task Save(Api.PostProject request)
        {
            // Tasks
            var currentProjectTasks = await Db.SelectAsync<ProjectTask>(w => w.ProjectId == request.Id);
            await Db.DeleteByIdsAsync<ProjectTask>(currentProjectTasks.Select(x => x.Id).Except(request.Tasks.Where(w => w.Id != 0).Select(s => s.Id)));
            foreach (var item in request.Tasks)
            {
                if (item.Id == 0)
                {
                    item.ProjectId = request.Id;
                    Db.Insert(item);
                }
                else
                {
                    Db.Update(item);
                }
            }

            // Categories
            var currentProjectCategories = await Db.SelectAsync<ProjectCategory>(w => w.ProjectId == request.Id);
            await Db.DeleteByIdsAsync<ProjectCategory>(currentProjectCategories.Select(x => x.Id).Except(request.Categories.Where(w => w.Id != 0).Select(s => s.Id)));
            foreach (var item in request.Categories)
            {
                var dataItem = item.ConvertTo<ProjectCategory>();
                dataItem.ProjectId = request.Id;
                dataItem.CategoryId = item.Category.Id;
                if (dataItem.Id == 0)
                {
                    Db.Insert(dataItem);
                }
                else
                {
                    Db.Update(dataItem);
                }
            }

            // Places
            var currentProjectPlaces = await Db.SelectAsync<ProjectPlace>(w => w.ProjectId == request.Id);
            await Db.DeleteByIdsAsync<ProjectPlace>(currentProjectPlaces.Select(x => x.Id).Except(request.Places.Where(w => w.Id != 0).Select(s => s.Id)));
            foreach (var item in request.Places)
            {
                var dataItem = item.ConvertTo<ProjectPlace>();
                dataItem.ProjectId = request.Id;
                dataItem.PlaceId = item.Place.Id;
                if (item.Id == 0)
                {
                    Db.Insert(dataItem);
                }
                else
                {
                    Db.Update(dataItem);
                }
            }

            //Members
            var currentMembers = await Db.SelectAsync(Db.From<ProjectMember>().Where(w => w.ProjectId == request.Id));
            await Db.DeleteByIdsAsync<ProjectMember>(currentMembers.Select(x => x.Id).Except(request.Members.Where(w => w.Id != 0).Select(s => s.Id)));
            foreach (var member in request.Members)
            {
                var projectMember = member.ConvertTo<ProjectMember>();
                projectMember.ProjectId = request.Id;
                projectMember.PersonId = member.Person.Id;
                projectMember.RoleId = member.Role.Id;
                if (projectMember.Id == 0)
                {
                    projectMember.Id = (int) await Db.InsertAsync(projectMember, true);

                    //Tags
                    foreach (var tag in member.Tags)
                    {
                        var projectMemberTag = new ProjectMemberTag
                        {
                            ProjectMemberId = projectMember.Id,
                            Name = tag
                        };
                        await Db.InsertAsync(projectMemberTag);
                    }
                }
                else
                {
                    await Db.UpdateAsync(projectMember);

                    var currentProjectMemberTags = await Db.SelectAsync(Db.From<ProjectMemberTag>().Where(w => w.ProjectMemberId == projectMember.Id));
                    var tagsToDelete = currentProjectMemberTags.Where(w => !member.Tags.Contains(w.Name));
                    await Db.DeleteByIdsAsync<ProjectMemberTag>(tagsToDelete.Select(x => x.Id));
                    var tagsToAdd = member.Tags.Except(currentProjectMemberTags.Select(x => x.Name)).Select(x => new ProjectMemberTag { Name = x, ProjectMemberId = projectMember.Id });
                    await Db.InsertAllAsync(tagsToAdd);
                }
            }
        }

        public async Task<Api.Project> Get(Api.GetProject request)
        {
            return await _projectRepository.GetProject(Db, Session, request.Id);
        }

        public async Task<object> Get(Api.QueryProjects request)
        {
            // Si no trae view asigno all
            if (!request.View.HasValue) request.View = 5;

            var parameters = Request.GetRequestParams();
            var q = Db.From<Project>()
                .Join<Project, Domain.System.Workflows.WorkflowInstance>()
                .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id)
                .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.Workflow>();

            var roleIds = Db.Column<int>(Session.Roles.Contains("admin") ?
                Db.From<Domain.System.Role>().Where(w => w.TenantId == Session.TenantId && w.Name != "admin").Select(x => x.Id) :
                Db.From<Domain.System.Role>().Where(w => Sql.In(w.Name, Session.Roles)));

            var wiIdsQ = Db.From<Project>();
            switch (request.View)
            {
                case 0: //Own
                    wiIdsQ
                        .Join<Project, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && Sql.In(wir.RoleId, roleIds));
                    break;
                case 1: //Supervised
                    wiIdsQ
                        .Join<Project, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Workflows.WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission == 2 && Sql.In(warp.RoleId, roleIds));
                    break;
                case 2: //Others
                    wiIdsQ
                        .Join<Project, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive && !Sql.In(wir.RoleId, roleIds))
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Workflows.WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && warp.Permission < 2 && Sql.In(warp.RoleId, roleIds));
                    break;
                case 3: //Terminated
                    wiIdsQ
                        .Join<Project, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, WorkflowInstanceAssignments>((wi, wir) => wi.Id == wir.WorkflowInstanceId && wir.IsActive)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivityRole>((wi, war) => war.WorkflowActivityId == wi.CurrentWorkflowActivityId)
                        .Join<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Workflows.WorkflowActivityRolePermission>((war, warp) => war.Id == warp.WorkflowActivityRoleId && Sql.In(warp.RoleId, roleIds));
                    break;
                case 4: //Finished
                    wiIdsQ
                        .Join<Project, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id && wi.IsTerminated == false)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id && wa.IsFinal);
                    break;
                case 5: //All
                    wiIdsQ
                        .Join<Project, Domain.System.Workflows.WorkflowInstance>((c, wi) => c.WorkflowInstanceId == wi.Id)
                        .Join<Domain.System.Workflows.WorkflowInstance, Domain.System.Workflows.WorkflowActivity>((wi, wa) => wi.CurrentWorkflowActivityId == wa.Id);
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
                wiIdsQ.UnsafeOr("Projects.MessageThreadId IN (SELECT DISTINCT m.MessageThreadId FROM Messages m WHERE CONTAINS(m.Body, {0}))", "\"" + request.Q + "*\"");
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

            wiIdsQ.Where(w => w.TenantId == Session.TenantId);
            wiIdsQ.SelectDistinct(x => x.WorkflowInstanceId);
            q.Where<Domain.System.Workflows.WorkflowInstance>(w => Sql.In(w.Id, wiIdsQ));
            var count = (int) Db.Count(q);
            q = request.GetLimit(q).Select(x => x.Id);
            var ids = Db.Select<int>(q);
            var items = new List<Api.Project>();

            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    items.Add(await _projectRepository.GetProject(Db, Session, id));
                }

                var workflowInstanceIds = items.Select(x => x.WorkflowInstance?.Id).ToList();
                if (workflowInstanceIds.Any())
                {
                    var roleMap = Db.Select<WorkflowInstanceRoleMap>(Db
                        .From<WorkflowInstanceAssignments>()
                        .Join<Domain.System.Role>()
                        .Where(w => Sql.In(w.WorkflowInstanceId, workflowInstanceIds))
                        .And(w => w.IsActive));
                    items.ForEach(x => x.Roles = string.Join(",",
                        roleMap.Where(w => w.WorkflowInstanceId == x.WorkflowInstance.Id).Select(w => w.RoleName)));
                }
            }

            var model = new QueryResponse<Api.Project>
            {
                Total = count,
                Offset = request.Skip ?? 0,
                Results = items
            };

            return model;
        }

        public object Get(Api.LookupProject request)
        {
            var query = Db.From<Project>();
            query.Where(w => w.TenantId == Session.TenantId);
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

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Number }),
                Total = (int)count
            };
            return result;
        }
    }
}