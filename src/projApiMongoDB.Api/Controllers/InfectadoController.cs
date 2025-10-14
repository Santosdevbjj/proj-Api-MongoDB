using Microsoft.AspNetCore.Mvc;
using projApiMongoDB.Api.DTOs;
using projApiMongoDB.Api.Models;
using projApiMongoDB.Api.Repositories;
using System;
using System.Threading.Tasks;

namespace projApiMongoDB.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfectadoController : ControllerBase
    {
        private readonly IInfectadoRepository _repo;

        public InfectadoController(IInfectadoRepository repo)
        {
            _repo = repo;
        }

        // GET api/infectado?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            // ... (Método GetAll) ...
            var items = await _repo.GetAllAsync(page, pageSize);
            return Ok(items);
        }

        // ... (Outros métodos: GetById, Create, Update, Delete) ...

        // GET api/infectado/proximity?lat=-23.5&lon=-46.6&maxKm=5&limit=20
        [HttpGet("proximity")]
        public async Task<IActionResult> Proximity([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double maxKm = 10, [FromQuery] int limit = 50)
        {
            var results = await _repo.GetByProximityAsync(lat, lon, maxKm, limit);
            return Ok(results);
        }

        // GET api/infectado/nearby?lat=-23.56&lon=-46.65&maxKm=5&limit=20
        // ESTE É O MÉTODO QUE ESTAVA FORA DA CLASSE/NAMESPACE
        [HttpGet("nearby")]
        public async Task<IActionResult> Nearby([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double maxKm = 10, [FromQuery] int limit = 50)
        {
            var results = await _repo.GetByProximityAsync(lat, lon, maxKm, limit);
            return Ok(results);
        }
    } // AQUI DEVE FECHAR A CLASSE
} // AQUI DEVE FECHAR O NAMESPACE
