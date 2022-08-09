using Xunit;

namespace CentralOperativa.Api.Tests
{
    [CollectionDefinition("Api collection")]
    public class ApiCollectionFixture : ICollectionFixture<ApiClientFixture>
    {
    }
}
