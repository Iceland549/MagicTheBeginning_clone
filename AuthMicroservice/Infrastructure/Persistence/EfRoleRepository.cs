using System.Collections.Generic;
using System.Threading.Tasks;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation for role repository.
    /// </summary>
    public class EfRoleRepository : IRoleRepository
    {
        private readonly AuthDbContext _ctx;

        public EfRoleRepository(AuthDbContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// Récupère un rôle par son identifiant.
        /// </summary>
        public async Task<Role?> GetByIdAsync(string roleId) =>
            await _ctx.Roles.FindAsync(roleId);

        /// <summary>
        /// Récupère tous les rôles existants.
        /// </summary>
        public async Task<List<Role>> GetAllAsync() =>
            await _ctx.Roles.ToListAsync();

        // Méthodes CRUD complémentaires
        public async Task AddAsync(Role role)
        {
            _ctx.Roles.Add(role);
            await _ctx.SaveChangesAsync();
        }


        public async Task UpdateAsync(Role role)
        {
            _ctx.Roles.Update(role);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(string roleId)
        {
            var role = await GetByIdAsync(roleId);
            if (role != null)
            {
                _ctx.Roles.Remove(role);
                await _ctx.SaveChangesAsync();
            }
        }
    }
}