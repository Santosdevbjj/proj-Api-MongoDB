using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace projApiMongoDB.Api.Models
{
    /// <summary>
    /// Infectado com campo GeoJSON para consultas geoespaciais eficientes.
    /// </summary>
    public class Infectado
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime DataNascimento { get; set; }

        public string Sexo { get; set; } = string.Empty;

        /// <summary>
        /// GeoJSON Point: { type: "Point", coordinates: [longitude, latitude] }
        /// Stored as BsonDocument or a typed class below.
        /// </summary>
        [BsonElement("location")]
        public GeoJsonPoint Location { get; set; } = new GeoJsonPoint();

        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Classe simples para representar GeoJSON Point esperado pelo MongoDB C# Driver.
    /// </summary>
    public class GeoJsonPoint
    {
        [BsonElement("type")]
        public string Type { get; set; } = "Point";

        [BsonElement("coordinates")]
        public double[] Coordinates { get; set; } = new double[2]; // [lon, lat]
    }
}
