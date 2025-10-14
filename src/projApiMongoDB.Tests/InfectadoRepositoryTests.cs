using Xunit;
using Moq;
using MongoDB.Driver;
using projApiMongoDB.Api.Repositories;
using projApiMongoDB.Api.Settings;

public class InfectadoRepositoryTests
{
    [Fact]
    public void Constructor_DoesNotThrow_WithValidParameters()
    {
        var mockClient = new Mock<IMongoClient>();
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "projApiMongoDB_Test",
            InfectadosCollectionName = "infectados"
        };

        // only ensures constructor runs - deeper integration tests should use Testcontainers or a running MongoDB
        var repo = new InfectadoRepository(mockClient.Object, settings);
        Assert.NotNull(repo);
    }
}
