using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.DocumentManagement
{
    [Alias("FolderFolders")]
    public class FolderFolder
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [References(typeof(Folder))]
        public int ParentId { get; set; }

        [References(typeof(Folder))]
        public int ChildId { get; set; }

        public DateTime CreateDate { get; set; }
    }
}