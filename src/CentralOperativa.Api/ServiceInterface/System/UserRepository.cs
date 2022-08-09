using System;
using System.Data;
using System.Threading.Tasks;
using CentralOperativa.Domain.System;
using CentralOperativa.ServiceInterface.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System
{
    using Api = ServiceModel.System;

    public class UserRepository
    {
        private readonly PersonRepository _personRepository;

        public UserRepository(PersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Api.User> GetUser(IDbConnection db, int id)
        {
            var user = await db.SingleByIdAsync<User>(id);

            // Initialize profile and my documents folders for the user.
            if (!user.FolderId.HasValue)
            {
                var profileFolder = new Domain.System.DocumentManagement.Folder
                {
                    CreateDate = DateTime.UtcNow,
                    Guid = Guid.NewGuid(),
                    Name = "sy.dm.profile"
                };
                profileFolder.Id = (int) await db.InsertAsync(profileFolder, true);

                var myDocumentsFolder = new Domain.System.DocumentManagement.Folder
                {
                    CreateDate = DateTime.UtcNow,
                    Guid = Guid.NewGuid(),
                    Name = "sy.dm.mydocuments"
                };
                myDocumentsFolder.Id = (int) await db.InsertAsync(myDocumentsFolder, true);
                await db.InsertAsync(new Domain.System.DocumentManagement.FolderFolder { ParentId = profileFolder.Id, ChildId = myDocumentsFolder.Id, CreateDate = DateTime.UtcNow });
                user.FolderId = profileFolder.Id;
                await db.UpdateAsync(user);
            }

            var model = user.ConvertTo<Api.User>();
            model.Person = await _personRepository.GetPerson(db, model.PersonId);
            return model;
        }
    }
}