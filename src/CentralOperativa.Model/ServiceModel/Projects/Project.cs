using System;
using System.Collections.Generic;
using CentralOperativa.Domain.Financials.Controlling;
using CentralOperativa.Domain.Projects;
using CentralOperativa.Domain.System.Location;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.System.Persons;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Projects
{
    [Route("/projects/lookup", "GET")]
    public class LookupProject : LookupRequest
    {
    }

    [Route("/projects", "GET")]
    public class QueryProjects : QueryRequestBase<Domain.Projects.Project>, IReturn<QueryResponse<Project>>
    {
        public byte? View { get; set; }
    }

    [Route("/projects/{Id}", "GET")]
    public class GetProject : IReturn<Project>
    {
        public int Id { get; set; }
    }

    public class Project : IBusinessEntity<Project>
    {
        public Project()
        {
            Tasks = new List<ProjectTask>();
            Places = new List<ProjectPlace>();
            Categories = new List<ProjectCategory>();
            Members = new List<ProjectMember>();
        }

        public int Id { get; set; }

        public int CreatedBy { get; set; }

        public Guid? FolderGuid { get; set; }

        public Guid? MessageThreadGuid { get; set; }

        public int? FundingTypeId { get; set; }

        public System.Workflows.WorkflowInstance.GetWorkflowInstanceResponse WorkflowInstance { get; set; }

        public Domain.Financials.Currency Currency { get; set; }

        public string Number { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Review { get; set; }

        public Guid Guid { get; set; }

        public ProjectStatus Status { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public decimal? Investment { get; set; }

        public decimal? ContractAmount { get; set; }

        public decimal? AditionalAmount { get; set; }

        public decimal? AdjustedAmount { get; set; }

        public decimal? Total { get; set; }

        public List<ProjectTask> Tasks { get; set; }

        public List<ProjectPlace> Places { get; set; }

        public List<ProjectCategory> Categories { get; set; }

        public List<ProjectMember> Members { get; set; }

        public string Roles { get; set; }
    }

    public class ProjectCategory
    {
        public int Id { get; set; }

        public Category Category { get; set; }
    }

    public class ProjectPlace
    {
        public int Id { get; set; }

        public Place Place { get; set; }
    }

    public class ProjectMember
    {
        public int Id { get; set; }

        public Person Person { get; set; }

        public ProjectMemberRole Role { get; set; }

        public List<string> Tags { get; set; }

        public string Description { get; set; }

        public ProjectMember()
        {
            Tags = new List<string>();
        }
    }

    [Route("/projects", "POST")]
    [Route("/projects/{Id}", "PUT")]
    public class PostProject : Project, IReturn<Project>
    {
    }

    [Route("/projects/{Id}", "DELETE")]
    public class DeleteProject : IReturnVoid
    {
        public int Id { get; set; }
    }
}