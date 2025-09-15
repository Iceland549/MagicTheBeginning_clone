using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Application.Interfaces
{
    public interface IServiceClientRepository
    {
        Task<ServiceClient?> GetByClientIdAsync(string clientId);
        Task AddAsync(ServiceClient client);  
        Task<bool> ExistsAsync(string clientId);  
    }
}
