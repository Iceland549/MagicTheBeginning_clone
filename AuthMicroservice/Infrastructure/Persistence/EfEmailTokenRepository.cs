using System.Threading.Tasks;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation for managing email tokens.
    /// </summary>
    public class EfEmailTokenRepository : IEmailTokenRepository
    {
        private readonly AuthDbContext _ctx;

        public EfEmailTokenRepository(AuthDbContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// Adds a new email token.
        /// </summary>
        public async Task AddAsync(EmailToken emailToken)
        {
            _ctx.EmailTokens.Add(emailToken);
            await _ctx.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing email token.
        /// </summary>
        public async Task UpdateAsync(EmailToken emailToken)
        {
            _ctx.EmailTokens.Update(emailToken);
            await _ctx.SaveChangesAsync();
        }

        /// <summary>
        /// Finds an email token by its value.
        /// </summary>
        public async Task<EmailToken?> GetByTokenAsync(string token)
        {
            return await _ctx.EmailTokens
                             .FirstOrDefaultAsync(et => et.Token == token);
        }

        /// <summary>
        /// Deletes a token by its value.
        /// </summary>
        public async Task DeleteAsync(string token)
        {
            var entity = await _ctx.EmailTokens.FindAsync(token);
            if (entity != null)
            {
                _ctx.EmailTokens.Remove(entity);
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
