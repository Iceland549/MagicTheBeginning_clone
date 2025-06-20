using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation for refresh token repository.
    /// </summary>
    public class EfRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;

        public EfRefreshTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(RefreshToken refreshToken)
        {
            _context.Set<RefreshToken>().Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.Set<RefreshToken>()
                                 .Include(rt => rt.User)
                                 .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task InvalidateAsync(string token)
        {
            var rt = await _context.Set<RefreshToken>()
                                   .FirstOrDefaultAsync(x => x.Token == token);
            if (rt != null)
            {
                rt.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}