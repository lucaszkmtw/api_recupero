using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.HumanResources
{
    public class Employee
    {
        [Route("/hr/employees/{Id}", "PUT")]
        public class Put : Domain.HumanResources.Employee
        {
        }

        [Route("/hr/employees", "POST")]
        public class Post : Domain.HumanResources.Employee
        {
        }

        [Route("/hr/employees/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/hr/employees/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        [Route("/hr/employees", "GET")]
        public class Query : QueryDb<Domain.HumanResources.Employee, QueryResult>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string VatNumber { get; set; }

            public string FullName
            {
                get
                {
                    if (!string.IsNullOrEmpty(this.LastName) && !string.IsNullOrEmpty(this.FirstName))
                    {
                        return this.LastName.Trim() + ", " + this.FirstName.Trim();

                    }

                    return !string.IsNullOrEmpty(this.LastName) ? this.LastName : null;
                }
            }
        }
    }
}
