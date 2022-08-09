using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.DocumentManagement
{
    [Alias("Files")]
    public class File
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public short ProviderId { get; set; }

        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public DateTime CreateDate { get; set; }
    }
}