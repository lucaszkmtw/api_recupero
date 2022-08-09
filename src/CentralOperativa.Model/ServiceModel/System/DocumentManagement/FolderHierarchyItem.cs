using System;

namespace CentralOperativa.ServiceModel.System.DocumentManagement
{
    public class FolderHierarchyItem
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public int Level { get; set; }
        public string Path { get; set; }
    }
}