using System;
using System.Threading.Tasks;
using CentralOperativa.Domain.System.Messages;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.ServiceModel.System;
using CentralOperativa.ServiceInterface.System.DocumentManagement;
using CentralOperativa.ServiceInterface.System.Workflows;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System
{
    [Authenticate]
    public class FeedbackTicketService : ApplicationService
    {
        private readonly WorkflowActivityRepository _workflowActivityRepository;
        private readonly FileRepository _fileRepository;

        public FeedbackTicketService(
            WorkflowActivityRepository workflowActivityRepository,
            FileRepository fileRepository)
        {
            _workflowActivityRepository = workflowActivityRepository;
            _fileRepository = fileRepository;
        }

        public async Task<Domain.System.FeedbackTicket> Post(PostFeedbackTicket request)
        {
            var ticket = new Domain.System.FeedbackTicket
            {
                CreatedByUserId = Session.UserId,
                CreateDate = DateTime.UtcNow
            };

            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    //WorkflowInstance
                    var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 1, (short) WellKnownWorkflowTypes.FeedbackTicket);
                    var workflowInstance = new WorkflowInstance
                    {
                        WorkflowId = currentActivity.WorkflowId,
                        CreatedByUserId = Session.UserId,
                        CreateDate = DateTime.UtcNow,
                        Guid = Guid.NewGuid(),
                        CurrentWorkflowActivityId = currentActivity.Id
                    };
                    workflowInstance.Id = (int) await Db.InsertAsync(workflowInstance, true);
                    ticket.WorkflowInstanceId = workflowInstance.Id;

                    var messageTrhead = new MessageThread { CreateDate = DateTime.UtcNow };
                    messageTrhead.Id = (int) await Db.InsertAsync(messageTrhead, true);

                    var message = new Message
                    {
                        Body = request.Feedback.Note,
                        CreateDate = DateTime.UtcNow,
                        MessageThreadId = messageTrhead.Id,
                        SenderId = Session.UserId
                    };
                    message.Id = (int) await Db.InsertAsync(message, true);

                    ticket.MessageThreadId = messageTrhead.Id;
                    ticket.Id = (int) await Db.InsertAsync(ticket, true);

                    if (!string.IsNullOrEmpty(request.Feedback.Img))
                    {
                        var startIndex = request.Feedback.Img.IndexOf(",", StringComparison.Ordinal)+1;
                        var base64EncodedImage = request.Feedback.Img.Substring(startIndex);
                        var file = await _fileRepository.CreateFile(Db, base64EncodedImage, "centraloperativa-feedbacktickets", "Ticket-" + ticket.Id + ".png", "image/png");
                        var messageFile = new MessageFile
                        {
                            MessageId = message.Id,
                            FileId = file.Id
                        };
                        await Db.InsertAsync(messageFile);
                    }

                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return ticket;
        }
    }
}
