using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.DocumentManagement
{
    [Alias("FileSystemProvider")]
    public class FileSystemProvider
    {
        [AutoIncrement, PrimaryKey]
        public short Id { get; set; }

        public byte Type { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }

        public string Configuration { get; set; }
    }
}