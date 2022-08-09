using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.Projects;
using CentralOperativa.Domain.System.Location;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.Financials.Controlling;
using CentralOperativa.ServiceInterface.System.DocumentManagement;
using CentralOperativa.ServiceInterface.System.Persons;
using CentralOperativa.ServiceInterface.System.Workflows;
using Api = CentralOperativa.ServiceModel.Projects;

namespace CentralOperativa.ServiceInterface.Projects
{
    public class ProjectRepository
    {
        private readonly PersonRepository _personRepository;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;
        private readonly CostCenterRepository _costCenterRepository;
        private readonly FolderRepository _folderRepository;

        public ProjectRepository(
            PersonRepository personRepository,
            WorkflowInstanceRepository workflowInstanceRepository,
            CostCenterRepository costCenterRepository,
            FolderRepository folderRepository)
        {
            _personRepository = personRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
            _costCenterRepository = costCenterRepository;
            _folderRepository = folderRepository;
        }

        public async Task<List<Api.ProjectCostCenter>> GetProjectCostCenters(IDbConnection db, int projectId)
        {
            var costCenters = await db.SelectAsync(db.From<Domain.Financials.Controlling.CostCenter>()
                .Join<Domain.Financials.Controlling.CostCenter, ProjectCostCenter>()
                .Where<ProjectCostCenter>(w => w.ProjectId == projectId).SelectDistinct());
            var costCentersDictionary = costCenters.ToDictionary(x => x.Id);
            var data = db.Select(db.From<ProjectCostCenter>().Where(w => w.ProjectId == projectId));
            var model = new List<Api.ProjectCostCenter>();
            data.ForEach(x => model.Add(new Api.ProjectCostCenter
            {
                Id = x.Id,
                CostCenter = costCentersDictionary[x.CostCenterId]
                    .ConvertTo<ServiceModel.Financials.Controlling.CostCenter>()
            }));
            return model;
        }

        public async Task<Api.ProjectCostCenter> GetProjectCostCenter(IDbConnection db, int id)
        {
            var data = await db.SingleByIdAsync<ProjectCostCenter>(id);
            var costCenter = await _costCenterRepository.GetCostCenter(db, data.CostCenterId);
            var model = new Api.ProjectCostCenter {Id = data.Id, CostCenter = costCenter};
            return model;
        }

        public async Task<Api.Project> GetProject(IDbConnection db, Session session, int id)
        {
            var data = await db.SingleByIdAsync<Project>(id);
            var model = data.ConvertTo<Api.Project>();

            // FolderGuid
            if (data.FolderId.HasValue)
            {
                var folder = await _folderRepository.GetFolder(db, data.FolderId.Value, session, false);
                model.FolderGuid = folder.Guid;
            }


            // Tasks
            var tasks = await db.SelectAsync<ProjectTask>(w => w.ProjectId == model.Id);
            model.Tasks.AddRange(tasks);

            // WorkflowInstance
            var workflowInstance =
                await _workflowInstanceRepository.GetWorkflowInstance(db, session, data.WorkflowInstanceId);
            model.WorkflowInstance = workflowInstance;

            // Members
            var membersQ = db.From<ProjectMember>()
                .Join<ProjectMember, ProjectMemberRole>()
                .Where(w => w.ProjectId == id);
            var membersR = db.SelectMulti<ProjectMember, ProjectMemberRole>(membersQ);

            var members = new List<Api.ProjectMember>();
            foreach (var tuple in membersR)
            {
                var member = new Api.ProjectMember
                {
                    Id = tuple.Item1.Id,
                    Description = tuple.Item1.Description,
                    Person = await _personRepository.GetPerson(db, tuple.Item1.PersonId),
                    Role = tuple.Item2,
                    Tags = (await db.SelectAsync(db
                        .From<ProjectMemberTag>()
                        .Where(w => w.ProjectMemberId == tuple.Item1.Id))).Select(x => x.Name).ToList()
                };
                members.Add(member);
            }

            // Places
            var placesData = await db.SelectMultiAsync<ProjectPlace, Place>(db.From<ProjectPlace>()
                .Join<ProjectPlace, Place>().Where(w => w.ProjectId == model.Id));
            var places = new List<Api.ProjectPlace>();
            foreach (var placeData in placesData)
            {
                var place = new Api.ProjectPlace {Id = placeData.Item1.Id, Place = placeData.Item2};
                places.Add(place);
            }
            model.Places.AddRange(places);

            // Categories
            var categoriesData = await db.SelectMultiAsync<ProjectCategory, Category>(db.From<ProjectCategory>()
                .Join<ProjectCategory, Category>().Where(w => w.ProjectId == model.Id));
            var categories = new List<Api.ProjectCategory>();
            foreach (var categoryData in categoriesData)
            {
                var category = new Api.ProjectCategory
                {
                    Id = categoryData.Item1.Id,
                    Category = categoryData.Item2.ConvertTo<Api.Category>()
                };
                categories.Add(category);
            }
            model.Categories.AddRange(categories);

            model.Members = members;
            return model;
        }
    }
}