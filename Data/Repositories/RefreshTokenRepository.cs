using MongoDB.Driver;
using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;
using System.Linq.Expressions;

namespace RbacApi.Data.Repositories
{
    public class RefreshTokenRepository(CollectionsProvider collections) : IRefreshTokenRepository
    {
        private readonly IMongoCollection<RefreshToken> _refreshTokens = collections.RefreshTokens;

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
            => await _refreshTokens.InsertOneAsync(refreshToken);

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
            => await _refreshTokens.Find(t => t.Token == token).FirstOrDefaultAsync();

        public async Task<RefreshToken?> GetRefreshTokenAsync(Expression<Func<RefreshToken, bool>> expression)
            => await _refreshTokens.Find(expression).FirstOrDefaultAsync();

        public async Task RevokeAllTokensAsync(string userId, string? replacedByToken = null)
        {
            var filterActive = Builders<RefreshToken>.Filter.Where(t => t.UserId == userId && !t.Revoked);
            var updateRevoke = Builders<RefreshToken>.Update
                .Set(t => t.Revoked, true)
                .Set(t => t.ReplacedByToken, replacedByToken ?? $"revoked_due_to_reuse_{DateTime.UtcNow.Ticks}");

            await _refreshTokens.UpdateManyAsync(filterActive, updateRevoke);
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
            => await _refreshTokens.ReplaceOneAsync(t => t.Id == refreshToken.Id, refreshToken);
    }
}
