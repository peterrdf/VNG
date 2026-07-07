using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VNGService.Models;
using VNGService.Services;

namespace VNGService.Pages
{
    public class LandRegistryModel : PageModel
    {
        private readonly ILogger<LandRegistryModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ILandRegistryService _landRegistryService;

        public LandRegistryModel(IConfiguration configuration, ILogger<LandRegistryModel> logger, ILandRegistryService landRegistryService)
        {
            _configuration = configuration;
            _logger = logger;
            _landRegistryService = landRegistryService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetBuildings(double eastings, double northings)
        {
            try
            {
                var buildings = await _landRegistryService.GetBuildings(eastings, northings, bboxLength: 400); // #todo: bboxLength as parameter
                return new JsonResult(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Building.");
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetParcels(double eastings, double northings)
        {
            try
            {
                var parcels = await _landRegistryService.GetParcels(eastings, northings, bboxLength: 400); // #todo: bboxLength as parameter
                return new JsonResult(parcels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Parcels.");
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult(new { error = ex.Message });
            }
        }
    }
}