using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.DocumentManagement
{
    [Alias("Folders")]
    public class Folder
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public Guid Guid { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }
    }
}