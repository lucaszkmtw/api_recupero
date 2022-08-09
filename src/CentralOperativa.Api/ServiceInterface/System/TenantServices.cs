using System;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.System;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.DocumentManagement;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System;

namespace CentralOperativa.ServiceInterface.System
{
    [Authenticate]
    public class TenantService : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly TenantRepository _tenantRepository;
        private readonly FolderRepository _folderRepository;

        public TenantService(IAutoQueryDb autoQuery, TenantRepository tenantRepository, FolderRepository folderRepository)
        {
            _autoQuery = autoQuery;
            _tenantRepository = tenantRepository;
            _folderRepository = folderRepository;
        }

        public LookupResult Get(Api.LookupTenant request)
        {
            var query = Db
                .From<Tenant>()
                .Join<Tenant, Domain.System.Persons.Person>();
            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            else
            {
                if (!string.IsNullOrEmpty(request.Q))
                {
                    var tokens = request.Q.Split(' ');
                    foreach (var token in tokens)
                    {
                        if (int.TryParse(token, out _))
                        {
                            query.Where<Domain.System.Persons.Person>(
                                x => x.Name.Contains(token) || x.Code.Contains(token));
                        }
                        else
                        {
                            query.Where<Domain.System.Persons.Person>(x => x.Name.Contains(token));
                        }
                    }
                }
            }

            var count = Db.Count(query);
            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));
            var result = new LookupResult
            {
                Data = Db
                    .Select<Api.GetTenantResponse>(query)
                    .Select(x => new LookupItem { Id = x.Id, Text = $"{x.Name} ({x.PersonName})"}), Total = (int)count
            };
            return result;
        }

        public Api.PostTenant Put(Api.PostTenant request)
        {
            var tenant = Db.SingleById<Tenant>(request.Id);
            tenant.Name = request.Name;
            tenant.PersonId = request.PersonId;
            Db.Update(tenant);
            return request;
        }

        public async Task<Api.PostTenant> Post(Api.PostTenant request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var folder = await _folderRepository.CreateFolder(Db, Session, null, "sy.dm.tenantprofile");
                    request.FolderId = folder.Id;
                    request.Id = (int) await Db.InsertAsync((Tenant)request, true);

                    //Genero el rol admin para este nuevo tenant
                    var role = new Role { Name = "admin", TenantId = request.Id };
                    role.Id = (int) await Db.InsertAsync(role, true);

                    var permission = await Db.SingleAsync(Db.From<Permission>().Where(w => w.Name == "system.roles"));
                    var rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
                    await Db.InsertAsync(rolePermission);

                    //Genero la sección root para el help
                    var helpSection = new Domain.Cms.Section { Name = "Help", TenantId = request.Id, CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow };
                    await Db.InsertAsync(helpSection);

                    //Asigno los usuarios system.sysadmin a este rol administrativo del nuevo tenant y los asigno como usuarios
                    var sysAdmins = await Db.SelectAsync(Db.From<User>()
                                .Join<User, UserPermission>()
                                .Join<UserPermission, Permission>()
                                .Where<Permission>(w => w.Name == "system.sysadmin")
                                .SelectDistinct());
                    foreach (var sysAdmin in sysAdmins)
                    {
                        var userRole = new UserRole { UserId = sysAdmin.Id, RoleId = role.Id };
                        await Db.InsertAsync(userRole);

                        var tenantUser = new TenantUser { TenantId = request.Id, UserId = sysAdmin.Id, CreatedById = Session.UserId, CreateDate = DateTime.UtcNow };
                        await Db.InsertAsync(tenantUser);
                    }

                    await _tenantRepository.InitializeTenant(Db, request.Id);

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

        public object Get(Api.QueryTenants request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "PersonName";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            return _autoQuery.Execute(request, q);
        }

        public async Task<Api.GetTenantResponse> Get(Api.GetTenant request)
        {
            return await _tenantRepository.GetTenant(Db, request.Id);
        }

        public async Task Delete(Api.DeleteTenant request)
        {
            await Db.DeleteByIdAsync<Tenant>(request.Id);
        }

        public async Task<object> Get(Api.CheckTenants request)
        {
            //Check workflows are initialized
            var tenants = await Db.SelectAsync(Db.From<Tenant>());
            var workflowTypes = await Db.SelectAsync(Db.From<Domain.System.Workflows.WorkflowType>());
            var workflows = await Db.SelectAsync(Db.From<Domain.System.Workflows.Workflow>());
            foreach(var tenant in tenants)
            {
                foreach(var workflowType in workflowTypes)
                {
                    var workflow = workflows.Where(w => w.TenantId == tenant.Id && w.TypeId == workflowType.Id).FirstOrDefault();
                    if(workflow == null)
                    {
                        workflow = workflows.Where(w => w.TypeId == workflowType.Id).FirstOrDefault();
                        if (workflow != null)
                        {
                            Db.CopyWorkflow(workflow.Id, tenant.Id);
                        }
                    }
                }
            }

            return true;
        }
    }
}