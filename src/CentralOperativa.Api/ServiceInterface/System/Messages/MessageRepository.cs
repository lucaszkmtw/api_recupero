using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System.Messages;

namespace CentralOperativa.ServiceInterface.System.Messages
{
    [Authenticate]
    public class MessageRepository
    {
        public async Task<List<Api.Message>> GetMessages(IDbConnection db, int threadId)
        {
            var model = await db.SelectAsync<Api.Message>(db
                .From<Domain.System.Messages.MessageThread>()
                .Join<Domain.System.Messages.MessageThread, Domain.System.Messages.Message>()
                .Join<Domain.System.Messages.Message, Domain.System.User>()
                .Where(w => w.Id == threadId));
            return model;
        }
    }
}