using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.DocumentManagement
{
    [Alias("FolderFiles")]
    public class FolderFile
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [References(typeof(Folder))]
        public int FolderId { get; set; }

        [References(typeof(File))]
        public int FileId { get; set; }

        public DateTime CreateDate { get; set; }
    }
}