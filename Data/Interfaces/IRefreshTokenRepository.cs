using RbacApi.Data.Entities;
using System.Linq.Expressions;

namespace RbacApi.Data.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<RefreshToken?> GetRefreshTokenAsync(Expression<Func<RefreshToken, bool>> filter);
        Task RevokeAllTokensAsync(string userId, string? replacedByToken = null);
        Task UpdateAsync(RefreshToken refreshToken);
    }
}
