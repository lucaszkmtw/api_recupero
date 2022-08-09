using System;
using System.Collections.Generic;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.System.Persons;
using ServiceStack;

namespace CentralOperativa.ServiceModel.BusinessDocuments
{
    [Route("/businessdocuments/documents/migration/submit", "POST")]
    public class PostBusinessDocumentMigration
    {
    }

    [Route("/businessdocuments/documents/migration/acivate", "POST")]
    public class PostBusinessDocumentMigrationActivate
    {
    }
}