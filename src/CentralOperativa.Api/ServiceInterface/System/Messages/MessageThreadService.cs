using System.Threading.Tasks;
using Api = CentralOperativa.ServiceModel.System.Messages;

namespace CentralOperativa.ServiceInterface.System.Messages
{    
    public class MessageThreadService : ApplicationService
    {
        private readonly MessageRepository _messageRepository;

        public MessageThreadService(MessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<object> Get(Api.Message.QueryMessages request)
        {
            return await _messageRepository.GetMessages(Db, request.MessageThreadId);
        }
    }
}
