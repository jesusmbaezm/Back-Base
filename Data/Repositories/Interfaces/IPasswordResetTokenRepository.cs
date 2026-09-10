using Data.Entities;

namespace Data.Repositories.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetActiveByUserIdAsync(int userId);
        Task InvalidateActiveTokensAsync(int userId, int modifiedByUserId);
        Task SaveChangesAsync();
    }
}
