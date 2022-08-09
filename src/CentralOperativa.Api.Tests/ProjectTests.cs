using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CentralOperativa.Domain.Projects;
using Microsoft.AspNetCore.Razor.Language;
using ServiceStack;
using Xunit;

namespace CentralOperativa.Api.Tests
{
    [Collection("Api collection")]
    public class ProjectTests
    {
        private readonly ApiClientFixture _fixture;

        public ProjectTests(ApiClientFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CanCreateProject()
        {
            var project = new ServiceModel.Projects.PostProject
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = _fixture.Session.UserId,
                Description = "Test Description",
                Guid = Guid.NewGuid(),
                Name = "Test",
                Number = "P" + DateTime.UtcNow.Ticks,
                Status = ProjectStatus.Proposed
            };

            // Categories
            var category = _fixture.ServiceClient.Get(new ServiceModel.Projects.GetCategory { Id = new Random(2).Next(1, 21) });
            var projectCategory = new ServiceModel.Projects.ProjectCategory { Category = category };
            project.Categories.Add(projectCategory);

            category = _fixture.ServiceClient.Get(new ServiceModel.Projects.GetCategory { Id = new Random(2).Next(1, 21) });
            projectCategory = new ServiceModel.Projects.ProjectCategory { Category = category };
            project.Categories.Add(projectCategory);

            // Members
            var person = _fixture.ServiceClient.Get(new ServiceModel.System.Persons.GetPerson { Id = 1 });
            var tags = new List<string> { "Lider de proyecto" };
            var projectMember = new ServiceModel.Projects.ProjectMember
            {
                Description = "Descripción del miembro del proyecto.",
                Person = person,
                Role = new ProjectMemberRole { Id = 1, Name = "sponsor", TenantId = 11 },
                Tags = tags
            };
            project.Members.Add(projectMember);

            person = _fixture.ServiceClient.Get(new ServiceModel.System.Persons.GetPerson { Id = 35237 });
            tags = new List<string> { "Desarrollador" };
            projectMember = new ServiceModel.Projects.ProjectMember
            {
                Description = "Descripción del miembro del proyecto.",
                Person = person,
                Role = new ProjectMemberRole { Id = 2, Name = "teammember", TenantId = 11 },
                Tags = tags
            };
            project.Members.Add(projectMember);

            person = _fixture.ServiceClient.Get(new ServiceModel.System.Persons.GetPerson { Id = 47248 });
            tags = new List<string> { "Desarrollador" };
            projectMember = new ServiceModel.Projects.ProjectMember
            {
                Description = "Descripción del miembro del proyecto.",
                Person = person,
                Role = new ProjectMemberRole { Id = 2, Name = "teammember", TenantId = 11 },
                Tags = tags
            };
            project.Members.Add(projectMember);

            // Places

            var response = _fixture.ServiceClient.Post(project);
            Assert.NotNull(response);
        }

        [Fact]
        public void CanQueryProjects()
        {
            var data = _fixture.ServiceClient.Get(new ServiceModel.Projects.QueryProjects());
            Assert.True(data.Total > 0);
        }

        [Fact]
        public void CanGetProject()
        {
            var data = _fixture.ServiceClient.Get(new ServiceModel.Projects.QueryProjects());
            Assert.True(data.Total > 0);
            var projectId = data.Results.OrderByDescending(x => x.Id).First().Id;
            var project = _fixture.ServiceClient.Get(new ServiceModel.Projects.GetProject { Id = projectId });
            Assert.NotNull(project);
        }

        [Fact]
        public void CanUpdateProject()
        {
            var data = _fixture.ServiceClient.Get(new ServiceModel.Projects.QueryProjects());
            Assert.True(data.Total > 0);
            var projectId = data.Results.OrderByDescending(x => x.Id).First().Id;
            var project = _fixture.ServiceClient.Get(new ServiceModel.Projects.GetProject { Id = projectId });
            Assert.NotNull(project);
        }

        [Fact]
        public async void CanGetProjectCostCenters()
        {
            var data = await _fixture.ServiceClient.GetAsync(new ServiceModel.Projects.GetProjectCostCenters { ProjectId = 50 });
            Assert.True(data.Count > 0);
            Assert.True(data.First().CostCenter?.Id > 0);
        }

        [Fact]
        public async void CanAddProjectCostCenter()
        {
            var projectId = 50;

            var before = await _fixture.ServiceClient.GetAsync(new ServiceModel.Projects.GetProjectCostCenters { ProjectId = projectId });
            Assert.True(before.Count > 0);

            var costCenter = await _fixture.ServiceClient.PostAsync(new ServiceModel.Financials.Controlling.PostCostCenter { CurrenctyId = 1, Name = $"Test {DateTime.UtcNow.Ticks}" });
            Assert.True(costCenter.Id > 0);

            var newProjectCostCenter = await _fixture.ServiceClient.PostAsync(new ServiceModel.Projects.PostProjectCostCenter { ProjectId = projectId, CostCenterId = costCenter.Id });
            Assert.True(newProjectCostCenter.Id > 0);

            var after = await _fixture.ServiceClient.GetAsync(new ServiceModel.Projects.GetProjectCostCenters { ProjectId = projectId });
            Assert.True(after.Count > 0);
            Assert.True(after.Count == before.Count + 1);
        }
    }
}
