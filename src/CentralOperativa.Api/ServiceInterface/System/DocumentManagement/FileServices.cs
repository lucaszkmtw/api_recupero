using System.Threading.Tasks;
using CentralOperativa.Infraestructure;
using Api = CentralOperativa.ServiceModel.System.DocumentManagement;

namespace CentralOperativa.ServiceInterface.System.DocumentManagement
{    
    public class FileServices : ApplicationService
    {
        private readonly FileRepository _fileRepository;

        public FileServices(FileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task<object> Get(Api.GetFile request)
        {
            return await _fileRepository.GetFile(Db, request.Guid);
        }
    }
}
