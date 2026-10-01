using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;
using System.Xml;

namespace VNGPortal.Pages
{
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class ViewerModel : PageModel
    {
        private readonly ILogger<ViewerModel> _logger;
        private readonly IConfiguration _configuration;

        public ViewerModel(ILogger<ViewerModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public string? Model { get; set; }

        public void OnGet()
        {
        }
    }
}