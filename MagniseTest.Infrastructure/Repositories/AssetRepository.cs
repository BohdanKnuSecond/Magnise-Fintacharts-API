using MagniseTest.Application.Interfaces;
using MagniseTest.Domain.Entities;
using MagniseTest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MagniseTest.Infrastructure.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly AppDbContext _dbContext;

        public AssetRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
        {
            
            return await _dbContext.Assets.ToListAsync();
        }
    }
}