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
            var items = await _repo.GetAllAsync(page, pageSize);
            return Ok(items);
        }

        // GET api/infectado/{id}
        [HttpGet("{id:length(24)}", Name = "GetInfectado")]
        public async Task<IActionResult> GetById(string id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST api/infectado
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InfectadoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var model = new Infectado
            {
                DataNascimento = dto.DataNascimento,
                Sexo = dto.Sexo,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                DataRegistro = DateTime.UtcNow
            };

            await _repo.CreateAsync(model);
            return CreatedAtRoute("GetInfectado", new { id = model.Id }, model);
        }

        // PUT api/infectado/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] InfectadoDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.DataNascimento = dto.DataNascimento;
            existing.Sexo = dto.Sexo;
            existing.Latitude = dto.Latitude;
            existing.Longitude = dto.Longitude;
            await _repo.UpdateAsync(id, existing);
            return NoContent();
        }

        // DELETE api/infectado/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(id);
            return NoContent();
        }

        // GET api/infectado/proximity?lat=-23.5&lon=-46.6&maxKm=5&limit=20
        [HttpGet("proximity")]
        public async Task<IActionResult> Proximity([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double maxKm = 10, [FromQuery] int limit = 50)
        {
            var results = await _repo.GetByProximityAsync(lat, lon, maxKm, limit);
            return Ok(results);
        }
    }
}
