using MagniseTest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagniseTest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetRepository _assetRepository;

        public AssetsController(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetSupportedAssets()
        {
            var assets = await _assetRepository.GetAllAssetsAsync();
            return Ok(assets); 
        }
    }
}