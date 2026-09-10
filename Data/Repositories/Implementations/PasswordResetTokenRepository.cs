using Data.Contexts;
using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories.Implementations
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(PasswordResetToken token)
        {
            return _context.PasswordResetTokens.AddAsync(token).AsTask();
        }

        public Task<PasswordResetToken?> GetActiveByUserIdAsync(int userId)
        {
            return _context.PasswordResetTokens
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted &&
                    x.UsedAt == null);
        }

        public async Task InvalidateActiveTokensAsync(int userId, int modifiedByUserId)
        {
            var tokens = await _context.PasswordResetTokens
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsDeleted &&
                    x.UsedAt == null)
                .ToListAsync();

            var now = DateTimeOffset.UtcNow;
            foreach (var token in tokens)
            {
                token.IsDeleted = true;
                token.ModifiedAt = now;
                token.ModifiedByUserId = modifiedByUserId;
            }
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
