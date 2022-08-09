namespace CentralOperativa.ServiceModel.System.Persons
{
    public static class PersonExtensions
    {
        public static string GetFullName(this Domain.System.Persons.Person person)
        {
            return string.IsNullOrEmpty(person.FirstName) ? person.LastName : string.Concat(person.LastName, ", ", person.FirstName);
        }
    }
}