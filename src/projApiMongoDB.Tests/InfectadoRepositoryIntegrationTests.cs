using System;
using System.Threading.Tasks;
using Xunit;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MongoDB.Driver;
using projApiMongoDB.Api.Repositories;
using projApiMongoDB.Api.Settings;
using projApiMongoDB.Api.Models;

public class InfectadoRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly TestcontainersContainer _mongoContainer;
    private IMongoClient? _client;
    private InfectadoRepository? _repo;

    public InfectadoRepositoryIntegrationTests()
    {
        _mongoContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo:6.0")
            .WithName("test-mongo-" + Guid.NewGuid().ToString("n").Substring(0, 6))
            .WithPortBinding(27017, true)
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var mappedPort = _mongoContainer.GetMappedPublicPort(27017);
        var conn = $"mongodb://localhost:{mappedPort}";
        _client = new MongoClient(conn);

        var settings = new MongoDbSettings
        {
            ConnectionString = conn,
            DatabaseName = "projApiMongoDB_Test",
            InfectadosCollectionName = "infectados"
        };

        _repo = new InfectadoRepository(_client, settings);
    }

    public async Task DisposeAsync()
    {
        if (_mongoContainer != null) await _mongoContainer.StopAsync();
    }

    [Fact]
    public async Task CreateAndGetById_Works()
    {
        var infectado = new Infectado
        {
            DataNascimento = new DateTime(1990, 3, 1),
            Sexo = "M",
            Location = new GeoJsonPoint { Coordinates = new[] { -46.6565712, -23.5630994 } }
        };

        await _repo!.CreateAsync(infectado);
        Assert.False(string.IsNullOrEmpty(infectado.Id));

        var fetched = await _repo.GetByIdAsync(infectado.Id!);
        Assert.NotNull(fetched);
        Assert.Equal(infectado.Sexo, fetched!.Sexo);
    }

    [Fact]
    public async Task GeoQuery_ReturnsNearby()
    {
        // Insere um ponto próximo
        var point = new Infectado
        {
            DataNascimento = DateTime.UtcNow,
            Sexo = "F",
            Location = new GeoJsonPoint { Coordinates = new[] { -46.6565712, -23.5630994 } } // São Paulo exemplo
        };
        await _repo!.CreateAsync(point);

        // Consulta por proximity: lat, lon (note: lat then lon)
        var results = await _repo.GetByProximityAsync(-23.5630994, -46.6565712, maxDistanceKm: 1, limit: 10);
        Assert.NotEmpty(results);
    }
}
