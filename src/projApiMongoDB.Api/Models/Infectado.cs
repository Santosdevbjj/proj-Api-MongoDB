using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace projApiMongoDB.Api.Models
{
    /// <summary>
    /// Representa um registro de uma pessoa infectada (exemplo).
    /// Utiliza atributos do MongoDB para mapear o Id como ObjectId.
    /// </summary>
    public class Infectado
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime DataNascimento { get; set; }

        /// <summary>
        /// "M" | "F" | "O" etc. (validate upstream)
        /// </summary>
        public string Sexo { get; set; }

        /// <summary>
        /// Latitude (double)
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude (double)
        /// </summary>
        public double Longitude { get; set; }

        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }
}
