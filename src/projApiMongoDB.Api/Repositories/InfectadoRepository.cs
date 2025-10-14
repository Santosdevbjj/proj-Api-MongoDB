using MongoDB.Driver;
using MongoDB.Bson; // <--- CORREÇÃO PRINCIPAL: Adicionado para BsonDocument e BsonArray
using projApiMongoDB.Api.Models;
using projApiMongoDB.Api.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using MongoDB.Driver.GeoJson;

namespace projApiMongoDB.Api.Repositories
{
    public class InfectadoRepository : IInfectadoRepository
    {
        private readonly IMongoCollection<Infectado> _collection;

        public InfectadoRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            var db = mongoClient.GetDatabase(settings.DatabaseName);
            _collection = db.GetCollection<Infectado>(settings.InfectadosCollectionName);

            // Garantir índices (incl. 2dsphere para consultas geoespaciais)
            EnsureIndexes();
        }

        private void EnsureIndexes()
        {
            // Cria index 2dsphere no campo "location" para operações geoQuery.
            var indexKeys = Builders<Infectado>.IndexKeys.Geo2DSphere(i => i.Location);
            var indexModel = new CreateIndexModel<Infectado>(indexKeys);
            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<IEnumerable<Infectado>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            var skip = (Math.Max(1, page) - 1) * pageSize;
            return await _collection.Find(Builders<Infectado>.Filter.Empty)
                                    .Skip(skip)
                                    .Limit(pageSize)
                                    .ToListAsync();
        }

        public async Task<Infectado?> GetByIdAsync(string id)
        {
            var filter = Builders<Infectado>.Filter.Eq(i => i.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Infectado infectado)
        {
            await _collection.InsertOneAsync(infectado);
        }

        public async Task UpdateAsync(string id, Infectado infectado)
        {
            infectado.Id = id;
            var filter = Builders<Infectado>.Filter.Eq(i => i.Id, id);
            await _collection.ReplaceOneAsync(filter, infectado);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<Infectado>.Filter.Eq(i => i.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        /// <summary>
        /// Busca por proximidade utilizando $geoNear no MongoDB (eficiente).
        /// latitude, longitude em graus.
        /// maxDistanceKm é em quilômetros.
        /// </summary>
        public async Task<IEnumerable<Infectado>> GetByProximityAsync(double latitude, double longitude, double maxDistanceKm = 10, int limit = 50)
        {
            var maxDistanceMeters = maxDistanceKm * 1000.0;
            
            // 1. Define o ponto de busca (o driver espera (longitude, latitude))
            var point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                new GeoJson2DGeographicCoordinates(longitude, latitude)
            );

            // 2. Cria a Pipeline GeoNear usando a sintaxe de agregação do driver
            var pipeline = _collection.Aggregate()
                .GeoNear(
                    point,
                    distanceField: "Distancia", // Campo onde a distância será armazenada no resultado
                    maxDistance: maxDistanceMeters,
                    spherical: true
                )
                .Limit(limit); // Aplica o limite

            // 3. Executa a agregação
            var results = await pipeline.ToListAsync();
            
            return results;
        }
    }
} 
