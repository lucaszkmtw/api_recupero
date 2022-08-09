using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Messages
{
    [Alias("MessageFiles")]
    public class MessageFile
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Message))]
        public int MessageId { get; set; }

        [References(typeof(Domain.System.DocumentManagement.File))]
        public int FileId { get; set; }
    }
}