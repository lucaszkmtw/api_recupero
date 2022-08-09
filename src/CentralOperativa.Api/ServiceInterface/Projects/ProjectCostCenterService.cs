using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.Projects;
using CentralOperativa.ServiceInterface.Financials.Controlling;
using Api = CentralOperativa.ServiceModel.Projects;

namespace CentralOperativa.ServiceInterface.Projects
{
    [Authenticate]
    public class ProjectCostCenterService : ApplicationService
    {
        private readonly ProjectRepository _projectRepository;

        public ProjectCostCenterService(ProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<List<Api.ProjectCostCenter>> Get(Api.GetProjectCostCenters request)
        {
            return await _projectRepository.GetProjectCostCenters(Db, request.ProjectId);
        }

        public async Task<Api.ProjectCostCenter> Post(Api.PostProjectCostCenter request)
        {
            var data = request.ConvertTo<ProjectCostCenter>();
            data.Id = (int) await Db.InsertAsync(data, true);
            return await _projectRepository.GetProjectCostCenter(Db, data.Id);
        }
    }
}