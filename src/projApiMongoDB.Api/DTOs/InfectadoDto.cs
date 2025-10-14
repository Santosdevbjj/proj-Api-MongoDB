using System;
using System.ComponentModel.DataAnnotations;

namespace projApiMongoDB.Api.DTOs
{
    /// <summary>
    /// DTO para criação/atualização, evitando expor Id diretamente em criação.
    /// </summary>
    public class InfectadoDto
    {
        [Required]
        public DateTime DataNascimento { get; set; }

        [Required]
        [StringLength(2)]
        public string Sexo { get; set; }

        [Required]
        [Range(-90.0, 90.0)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180.0, 180.0)]
        public double Longitude { get; set; }
    }
}
