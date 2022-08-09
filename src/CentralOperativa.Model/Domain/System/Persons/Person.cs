using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("Persons")]
    public class Person
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public bool? IsOrganization { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public byte? Gender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime? DeathDate { get; set; }

        public string Data1 { get; set; }
        public bool? IsValid { get; set; }

        public string WebUrl { get; set; }

        public string ProfilePictureUrl { get; set; }

         public string RNOS { get; set; }
    }
}