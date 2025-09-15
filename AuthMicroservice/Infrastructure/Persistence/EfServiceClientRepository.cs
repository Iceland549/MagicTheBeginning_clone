using System.Threading.Tasks;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Infrastructure.Persistence
{
    public class EfServiceClientRepository : IServiceClientRepository
    {
        private readonly AuthDbContext _ctx;

        public EfServiceClientRepository(AuthDbContext ctx) => _ctx = ctx;

        public Task<ServiceClient?> GetByClientIdAsync(string clientId) =>
            _ctx.ServiceClients.FirstOrDefaultAsync(c => c.ClientId == clientId);

        public async Task AddAsync(ServiceClient client)
        {
            _ctx.ServiceClients.Add(client);
            await _ctx.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(string clientId) =>
            _ctx.ServiceClients.AnyAsync(c => c.ClientId == clientId);
    }
}
