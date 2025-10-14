using System.Collections.Generic;
using System.Threading.Tasks;
using projApiMongoDB.Api.Models;

namespace projApiMongoDB.Api.Repositories
{
    public interface IInfectadoRepository
    {
        Task<IEnumerable<Infectado>> GetAllAsync(int page = 1, int pageSize = 50);
        
        // Linha 10 (ou aproximada): Mantenha APENAS esta declaração.
        // O '?' resolve o warning de nulidade.
        Task<Infectado?> GetByIdAsync(string id); 
        
        Task CreateAsync(Infectado infectado);
        Task UpdateAsync(string id, Infectado infectado);
        Task DeleteAsync(string id);

        // A LINHA DUPLICADA DEVE SER EXCLUÍDA.
        // Se a linha 15 do seu arquivo for uma segunda declaração de GetByIdAsync, ela deve ser removida.

        // Busca por proximidade — retorna infectados ordenados por distância crescente
        Task<IEnumerable<Infectado>> GetByProximityAsync(double latitude, double longitude, double maxDistanceKm = 10, int limit = 50);
    }
}
