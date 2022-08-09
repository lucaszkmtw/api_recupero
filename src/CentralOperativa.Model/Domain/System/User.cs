using System;
using CentralOperativa.Domain.System.DocumentManagement;
using CentralOperativa.Domain.System.Persons;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("Users")]
    public class User
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(Folder))]
        public int? FolderId { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }
    }
}