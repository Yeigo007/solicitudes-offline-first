using Xunit;

namespace Solicitudes.Tests.Integration;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApiTestCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "API integration";
}
