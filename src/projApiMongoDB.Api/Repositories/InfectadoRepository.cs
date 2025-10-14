using MongoDB.Driver;
using projApiMongoDB.Api.Models;
using projApiMongoDB.Api.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using System;

namespace projApiMongoDB.Api.Repositories
{
    public class InfectadoRepository : IInfectadoRepository
    {
        private readonly IMongoCollection<Infectado> _collection;

        public InfectadoRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            var db = mongoClient.GetDatabase(settings.DatabaseName);
            _collection = db.GetCollection<Infectado>(settings.InfectadosCollectionName);
            // Indexes could be created here (e.g., geospatial index)
            EnsureIndexes();
        }

        private void EnsureIndexes()
        {
            // Cria índices para latitude/longitude para consultas eficientes (2dsphere requires GeoJSON)
            // Aqui estamos deixando como comentário: se quiser usar geo queries, armazene coords como GeoJSON.
        }

        public async Task<IEnumerable<Infectado>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            var skip = (Math.Max(1, page) - 1) * pageSize;
            return await _collection.Find(Builders<Infectado>.Filter.Empty)
                                    .Skip(skip)
                                    .Limit(pageSize)
                                    .ToListAsync();
        }

        public async Task<Infectado> GetByIdAsync(string id)
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

        public async Task<IEnumerable<Infectado>> GetByProximityAsync(double latitude, double longitude, double maxDistanceKm = 10, int limit = 50)
        {
            // Simples implementação: busca todos e calcula distância Haversine em memória
            // Para produção use GeoJSON + 2dsphere index e $geoNear no MongoDB.
            var all = await _collection.Find(Builders<Infectado>.Filter.Empty).ToListAsync();

            double ToRad(double deg) => deg * Math.PI / 180.0;

            double Haversine(double lat1, double lon1, double lat2, double lon2)
            {
                var R = 6371.0; // km
                var dLat = ToRad(lat2 - lat1);
                var dLon = ToRad(lon2 - lon1);
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
                return R * c;
            }

            var results = new List<(Infectado inf, double dist)>();
            foreach (var inf in all)
            {
                var d = Haversine(latitude, longitude, inf.Latitude, inf.Longitude);
                if (d <= maxDistanceKm) results.Add((inf, d));
            }

            results.Sort((a, b) => a.dist.CompareTo(b.dist));
            var selected = results.Take(limit).Select(r => r.inf);

            return selected;
        }
    }
}
